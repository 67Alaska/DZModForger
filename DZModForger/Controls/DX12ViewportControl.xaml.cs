using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using UltimateDZForge.Services;

namespace UltimateDZForge.Controls
{
    /// <summary>
    /// DX12 Viewport Control - Handles 3D rendering and user interaction
    /// </summary>
    public sealed partial class DX12ViewportControl : UserControl
    {
        private CameraController _cameraController;
        private DX12RenderingService _renderingService;
        private ModelData _currentModel;

        // Input state
        private bool _isMouseDragging;
        private Windows.Foundation.Point _lastMousePos;
        private bool _isShiftPressed;

        // Display state
        private ShadeMode _currentShadeMode = ShadeMode.Solid;
        private bool _gridVisible = true;
        private bool _axesVisible = true;
        private bool _boundsVisible = false;

        // Frame rate tracking
        private int _frameCount;
        private DateTime _lastFPSUpdate;
        private int _currentFPS;

        public DX12ViewportControl()
        {
            this.InitializeComponent();

            Debug.WriteLine("[VIEWPORTCONTROL] Initializing");

            _cameraController = new CameraController();
            _renderingService = new DX12RenderingService();

            _isMouseDragging = false;
            _lastMousePos = new Windows.Foundation.Point(0, 0);
            _isShiftPressed = false;

            _frameCount = 0;
            _lastFPSUpdate = DateTime.UtcNow;
            _currentFPS = 0;

            // Attach viewport events
            this.Loaded += DX12ViewportControl_Loaded;
            this.Unloaded += DX12ViewportControl_Unloaded;

            Debug.WriteLine("[VIEWPORTCONTROL] Initialization complete");
        }

        // ==================== LIFECYCLE ====================

        private void DX12ViewportControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[VIEWPORTCONTROL] Loaded");

                // Initialize rendering service
                if (!_renderingService.IsInitialized)
                {
                    // Get HWND from viewport canvas
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this.XamlRoot.Content as Window);
                    _renderingService.InitializeViewport(hwnd, 1600, 980);
                }

