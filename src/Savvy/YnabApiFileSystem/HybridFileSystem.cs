using System;
using System.Collections.Generic;
using System.IO;
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
            var root = await this.Synchronization.CreateUserFolderAsync();

            var folderStructure = Path.GetDirectoryName(filePath);
            var folder = await root.CreateFolderStructureAsync(folderStructure);
            var file = await folder.CreateFileAsync(Path.GetFileName(filePath), CreationCollisionOption.OpenIfExists);

            using (var stream = await file.OpenStreamForReadAsync())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task<IList<string>> GetFilesAsync(string directory)
        {
            var root = await this.Synchronization.CreateUserFolderAsync();

            var folder = await root.CreateFolderStructureAsync(directory);
            var allFiles = await folder.GetFilesAsync();

            return allFiles
                .Select(f => f.Name)
                .Select(f => Path.Combine(directory, f))
                .ToList();
        }

        public async Task WriteFileAsync(string filePath, string content)
        {
            var root = await this.Synchronization.CreateUserFolderAsync();

            var folderStructure = Path.GetDirectoryName(filePath);
            var folder = await root.CreateFolderStructureAsync(folderStructure);
            var file = await folder.CreateFileAsync(Path.GetFileName(filePath), CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(content);
            }

            await this.Synchronization.WriteFileAsync(filePath, content);
        }

        public async Task CreateDirectoryAsync(string directory)
        {
            var root = await this.Synchronization.CreateUserFolderAsync();
            await root.CreateFolderStructureAsync(directory);
        }
    }
}