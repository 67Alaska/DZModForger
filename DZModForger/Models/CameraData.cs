using System;
using Vortice.Mathematics;

namespace DZModForger.Models
{
    /// <summary>
    /// Represents camera state and matrices
    /// </summary>
    public class CameraData
    {
        /// <summary>
        /// Distance from camera to target
        /// </summary>
        public float Distance { get; set; } = 10.0f;

        /// <summary>
        /// Rotation around X axis (pitch)
        /// </summary>
        public float RotationX { get; set; } = 0.5f;

        /// <summary>
        /// Rotation around Y axis (yaw)
        /// </summary>
        public float RotationY { get; set; } = 0.5f;

        /// <summary>
        /// Pan offset X
        /// </summary>
        public float PanX { get; set; } = 0.0f;

        /// <summary>
        /// Pan offset Y
        /// </summary>
        public float PanY { get; set; } = 0.0f;

        /// <summary>
        /// View matrix
        /// </summary>
        public Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;

        /// <summary>
        /// Field of view in degrees
        /// </summary>
        public float FieldOfView { get; set; } = 45.0f;

        /// <summary>
        /// Aspect ratio (width/height)
        /// </summary>
        public float AspectRatio { get; set; } = 16.0f / 9.0f;

        /// <summary>
        /// Near clip plane
        /// </summary>
        public float NearPlane { get; set; } = 0.1f;

        /// <summary>
        /// Far clip plane
        /// </summary>
        public float FarPlane { get; set; } = 1000.0f;

        public CameraData()
        {
        }

        /// <summary>
        /// Get projection matrix
        /// </summary>
        public Matrix4x4 GetProjectionMatrix()
        {
            float fovRadians = FieldOfView * (float)Math.PI / 180.0f;
            return Matrix4x4.CreatePerspectiveFieldOfView(
                fovRadians,
                AspectRatio,
                NearPlane,
                FarPlane
            );
        }

        /// <summary>
        /// Reset camera to default
        /// </summary>
        public void Reset()
        {
            Distance = 10.0f;
            RotationX = 0.5f;
            RotationY = 0.5f;
            PanX = 0.0f;
            PanY = 0.0f;
            ViewMatrix = Matrix4x4.Identity;
        }

        /// <summary>
        /// Clone this camera data
        /// </summary>
        public CameraData Clone()
        {
            return new CameraData
            {
                Distance = Distance,
                RotationX = RotationX,
                RotationY = RotationY,
                PanX = PanX,
                PanY = PanY,
                ViewMatrix = ViewMatrix,
                FieldOfView = FieldOfView,
                AspectRatio = AspectRatio,
                NearPlane = NearPlane,
                FarPlane = FarPlane
            };
        }

        public override string ToString()
        {
            return $"Camera: Distance={Distance:F2} " +
                   $"Rotation=({RotationX:F2},{RotationY:F2}) " +
                   $"Pan=({PanX:F2},{PanY:F2}) " +
                   $"FOV={FieldOfView:F2}° " +
                   $"Aspect={AspectRatio:F2} " +
                   $"Near={NearPlane:F2} " +
                   $"Far={FarPlane:F2}";
        }
    }
}
