using DZModForger.Interop;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using Windows.Storage.Pickers;

namespace DZModForger
{
    public sealed partial class MainWindow : Window
    {
        private Windows.Foundation.Point _lastMousePos;
        private bool _isOrbiting = false;
        private DX12RenderingService? _renderingService;
        private IntPtr _viewportHandle = IntPtr.Zero;

        public MainWindow()
        {
            this.InitializeComponent();
            Debug.WriteLine("[MAINWINDOW] Initialized");

            try
            {
                // Initialize rendering service (NO WinRT calls here!)
                _renderingService = new DX12RenderingService();
                StatusText.Text = $"Ready - {DX12InteropHelper.GetEngineVersion()}";
                Debug.WriteLine("[MAINWINDOW] Rendering service initialized");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
                Debug.WriteLine($"[MAINWINDOW] Init error: {ex.Message}");
            }
        }

        // ==================== FILE OPERATIONS ====================

        private async void BtnOpenModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".fbx");
                picker.FileTypeFilter.Add(".obj");
                picker.FileTypeFilter.Add(".gltf");
                picker.FileTypeFilter.Add(".glb");

                // ONLY call WinRT interop when actually using the picker
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    TxtModelFile.Text = file.Name;
                    StatusText.Text = $"Loaded: {file.Name}";
                    Debug.WriteLine($"[MAINWINDOW] Model: {file.Path}");
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
                Debug.WriteLine($"[MAINWINDOW] File open error: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Saved";
                Debug.WriteLine("[MAINWINDOW] Save initiated");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Save error: {ex.Message}";
            }
        }

        // ==================== VIEWPORT INPUT ====================

        private void ViewportCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var properties = e.GetCurrentPoint(ViewportCanvas).Properties;
                _lastMousePos = e.GetCurrentPoint(ViewportCanvas).Position;

                if (properties.IsLeftButtonPressed)
                {
                    _isOrbiting = true;
                    StatusText.Text = "Orbiting...";
                }

                ViewportCanvas.CapturePointer(e.Pointer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT] Pointer pressed: {ex.Message}");
            }
        }

        private void ViewportCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                _isOrbiting = false;
                StatusText.Text = "Ready";
                ViewportCanvas.ReleasePointerCapture(e.Pointer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT] Pointer released: {ex.Message}");
            }
        }

        private void ViewportCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (!_isOrbiting || _renderingService == null)
                    return;

                var currentPos = e.GetCurrentPoint(ViewportCanvas).Position;
                float deltaX = (float)(currentPos.X - _lastMousePos.X);
                float deltaY = (float)(currentPos.Y - _lastMousePos.Y);

                _renderingService.RotateCamera(deltaX, deltaY);
                StatusText.Text = $"Orbit: ΔX={deltaX:F1} ΔY={deltaY:F1}";
                _lastMousePos = currentPos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT] Pointer moved: {ex.Message}");
            }
        }

        private void ViewportCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (_renderingService == null)
                    return;

                var properties = e.GetCurrentPoint(ViewportCanvas).Properties;
                int delta = properties.MouseWheelDelta;
                float zoomFactor = delta > 0 ? 1.1f : 0.9f;

                _renderingService.SetCameraZoom(zoomFactor);
                StatusText.Text = $"Zoom: {_renderingService.CameraZoom:F2}x";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VIEWPORT] Wheel: {ex.Message}");
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            try
            {
                _renderingService?.Dispose();
                DX12InteropHelper.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] Close error: {ex.Message}");
            }
        }
    }
}
