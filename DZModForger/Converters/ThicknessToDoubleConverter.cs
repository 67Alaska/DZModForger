using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Extracts a single value from Thickness (Left, Top, Right, or Bottom)
    /// </summary>
    public class ThicknessToDoubleConverter : IValueConverter
    {
        /// <summary>
        /// Which side to extract: "Left", "Top", "Right", "Bottom"
        /// </summary>
        public string Side { get; set; } = "Left";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Thickness thickness)
            {
                return Side.ToLower() switch
                {
                    "left" => thickness.Left,
                    "top" => thickness.Top,
                    "right" => thickness.Right,
                    "bottom" => thickness.Bottom,
                    _ => thickness.Left
                };
            }

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // This converter is primarily one-way
            return new Thickness(0);
        }
    }
}
