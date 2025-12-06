using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace DZModForger.Utilities
{
    /// <summary>
    /// String utility extension methods
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if string is null or empty
        /// </summary>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Checks if string is null, empty, or whitespace
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Truncates string to maximum length with ellipsis
        /// </summary>
        public static string Truncate(this string value, int maxLength, string ellipsis = "...")
        {
            if (value?.Length <= maxLength)
                return value;

            return value.Substring(0, Math.Max(0, maxLength - ellipsis.Length)) + ellipsis;
        }

        /// <summary>
        /// Removes all whitespace from string
        /// </summary>
        public static string RemoveWhitespace(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(value, @"\s+", "");
        }

        /// <summary>
        /// Converts camelCase to Separate Words
        /// </summary>
        public static string CamelCaseToWords(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result = new StringBuilder();
            foreach (char c in value)
            {
                if (char.IsUpper(c) && result.Length > 0)
                    result.Append(' ');

                result.Append(c);
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts PascalCase to Separate Words
        /// </summary>
        public static string PascalCaseToWords(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result = new StringBuilder();
            foreach (char c in value)
            {
                if (char.IsUpper(c) && result.Length > 0)
                    result.Append(' ');

                result.Append(c);
            }

            return result.ToString();
        }

        /// <summary>
        /// Reverses a string
        /// </summary>
        public static string Reverse(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            char[] charArray = value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Repeats a string N times
        /// </summary>
        public static string Repeat(this string value, int count)
        {
            if (string.IsNullOrEmpty(value) || count <= 0)
                return string.Empty;

            var result = new StringBuilder(value.Length * count);
            for (int i = 0; i < count; i++)
                result.Append(value);

            return result.ToString();
        }

        /// <summary>
        /// Checks if string contains another string (case-insensitive)
        /// </summary>
        public static bool ContainsIgnoreCase(this string value, string searchString)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(searchString))
                return false;

            return value.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Removes specified characters from string
        /// </summary>
        public static string RemoveCharacters(this string value, string charactersToRemove)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result = new StringBuilder();
            foreach (char c in value)
            {
                if (!charactersToRemove.Contains(c))
                    result.Append(c);
            }

            return result.ToString();
        }

        /// <summary>
        /// Keeps only specified characters in string
        /// </summary>
        public static string KeepOnlyCharacters(this string value, string charactersToKeep)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result = new StringBuilder();
            foreach (char c in value)
            {
                if (charactersToKeep.Contains(c))
                    result.Append(c);
            }

            return result.ToString();
        }

        /// <summary>
        /// Counts occurrences of substring in string
        /// </summary>
        public static int CountOccurrences(this string value, string substring)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(substring))
                return 0;

            int count = 0;
            int index = 0;

            while ((index = value.IndexOf(substring, index)) != -1)
            {
                count++;
                index += substring.Length;
            }

            return count;
        }

        /// <summary>
        /// Validates email address format
        /// </summary>
        public static bool IsValidEmail(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(value);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates hex color code
        /// </summary>
        public static bool IsValidHexColor(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Regex.IsMatch(value, @"^#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$");
        }

        /// <summary>
        /// Left-pads string with specified character
        /// </summary>
        public static string PadLeft(this string value, int totalLength, char paddingChar = ' ')
        {
            return value.PadLeft(totalLength, paddingChar);
        }

        /// <summary>
        /// Right-pads string with specified character
        /// </summary>
        public static string PadRight(this string value, int totalLength, char paddingChar = ' ')
        {
            return value.PadRight(totalLength, paddingChar);
        }
    }
}
