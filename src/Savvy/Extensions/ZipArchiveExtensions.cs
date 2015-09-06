using System;
using System.IO.Compression;
using System.Linq;

namespace Savvy.Extensions
{
    public static class ZipArchiveExtensions
    {
        public static ZipArchiveEntry GetOrCreateEntry(this ZipArchive archive, string path)
        {
            var existingEntry = archive.Entries.FirstOrDefault(f => string.Equals(f.FullName.NormalizePath(), path.NormalizePath(), StringComparison.OrdinalIgnoreCase));

            if (existingEntry != null)
                return existingEntry;

            return archive.CreateEntry(path);
        }

        public static ZipArchiveEntry GetOrCreateNewEntry(this ZipArchive archive, string path)
        {
            var entry = archive.GetOrCreateEntry(path);
            entry.Delete();

            return archive.CreateEntry(path);
        }
    }
}