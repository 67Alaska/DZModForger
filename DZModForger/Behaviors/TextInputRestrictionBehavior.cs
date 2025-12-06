using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace DZModForger.Behaviors
{
    /// <summary>
    /// Attached behavior that restricts text input to specific patterns
    /// </summary>
    public static class TextInputRestrictionBehavior
    {
        /// <summary>
        /// Input type: "NumericOnly", "AlphabeticOnly", "AlphaNumeric", "FloatNumber", "HexColor"
        /// </summary>
        public static string GetInputType(DependencyObject obj)
        {
            return (string)obj.GetValue(InputTypeProperty);
        }

        public static void SetInputType(DependencyObject obj, string value)
        {
            obj.SetValue(InputTypeProperty, value);
        }

        public static readonly DependencyProperty InputTypeProperty =
            DependencyProperty.RegisterAttached(
                "InputType",
                typeof(string),
                typeof(TextInputRestrictionBehavior),
                new PropertyMetadata("None", OnInputTypeChanged));

        private static void OnInputTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && e.NewValue is string inputType && inputType != "None")
            {
                textBox.PreviewKeyDown += (s, args) =>
                {
                    args.Handled = !IsValidInput(textBox.Text, inputType, args.Key);
                };

                textBox.TextChanged += (s, args) =>
                {
                    ValidateAndCleanText(textBox, inputType);
                };
            }
        }

        private static bool IsValidInput(string currentText, string inputType, Windows.System.VirtualKey key)
        {
            // Allow basic control keys
            if (IsControlKey(key))
                return true;

            char inputChar = (char)key;

            return inputType switch
            {
                "NumericOnly" => char.IsDigit(inputChar) || IsControlKey(key),
                "AlphabeticOnly" => char.IsLetter(inputChar) || IsControlKey(key),
                "AlphaNumeric" => char.IsLetterOrDigit(inputChar) || IsControlKey(key),
                "FloatNumber" => (char.IsDigit(inputChar) || inputChar == '.' || inputChar == '-') || IsControlKey(key),
                "HexColor" => (char.IsDigit(inputChar) || (inputChar >= 'A' && inputChar <= 'F') || (inputChar >= 'a' && inputChar <= 'f')) || IsControlKey(key),
                _ => true
            };
        }

        private static void ValidateAndCleanText(TextBox textBox, string inputType)
        {
            string original = textBox.Text;
            string cleaned = original;

            if (inputType == "FloatNumber")
            {
                // Allow only one decimal point and minus sign at start
                int decimalCount = cleaned.Length - cleaned.Replace(".", "").Length;
                if (decimalCount > 1)
                {
                    cleaned = original.Remove(original.LastIndexOf('.'), 1);
                }
            }
            else if (inputType == "HexColor")
            {
                // Remove invalid hex characters
                cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "[^0-9A-Fa-f]", "");
            }

            if (cleaned != original)
            {
                int cursorPos = textBox.SelectionStart;
                textBox.Text = cleaned;
                textBox.Select(Math.Min(cursorPos, cleaned.Length), 0);
            }
        }

        private static bool IsControlKey(Windows.System.VirtualKey key)
        {
            return key == Windows.System.VirtualKey.Back ||
                   key == Windows.System.VirtualKey.Delete ||
                   key == Windows.System.VirtualKey.Tab ||
                   key == Windows.System.VirtualKey.Enter ||
                   key == Windows.System.VirtualKey.Escape;
        }
    }
}
