using System;
using System.Numerics;

namespace DZModForger.Utilities
{
    /// <summary>
    /// Vector3 utility extension methods for 3D math
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Calculates distance between two vectors
        /// </summary>
        public static float Distance(this Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        /// <summary>
        /// Calculates squared distance (faster, no sqrt)
        /// </summary>
        public static float DistanceSquared(this Vector3 a, Vector3 b)
        {
            return Vector3.DistanceSquared(a, b);
        }

        /// <summary>
        /// Normalizes vector
        /// </summary>
        public static Vector3 Normalize(this Vector3 vector)
        {
            return Vector3.Normalize(vector);
        }

        /// <summary>
        /// Clamps vector components between min and max
        /// </summary>
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Math.Clamp(vector.X, min.X, max.X),
                Math.Clamp(vector.Y, min.Y, max.Y),
                Math.Clamp(vector.Z, min.Z, max.Z));
        }

        /// <summary>
        /// Linearly interpolates between two vectors
        /// </summary>
        public static Vector3 Lerp(this Vector3 a, Vector3 b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Vector3.Lerp(a, b, t);
        }

        /// <summary>
        /// Calculates dot product
        /// </summary>
        public static float Dot(this Vector3 a, Vector3 b)
        {
            return Vector3.Dot(a, b);
        }

        /// <summary>
        /// Calculates cross product
        /// </summary>
        public static Vector3 Cross(this Vector3 a, Vector3 b)
        {
            return Vector3.Cross(a, b);
        }

        /// <summary>
        /// Gets magnitude (length) of vector
        /// </summary>
        public static float Magnitude(this Vector3 vector)
        {
            return vector.Length();
        }

        /// <summary>
        /// Gets squared magnitude (faster, no sqrt)
        /// </summary>
        public static float MagnitudeSquared(this Vector3 vector)
        {
            return vector.LengthSquared();
        }

        /// <summary>
        /// Reflects vector around normal
        /// </summary>
        public static Vector3 Reflect(this Vector3 vector, Vector3 normal)
        {
            return Vector3.Reflect(vector, normal);
        }

        /// <summary>
        /// Rotates vector around axis by angle (radians)
        /// </summary>
        public static Vector3 RotateAroundAxis(this Vector3 vector, Vector3 axis, float angle)
        {
            var quaternion = System.Numerics.Quaternion.CreateFromAxisAngle(axis, angle);
            return Vector3.Transform(vector, quaternion);
        }

        /// <summary>
        /// Absolute value of vector components
        /// </summary>
        public static Vector3 Abs(this Vector3 vector)
        {
            return new Vector3(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
        }

        /// <summary>
        /// Checks if vector is approximately zero
        /// </summary>
        public static bool IsApproximatelyZero(this Vector3 vector, float tolerance = 0.0001f)
        {
            return Math.Abs(vector.X) < tolerance &&
                   Math.Abs(vector.Y) < tolerance &&
                   Math.Abs(vector.Z) < tolerance;
        }

        /// <summary>
        /// Scales vector by scalar
        /// </summary>
        public static Vector3 Scale(this Vector3 vector, float scale)
        {
            return vector * scale;
        }
    }
}
