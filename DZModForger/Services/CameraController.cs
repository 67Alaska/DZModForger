using System;
using System.Diagnostics;

namespace DZModForger.Services
{
    /// <summary>
    /// Manages 3D camera controls for the viewport
    /// Implements Blender-style orbital, pan, and zoom controls
    /// </summary>
    public class CameraController
    {
        // Camera state
        private float _distance;      // Distance from target
        private float _yaw;           // Horizontal rotation (degrees)
        private float _pitch;         // Vertical rotation (degrees)
        private float _panX;          // Pan offset X
        private float _panY;          // Pan offset Y
        private float _panZ;          // Pan offset Z

        // Camera limits
        private const float MinDistance = 0.1f;
        private const float MaxDistance = 1000.0f;
        private const float MinPitch = -89.0f;
        private const float MaxPitch = 89.0f;
        private const float ZoomSensitivity = 1.1f;
        private const float RotationSensitivity = 0.5f;
        private const float PanSensitivity = 0.01f;

        // Target point (what the camera looks at)
        private Vector3f _targetPoint;

        // Camera matrices
        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;

        // Events
        public event EventHandler CameraChanged;

        public CameraController()
        {
            Debug.WriteLine("[CAMERACONTROLLER] Initializing");

            // Initialize camera state
            _distance = 5.0f;
            _yaw = 0.0f;
            _pitch = 30.0f;
            _panX = 0.0f;
            _panY = 0.0f;
            _panZ = 0.0f;

            _targetPoint = new Vector3f(0, 0, 0);

            // Initialize matrices
            _viewMatrix = Matrix4x4.Identity;
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                (float)Math.PI / 4.0f,  // FOV 45 degrees
                1920.0f / 1080.0f,       // Aspect ratio
                0.1f,                    // Near plane
                1000.0f                  // Far plane
            );

            UpdateViewMatrix();

            Debug.WriteLine("[CAMERACONTROLLER] Initialization complete");
        }

        // ==================== PROPERTIES ====================

        public float Distance
        {
            get => _distance;
            set
            {
                var newDistance = Math.Max(MinDistance, Math.Min(MaxDistance, value));
                if (newDistance != _distance)
                {
                    _distance = newDistance;
                    UpdateViewMatrix();
                    OnCameraChanged();
                }
            }
        }

