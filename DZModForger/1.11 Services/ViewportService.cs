using System;
using System.Diagnostics;

namespace DZModForger.Services
{
    /// <summary>
    /// Service for managing viewport rendering and camera
    /// </summary>
    public class ViewportService
    {
        private CameraData _camera = new();
        private ViewportState _viewportState = new();

        public event EventHandler<CameraEventArgs>? CameraChanged;
        public event EventHandler<ViewportEventArgs>? ViewportStateChanged;

        public ViewportService()
        {
            InitializeDefaultCamera();
        }

        /// <summary>
        /// Get current camera data
        /// </summary>
        public CameraData GetCamera()
        {
            return _camera;
        }

        /// <summary>
        /// Get current viewport state
        /// </summary>
        public ViewportState GetViewportState()
        {
            return _viewportState;
        }

        /// <summary>
        /// Orbit camera around target
        /// </summary>
        public void OrbitCamera(float deltaX, float deltaY)
        {
            _camera.RotationY += deltaX * 0.01f;
            _camera.RotationX += deltaY * 0.01f;

            // Clamp vertical rotation
            _camera.RotationX = Math.Clamp(_camera.RotationX, -1.5f, 1.5f);

            UpdateCameraMatrix();
            OnCameraChanged();
        }

        /// <summary>
        /// Pan camera
        /// </summary>
        public void PanCamera(float deltaX, float deltaY)
        {
            float panSpeed = _camera.Distance * 0.01f;
            _camera.PanX += deltaX * panSpeed;
            _camera.PanY -= deltaY * panSpeed;

            UpdateCameraMatrix();
            OnCameraChanged();
        }

        /// <summary>
        /// Zoom camera
        /// </summary>
        public void ZoomCamera(float delta)
        {
            _camera.Distance -= delta * _camera.Distance * 0.1f;
            _camera.Distance = Math.Clamp(_camera.Distance, 0.1f, 100.0f);

            UpdateCameraMatrix();
            OnCameraChanged();
        }

        /// <summary>
        /// Reset camera to default view
        /// </summary>
        public void ResetCamera()
        {
            InitializeDefaultCamera();
            UpdateCameraMatrix();
            OnCameraChanged();
        }

        /// <summary>
        /// Focus camera on bounding box
        /// </summary>
        public void FocusOnBoundingBox(float[] minPoint, float[] maxPoint)
        {
            // Calculate center
            float centerX = (minPoint + maxPoint) / 2.0f;
            float centerY = (minPoint + maxPoint) / 2.0f;
            float centerZ = (minPoint + maxPoint) / 2.0f;

            // Calculate size
            float sizeX = maxPoint - minPoint;
            float sizeY = maxPoint - minPoint;
            float sizeZ = maxPoint - minPoint;

            float maxSize = Math.Max(Math.Max(sizeX, sizeY), sizeZ);
            float distance = maxSize * 1.5f;

            _camera.Distance = distance;
            _camera.PanX = centerX;
            _camera.PanY = centerY;

            UpdateCameraMatrix();
            OnCameraChanged();
        }

        /// <summary>
        /// Toggle grid visibility
        /// </summary>
        public void SetGridVisible(bool visible)
        {
            _viewportState.IsGridVisible = visible;
            OnViewportStateChanged();
        }

        /// <summary>
        /// Toggle axes visibility
        /// </summary>
        public void SetAxesVisible(bool visible)
        {
            _viewportState.IsAxisVisible = visible;
            OnViewportStateChanged();
        }

        /// <summary>
        /// Set gizmo mode (Move, Rotate, Scale, or None)
        /// </summary>
        public void SetGizmoMode(string mode)
        {
            _viewportState.GizmoMode = mode;
            OnViewportStateChanged();
        }

        /// <summary>
        /// Toggle wireframe mode
        /// </summary>
        public void SetWireframeMode(bool enabled)
        {
            _viewportState.IsWireframe = enabled;
            OnViewportStateChanged();
        }

        private void InitializeDefaultCamera()
        {
            _camera = new CameraData
            {
                Distance = 10.0f,
                RotationX = 0.5f,
                RotationY = 0.5f,
                PanX = 0.0f,
                PanY = 0.0f
            };
        }

        private void UpdateCameraMatrix()
        {
            // Calculate view matrix from camera parameters
            // This would use actual matrix math in production
            // For now, store as-is for rendering system to use

            _camera.ViewMatrix = Vortice.Mathematics.Matrix4x4.Identity;

            // In a real implementation:
            // 1. Create rotation matrix from RotationX and RotationY
            // 2. Create translation for Distance
            // 3. Apply pan offsets
            // 4. Combine into view matrix
        }

        protected virtual void OnCameraChanged()
        {
            CameraChanged?.Invoke(this, new CameraEventArgs(_camera));
        }

        protected virtual void OnViewportStateChanged()
        {
            ViewportStateChanged?.Invoke(this, new ViewportEventArgs(_viewportState));
        }
    }

    public class CameraEventArgs : EventArgs
    {
        public CameraData Camera { get; }

        public CameraEventArgs(CameraData camera)
        {
            Camera = camera;
        }
    }

    public class ViewportEventArgs : EventArgs
    {
        public ViewportState State { get; }

        public ViewportEventArgs(ViewportState state)
        {
            State = state;
        }
    }
}
