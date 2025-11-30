using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DZModForger.Services;
using DZModForger.Interop;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using System.Diagnostics;

namespace DZModForger
{
    public sealed partial class MainWindow : Window
    {
        private ModelLoaderService? _modelLoaderService;
        private DispatcherTimer? _statsUpdateTimer;
        private DX12ViewportHost? _viewportHost;
        private bool _isFullscreen = false;

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeServices();
        }

        private void InitializeServices()
        {
            try
            {
                Debug.WriteLine("[MainWindow] Initializing services...");

                _modelLoaderService = new ModelLoaderService();
                _modelLoaderService.ModelLoaded += OnModelLoaded;
                _modelLoaderService.ModelLoadError += OnModelLoadError;

                _viewportHost = new DX12ViewportHost();
                _viewportHost.RenderError += OnViewportRenderError;

                _statsUpdateTimer = new DispatcherTimer();
                _statsUpdateTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
                _statsUpdateTimer.Tick += OnStatsUpdateTick;
                _statsUpdateTimer.Start();

                this.Closed += Window_Closed;
                this.Activated += OnWindowActivated;

                // Initialize viewport with window handle
                if (ViewportCanvas != null && _viewportHost != null)
                {
                    IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                    Debug.WriteLine($"[MainWindow] Window HWND: {hwnd}");

                    uint width = 1920;
                    uint height = 1080;

                    _viewportHost.Initialize(hwnd, width, height);
                    Debug.WriteLine("[MainWindow] Viewport initialized successfully");

                    if (StatusText != null)
                    {
                        StatusText.Text = "Viewport initialized - Ready to load models";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Initialization error: {ex.Message}\n{ex.StackTrace}");
                if (StatusText != null)
                {
                    StatusText.Text = $"Initialization error: {ex.Message}";
                }
            }
        }

        private void OnModelLoaded(object? sender, ModelLoadedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    if (StatusText != null)
                    {
                        StatusText.Text = $"Model loaded: {e.FilePath ?? "Unknown"}";
                    }
                    if (TxtModelFile != null)
                    {
                        TxtModelFile.Text = System.IO.Path.GetFileName(e.FilePath ?? "");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[OnModelLoaded] Error: {ex.Message}");
                }
            });
        }

        private void OnModelLoadError(object? sender, ModelLoadErrorEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    if (StatusText != null)
                    {
                        StatusText.Text = $"Error loading model: {e.Exception?.Message ?? "Unknown error"}";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[OnModelLoadError] Error: {ex.Message}");
                }
            });
        }

        private void OnViewportRenderError(object? sender, RenderErrorEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    if (StatusText != null)
                    {
                        StatusText.Text = $"Render error: {e.Exception?.Message ?? "Unknown error"}";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[OnViewportRenderError] Error: {ex.Message}");
                }
            });
        }

        private void OnStatsUpdateTick(object? sender, object? e)
        {
            try
            {
                if (_viewportHost != null && TxtFPS != null)
                {
                    // There is no GetFrameRate method in DX12ViewportHost.
                    // You need to implement this method or remove this line.
                    // For now, display a placeholder or remove the FPS update.
                    TxtFPS.Text = "N/A";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OnStatsUpdateTick] Error: {ex.Message}");
            }
        }

        // ==================== EVENT HANDLERS ====================

        private void ViewportHost_RenderError(object sender, RenderErrorEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = $"Viewport error: {e.Exception?.Message ?? "Unknown error"}";
            }
        }

        private void ChkShowGrid_Checked(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = "Grid enabled";
            }
        }

        private void ChkShowGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = "Grid disabled";
            }
        }

        private void BtnShadeWire_Click(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = "Switched to wireframe mode";
            }
        }

        private void BtnShadeSolid_Click(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = "Switched to solid mode";
            }
        }

        private void BtnShadeRender_Click(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = "Switched to render mode";
            }
        }

        private void BtnObjectMode_Click(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = "Switched to object mode";
            }
        }

        private void BtnEditMode_Click(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = "Switched to edit mode";
            }
        }

        private void BtnSculptMode_Click(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = "Switched to sculpt mode";
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            ShowAboutDialog();
        }

        private void MenuResetCamera_Click(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
            {
                StatusText.Text = "Camera reset";
            }
            _viewportHost?.SetCamera(5.0f, 0.0f, 30.0f, 0.0f, 0.0f, 0.0f);
        }

        private void MenuFullscreen_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullscreen();
        }

        private void MenuPreferences_Click(object sender, RoutedEventArgs e)
        {
            ShowPreferencesDialog();
        }

        private void MenuOpenModel_Click(object sender, RoutedEventArgs e)
        {
            _ = OpenModelFileAsync();
        }

        private void MenuSaveProject_Click(object sender, RoutedEventArgs e)
        {
            SaveProject();
        }

        private void MenuExportModel_Click(object sender, RoutedEventArgs e)
        {
            ExportModel();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        // ==================== HELPER METHODS ====================

        private async Task OpenModelFileAsync()
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".fbx");
                picker.FileTypeFilter.Add(".obj");
                picker.FileTypeFilter.Add(".dae");

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null && _modelLoaderService != null)
                {
                    await _modelLoaderService.LoadModelAsync(file.Path);
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                {
                    StatusText.Text = $"Error opening file: {ex.Message}";
                }
            }
        }

        private void SaveProject()
        {
            try
            {
                if (StatusText != null)
                {
                    StatusText.Text = "Project saved";
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                {
                    StatusText.Text = $"Error saving project: {ex.Message}";
                }
            }
        }

        private void ExportModel()
        {
            try
            {
                if (StatusText != null)
                {
                    StatusText.Text = "Model exported";
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                {
                    StatusText.Text = $"Error exporting model: {ex.Message}";
                }
            }
        }

        private async void ShowAboutDialog()
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "About ModForgerQuantum",
                    Content = new TextBlock
                    {
                        Text = "ModForgerQuantum v1.0.0\n\nProfessional 3D Model Editor for DayZ\n\nBuilt with DirectX 12 and WinUI 3",
                        TextWrapping = TextWrapping.Wrap
                    },
                    PrimaryButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ShowAboutDialog] Error: {ex.Message}");
            }
        }

        private async void ShowPreferencesDialog()
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Preferences",
                    Content = new TextBlock
                    {
                        Text = "Preferences not yet implemented",
                        TextWrapping = TextWrapping.Wrap
                    },
                    PrimaryButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ShowPreferencesDialog] Error: {ex.Message}");
            }
        }

        private void ToggleFullscreen()
        {
            try
            {
                _isFullscreen = !_isFullscreen;

                if (StatusText != null)
                {
                    StatusText.Text = _isFullscreen ? "Fullscreen enabled" : "Fullscreen disabled";
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                {
                    StatusText.Text = $"Error toggling fullscreen: {ex.Message}";
                }
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            try
            {
                _statsUpdateTimer?.Stop();
                _modelLoaderService?.Dispose();
                _viewportHost?.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Window_Closed] Error: {ex.Message}");
            }
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
        {
            try
            {
                // Window activation handling
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OnWindowActivated] Error: {ex.Message}");
            }
        }
    }
}
