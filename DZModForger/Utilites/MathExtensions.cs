using System;

namespace DZModForger.Utilities
{
    /// <summary>
    /// Math utility extension methods
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Clamps a value between min and max
        /// </summary>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;

            if (value.CompareTo(max) > 0)
                return max;

            return value;
        }

        /// <summary>
        /// Linear interpolation between two values
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t.Clamp(0f, 1f);
        }

        /// <summary>
        /// Inverse linear interpolation
        /// </summary>
        public static float InverseLerp(float a, float b, float value)
        {
            if (a == b)
                return 0f;

            return ((value - a) / (b - a)).Clamp(0f, 1f);
        }

        /// <summary>
        /// Remaps value from one range to another
        /// </summary>
        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            float t = InverseLerp(inMin, inMax, value);
            return Lerp(outMin, outMax, t);
        }

        /// <summary>
        /// Checks if two floats are approximately equal
        /// </summary>
        public static bool Approximately(float a, float b, float tolerance = 0.0001f)
        {
            return Math.Abs(a - b) < tolerance;
        }

        /// <summary>
        /// Rounds to nearest multiple
        /// </summary>
        public static float RoundToNearest(float value, float multiple)
        {
            if (multiple == 0)
                return value;

            return (float)Math.Round(value / multiple) * multiple;
        }

        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        public static float ToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180f;
        }

        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        public static float ToDegrees(float radians)
        {
            return radians * 180f / (float)Math.PI;
        }

        /// <summary>
        /// Returns absolute value
        /// </summary>
        public static float Abs(float value)
        {
            return Math.Abs(value);
        }

        /// <summary>
        /// Returns minimum of two values
        /// </summary>
        public static T Min<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) < 0 ? a : b;
        }

        /// <summary>
        /// Returns maximum of two values
        /// </summary>
        public static T Max<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) > 0 ? a : b;
        }

        /// <summary>
        /// Power function
        /// </summary>
        public static float Pow(float value, float power)
        {
            return (float)Math.Pow(value, power);
        }

        /// <summary>
        /// Square root function
        /// </summary>
        public static float Sqrt(float value)
        {
            return (float)Math.Sqrt(value);
        }
    }
}
