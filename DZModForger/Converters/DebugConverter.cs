using Microsoft.UI.Xaml.Data;
using System;
using System.Diagnostics;

namespace DZModForger.Converters
{
    /// <summary>
    /// Debug converter that outputs value to debug console and passes through unchanged
    /// Useful for debugging binding issues
    /// </summary>
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Debug.WriteLine($"[DEBUG] Converter Input: {value?.GetType().Name} = {value}");
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Debug.WriteLine($"[DEBUG] ConvertBack Input: {value?.GetType().Name} = {value}");
            return value;
        }
    }
}
