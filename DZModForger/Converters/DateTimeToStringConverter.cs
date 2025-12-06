using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts DateTime to formatted string
    /// </summary>
    public class DateTimeToStringConverter : IValueConverter
    {
        /// <summary>
        /// Date format string (e.g., "yyyy-MM-dd HH:mm:ss")
        /// </summary>
        public string Format { get; set; } = "yyyy-MM-dd HH:mm:ss";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Check for parameter override
            if (parameter is string paramFormat && !string.IsNullOrEmpty(paramFormat))
            {
                Format = paramFormat;
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToString(Format);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue && DateTime.TryParse(stringValue, out DateTime result))
            {
                return result;
            }

            return DateTime.Now;
        }
    }
}