                UpdateStatus("Viewport initialized");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORTCONTROL] Exception in Loaded: {ex.Message}");
                UpdateStatus($"Error: {ex.Message}");
            }
        }

        private void DX12ViewportControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Unloading");
            _renderingService?.Dispose();
        }

        // ==================== VIEWPORT INPUT ====================

        private void ViewportCanvas_MouseLeftButtonDown(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Mouse left button down");

            _isMouseDragging = true;
            _lastMousePos = e.GetCurrentPoint(ViewportCanvas).Position;
            ViewportCanvas.CapturePointer(e.Pointer);
        }

        private void ViewportCanvas_MouseLeftButtonUp(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Mouse left button up");

            _isMouseDragging = false;
            ViewportCanvas.ReleasePointerCapture(e.Pointer);
        }

        private void ViewportCanvas_MouseMove(object sender, PointerRoutedEventArgs e)
        {
            if (!_isMouseDragging) return;

            var currentPos = e.GetCurrentPoint(ViewportCanvas).Position;
            var deltaX = (float)(currentPos.X - _lastMousePos.X);
            var deltaY = (float)(currentPos.Y - _lastMousePos.Y);

            var keyStates = Windows.UI.Core.CoreWindow.GetForCurrentThread().GetKeyState(Windows.System.VirtualKey.Shift);
            _isShiftPressed = (keyStates & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;

            if (_isShiftPressed)
            {
                // Pan camera
                _cameraController.Pan(deltaX, deltaY);
                Debug.WriteLine($"[VIEWPORTCONTROL] Camera pan: X={deltaX}, Y={deltaY}");
            }
            else
            {
                // Orbit camera
                _cameraController.Orbit(deltaX * 0.5f, deltaY * 0.5f);
                Debug.WriteLine($"[VIEWPORTCONTROL] Camera orbit: dYaw={deltaX * 0.5f}, dPitch={deltaY * 0.5f}");
            }

            _lastMousePos = currentPos;
            UpdateCameraInfo();
        }

        private void ViewportCanvas_MouseWheel(object sender, PointerRoutedEventArgs e)
        {
            var wheelDelta = e.GetCurrentPoint(ViewportCanvas).Properties.MouseWheelDelta;
            var zoomFactor = wheelDelta > 0 ? 0.9f : 1.1f;

            _cameraController.Zoom(zoomFactor);
            Debug.WriteLine($"[VIEWPORTCONTROL] Camera zoom: {zoomFactor}");

            UpdateCameraInfo();
        }

        private void ViewportCanvas_MouseRightButtonDown(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Right mouse button down");
            // Reserved for future context menu
        }

        private void ViewportCanvas_MouseRightButtonUp(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Right mouse button up");
        }

        // ==================== VIEW PRESETS ====================

        private void BtnViewFront_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Front view");
            _cameraController.ViewFront();
            UpdateCameraInfo();
            UpdateStatus("Front View");
        }

        private void BtnViewTop_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Top view");
            _cameraController.ViewTop();
            UpdateCameraInfo();
            UpdateStatus("Top View");
        }

        private void BtnViewSide_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Side view");
            _cameraController.ViewSide();
            UpdateCameraInfo();
            UpdateStatus("Side View");
        }

        private void BtnViewIso_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Isometric view");
            _cameraController.ViewIsometric();
            UpdateCameraInfo();
            UpdateStatus("Isometric View");
        }

        // ==================== SHADING MODES ====================

        private void BtnWireframe_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Wireframe mode");
            _currentShadeMode = ShadeMode.Wireframe;
            UpdateShadeButtons();
            ViewportMode.Text = "Wireframe Mode";
            UpdateStatus("Wireframe Mode");
        }

        private void BtnSolid_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Solid mode");
            _currentShadeMode = ShadeMode.Solid;
            UpdateShadeButtons();
            ViewportMode.Text = "Solid Mode";
            UpdateStatus("Solid Mode");
        }

        private void BtnMaterial_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Material mode");
            _currentShadeMode = ShadeMode.Material;
            UpdateShadeButtons();
            ViewportMode.Text = "Material Mode";
            UpdateStatus("Material Mode");
        }

        private void BtnRender_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Render mode");
            _currentShadeMode = ShadeMode.Render;
            UpdateShadeButtons();
            ViewportMode.Text = "Render Mode";
            UpdateStatus("Render Mode");
        }

        private void UpdateShadeButtons()
        {
            var accentBrush = (Microsoft.UI.Xaml.Media.Brush)Resources["AccentBrush"];
            var panelBrush = (Microsoft.UI.Xaml.Media.Brush)Resources["PanelBrush"];

            BtnWireframe.Background = (_currentShadeMode == ShadeMode.Wireframe) ? accentBrush : panelBrush;
            BtnSolid.Background = (_currentShadeMode == ShadeMode.Solid) ? accentBrush : panelBrush;
            BtnMaterial.Background = (_currentShadeMode == ShadeMode.Material) ? accentBrush : panelBrush;
            BtnRender.Background = (_currentShadeMode == ShadeMode.Render) ? accentBrush : panelBrush;
        }

        // ==================== VISIBILITY TOGGLES ====================

        private void ChkShowGrid_Checked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Grid enabled");
            _gridVisible = true;
            _renderingService?.ShowGrid(true);
            GridIndicator.Text = "Grid: ON";
            UpdateStatus("Grid: Visible");
        }

        private void ChkShowGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Grid disabled");
            _gridVisible = false;
            _renderingService?.ShowGrid(false);
            GridIndicator.Text = "Grid: OFF";
            UpdateStatus("Grid: Hidden");
        }

        private void ChkShowAxes_Checked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Axes enabled");
            _axesVisible = true;
            UpdateStatus("Axes: Visible");
        }

        private void ChkShowAxes_Unchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Axes disabled");
            _axesVisible = false;
            UpdateStatus("Axes: Hidden");
        }

        private void ChkShowBounds_Checked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Bounds enabled");
            _boundsVisible = true;
            UpdateStatus("Bounds: Visible");
        }

        private void ChkShowBounds_Unchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VIEWPORTCONTROL] Bounds disabled");
            _boundsVisible = false;
            UpdateStatus("Bounds: Hidden");
        }

        // ==================== DISPLAY UPDATES ====================

        private void UpdateCameraInfo()
        {
            try
            {
                var camPos = _cameraController.CameraPosition;
                CameraInfo.Text = $"Cam: ({camPos.X:F1}, {camPos.Y:F1}, {camPos.Z:F1})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORTCONTROL] Exception in UpdateCameraInfo: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            try
            {
                StatusText.Text = message;
                Debug.WriteLine($"[VIEWPORTCONTROL] Status: {message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORTCONTROL] Exception in UpdateStatus: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates FPS counter
        /// </summary>
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

                    FPSCounter.Text = $"FPS: {_currentFPS}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORTCONTROL] Exception in UpdateFrameRate: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates model statistics display
        /// </summary>
        public void UpdateModelStats(ModelData model)
        {
            try
            {
                if (model == null)
                {
                    StatVertices.Text = "Vertices: 0";
                    StatFaces.Text = "Faces: 0";
                    StatGPUMemory.Text = "GPU Memory: 0 MB";
                    return;
                }

                _currentModel = model;

                StatVertices.Text = $"Vertices: {model.VertexCount}";
                StatFaces.Text = $"Faces: {model.FaceCount}";

                var memoryMB = model.GetMemoryUsageMB();
                StatGPUMemory.Text = $"GPU Memory: {memoryMB:F1} MB";

                Debug.WriteLine($"[VIEWPORTCONTROL] Model stats updated: {model.VertexCount} verts, {model.FaceCount} faces");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORTCONTROL] Exception in UpdateModelStats: {ex.Message}");
            }
        }

        // ==================== PUBLIC METHODS ====================

        /// <summary>
        /// Loads a model into the viewport
        /// </summary>
        public bool LoadModel(ModelData model)
        {
            try
            {
                Debug.WriteLine($"[VIEWPORTCONTROL] Loading model: {model?.FileName}");

                if (_renderingService == null)
                    return false;

                bool success = _renderingService.LoadModel(model);

                if (success)
                {
                    UpdateModelStats(model);
                    UpdateStatus($"Loaded: {model.FileName}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORTCONTROL] Exception in LoadModel: {ex.Message}");
                UpdateStatus($"Error loading model: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets current camera controller
        /// </summary>
        public CameraController GetCameraController() => _cameraController;

        /// <summary>
        /// Gets current rendering service
        /// </summary>
        public DX12RenderingService GetRenderingService() => _renderingService;

        /// <summary>
        /// Focuses camera on model
        /// </summary>
        public void FocusOnModel()
        {
            try
            {
                if (_currentModel == null)
                    return;

                _cameraController.FocusOn(_currentModel.BoundsCenter,
                    Math.Max(_currentModel.BoundsSize.X,
                    Math.Max(_currentModel.BoundsSize.Y, _currentModel.BoundsSize.Z)) * 1.5f);

                UpdateCameraInfo();
                UpdateStatus("Focused on model");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORTCONTROL] Exception in FocusOnModel: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets viewport to default
        /// </summary>
        public void ResetViewport()
        {
            try
            {
                _cameraController.ResetView();
                _currentShadeMode = ShadeMode.Solid;
                UpdateShadeButtons();
                UpdateCameraInfo();
                UpdateStatus("Viewport reset");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORTCONTROL] Exception in ResetViewport: {ex.Message}");
            }
        }
    }

    // ==================== ENUMS ====================

    public enum ShadeMode
    {
        Wireframe,
        Solid,
        Material,
        Render
    }
}
