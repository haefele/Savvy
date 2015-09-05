using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Savvy.Extensions
{
    public static class StorageFolderExtensions
    {
        public static async Task<StorageFolder> CreateFolderStructureAsync(this StorageFolder currentFolder, string path)
        {
            foreach (var folder in path.Split('/', '\\').Where(f => string.IsNullOrWhiteSpace(f) == false))
            {
                currentFolder = await currentFolder.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
            }

            return currentFolder;
        }
    }
}