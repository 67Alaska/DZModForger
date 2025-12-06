using System;
using System.IO;

namespace DZModForger.Utilities
{
    /// <summary>
    /// File and directory path helper
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Combines multiple path segments safely
        /// </summary>
        public static string CombinePaths(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            return Path.Combine(paths);
        }

        /// <summary>
        /// Normalizes path separators for current OS
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return path.Replace('\\', Path.DirectorySeparatorChar)
                      .Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Makes path relative to base path
        /// </summary>
        public static string MakeRelativePath(string basePath, string targetPath)
        {
            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(targetPath))
                return targetPath;

            try
            {
                var baseUri = new Uri(basePath);
                var targetUri = new Uri(targetPath);
                return baseUri.MakeRelativeUri(targetUri).ToString();
            }
            catch
            {
                return targetPath;
            }
        }

        /// <summary>
        /// Gets unique filename by appending number if file exists
        /// </summary>
        public static string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            string directory = Path.GetDirectoryName(filePath);
            string filename = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            int counter = 1;
            string newFilePath;

            do
            {
                newFilePath = Path.Combine(directory, $"{filename} ({counter}){extension}");
                counter++;
            } while (File.Exists(newFilePath));

            return newFilePath;
        }

        /// <summary>
        /// Checks if path is accessible/readable
        /// </summary>
        public static bool IsPathAccessible(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.GetAccessControl(path);
                    return true;
                }

                if (File.Exists(path))
                {
                    File.GetAccessControl(path);
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
        /// Gets application data folder
        /// </summary>
        public static string GetAppDataFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        /// Gets documents folder
        /// </summary>
        public static string GetDocumentsFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>
        /// Gets temporary folder
        /// </summary>
        public static string GetTempFolder()
        {
            return Path.GetTempPath();
        }

        /// <summary>
        /// Creates directory if it doesn't exist
        /// </summary>
        public static bool CreateDirectoryIfNotExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
