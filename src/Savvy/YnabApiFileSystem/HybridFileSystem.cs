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
using YnabApi.Files;

namespace Savvy.YnabApiFileSystem
{
    public class HybridFileSystem : IFileSystem
    {
        public HybridFileSystem(StorageFolder rootFolder, DropboxAuth auth)
        {
            this.Synchronization = new DropboxSynchronization(rootFolder, auth);
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

        public async Task WriteFileAsync(string filePath, string content)
        {
            using (var archive = await this.Synchronization.GetUserArchiveAsync(ZipArchiveMode.Update))
            {
                var entry = archive.GetOrCreateNewEntry(filePath);

                using (var stream = entry.Open())
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    await writer.WriteAsync(content);
                }
            }

            await this.Synchronization.WriteFileAsync(filePath, content);
        }

        public Task CreateDirectoryAsync(string directory)
        {
            return Task.CompletedTask;
        }

        public Task FlushWritesAsync()
        {
            return Task.CompletedTask;
        }
    }
}