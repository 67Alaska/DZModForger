using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts decimal values (0-1) to percentage strings (0-100%)
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        /// <summary>
        /// Number of decimal places in output
        /// </summary>
        public int DecimalPlaces { get; set; } = 1;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is float floatValue)
            {
                float percentage = floatValue * 100f;
                return percentage.ToString($"F{DecimalPlaces}") + "%";
            }

            if (value is double doubleValue)
            {
                double percentage = doubleValue * 100.0;
                return percentage.ToString($"F{DecimalPlaces}") + "%";
            }

            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue)
            {
                // Remove percentage sign and parse
                string cleanValue = stringValue.Replace("%", "").Trim();

                if (double.TryParse(cleanValue, out double percentageValue))
                {
                    double decimalValue = percentageValue / 100.0;

                    if (targetType == typeof(float))
                        return (float)decimalValue;

                    if (targetType == typeof(double))
                        return decimalValue;
                }
            }

            return 0.0;
        }
    }
}
