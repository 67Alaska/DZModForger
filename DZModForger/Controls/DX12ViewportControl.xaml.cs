using DZModForger.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using UltimateDZForge.Services;

namespace DZModForger.Controls
{
    public sealed partial class DX12ViewportControl : UserControl
    {
        private DX12RenderingService _renderingService;
        private bool _isOrbiting = false;
        private bool _isPanning = false;
        private float _lastX;
        private float _lastY;

        public DX12ViewportControl()
        {
            this.InitializeComponent();

            // Initialize rendering service
            _renderingService = new DX12RenderingService();

            // Hook into loaded event to initialize DX12
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Get window handle for swap chain
            var window = (Application.Current as App)?.MainWindow;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize engine
            uint width = (uint)this.ActualWidth;
            uint height = (uint)this.ActualHeight;

            // Ensure valid dimensions
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            if (_renderingService.Initialize(hwnd, width, height))
            {
                // Start rendering loop (using CompositionTarget in real app, or timer)
                // For now, we just initialized
                CameraInfoText.Text = "Status: DX12 Initialized";
            }
            else
            {
                CameraInfoText.Text = "Error: DX12 Init Failed";
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _renderingService?.Dispose();
        }

        // ==================== INPUT HANDLING (WinUI 3) ====================

        private void ViewportCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint(ViewportCanvas).Properties;
            var position = e.GetCurrentPoint(ViewportCanvas).Position;

            _lastX = (float)position.X;
            _lastY = (float)position.Y;

            if (properties.IsLeftButtonPressed)
            {
                _isOrbiting = true;
                ViewportCanvas.CapturePointer(e.Pointer);
            }
            else if (properties.IsMiddleButtonPressed || properties.IsRightButtonPressed)
            {
                _isPanning = true;
                ViewportCanvas.CapturePointer(e.Pointer);
            }
        }

        private void ViewportCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isOrbiting = false;
            _isPanning = false;
            ViewportCanvas.ReleasePointerCapture(e.Pointer);
        }

        private void ViewportCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var position = e.GetCurrentPoint(ViewportCanvas).Position;
            float currentX = (float)position.X;
            float currentY = (float)position.Y;

            float deltaX = currentX - _lastX;
            float deltaY = currentY - _lastY;

            if (_isOrbiting)
            {
                // Update orbital camera
                // _renderingService.RotateCamera(deltaX, deltaY);
                CameraInfoText.Text = $"Orbit: {deltaX:F1}, {deltaY:F1}";
            }
            else if (_isPanning)
            {
                // Update pan
                // _renderingService.PanCamera(deltaX, deltaY);
                CameraInfoText.Text = $"Pan: {deltaX:F1}, {deltaY:F1}";
            }

            _lastX = currentX;
            _lastY = currentY;
        }

        private void ViewportCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint(ViewportCanvas).Properties;
            int delta = properties.MouseWheelDelta;

            // Zoom logic
            float zoomFactor = delta > 0 ? 0.9f : 1.1f;
            _renderingService.SetCameraZoom(zoomFactor);

            CameraInfoText.Text = $"Zoom: {delta}";
        }
    }
}
