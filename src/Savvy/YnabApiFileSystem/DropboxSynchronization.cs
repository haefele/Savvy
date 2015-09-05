using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json.Linq;
using Savvy.Extensions;
using Savvy.Services.DropboxAuthentication;

namespace Savvy.YnabApiFileSystem
{
    public class DropboxSynchronization
    {
        private readonly StorageFolder _rootFolder;
        private readonly DropboxAuth _auth;

        public DropboxSynchronization(StorageFolder rootFolder, DropboxAuth auth)
        {
            this._rootFolder = rootFolder;
            this._auth = auth;
        }
        
        public async Task RefreshLocalStateAsync()
        {
            var userFolder = await this.CreateUserFolderAsync();

            var ynabSettingsYroot = await this.UpdateYnabSettingsYroot(userFolder);

            var knownBudgets = ynabSettingsYroot
                .Value<JArray>("relativeKnownBudgets")
                .Values<string>();

            foreach (var knownBudget in knownBudgets)
            {
                await this.UpdateBudget(knownBudget, userFolder);
            }
        }
        
        public async Task<StorageFolder> CreateUserFolderAsync()
        {
            return await this._rootFolder.CreateFolderAsync($"Dropbox-User-{this._auth.UserId}", CreationCollisionOption.OpenIfExists);
        }

        public Task WriteFileAsync(string file, string content)
        {
            return this.GetClient()
                .PutAsync($"https://content.dropboxapi.com/1/files_put/auto/{file}", new StringContent(content));
        }

        #region Private Methods

        private async Task<JObject> UpdateYnabSettingsYroot(StorageFolder userFolder)
        {
            var dropboxResult = await this.GetClient()
                .GetAsync("https://content.dropboxapi.com/1/files/auto/.ynabSettings.yroot");

            if (dropboxResult.StatusCode != HttpStatusCode.OK)
                return null;

            var content = await dropboxResult.Content.ReadAsByteArrayAsync();
            var localYnabSettingsYrootFile = await userFolder.CreateFileAsync(".ynabSettings.yroot", CreationCollisionOption.ReplaceExisting);

            using (var stream = await localYnabSettingsYrootFile.OpenStreamForWriteAsync())
            {
                await stream.WriteAsync(content, 0, content.Length);
            }

            return JObject.Parse(Encoding.UTF8.GetString(content));
        }

        private async Task UpdateBudget(string knownBudgetPath, StorageFolder userFolder)
        {
            var budgetFolder = await userFolder.CreateFolderStructureAsync(knownBudgetPath);
            var dropboxCursor = await this.ReadCursorForBudget(budgetFolder);

            var changes = await this.GetChangesAsync(knownBudgetPath, dropboxCursor);
            await this.ApplyChangesAsync(knownBudgetPath, budgetFolder, changes);

            await this.UpdateCursorForBudget(budgetFolder, changes.Cursor);
        }

        private async Task UpdateCursorForBudget(StorageFolder budgetFolder, string cursor)
        {
            var cursorFile = await budgetFolder.CreateFileAsync("DropboxDeltaCursor.txt", CreationCollisionOption.OpenIfExists);

            using (var stream = await cursorFile.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(cursor);
            }
        }

        private async Task<string> ReadCursorForBudget(StorageFolder budgetFolder)
        {
            var cursorFile = await budgetFolder.CreateFileAsync("DropboxDeltaCursor.txt", CreationCollisionOption.OpenIfExists);

            using (var stream = await cursorFile.OpenStreamForReadAsync())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task<Changes> GetChangesAsync(string budgetPath, string dropboxCursor)
        {
            var changes = new Dictionary<string, JObject>();
            bool hasMoreChanges = true;
            bool reset = false;

            while (hasMoreChanges)
            {
                var deltaResult = await this.GetClient()
                    .PostAsync($"https://api.dropboxapi.com/1/delta?path_prefix=/{budgetPath}&cursor={dropboxCursor}", new StringContent(string.Empty));

                var content = await deltaResult.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                hasMoreChanges = json.Value<bool>("has_more");
                dropboxCursor = json.Value<string>("cursor");

                if (reset == false)
                    reset = json.Value<bool>("reset");

                var currentChanges = json
                    .Value<JArray>("entries")
                    .Values<JArray>()
                    .ToDictionary(f => f.First.Value<string>(), f => f.Last.Value<JObject>());

                foreach (var change in currentChanges)
                {
                    changes.Add(change.Key, change.Value);
                }
            }

            return new Changes(dropboxCursor, changes, reset);
        }

        private async Task ApplyChangesAsync(string knownBudgetPath, StorageFolder budgetFolder, Changes changes)
        {
            if (changes.Reset)
            {
                await this.ResetBudgetFolderAsync(budgetFolder);
            }

            foreach (KeyValuePair<string, JObject> change in changes.Items)
            {
                bool isDirectory = change.Value.Value<bool>("is_dir");
                string relativePath = change.Value.Value<string>("path").Substring(knownBudgetPath.Length + 1); // +1 for trailing slash
                bool delete = change.Value == null;

                if (isDirectory)
                {
                    await this.ApplyDirectoryChangeAsync(budgetFolder, relativePath, delete);
                }
                else
                {
                    await this.ApplyFileChangeAsync(budgetFolder, relativePath, change.Key, delete);
                }
            }
        }

        private async Task ApplyDirectoryChangeAsync(StorageFolder budgetFolder, string relativePath, bool delete)
        {
            var folder = await budgetFolder.CreateFolderStructureAsync(relativePath);

            if (delete)
            {
                await folder.DeleteAsync();
            }
        }

        private async Task ApplyFileChangeAsync(StorageFolder budgetFolder, string relativePath, string filePath, bool delete)
        {
            var folders = Path.GetDirectoryName(relativePath);
            var fileName = Path.GetFileName(relativePath);

            var subFolder = await budgetFolder.CreateFolderStructureAsync(folders);

            var file = await subFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            if (delete)
            {
                await file.DeleteAsync();
            }
            else
            {
                var fileResponse = await this.GetClient()
                    .GetAsync($"https://content.dropboxapi.com/1/files/auto/{filePath}");

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    var responseStream = await fileResponse.Content.ReadAsStreamAsync();
                    await responseStream.CopyToAsync(stream);
                }
            }
        }

        private async Task ResetBudgetFolderAsync(StorageFolder budgetFolder)
        {
            foreach (var folder in await budgetFolder.GetFoldersAsync())
            {
                await folder.DeleteAsync();
            }
            foreach (var file in await budgetFolder.GetFilesAsync())
            {
                if (file.Name == "DropboxDeltaCursor.txt")
                    continue;

                await file.DeleteAsync();
            }
        }

        private HttpClient GetClient()
        {
            var handler = new AccessCodeMessageHandler(this._auth.AccessCode, new HttpClientHandler());
            return new HttpClient(handler);
        }
        #endregion

        #region Internal
        private class Changes
        {
            public Changes(string cursor, Dictionary<string, JObject> items, bool reset)
            {
                this.Cursor = cursor;
                this.Items = items;
                this.Reset = reset;
            }

            public string Cursor { get; }
            public Dictionary<string, JObject> Items { get; }
            public bool Reset { get; }
        }
        #endregion
    }
}