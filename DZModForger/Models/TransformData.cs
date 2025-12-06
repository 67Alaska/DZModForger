using System;
using Vortice.Mathematics;

namespace DZModForger.Models
{
    /// <summary>
    /// Represents transformation (position, rotation, scale)
    /// </summary>
    public class TransformData
    {
        /// <summary>
        /// Position in 3D space (X, Y, Z)
        /// </summary>
        public float[] Position { get; set; } = new[] { 0f, 0f, 0f };

        /// <summary>
        /// Rotation in Euler angles (X, Y, Z) in radians
        /// </summary>
        public float[] Rotation { get; set; } = new[] { 0f, 0f, 0f };

        /// <summary>
        /// Scale (X, Y, Z)
        /// </summary>
        public float[] Scale { get; set; } = new[] { 1f, 1f, 1f };

        public TransformData()
        {
        }

        public TransformData(float[] position, float[] rotation, float[] scale)
        {
            Position = (float[])position.Clone();
            Rotation = (float[])rotation.Clone();
            Scale = (float[])scale.Clone();
        }

        /// <summary>
        /// Set position
        /// </summary>
        public void SetPosition(float x, float y, float z)
        {
            Position = x;
            Position = y;
            Position = z;
        }

        /// <summary>
        /// Set rotation (in degrees)
        /// </summary>
        public void SetRotationDegrees(float x, float y, float z)
        {
            float toRad = (float)Math.PI / 180f;
            Rotation = x * toRad;
            Rotation = y * toRad;
            Rotation = z * toRad;
        }

        /// <summary>
        /// Set rotation (in radians)
        /// </summary>
        public void SetRotationRadians(float x, float y, float z)
        {
            Rotation = x;
            Rotation = y;
            Rotation = z;
        }

        /// <summary>
        /// Set scale
        /// </summary>
        public void SetScale(float x, float y, float z)
        {
            Scale = x;
            Scale = y;
            Scale = z;
        }

        /// <summary>
        /// Get rotation in degrees
        /// </summary>
        public float[] GetRotationDegrees()
        {
            float toDeg = 180f / (float)Math.PI;
            return new[]
            {
                Rotation * toDeg,
                Rotation * toDeg,
                Rotation * toDeg
            };
        }

        /// <summary>
        /// Calculate world matrix from transform
        /// </summary>
        public Matrix4x4 GetWorldMatrix()
        {
            // Create scale matrix
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(Scale, Scale, Scale);

            // Create rotation matrix from Euler angles
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(
                Rotation, // Yaw (Y rotation)
                Rotation, // Pitch (X rotation)
                Rotation  // Roll (Z rotation)
            );

            // Create translation matrix
            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(
                Position, Position, Position
            );

            // Combine: T * R * S
            return scaleMatrix * rotationMatrix * translationMatrix;
        }

        /// <summary>
        /// Clone this transform
        /// </summary>
        public TransformData Clone()
        {
            return new TransformData(
                (float[])Position.Clone(),
                (float[])Rotation.Clone(),
                (float[])Scale.Clone()
            );
        }

        public override string ToString()
        {
            return $"Pos({Position:F2},{Position:F2},{Position:F2}) " +
                   $"Rot({Rotation:F2},{Rotation:F2},{Rotation:F2}) " +
                   $"Scale({Scale:F2},{Scale:F2},{Scale:F2})";
        }
    }
}
