using Microsoft.UI.Xaml.Data;
using System;

namespace DZModForger.Converters
{
    /// <summary>
    /// Converts bounding box (min/max points) to size display string
    /// </summary>
    public class BoundingBoxToSizeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is float[] boundingBox && boundingBox.Length == 6)
            {
                // Assuming format: [minX, minY, minZ, maxX, maxY, maxZ]
                float sizeX = Math.Abs(boundingBox - boundingBox);
                float sizeY = Math.Abs(boundingBox - boundingBox);
                float sizeZ = Math.Abs(boundingBox - boundingBox);

                return $"{sizeX:F2} x {sizeY:F2} x {sizeZ:F2}";
            }

            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // This converter is primarily one-way
            return null;
        }
    }
}
