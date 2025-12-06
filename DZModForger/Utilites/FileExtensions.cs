using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DZModForger.Utilities
{
    /// <summary>
    /// File utility extension methods
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Gets file size in human-readable format
        /// </summary>
        public static string GetFileSize(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                return "0 B";

            long bytes = fileInfo.Length;
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Gets file size from path
        /// </summary>
        public static string GetFileSizeFromPath(this string filePath)
        {
            if (!File.Exists(filePath))
                return "0 B";

            var fileInfo = new FileInfo(filePath);
            return fileInfo.GetFileSize();
        }

        /// <summary>
        /// Reads file as string
        /// </summary>
        public static async Task<string> ReadFileAsStringAsync(this string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Writes string to file
        /// </summary>
        public static async Task<bool> WriteFileAsync(this string filePath, string content)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                await File.WriteAllTextAsync(filePath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Appends text to file
        /// </summary>
        public static async Task<bool> AppendFileAsync(this string filePath, string content)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                await File.AppendAllTextAsync(filePath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates backup of file with timestamp
        /// </summary>
        public static bool CreateBackup(this string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                string backupPath = Path.Combine(
                    Path.GetDirectoryName(filePath),
                    Path.GetFileNameWithoutExtension(filePath) +
                    "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") +
                    Path.GetExtension(filePath));

                File.Copy(filePath, backupPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes file safely (checks existence first)
        /// </summary>
        public static bool DeleteSafely(this string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if file exists
        /// </summary>
        public static bool Exists(this string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Gets file extension without dot
        /// </summary>
        public static string GetExtensionWithoutDot(this string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.StartsWith(".") ? extension.Substring(1) : extension;
        }
    }
}
