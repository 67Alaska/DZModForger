using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts Color struct to SolidColorBrush for UI binding
    /// </summary>
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }

            if (value is float[] floatArray && floatArray.Length >= 3)
            {
                // Assume normalized float array (0-1)
                byte r = (byte)(Math.Clamp(floatArray, 0f, 1f) * 255);
                byte g = (byte)(Math.Clamp(floatArray, 0f, 1f) * 255);
                byte b = (byte)(Math.Clamp(floatArray, 0f, 1f) * 255);
                byte a = floatArray.Length >= 4
                    ? (byte)(Math.Clamp(floatArray, 0f, 1f) * 255)
                    : (byte)255;

                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }

            return Colors.Transparent;
        }
    }
}
