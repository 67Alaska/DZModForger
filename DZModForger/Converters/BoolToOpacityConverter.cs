using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts boolean values to opacity (0.5 or 1.0)
    /// </summary>
    public class BoolToOpacityConverter : IValueConverter
    {
        /// <summary>
        /// Opacity when false (disabled state)
        /// </summary>
        public double FalseOpacity { get; set; } = 0.5;

        /// <summary>
        /// Opacity when true (enabled state)
        /// </summary>
        public double TrueOpacity { get; set; } = 1.0;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueOpacity : FalseOpacity;
            }

            return TrueOpacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double opacity)
            {
                return opacity >= (TrueOpacity + FalseOpacity) / 2;
            }

            return false;
        }
    }
}
