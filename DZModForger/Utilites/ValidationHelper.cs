using System;
using System.Text.RegularExpressions;

namespace DZModForger.Utilities
{
    /// <summary>
    /// Validation helper for common patterns
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates email address
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates URL format
        /// </summary>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                       (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates IPv4 address
        /// </summary>
        public static bool IsValidIPv4(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            return Regex.IsMatch(ip, @"^(\d{1,3}\.){3}\d{1,3}$") &&
                   System.Net.IPAddress.TryParse(ip, out _);
        }

        /// <summary>
        /// Validates hex color code
        /// </summary>
        public static bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            return Regex.IsMatch(color, @"^#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$");
        }

        /// <summary>
        /// Validates JSON format
        /// </summary>
        public static bool IsValidJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { AllowTrailingCommas = true };
                System.Text.Json.JsonSerializer.Deserialize(json, options);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates phone number (basic format)
        /// </summary>
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            return Regex.IsMatch(phoneNumber, @"^[+]?[(]?[0-9]{3}[)]?[-\s.]?[0-9]{3}[-\s.]?[0-9]{4,6}$");
        }

        /// <summary>
        /// Validates zip code format (US)
        /// </summary>
        public static bool IsValidZipCode(string zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
                return false;

            return Regex.IsMatch(zipCode, @"^\d{5}(-\d{4})?$");
        }

        /// <summary>
        /// Checks if value is numeric
        /// </summary>
        public static bool IsNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return double.TryParse(value, out _);
        }

        /// <summary>
        /// Checks if value is integer
        /// </summary>
        public static bool IsInteger(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return int.TryParse(value, out _);
        }

        /// <summary>
        /// Checks if string contains only alphanumeric characters
        /// </summary>
        public static bool IsAlphaNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Regex.IsMatch(value, @"^[a-zA-Z0-9]+$");
        }
    }
}
