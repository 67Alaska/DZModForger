using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts TimeSpan to human-readable format
    /// </summary>
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan.TotalHours >= 1)
                {
                    return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
                }
                else if (timeSpan.TotalMinutes >= 1)
                {
                    return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
                }
                else
                {
                    return $"{timeSpan.Seconds}s";
                }
            }

            return "0s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // This converter is primarily one-way
            return TimeSpan.Zero;
        }
    }
}
