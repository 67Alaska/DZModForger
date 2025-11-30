using DZModForger.Interop;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;

namespace DZModForger.Controls
{
    public sealed partial class DX12ViewportControl : UserControl
    {
        private DX12RenderingService _renderingService;
        private bool _isOrbitMode = false;
        private bool _isPanMode = false;
        private Windows.Foundation.Point _lastMousePos;

        // FPS tracking
        private int _frameCount = 0;
        private DateTime _lastFPSUpdate = DateTime.UtcNow;
        private int _currentFPS = 0;

        public DX12ViewportControl()
        {
            this.InitializeComponent();
            Debug.WriteLine("[VIEWPORT_CONTROL] Initializing DX12ViewportControl");

            try
            {
                _renderingService = new DX12RenderingService();
                _lastMousePos = new Windows.Foundation.Point(0, 0);

                // Hook events
                this.Loaded += DX12ViewportControl_Loaded;
                this.Unloaded += DX12ViewportControl_Unloaded;

                Debug.WriteLine("[VIEWPORT_CONTROL] Initialization complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] Init error: {ex.Message}");
                StatusText.Text = $"Init Error: {ex.Message}";
            }
        }

        // ==================== LIFECYCLE ====================

        private void DX12ViewportControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[VIEWPORT_CONTROL] Loaded event fired");
                StatusText.Text = "DX12 Viewport Ready";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] Loaded error: {ex.Message}");
            }
        }

        private void DX12ViewportControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[VIEWPORT_CONTROL] Unloading");
                _renderingService?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] Unload error: {ex.Message}");
            }
        }

        // ==================== INPUT HANDLING ====================

        private void ViewportCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var properties = e.GetCurrentPoint(ViewportCanvas).Properties;
                var position = e.GetCurrentPoint(ViewportCanvas).Position;

                _lastMousePos = position;

                if (properties.IsLeftButtonPressed)
                {
                    _isOrbitMode = true;
                    StatusText.Text = "Orbiting";
                }
                else if (properties.IsRightButtonPressed)
                {
                    _isPanMode = true;
                    StatusText.Text = "Panning";
                }

                ViewportCanvas.CapturePointer(e.Pointer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] PointerPressed error: {ex.Message}");
            }
        }

        private void ViewportCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                _isOrbitMode = false;
                _isPanMode = false;
                StatusText.Text = "Ready";
                ViewportCanvas.ReleasePointerCapture(e.Pointer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] PointerReleased error: {ex.Message}");
            }
        }

        private void ViewportCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (!_isOrbitMode && !_isPanMode)
                    return;

                var position = e.GetCurrentPoint(ViewportCanvas).Position;
                float deltaX = (float)(position.X - _lastMousePos.X);
                float deltaY = (float)(position.Y - _lastMousePos.Y);

                if (_isOrbitMode)
                {
                    // Orbit camera
                    _renderingService?.RotateCamera(deltaX * 0.5f, deltaY * 0.5f);
                    CameraInfoText.Text = $"Orbit: Δx={deltaX:F1} Δy={deltaY:F1}";
                }
                else if (_isPanMode)
                {
                    // Pan camera
                    _renderingService?.PanCamera(deltaX, deltaY);
                    CameraInfoText.Text = $"Pan: Δx={deltaX:F1} Δy={deltaY:F1}";
                }

                _lastMousePos = position;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] PointerMoved error: {ex.Message}");
            }
        }

        private void ViewportCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var properties = e.GetCurrentPoint(ViewportCanvas).Properties;
                int delta = properties.MouseWheelDelta;

                float zoomFactor = delta > 0 ? 0.9f : 1.1f;
                _renderingService?.SetCameraZoom(zoomFactor);

                StatusText.Text = $"Zoom: {zoomFactor:F2}x";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] PointerWheelChanged error: {ex.Message}");
            }
        }

        // ==================== PUBLIC API ====================

        public void UpdateFrameRate()
        {
            try
            {
                _frameCount++;
                var now = DateTime.UtcNow;
                var elapsed = now - _lastFPSUpdate;

                if (elapsed.TotalSeconds >= 1.0)
                {
                    _currentFPS = _frameCount;
                    _frameCount = 0;
                    _lastFPSUpdate = now;

                    FPSCounter.Text = _currentFPS.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] UpdateFrameRate error: {ex.Message}");
            }
        }

        public void UpdateModelStats(int vertexCount, int faceCount, ulong gpuMemoryMB)
        {
            try
            {
                StatVertices.Text = vertexCount.ToString();
                StatFaces.Text = faceCount.ToString();
                StatGPUMemory.Text = $"{gpuMemoryMB} MB";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] UpdateModelStats error: {ex.Message}");
            }
        }

        public void ResetView()
        {
            try
            {
                _renderingService?.ResetCamera();
                CameraInfoText.Text = "Camera: Reset";
                StatusText.Text = "View Reset";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT_CONTROL] ResetView error: {ex.Message}");
            }
        }
    }
}