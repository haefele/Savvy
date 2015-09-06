using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
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
            //No using here, because if an error happens, we don't overwrite the zip file
            var userArchive = await this.GetUserArchiveAsync(ZipArchiveMode.Update);

            var ynabSettingsYroot = await this.UpdateYnabSettingsYroot(userArchive);

            var knownBudgets = ynabSettingsYroot
                .Value<JArray>("relativeKnownBudgets")
                .Values<string>();

            foreach (var knownBudget in knownBudgets)
            {
                await this.UpdateBudget(knownBudget, userArchive);
            }

            userArchive.Dispose();
        }

        public async Task<ZipArchive> GetUserArchiveAsync(ZipArchiveMode mode)
        {
            var zipFile = await this._rootFolder.CreateFileAsync($"{this._auth.UserId}.zip", CreationCollisionOption.OpenIfExists);
            IRandomAccessStream stream = await zipFile.OpenAsync(mode == ZipArchiveMode.Read ? FileAccessMode.Read : FileAccessMode.ReadWrite, StorageOpenOptions.None);

            return new ZipArchive(stream.AsStream(), mode, false, Encoding.UTF8);
        }
        
        public Task WriteFileAsync(string file, string content)
        {
            return this.GetClient()
                .PutAsync($"https://content.dropboxapi.com/1/files_put/auto/{file}", new StringContent(content));
        }

        #region Private Methods

        private async Task<JObject> UpdateYnabSettingsYroot(ZipArchive userArchive)
        {
            var dropboxResult = await this.GetClient()
                .GetAsync("https://content.dropboxapi.com/1/files/auto/.ynabSettings.yroot");

            if (dropboxResult.StatusCode != HttpStatusCode.OK)
                return null;

            var content = await dropboxResult.Content.ReadAsByteArrayAsync();

            var entry = userArchive.GetOrCreateEntry(".ynabSettings.yroot");
            using (var stream = entry.Open())
            {
                await stream.WriteAsync(content, 0, content.Length);
            }

            return JObject.Parse(Encoding.UTF8.GetString(content));
        }

        private async Task UpdateBudget(string budgetPath, ZipArchive userArchive)
        {
            var dropboxCursor = await this.ReadCursorForBudget(userArchive, budgetPath);

            var changes = await this.GetChangesAsync(budgetPath, dropboxCursor);
            await this.ApplyChangesAsync(userArchive, budgetPath, changes);

            await this.UpdateCursorForBudget(userArchive, budgetPath, changes.Cursor);
        }

        private async Task<string> ReadCursorForBudget(ZipArchive userArchive, string budgetPath)
        {
            var cursorFilePath = Path.Combine(budgetPath, "DropboxDeltaCursor.txt");
            
            using (var stream = userArchive.GetOrCreateEntry(cursorFilePath).Open())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
        private async Task UpdateCursorForBudget(ZipArchive userArchive, string budgetPath, string cursor)
        {
            var cursorFilePath = Path.Combine(budgetPath, "DropboxDeltaCursor.txt");

            using (var stream = userArchive.GetOrCreateEntry(cursorFilePath).Open())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(cursor);
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

        private async Task ApplyChangesAsync(ZipArchive userArchive, string budgetPath, Changes changes)
        {
            if (changes.Reset)
            {
                this.ResetBudgetFolder(userArchive, budgetPath);
            }

            foreach (KeyValuePair<string, JObject> change in changes.Items)
            {
                bool isDirectory = change.Value.Value<bool>("is_dir");
                string path = change.Value.Value<string>("path");
                bool delete = change.Value == null;

                if (isDirectory)
                {
                    this.ApplyDirectoryChange(userArchive, path, delete);
                }
                else
                {
                    await this.ApplyFileChangeAsync(userArchive, path, delete);
                }
            }
        }

        private void ResetBudgetFolder(ZipArchive userArchive, string budgetPath)
        {
            var entriesToDelete = userArchive.Entries
                .Where(f => f.FullName.StartsWith(budgetPath))
                .Where(f => f.Name != "DropboxDeltaCursor.txt")
                .ToList();

            foreach (var entry in entriesToDelete)
            {
                entry.Delete();
            }
        }

        private void ApplyDirectoryChange(ZipArchive userArchive, string path, bool delete)
        {
            if (delete)
            {
                var entriesToDelete = userArchive.Entries.Where(f => f.FullName.StartsWith(path));
                foreach (var entry in entriesToDelete)
                {
                    entry.Delete();
                }
            }
        }

        private async Task ApplyFileChangeAsync(ZipArchive userArchive, string path, bool delete)
        {
            path = path.TrimStart('/', '\\');

            var entry = userArchive.GetOrCreateEntry(path);

            if (delete)
            {
                entry.Delete();
            }
            else
            {
                var fileResponse = await this.GetClient()
                    .GetAsync($"https://content.dropboxapi.com/1/files/auto/{path}");

                using (var stream = entry.Open())
                {
                    var responseStream = await fileResponse.Content.ReadAsStreamAsync();
                    await responseStream.CopyToAsync(stream);
                }
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