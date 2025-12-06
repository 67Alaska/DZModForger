using Windows.UI;
using System;

namespace DZModForger.Utilities
{
    /// <summary>
    /// Color utility extension methods
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Converts Color to hex string
        /// </summary>
        public static string ToHex(this Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Converts Color to hex string with alpha
        /// </summary>
        public static string ToHexWithAlpha(this Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Converts hex string to Color
        /// </summary>
        public static Color FromHex(string hexColor)
        {
            hexColor = hexColor.Replace("#", "");

            if (hexColor.Length == 6)
            {
                byte r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                return Color.FromArgb(255, r, g, b);
            }
            else if (hexColor.Length == 8)
            {
                byte a = Convert.ToByte(hexColor.Substring(0, 2), 16);
                byte r = Convert.ToByte(hexColor.Substring(2, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(4, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(6, 2), 16);
                return Color.FromArgb(a, r, g, b);
            }

            return Colors.Transparent;
        }

        /// <summary>
        /// Converts RGB float array to Color
        /// </summary>
        public static Color FromRGBFloat(float[] rgbArray)
        {
            if (rgbArray == null || rgbArray.Length < 3)
                return Colors.Transparent;

            byte r = (byte)(Math.Clamp(rgbArray, 0f, 1f) * 255);
            byte g = (byte)(Math.Clamp(rgbArray, 0f, 1f) * 255);
            byte b = (byte)(Math.Clamp(rgbArray, 0f, 1f) * 255);
            byte a = rgbArray.Length > 3
                ? (byte)(Math.Clamp(rgbArray, 0f, 1f) * 255)
                : (byte)255;

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Converts Color to RGB float array
        /// </summary>
        public static float[] ToRGBFloat(this Color color)
        {
            return new[]
            {
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f
            };
        }

        /// <summary>
        /// Lightens a color by specified amount (0-1)
        /// </summary>
        public static Color Lighten(this Color color, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            byte r = (byte)(color.R + (255 - color.R) * amount);
            byte g = (byte)(color.G + (255 - color.G) * amount);
            byte b = (byte)(color.B + (255 - color.B) * amount);
            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Darkens a color by specified amount (0-1)
        /// </summary>
        public static Color Darken(this Color color, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            byte r = (byte)(color.R * (1 - amount));
            byte g = (byte)(color.G * (1 - amount));
            byte b = (byte)(color.B * (1 - amount));
            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Changes color alpha
        /// </summary>
        public static Color WithAlpha(this Color color, byte alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>
        /// Inverts color
        /// </summary>
        public static Color Invert(this Color color)
        {
            byte r = (byte)(255 - color.R);
            byte g = (byte)(255 - color.G);
            byte b = (byte)(255 - color.B);
            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Blends two colors
        /// </summary>
        public static Color Blend(this Color color1, Color color2, float blend)
        {
            blend = Math.Clamp(blend, 0f, 1f);
            byte r = (byte)(color1.R + (color2.R - color1.R) * blend);
            byte g = (byte)(color1.G + (color2.G - color1.G) * blend);
            byte b = (byte)(color1.B + (color2.B - color1.B) * blend);
            byte a = (byte)(color1.A + (color2.A - color1.A) * blend);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Calculates luminance of color
        /// </summary>
        public static float GetLuminance(this Color color)
        {
            return (0.299f * color.R + 0.587f * color.G + 0.114f * color.B) / 255f;
        }
    }
}
