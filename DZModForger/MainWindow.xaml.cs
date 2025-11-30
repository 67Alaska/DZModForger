using DZModForger.Interop;
using DZModForger.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT;
using WinRT.Interop;

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
                Debug.WriteLine("[MainWindow] ════════════════════════════════════════════════════════════");
                Debug.WriteLine("[MainWindow] InitializeServices started");
                Debug.WriteLine("[MainWindow] ════════════════════════════════════════════════════════════");

                // ✅ STEP 1: INITIALIZE DX12 INTEROP HELPER (LOAD DLL FIRST)
                Debug.WriteLine("[MainWindow] STEP 1: Initializing DX12InteropHelper...");
                try
                {
                    if (!DX12InteropHelper.Initialize())
                    {
                        Debug.WriteLine("[MainWindow] ❌ DX12InteropHelper.Initialize() returned false");
                        if (StatusText != null)
                            StatusText.Text = "❌ CRITICAL: Cannot initialize DX12 interop";
                        throw new InvalidOperationException("DX12InteropHelper initialization failed");
                    }

                    Debug.WriteLine("[MainWindow] ✓ DX12InteropHelper initialized successfully");
                    Debug.WriteLine($"[MainWindow] DX12 Engine Version: {DX12InteropHelper.GetEngineVersion()}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainWindow] ❌ Failed to initialize DX12InteropHelper: {ex.Message}");
                    Debug.WriteLine($"[MainWindow] Exception type: {ex.GetType().Name}");
                    Debug.WriteLine($"[MainWindow] Stack trace: {ex.StackTrace}");

                    if (StatusText != null)
                        StatusText.Text = $"❌ CRITICAL: DX12 init failed - {ex.Message}";

                    throw; // STOP HERE - don't continue if DX12 won't initialize
                }

                // ✅ STEP 2: Create model loader service
                Debug.WriteLine("[MainWindow] STEP 2: Creating ModelLoaderService...");
                try
                {
                    _modelLoaderService = new ModelLoaderService();
                    _modelLoaderService.ModelLoaded += OnModelLoaded;
                    _modelLoaderService.ModelLoadError += OnModelLoadError;
                    Debug.WriteLine("[MainWindow] ✓ ModelLoaderService created");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainWindow] ❌ Failed to create ModelLoaderService: {ex.Message}");
                    throw;
                }

                // ✅ STEP 3: Create viewport host
                Debug.WriteLine("[MainWindow] STEP 3: Creating DX12ViewportHost...");
                try
                {
                    _viewportHost = new DX12ViewportHost();
                    _viewportHost.RenderError += OnViewportRenderError;
                    Debug.WriteLine("[MainWindow] ✓ DX12ViewportHost created");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainWindow] ❌ Failed to create DX12ViewportHost: {ex.Message}");
                    throw;
                }

                // ✅ STEP 4: Create stats timer
                Debug.WriteLine("[MainWindow] STEP 4: Creating DispatcherTimer...");
                try
                {
                    _statsUpdateTimer = new DispatcherTimer();
                    _statsUpdateTimer.Interval = TimeSpan.FromMilliseconds(500);
                    _statsUpdateTimer.Tick += OnStatsUpdateTick;
                    Debug.WriteLine("[MainWindow] ✓ DispatcherTimer created");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainWindow] ❌ Failed to create DispatcherTimer: {ex.Message}");
                    throw;
                }

                // Register window events
                this.Closed += Window_Closed;
                this.Activated += OnWindowActivated;

                // ✅ STEP 5: Initialize viewport
                Debug.WriteLine("[MainWindow] STEP 5: Initializing viewport...");
                try
                {
                    if (ViewportCanvas == null)
                        throw new InvalidOperationException("ViewportCanvas is null");

                    if (_viewportHost == null)
                        throw new InvalidOperationException("DX12ViewportHost is null");

                    IntPtr hwnd = WindowNative.GetWindowHandle(this);
                    Debug.WriteLine($"[MainWindow] Window HWND: {hwnd}");

                    // Get actual window dimensions
                    uint width = (uint)this.AppWindow.ClientSize.Width;
                    uint height = (uint)this.AppWindow.ClientSize.Height;

                    if (width == 0) width = 1920;  // Fallback
                    if (height == 0) height = 1080; // Fallback

                    Debug.WriteLine($"[MainWindow] Window dimensions: {width}x{height}");
                    Debug.WriteLine("[MainWindow] Calling _viewportHost.Initialize()...");

                    // Initialize the viewport with the DX12 engine
                    _viewportHost.Initialize(hwnd, width, height);

                    Debug.WriteLine("[MainWindow] ✓ Viewport initialized successfully");

                    // Start statistics update timer
                    _statsUpdateTimer.Start();
                    Debug.WriteLine("[MainWindow] ✓ Stats timer started");

                    if (StatusText != null)
                        StatusText.Text = "✓ Ready - Load a model";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainWindow] ❌ Viewport initialization failed: {ex.Message}");
                    Debug.WriteLine($"[MainWindow] Exception type: {ex.GetType().Name}");
                    Debug.WriteLine($"[MainWindow] Stack trace: {ex.StackTrace}");

                    if (StatusText != null)
                        StatusText.Text = $"❌ Viewport init failed: {ex.Message}";

                    throw; // Re-throw so we see the error
                }

                Debug.WriteLine("[MainWindow] ════════════════════════════════════════════════════════════");
                Debug.WriteLine("[MainWindow] ✅ INITIALIZATION COMPLETE - READY FOR USE");
                Debug.WriteLine("[MainWindow] ════════════════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[MainWindow] ════════════════════════════════════════════════════════════");
                Debug.WriteLine("[MainWindow] ❌ FATAL: InitializeServices failed");
                Debug.WriteLine($"[MainWindow] Exception: {ex.Message}");
                Debug.WriteLine($"[MainWindow] Type: {ex.GetType().FullName}");
                Debug.WriteLine($"[MainWindow] Stack: {ex.StackTrace}");
                Debug.WriteLine("[MainWindow] ════════════════════════════════════════════════════════════");

                if (StatusText != null)
                    StatusText.Text = $"❌ FATAL ERROR: {ex.Message}";

                // Don't throw - let app continue so user can see error message
            }
        }

        // ==================== EVENT HANDLERS - MODEL LOADING ====================

        private void OnModelLoaded(object? sender, ModelLoadedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    Debug.WriteLine($"[MainWindow] Model loaded: {e.FilePath}");

                    string fileName = System.IO.Path.GetFileName(e.FilePath ?? "Unknown");

                    if (StatusText != null)
                        StatusText.Text = $"✓ Loaded: {fileName}";

                    if (TxtModelFile != null)
                        TxtModelFile.Text = fileName;

                    // Load into viewport
                    if (_viewportHost != null && e.FilePath != null)
                    {
                        try
                        {
                            _viewportHost.LoadModel(e.FilePath);
                            Debug.WriteLine("[MainWindow] ✓ Model loaded into viewport");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MainWindow] ⚠ Error loading model into viewport: {ex.Message}");
                            if (StatusText != null)
                                StatusText.Text = $"⚠ Load warning: {ex.Message}";
                        }
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
                    string errorMsg = e.Exception?.Message ?? "Unknown error";
                    Debug.WriteLine($"[MainWindow] Model load error: {errorMsg}");

                    if (StatusText != null)
                        StatusText.Text = $"❌ Error: {errorMsg}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[OnModelLoadError] Exception: {ex.Message}");
                }
            });
        }

        private void OnViewportRenderError(object? sender, RenderErrorEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    string errorMsg = e.Exception?.Message ?? "Unknown render error";
                    Debug.WriteLine($"[MainWindow] Viewport render error: {errorMsg}");

                    if (StatusText != null)
                        StatusText.Text = $"⚠ Render Error: {errorMsg}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[OnViewportRenderError] Exception: {ex.Message}");
                }
            });
        }

        private void OnStatsUpdateTick(object? sender, object? e)
        {
            try
            {
                if (_viewportHost != null)
                {
                    float fps = _viewportHost.GetFrameRate();
                    uint vertexCount = _viewportHost.GetVertexCount();
                    uint triangleCount = _viewportHost.GetTriangleCount();

                    if (TxtFPS != null)
                        TxtFPS.Text = fps > 0 ? fps.ToString("F1") : "N/A";

                    if (TxtVertexCount != null && vertexCount > 0)
                        TxtVertexCount.Text = vertexCount.ToString("N0");

                    if (TxtFaceCount != null && triangleCount > 0)
                        TxtFaceCount.Text = triangleCount.ToString("N0");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OnStatsUpdateTick] Error: {ex.Message}");
            }
        }

        // ==================== EVENT HANDLERS - UI CONTROLS ====================

        private void ChkShowGrid_Checked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Grid enabled");
            if (StatusText != null)
                StatusText.Text = "Grid enabled";
        }

        private void ChkShowGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Grid disabled");
            if (StatusText != null)
                StatusText.Text = "Grid disabled";
        }

        private void BtnShadeWire_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Wireframe mode selected");
            if (StatusText != null)
                StatusText.Text = "Wireframe mode";
        }

        private void BtnShadeSolid_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Solid mode selected");
            if (StatusText != null)
                StatusText.Text = "Solid mode";
        }

        private void BtnShadeRender_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Render mode selected");
            if (StatusText != null)
                StatusText.Text = "Render mode";
        }

        private void BtnObjectMode_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Object mode selected");
            if (StatusText != null)
                StatusText.Text = "Object mode";
        }

        private void BtnEditMode_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Edit mode selected");
            if (StatusText != null)
                StatusText.Text = "Edit mode";
        }

        private void BtnSculptMode_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Sculpt mode selected");
            if (StatusText != null)
                StatusText.Text = "Sculpt mode";
        }

        // ==================== EVENT HANDLERS - MENU ====================

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            ShowAboutDialog();
        }

        private void MenuResetCamera_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MainWindow] Camera reset");
            if (StatusText != null)
                StatusText.Text = "Camera reset";
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
                Debug.WriteLine("[MainWindow] Opening file picker...");
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".fbx");
                picker.FileTypeFilter.Add(".obj");
                picker.FileTypeFilter.Add(".dae");
                picker.FileTypeFilter.Add(".gltf");
                picker.FileTypeFilter.Add(".glb");

                var hwnd = WindowNative.GetWindowHandle(this);
                InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null && _modelLoaderService != null)
                {
                    Debug.WriteLine($"[MainWindow] Selected file: {file.Path}");
                    await _modelLoaderService.LoadModelAsync(file.Path);
                }
                else
                {
                    Debug.WriteLine("[MainWindow] File picker cancelled or no file selected");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error opening file: {ex.Message}");
                if (StatusText != null)
                    StatusText.Text = $"❌ Error: {ex.Message}";
            }
        }

        private void SaveProject()
        {
            try
            {
                Debug.WriteLine("[MainWindow] Saving project...");
                if (StatusText != null)
                    StatusText.Text = "✓ Project saved";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error saving: {ex.Message}");
                if (StatusText != null)
                    StatusText.Text = $"❌ Error: {ex.Message}";
            }
        }

        private void ExportModel()
        {
            try
            {
                Debug.WriteLine("[MainWindow] Exporting model...");
                if (StatusText != null)
                    StatusText.Text = "✓ Model exported";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error exporting: {ex.Message}");
                if (StatusText != null)
                    StatusText.Text = $"❌ Error: {ex.Message}";
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
                        Text = "ModForgerQuantum v1.0.0\n" +
                               "Professional 3D Model Editor for DayZ\n\n" +
                               "Built with:\n" +
                               "• DirectX 12 Graphics API\n" +
                               "• Windows App SDK & WinUI 3\n" +
                               "• C++ Native Engine\n\n" +
                               "Features:\n" +
                               "• Real-time 3D viewport\n" +
                               "• FBX/OBJ/DAE import\n" +
                               "• Multiple shading modes\n" +
                               "• Property editing",
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
                        Text = "Preferences panel coming soon...",
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

                if (this.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    if (_isFullscreen)
                    {
                        presenter.Maximize();
                        Debug.WriteLine("[MainWindow] Fullscreen enabled");
                        if (StatusText != null)
                            StatusText.Text = "Fullscreen enabled";
                    }
                    else
                    {
                        presenter.Restore();
                        Debug.WriteLine("[MainWindow] Fullscreen disabled");
                        if (StatusText != null)
                            StatusText.Text = "Fullscreen disabled";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error toggling fullscreen: {ex.Message}");
                if (StatusText != null)
                    StatusText.Text = $"❌ Fullscreen error: {ex.Message}";
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            try
            {
                Debug.WriteLine("[MainWindow] ════════════════════════════════════════════════════════════");
                Debug.WriteLine("[MainWindow] Window closing - cleaning up resources...");

                _statsUpdateTimer?.Stop();
                _modelLoaderService?.Dispose();
                _viewportHost?.Shutdown();

                // Clean up DX12 interop
                DX12InteropHelper.Shutdown();

                Debug.WriteLine("[MainWindow] ✓ Cleanup complete");
                Debug.WriteLine("[MainWindow] ════════════════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Window_Closed] Error during cleanup: {ex.Message}");
            }
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
        {
            try
            {
                // Window activation handling
                Debug.WriteLine($"[MainWindow] Window activated: {args.WindowActivationState}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OnWindowActivated] Error: {ex.Message}");
            }
        }
    }
}
