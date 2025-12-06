using System;

namespace DZModForger.Models
{
    /// <summary>
    /// Represents material properties for rendering
    /// </summary>
    public class MaterialData
    {
        /// <summary>
        /// Unique identifier for this material
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name of the material
        /// </summary>
        public string Name { get; set; } = "Material";

        /// <summary>
        /// Diffuse color (R, G, B, A)
        /// </summary>
        public float[] DiffuseColor { get; set; } = new[] { 0.8f, 0.8f, 0.8f, 1.0f };

        /// <summary>
        /// Specular color (R, G, B, A)
        /// </summary>
        public float[] SpecularColor { get; set; } = new[] { 1.0f, 1.0f, 1.0f, 1.0f };

        /// <summary>
        /// Shininess exponent (0-128)
        /// </summary>
        public float Shininess { get; set; } = 32.0f;

        /// <summary>
        /// Transparency / Alpha (0-1)
        /// </summary>
        public float Transparency { get; set; } = 1.0f;

        /// <summary>
        /// Emissive color (R, G, B, A)
        /// </summary>
        public float[] EmissiveColor { get; set; } = new[] { 0.0f, 0.0f, 0.0f, 1.0f };

        /// <summary>
        /// Ambient color (R, G, B, A)
        /// </summary>
        public float[] AmbientColor { get; set; } = new[] { 0.2f, 0.2f, 0.2f, 1.0f };

        public MaterialData()
        {
        }

        public MaterialData(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Set diffuse color
        /// </summary>
        public void SetDiffuseColor(float r, float g, float b, float a = 1.0f)
        {
            DiffuseColor = new[] { r, g, b, a };
        }

        /// <summary>
        /// Set specular color
        /// </summary>
        public void SetSpecularColor(float r, float g, float b, float a = 1.0f)
        {
            SpecularColor = new[] { r, g, b, a };
        }

        /// <summary>
        /// Set emissive color
        /// </summary>
        public void SetEmissiveColor(float r, float g, float b, float a = 1.0f)
        {
            EmissiveColor = new[] { r, g, b, a };
        }

        /// <summary>
        /// Parse color from hex string (#RRGGBB or #RRGGBBAA)
        /// </summary>
        public static float[] ParseColorFromHex(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return new[] { 1.0f, 1.0f, 1.0f, 1.0f };

            if (hexColor.StartsWith("#"))
                hexColor = hexColor.Substring(1);

            try
            {
                float r = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255.0f;
                float g = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255.0f;
                float b = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255.0f;
                float a = hexColor.Length >= 8
                    ? int.Parse(hexColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255.0f
                    : 1.0f;

                return new[] { r, g, b, a };
            }
            catch
            {
                return new[] { 1.0f, 1.0f, 1.0f, 1.0f };
            }
        }

        /// <summary>
        /// Convert color to hex string
        /// </summary>
        public static string ColorToHex(float[] color)
        {
            if (color.Length < 3)
                return "#FFFFFF";

            int r = (int)(Math.Clamp(color, 0f, 1f) * 255);
            int g = (int)(Math.Clamp(color, 0f, 1f) * 255);
            int b = (int)(Math.Clamp(color, 0f, 1f) * 255);
            int a = color.Length >= 4 ? (int)(Math.Clamp(color, 0f, 1f) * 255) : 255;

            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        }

        /// <summary>
        /// Clone this material
        /// </summary>
        public MaterialData Clone()
        {
            return new MaterialData(Name + "_Clone")
            {
                DiffuseColor = (float[])DiffuseColor.Clone(),
                SpecularColor = (float[])SpecularColor.Clone(),
                EmissiveColor = (float[])EmissiveColor.Clone(),
                AmbientColor = (float[])AmbientColor.Clone(),
                Shininess = Shininess,
                Transparency = Transparency
            };
        }

        public override string ToString()
        {
            return $"Material: {Name} " +
                   $"Diffuse=RGB({DiffuseColor:F2},{DiffuseColor:F2},{DiffuseColor:F2}) " +
                   $"Specular=RGB({SpecularColor:F2},{SpecularColor:F2},{SpecularColor:F2}) " +
                   $"Shininess={Shininess:F2} " +
                   $"Alpha={Transparency:F2}";
        }
    }
}
