using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts byte values to megabyte strings with proper formatting
    /// </summary>
    public class BytesToMegabytesConverter : IValueConverter
    {
        /// <summary>
        /// Number of decimal places
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is long bytes)
            {
                double megabytes = bytes / (1024.0 * 1024.0);
                return megabytes.ToString($"F{DecimalPlaces}") + " MB";
            }

            if (value is ulong ubytes)
            {
                double megabytes = ubytes / (1024.0 * 1024.0);
                return megabytes.ToString($"F{DecimalPlaces}") + " MB";
            }

            return "0 MB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue)
            {
                string cleanValue = stringValue.Replace("MB", "").Trim();

                if (double.TryParse(cleanValue, out double megabytes))
                {
                    long bytes = (long)(megabytes * 1024.0 * 1024.0);
                    return bytes;
                }
            }

            return 0L;
        }
    }
}
