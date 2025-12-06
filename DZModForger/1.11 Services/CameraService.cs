using System;
using Vortice.Mathematics;

namespace DZModForger.Services
{
    /// <summary>
    /// Service for managing camera operations and matrices
    /// </summary>
    public class CameraService
    {
        private Vector3 _eye = new(10, 8, 10);
        private Vector3 _target = new(0, 0, 0);
        private Vector3 _up = new(0, 1, 0);

        private float _fieldOfView = 45.0f * (float)Math.PI / 180.0f;
        private float _aspectRatio = 16.0f / 9.0f;
        private float _nearPlane = 0.1f;
        private float _farPlane = 1000.0f;

        public event EventHandler<Matrix4x4EventArgs>? ViewMatrixChanged;
        public event EventHandler<Matrix4x4EventArgs>? ProjectionMatrixChanged;

        public CameraService()
        {
        }

        /// <summary>
        /// Get view matrix
        /// </summary>
        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(_eye, _target, _up);
        }

        /// <summary>
        /// Get projection matrix
        /// </summary>
        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(
                _fieldOfView,
                _aspectRatio,
                _nearPlane,
                _farPlane
            );
        }

        /// <summary>
        /// Set camera position
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            _eye = position;
            OnViewMatrixChanged(GetViewMatrix());
        }

        /// <summary>
        /// Set look target
        /// </summary>
        public void SetTarget(Vector3 target)
        {
            _target = target;
            OnViewMatrixChanged(GetViewMatrix());
        }

        /// <summary>
        /// Set camera up vector
        /// </summary>
        public void SetUpVector(Vector3 up)
        {
            _up = up;
            OnViewMatrixChanged(GetViewMatrix());
        }

        /// <summary>
        /// Set viewport aspect ratio
        /// </summary>
        public void SetAspectRatio(float width, float height)
        {
            if (height > 0)
            {
                _aspectRatio = width / height;
                OnProjectionMatrixChanged(GetProjectionMatrix());
            }
        }

        /// <summary>
        /// Set field of view (in degrees)
        /// </summary>
        public void SetFieldOfView(float fovDegrees)
        {
            _fieldOfView = fovDegrees * (float)Math.PI / 180.0f;
            OnProjectionMatrixChanged(GetProjectionMatrix());
        }

        /// <summary>
        /// Set near and far planes
        /// </summary>
        public void SetClipPlanes(float nearPlane, float farPlane)
        {
            _nearPlane = Math.Max(0.01f, nearPlane);
            _farPlane = Math.Max(_nearPlane + 1, farPlane);
            OnProjectionMatrixChanged(GetProjectionMatrix());
        }

        /// <summary>
        /// Get camera eye position
        /// </summary>
        public Vector3 GetPosition()
        {
            return _eye;
        }

        /// <summary>
        /// Get camera target
        /// </summary>
        public Vector3 GetTarget()
        {
            return _target;
        }

        /// <summary>
        /// Dolly camera (move forward/backward along view direction)
        /// </summary>
        public void Dolly(float distance)
        {
            Vector3 direction = Vector3.Normalize(_target - _eye);
            _eye += direction * distance;
            OnViewMatrixChanged(GetViewMatrix());
        }

        /// <summary>
        /// Track camera (move left/right and up/down in view plane)
        /// </summary>
        public void Track(float horizontalDelta, float verticalDelta)
        {
            Vector3 direction = Vector3.Normalize(_target - _eye);
            Vector3 right = Vector3.Normalize(Vector3.Cross(direction, _up));
            Vector3 actualUp = Vector3.Cross(right, direction);

            Vector3 movement = right * horizontalDelta + actualUp * verticalDelta;
            _eye += movement;
            _target += movement;

            OnViewMatrixChanged(GetViewMatrix());
        }

        /// <summary>
        /// Rotate camera around target
        /// </summary>
        public void RotateAroundTarget(float yawDegrees, float pitchDegrees)
        {
            // Convert to radians
            float yaw = yawDegrees * (float)Math.PI / 180.0f;
            float pitch = pitchDegrees * (float)Math.PI / 180.0f;

            // Get direction from target to eye
            Vector3 direction = _eye - _target;

            // Create rotation matrices
            Matrix4x4 yawMatrix = Matrix4x4.CreateFromAxisAngle(_up, yaw);

            // Right vector for pitch rotation
            Vector3 right = Vector3.Normalize(Vector3.Cross(direction, _up));
            Matrix4x4 pitchMatrix = Matrix4x4.CreateFromAxisAngle(right, pitch);

            // Apply rotations
            direction = Vector3.Transform(direction, yawMatrix);
            direction = Vector3.Transform(direction, pitchMatrix);

            _eye = _target + direction;
            OnViewMatrixChanged(GetViewMatrix());
        }

        /// <summary>
        /// Frame object (fit camera to show object)
        /// </summary>
        public void FrameObject(Vector3 center, float radius)
        {
            float distance = radius / (float)Math.Tan(_fieldOfView * 0.5f);
            Vector3 direction = Vector3.Normalize(_eye - _target);

            _target = center;
            _eye = center + direction * distance;

            OnViewMatrixChanged(GetViewMatrix());
        }

        protected virtual void OnViewMatrixChanged(Matrix4x4 matrix)
        {
            ViewMatrixChanged?.Invoke(this, new Matrix4x4EventArgs(matrix));
        }

        protected virtual void OnProjectionMatrixChanged(Matrix4x4 matrix)
        {
            ProjectionMatrixChanged?.Invoke(this, new Matrix4x4EventArgs(matrix));
        }
    }

    public class Matrix4x4EventArgs : EventArgs
    {
        public Matrix4x4 Matrix { get; }

        public Matrix4x4EventArgs(Matrix4x4 matrix)
        {
            Matrix = matrix;
        }
    }
}
