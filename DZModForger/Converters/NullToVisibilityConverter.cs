using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts null values to Visibility
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If true, shows when null (inverts logic)
        /// </summary>
        public bool ShowWhenNull { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isNull = value == null;

            if (ShowWhenNull)
                isNull = !isNull;

            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // This converter is primarily one-way
            return null;
        }
    }
}
