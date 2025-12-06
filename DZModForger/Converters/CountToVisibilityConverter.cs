using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts collection count to Visibility (shows when count > 0)
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If true, shows when count is 0 (inverts logic)
        /// </summary>
        public bool ShowWhenEmpty { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int count = 0;

            if (value is ICollection collection)
            {
                count = collection.Count;
            }
            else if (value is int intValue)
            {
                count = intValue;
            }

            bool hasItems = count > 0;

            if (ShowWhenEmpty)
                hasItems = !hasItems;

            return hasItems ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // This converter is primarily one-way
            return 0;
        }
    }
}
