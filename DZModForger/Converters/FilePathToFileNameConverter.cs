using Microsoft.UI.Xaml.Data;
using System;
using System.IO;

namespace DZModForger.Converters
{
    /// <summary>
    /// Extracts filename from full file path
    /// </summary>
    public class FilePathToFileNameConverter : IValueConverter
    {
        /// <summary>
        /// If true, removes file extension
        /// </summary>
        public bool RemoveExtension { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string filePath && !string.IsNullOrEmpty(filePath))
            {
                string fileName = Path.GetFileName(filePath);

                if (RemoveExtension)
                {
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                }

                return fileName;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // This converter is primarily one-way
            return value;
        }
    }
}
