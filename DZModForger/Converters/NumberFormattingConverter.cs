using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts numeric values to formatted strings
    /// </summary>
    public class NumberFormattingConverter : IValueConverter
    {
        /// <summary>
        /// Number format string (e.g., "F2" for 2 decimal places)
        /// </summary>
        public string Format { get; set; } = "F2";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Check for parameter override
            if (parameter is string paramFormat && !string.IsNullOrEmpty(paramFormat))
            {
                Format = paramFormat;
            }

            if (value is float floatValue)
            {
                return floatValue.ToString(Format);
            }

            if (value is double doubleValue)
            {
                return doubleValue.ToString(Format);
            }

            if (value is int intValue)
            {
                return intValue.ToString(Format);
            }

            if (value is uint uintValue)
            {
                return uintValue.ToString(Format);
            }

            return value?.ToString() ?? "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue)
            {
                if (targetType == typeof(float) && float.TryParse(stringValue, out float floatResult))
                    return floatResult;

                if (targetType == typeof(double) && double.TryParse(stringValue, out double doubleResult))
                    return doubleResult;

                if (targetType == typeof(int) && int.TryParse(stringValue, out int intResult))
                    return intResult;

                if (targetType == typeof(uint) && uint.TryParse(stringValue, out uint uintResult))
                    return uintResult;
            }

            return 0;
        }
    }
}
