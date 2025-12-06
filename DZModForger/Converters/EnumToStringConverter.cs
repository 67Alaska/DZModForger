using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts enum values to human-readable strings
    /// </summary>
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return string.Empty;

            Type enumType = value.GetType();

            if (!enumType.IsEnum)
                return value.ToString();

            // Convert enum name to readable format (PascalCase to Separate Words)
            string enumName = value.ToString();
            return InsertSpaces(enumName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue && targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, stringValue.Replace(" ", ""));
                }
                catch
                {
                    return Activator.CreateInstance(targetType);
                }
            }

            return value;
        }

        private string InsertSpaces(string text)
        {
            var result = new System.Text.StringBuilder();

            foreach (char c in text)
            {
                if (char.IsUpper(c) && result.Length > 0)
                {
                    result.Append(' ');
                }
                result.Append(c);
            }

            return result.ToString();
        }
    }
}
