using System;
using System.Diagnostics;

namespace DZModForger.Interop
{
    /// <summary>
    /// Camera and rendering state management service
    /// </summary>
    public class DX12RenderingService : IDisposable
    {
        private bool _isInitialized = false;
        private float _cameraYaw = 0f;
        private float _cameraPitch = 0f;
        private float _cameraZoom = 1f;
        private float _cameraDistance = 5f;

        public DX12RenderingService()
        {
            Debug.WriteLine("[DX12_RENDERING] Service initialized");
            _isInitialized = true;
        }

        /// <summary>
        /// Rotate camera (orbit mode)
        /// </summary>
        public void RotateCamera(float deltaYaw, float deltaPitch)
        {
            try
            {
                _cameraYaw += deltaYaw * 0.5f;
                _cameraPitch += deltaPitch * 0.5f;

                // Clamp pitch to avoid gimbal lock
                if (_cameraPitch > 89f) _cameraPitch = 89f;
                if (_cameraPitch < -89f) _cameraPitch = -89f;

                Debug.WriteLine($"[DX12_RENDERING] Rotate - Yaw: {_cameraYaw:F2}°, Pitch: {_cameraPitch:F2}°");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12_RENDERING] RotateCamera error: {ex.Message}");
            }
        }

        /// <summary>
        /// Pan camera (lateral movement)
        /// </summary>
        public void PanCamera(float deltaX, float deltaY)
        {
            try
            {
                Debug.WriteLine($"[DX12_RENDERING] Pan - X: {deltaX:F2}, Y: {deltaY:F2}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12_RENDERING] PanCamera error: {ex.Message}");
            }
        }

        /// <summary>
        /// Zoom camera (scale distance)
        /// </summary>
        public void SetCameraZoom(float zoomFactor)
        {
            try
            {
                _cameraZoom *= zoomFactor;

                // Clamp zoom to reasonable values
                if (_cameraZoom < 0.1f) _cameraZoom = 0.1f;
                if (_cameraZoom > 100f) _cameraZoom = 100f;

                Debug.WriteLine($"[DX12_RENDERING] Zoom: {_cameraZoom:F2}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12_RENDERING] SetCameraZoom error: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset camera to default state
        /// </summary>
        public void ResetCamera()
        {
            try
            {
                _cameraYaw = 0f;
                _cameraPitch = 0f;
                _cameraZoom = 1f;
                _cameraDistance = 5f;
                Debug.WriteLine("[DX12_RENDERING] Camera reset");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12_RENDERING] ResetCamera error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current camera yaw
        /// </summary>
        public float CameraYaw => _cameraYaw;

        /// <summary>
        /// Get current camera pitch
        /// </summary>
        public float CameraPitch => _cameraPitch;

        /// <summary>
        /// Get current camera zoom
        /// </summary>
        public float CameraZoom => _cameraZoom;

        /// <summary>
        /// Get current camera distance
        /// </summary>
        public float CameraDistance => _cameraDistance;

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_isInitialized)
                {
                    _isInitialized = false;
                    Debug.WriteLine("[DX12_RENDERING] Disposed");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12_RENDERING] Dispose error: {ex.Message}");
            }
        }
    }
}
