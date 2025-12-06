using System;

namespace DZModForger.Models
{
    /// <summary>
    /// Represents a single vertex with position, normal, texture coordinate, and color
    /// Total size: 48 bytes
    /// </summary>
    public class VertexData
    {
        /// <summary>
        /// Position (X, Y, Z) - 12 bytes
        /// </summary>
        public float[] Position { get; set; } = new float { 0f, 0f, 0f };

        /// <summary>
        /// Normal (X, Y, Z) - 12 bytes
        /// </summary>
        public float[] Normal { get; set; } = new float { 0f, 1f, 0f };

        /// <summary>
        /// Texture coordinate (U, V) - 8 bytes
        /// </summary>
        public float[] TexCoord { get; set; } = new float { 0f, 0f };

        /// <summary>
        /// Color (R, G, B, A) - 16 bytes
        /// </summary>
        public float[] Color { get; set; } = new float { 1f, 1f, 1f, 1f };

        public VertexData()
        {
        }

        public VertexData(float posX, float posY, float posZ)
        {
            Position = new[] { posX, posY, posZ };
        }

        public VertexData(float[] position, float[] normal, float[] texCoord, float[] color)
        {
            Position = (float[])position.Clone();
            Normal = (float[])normal.Clone();
            TexCoord = (float[])texCoord.Clone();
            Color = (float[])color.Clone();
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
        /// Set normal
        /// </summary>
        public void SetNormal(float x, float y, float z)
        {
            Normal = x;
            Normal = y;
            Normal = z;
        }

        /// <summary>
        /// Set texture coordinate
        /// </summary>
        public void SetTexCoord(float u, float v)
        {
            TexCoord = u;
            TexCoord = v;
        }

        /// <summary>
        /// Set color
        /// </summary>
        public void SetColor(float r, float g, float b, float a = 1f)
        {
            Color = r;
            Color = g;
            Color = b;
            Color = a;
        }

        /// <summary>
        /// Clone this vertex
        /// </summary>
        public VertexData Clone()
        {
            return new VertexData(
                (float[])Position.Clone(),
                (float[])Normal.Clone(),
                (float[])TexCoord.Clone(),
                (float[])Color.Clone()
            );
        }

        public override string ToString()
        {
            return $"V({Position:F2},{Position:F2},{Position:F2}) " +
                   $"N({Normal:F2},{Normal:F2},{Normal:F2}) " +
                   $"UV({TexCoord:F2},{TexCoord:F2}) " +
                   $"C({Color:F2},{Color:F2},{Color:F2},{Color:F2})";
        }
    }
}