        public float Yaw
        {
            get => _yaw;
            set
            {
                _yaw = value % 360.0f;
                UpdateViewMatrix();
                OnCameraChanged();
            }
        }

        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = Math.Max(MinPitch, Math.Min(MaxPitch, value));
                UpdateViewMatrix();
                OnCameraChanged();
            }
        }

        public float PanX
        {
            get => _panX;
            set
            {
                _panX = value;
                UpdateViewMatrix();
                OnCameraChanged();
            }
        }

        public float PanY
        {
            get => _panY;
            set
            {
                _panY = value;
                UpdateViewMatrix();
                OnCameraChanged();
            }
        }

        public float PanZ
        {
            get => _panZ;
            set
            {
                _panZ = value;
                UpdateViewMatrix();
                OnCameraChanged();
            }
        }

        public Vector3f TargetPoint
        {
            get => _targetPoint;
            set
            {
                _targetPoint = value;
                UpdateViewMatrix();
                OnCameraChanged();
            }
        }

        public Vector3f CameraPosition
        {
            get
            {
                // Calculate camera position from spherical coordinates
                float yawRad = DegreesToRadians(_yaw);
                float pitchRad = DegreesToRadians(_pitch);

                float x = _targetPoint.X + _distance * (float)Math.Cos(yawRad) * (float)Math.Cos(pitchRad);
                float y = _targetPoint.Y + _distance * (float)Math.Sin(pitchRad);
                float z = _targetPoint.Z + _distance * (float)Math.Sin(yawRad) * (float)Math.Cos(pitchRad);

                return new Vector3f(x + _panX, y + _panY, z + _panZ);
            }
        }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        // ==================== CAMERA CONTROLS ====================

        /// <summary>
        /// Orbits the camera around the target point
        /// </summary>
        public void Orbit(float deltaYaw, float deltaPitch)
        {
            try
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Orbit: Yaw={deltaYaw}, Pitch={deltaPitch}");

                Yaw += deltaYaw * RotationSensitivity;
                Pitch += deltaPitch * RotationSensitivity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in Orbit: {ex.Message}");
            }
        }

        /// <summary>
        /// Pans the camera in world space
        /// </summary>
        public void Pan(float deltaX, float deltaY)
        {
            try
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Pan: X={deltaX}, Y={deltaY}");

                // Pan relative to camera orientation
                float yawRad = DegreesToRadians(_yaw);

                float panDeltaX = (float)(deltaX * Math.Cos(yawRad) - deltaY * Math.Sin(yawRad)) * PanSensitivity;
                float panDeltaY = deltaY * PanSensitivity;
                float panDeltaZ = (float)(deltaX * Math.Sin(yawRad) + deltaY * Math.Cos(yawRad)) * PanSensitivity;

                PanX += panDeltaX;
                PanY += panDeltaY;
                PanZ += panDeltaZ;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in Pan: {ex.Message}");
            }
        }

        /// <summary>
        /// Zooms the camera in/out
        /// </summary>
        public void Zoom(float factor)
        {
            try
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Zoom: Factor={factor}");

                Distance *= factor;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in Zoom: {ex.Message}");
            }
        }

        /// <summary>
        /// Focuses the camera on a point
        /// </summary>
        public void FocusOn(Vector3f point, float distance = 5.0f)
        {
            try
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Focusing on: {point}");

                TargetPoint = point;
                Distance = distance;
                _panX = 0;
                _panY = 0;
                _panZ = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in FocusOn: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets camera to default position
        /// </summary>
        public void ResetView()
        {
            try
            {
                Debug.WriteLine("[CAMERACONTROLLER] Resetting view");

                _distance = 5.0f;
                _yaw = 0.0f;
                _pitch = 30.0f;
                _panX = 0.0f;
                _panY = 0.0f;
                _panZ = 0.0f;
                _targetPoint = new Vector3f(0, 0, 0);

                UpdateViewMatrix();
                OnCameraChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in ResetView: {ex.Message}");
            }
        }

        // ==================== VIEW PRESET ====================

        /// <summary>
        /// Sets camera to front view
        /// </summary>
        public void ViewFront()
        {
            try
            {
                _yaw = 0.0f;
                _pitch = 0.0f;
                UpdateViewMatrix();
                OnCameraChanged();
                Debug.WriteLine("[CAMERACONTROLLER] Front view");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in ViewFront: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets camera to top view
        /// </summary>
        public void ViewTop()
        {
            try
            {
                _yaw = 0.0f;
                _pitch = 90.0f;
                UpdateViewMatrix();
                OnCameraChanged();
                Debug.WriteLine("[CAMERACONTROLLER] Top view");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in ViewTop: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets camera to side view
        /// </summary>
        public void ViewSide()
        {
            try
            {
                _yaw = 90.0f;
                _pitch = 0.0f;
                UpdateViewMatrix();
                OnCameraChanged();
                Debug.WriteLine("[CAMERACONTROLLER] Side view");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in ViewSide: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets camera to isometric view
        /// </summary>
        public void ViewIsometric()
        {
            try
            {
                _yaw = 45.0f;
                _pitch = 45.0f;
                _distance = 7.071f; // sqrt(50)
                UpdateViewMatrix();
                OnCameraChanged();
                Debug.WriteLine("[CAMERACONTROLLER] Isometric view");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in ViewIsometric: {ex.Message}");
            }
        }

        // ==================== MATRIX CALCULATIONS ====================

        /// <summary>
        /// Updates the view matrix based on camera state
        /// </summary>
        private void UpdateViewMatrix()
        {
            try
            {
                var cameraPos = CameraPosition;
                var targetPos = _targetPoint;

                // Calculate view matrix using look-at
                _viewMatrix = Matrix4x4.CreateLookAt(cameraPos, targetPos, new Vector3f(0, 1, 0));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in UpdateViewMatrix: {ex.Message}");
            }
        }

        // ==================== UTILITY FUNCTIONS ====================

        private float DegreesToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180.0f;
        }

        private float RadiansToDegrees(float radians)
        {
            return radians * 180.0f / (float)Math.PI;
        }

        private void OnCameraChanged()
        {
            try
            {
                CameraChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CAMERACONTROLLER] Exception in OnCameraChanged: {ex.Message}");
            }
        }

        // ==================== SERIALIZATION ====================

        /// <summary>
        /// Gets camera state as a string for logging/debugging
        /// </summary>
        public override string ToString()
        {
            return $"Camera [Pos: {CameraPosition}, Yaw: {_yaw:F2}°, Pitch: {_pitch:F2}°, Dist: {_distance:F2}]";
        }
    }

    // ==================== MATH STRUCTURES ====================

    /// <summary>
    /// 3D Vector for camera calculations
    /// </summary>
    public struct Vector3f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3f(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3f operator +(Vector3f a, Vector3f b)
            => new Vector3f(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3f operator -(Vector3f a, Vector3f b)
            => new Vector3f(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3f operator *(Vector3f a, float scalar)
            => new Vector3f(a.X * scalar, a.Y * scalar, a.Z * scalar);

        public float Dot(Vector3f other)
            => X * other.X + Y * other.Y + Z * other.Z;

        public Vector3f Cross(Vector3f other)
            => new Vector3f(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X
            );

        public float Length()
            => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3f Normalized()
        {
            float len = Length();
            return len > 0 ? new Vector3f(X / len, Y / len, Z / len) : this;
        }

        public override string ToString()
            => $"({X:F2}, {Y:F2}, {Z:F2})";
    }

    /// <summary>
    /// 4x4 Matrix for camera transformations
    /// </summary>
    public struct Matrix4x4
    {
        public float M11, M12, M13, M14;
        public float M21, M22, M23, M24;
        public float M31, M32, M33, M34;
        public float M41, M42, M43, M44;

        public static Matrix4x4 Identity
        {
            get => new Matrix4x4
            {
                M11 = 1, M12 = 0, M13 = 0, M14 = 0,
                M21 = 0, M22 = 1, M23 = 0, M24 = 0,
                M31 = 0, M32 = 0, M33 = 1, M34 = 0,
                M41 = 0, M42 = 0, M43 = 0, M44 = 1
            };
        }

        public static Matrix4x4 CreateLookAt(Vector3f eye, Vector3f target, Vector3f up)
        {
            Vector3f zaxis = (eye - target).Normalized();
            Vector3f xaxis = up.Cross(zaxis).Normalized();
            Vector3f yaxis = zaxis.Cross(xaxis);

            var result = Identity;
            result.M11 = xaxis.X;
            result.M12 = yaxis.X;
            result.M13 = zaxis.X;
            result.M21 = xaxis.Y;
            result.M22 = yaxis.Y;
            result.M23 = zaxis.Y;
            result.M31 = xaxis.Z;
            result.M32 = yaxis.Z;
            result.M33 = zaxis.Z;
            result.M41 = -xaxis.Dot(eye);
            result.M42 = -yaxis.Dot(eye);
            result.M43 = -zaxis.Dot(eye);
            result.M44 = 1;

            return result;
        }

        public static Matrix4x4 CreatePerspectiveFieldOfView(float fov, float aspectRatio, float nearPlane, float farPlane)
        {
            float height = 1.0f / (float)Math.Tan(fov / 2.0f);
            float width = height / aspectRatio;
            float range = farPlane / (nearPlane - farPlane);

            var result = new Matrix4x4();
            result.M11 = width;
            result.M22 = height;
            result.M33 = range;
            result.M34 = -1;
            result.M43 = nearPlane * range;

            return result;
        }

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            var result = new Matrix4x4();

            result.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41;
            result.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42;
            result.M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43;
            result.M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44;

            result.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41;
            result.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42;
            result.M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43;
            result.M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44;

            result.M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41;
            result.M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42;
            result.M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43;
            result.M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44;

            result.M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41;
            result.M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42;
            result.M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43;
            result.M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44;

            return result;
        }
    }
}
