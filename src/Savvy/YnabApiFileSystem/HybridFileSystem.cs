using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Savvy.Extensions;
using Savvy.Services.DropboxAuthentication;
using Savvy.Services.SessionState;
using YnabApi.Files;

namespace Savvy.YnabApiFileSystem
{
    public class HybridFileSystem : IFileSystem
    {
        public HybridFileSystem(StorageFolder rootFolder, ISessionStateService sessionStateService)
        {
            this.Synchronization = new DropboxSynchronization(rootFolder, sessionStateService);
            this._queuedFilesToWrite = new List<Tuple<string, string>>();
        }

        public DropboxSynchronization Synchronization { get; }

        public async Task<string> ReadFileAsync(string filePath)
        {
            try
            {
                using (var archive = await this.Synchronization.GetUserArchiveAsync(ZipArchiveMode.Read))
                {
                    var entry = archive.GetOrCreateEntry(filePath);

                    using (var stream = entry.Open())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<IList<string>> GetFilesAsync(string directory)
        {
            try
            {
                using (var archive = await this.Synchronization.GetUserArchiveAsync(ZipArchiveMode.Read))
                {
                    return archive.Entries
                        .Where(f => f.FullName.NormalizePath().StartsWith(directory.NormalizePath(), StringComparison.OrdinalIgnoreCase))
                        .Select(f => f.Name)
                        .Select(f => Path.Combine(directory, f))
                        .ToList();
                }
            }
            catch
            {
                return null;
            }
        }

        private IList<Tuple<string, string>> _queuedFilesToWrite;

        public Task WriteFileAsync(string filePath, string content)
        {
            this._queuedFilesToWrite.Add(Tuple.Create(filePath, content));

            return Task.CompletedTask;
        }

        public Task CreateDirectoryAsync(string directory)
        {
            return Task.CompletedTask;
        }

        public async Task FlushWritesAsync()
        {
            //No using here, because if an error happens, we don't overwrite the zip file
            var archive = await this.Synchronization.GetUserArchiveAsync(ZipArchiveMode.Update);

            foreach(var fileToWrite in this._queuedFilesToWrite)
            { 
                var entry = archive.GetOrCreateNewEntry(fileToWrite.Item1);

                using (var stream = entry.Open())
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    var writeTask = writer.WriteAsync(fileToWrite.Item2);
                    var uploadTask = this.Synchronization.WriteFileAsync(fileToWrite.Item1, fileToWrite.Item2);

                    await Task.WhenAll(writeTask, uploadTask);
                }
            }

            this._queuedFilesToWrite = new List<Tuple<string, string>>();
            archive.Dispose();
        }
    }
}