using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts boolean values to Visibility enum for UI display
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If true, inverts the logic (Collapsed when true, Visible when false)
        /// </summary>
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                bool isVisible = boolValue;

                if (Invert)
                    isVisible = !isVisible;

                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                bool boolValue = visibility == Visibility.Visible;

                if (Invert)
                    boolValue = !boolValue;

                return boolValue;
            }

            return false;
        }
    }
}
