using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using DZModForger.Services;
using DZModForger.Interop;
using DZModForger.Configuration;

namespace DZModForger
{
    /// <summary>
    /// Main window for DZModForger
    /// Hosts DirectX 12 viewport via COM interop and manages UI interactions
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private ModelLoaderService _modelLoaderService;
        private DispatcherTimer _statsUpdateTimer;
        private bool _isInitialized = false;

        public MainWindow()
        {
            Debug.WriteLine("[MAINWINDOW] MainWindow constructor");

            this.InitializeComponent();

            this.Title = "DZModForger - Professional 3D Model Editor";
            this.Loaded += OnWindowLoaded;
            this.Closed += OnWindowClosed;

            Debug.WriteLine("[MAINWINDOW] MainWindow initialized");
        }

        // ==================== WINDOW LIFECYCLE ====================

        private void OnWindowClosed(object sender, WindowEventArgs args)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Window closing");

                // Stop stats timer
                if (_statsUpdateTimer != null)
                {
                    _statsUpdateTimer.Stop();
                    _statsUpdateTimer = null;
                }

                // Shutdown viewport
                if (ViewportHost != null)
                {
                    ViewportHost.Shutdown();
                }

                // Dispose model loader
                _modelLoaderService?.Dispose();

                Debug.WriteLine("[MAINWINDOW] ✓ Window closed cleanly");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in OnWindowClosed: {ex.Message}");
            }
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] ====================================");
                Debug.WriteLine("[MAINWINDOW] Window loaded - initializing components");
                Debug.WriteLine("[MAINWINDOW] ====================================");

                // Initialize model loader service
                _modelLoaderService = new ModelLoaderService();
                _modelLoaderService.ModelLoaded += OnModelLoaded;
                _modelLoaderService.ModelLoadError += OnModelLoadError;

                Debug.WriteLine("[MAINWINDOW] ✓ ModelLoaderService initialized");

                // Initialize DirectX 12 viewport
                if (ViewportHost != null)
                {
                    ViewportHost.RenderError += OnViewportRenderError;
                    Debug.WriteLine("[MAINWINDOW] ✓ ViewportHost connected");
                }

                // Initialize stats update timer
                InitializeStatsTimer();

                // Update initial UI
                UpdateStatusBar("Ready");
                TxtResolution.Text = $"{(uint)this.AppWindow.ClientSize.Width} x {(uint)this.AppWindow.ClientSize.Height}";

                _isInitialized = true;

                Debug.WriteLine("[MAINWINDOW] ✅ Window initialization complete");
                Debug.WriteLine("[MAINWINDOW] ====================================");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in OnWindowLoaded: {ex.Message}");
                Debug.WriteLine($"[MAINWINDOW] Stack trace: {ex.StackTrace}");
                UpdateStatusBar($"Error: {ex.Message}");
            }
        }

        // ==================== MENU COMMANDS ====================

        private async void MenuOpenModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Opening file picker");
                UpdateStatusBar("Opening model...");

                var filePicker = new FileOpenPicker();
                filePicker.FileTypeFilter.Add(".fbx");
                filePicker.FileTypeFilter.Add(".obj");
                filePicker.FileTypeFilter.Add(".dae");
                filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                var file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    await LoadModel(file.Path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in MenuOpenModel_Click: {ex.Message}");
                UpdateStatusBar($"Error: Failed to open model");
            }
        }

        private void MenuSaveProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Saving project");
                UpdateStatusBar("Project saved");

                // TODO: Implement project save functionality
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in MenuSaveProject_Click: {ex.Message}");
            }
        }

        private void MenuExportModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Exporting model");
                UpdateStatusBar("Exporting model...");

                // TODO: Implement model export functionality
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in MenuExportModel_Click: {ex.Message}");
            }
        }

        private void MenuResetCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Resetting camera");

                if (ViewportHost != null)
                {
                    // Default camera position
                    ViewportHost.SetCamera(5.0f, 0.0f, 30.0f, 0.0f, 0.0f, 0.0f);
                }

                UpdateStatusBar("Camera reset");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in MenuResetCamera_Click: {ex.Message}");
            }
        }

        private void MenuFullscreen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Toggling fullscreen");

                if (this.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    if (presenter.State == Microsoft.UI.Windowing.WindowState.Maximized)
                    {
                        presenter.Restore();
                    }
                    else
                    {
                        presenter.Maximize();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in MenuFullscreen_Click: {ex.Message}");
            }
        }

        private void MenuPreferences_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Opening preferences");
                UpdateStatusBar("Preferences opened");

                // TODO: Implement preferences dialog
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in MenuPreferences_Click: {ex.Message}");
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Showing about dialog");

                var dialog = new ContentDialog()
                {
                    Title = "About DZModForger",
                    Content = "Professional 3D Model Editor\nVersion 1.0.0\n\nBuilt with DirectX 12 and WinUI 3\nFBX SDK 2020.3.7",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.Content.XamlRoot
                };

                _ = dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in MenuAbout_Click: {ex.Message}");
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Exiting application");
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in MenuExit_Click: {ex.Message}");
            }
        }

        // ==================== MODE BUTTONS ====================

        private void BtnObjectMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Object mode selected");
                UpdateModeButtons(BtnObjectMode);
                UpdateStatusBar("Mode: Object");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in BtnObjectMode_Click: {ex.Message}");
            }
        }

        private void BtnEditMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Edit mode selected");
                UpdateModeButtons(BtnEditMode);
                UpdateStatusBar("Mode: Edit");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in BtnEditMode_Click: {ex.Message}");
            }
        }

        private void BtnSculptMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Sculpt mode selected");
                UpdateModeButtons(BtnSculptMode);
                UpdateStatusBar("Mode: Sculpt");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in BtnSculptMode_Click: {ex.Message}");
            }
        }

        private void UpdateModeButtons(Button activeButton)
        {
            var accentBrush = this.Resources["AccentBrush"] as Microsoft.UI.Xaml.Media.Brush;
            var panelBrush = this.Resources["PanelBrush"] as Microsoft.UI.Xaml.Media.Brush;
            var textPrimaryBrush = this.Resources["TextPrimaryBrush"] as Microsoft.UI.Xaml.Media.Brush;
            var textSecondaryBrush = this.Resources["TextSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush;

            foreach (var btn in new[] { BtnObjectMode, BtnEditMode, BtnSculptMode })
            {
                if (btn == activeButton)
                {
                    btn.Background = accentBrush;
                    btn.Foreground = textPrimaryBrush;
                    btn.FontWeight = Windows.UI.Text.FontWeights.SemiBold;
                }
                else
                {
                    btn.Background = panelBrush;
                    btn.Foreground = textSecondaryBrush;
                    btn.FontWeight = Windows.UI.Text.FontWeights.Normal;
                }
            }
        }

        // ==================== SHADING MODE BUTTONS ====================

        private void BtnShadeWire_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Wireframe shading selected");
                UpdateShadeButtons(BtnShadeWire);
                // TODO: Update viewport shading mode
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in BtnShadeWire_Click: {ex.Message}");
            }
        }

        private void BtnShadeSolid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Solid shading selected");
                UpdateShadeButtons(BtnShadeSolid);
                // TODO: Update viewport shading mode
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in BtnShadeSolid_Click: {ex.Message}");
            }
        }

        private void BtnShadeRender_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Render shading selected");
                UpdateShadeButtons(BtnShadeRender);
                // TODO: Update viewport shading mode
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in BtnShadeRender_Click: {ex.Message}");
            }
        }

        private void UpdateShadeButtons(Button activeButton)
        {
            var accentBrush = this.Resources["AccentBrush"] as Microsoft.UI.Xaml.Media.Brush;
            var panelBrush = this.Resources["PanelBrush"] as Microsoft.UI.Xaml.Media.Brush;
            var textPrimaryBrush = this.Resources["TextPrimaryBrush"] as Microsoft.UI.Xaml.Media.Brush;
            var textSecondaryBrush = this.Resources["TextSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush;

            foreach (var btn in new[] { BtnShadeWire, BtnShadeSolid, BtnShadeRender })
            {
                if (btn == activeButton)
                {
                    btn.Background = accentBrush;
                    btn.Foreground = textPrimaryBrush;
                    btn.FontWeight = Windows.UI.Text.FontWeights.SemiBold;
                }
                else
                {
                    btn.Background = panelBrush;
                    btn.Foreground = textSecondaryBrush;
                    btn.FontWeight = Windows.UI.Text.FontWeights.Normal;
                }
            }
        }

        // ==================== GRID TOGGLE ====================

        private void ChkShowGrid_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Grid visibility toggled: ON");
                // TODO: Show grid in viewport
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in ChkShowGrid_Checked: {ex.Message}");
            }
        }

        private void ChkShowGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[MAINWINDOW] Grid visibility toggled: OFF");
                // TODO: Hide grid in viewport
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in ChkShowGrid_Unchecked: {ex.Message}");
            }
        }

        // ==================== MODEL LOADING ====================

        private async Task LoadModel(string filePath)
        {
            try
            {
                Debug.WriteLine($"[MAINWINDOW] Loading model: {filePath}");
                UpdateStatusBar("Loading model...");

                var modelData = await _modelLoaderService.LoadModelAsync(filePath);

                if (modelData != null && ViewportHost != null)
                {
                    ViewportHost.LoadModel(filePath);
                    UpdateStatusBar($"Loaded: {Path.GetFileName(filePath)}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in LoadModel: {ex.Message}");
                UpdateStatusBar($"Error loading model: {ex.Message}");
            }
        }

        // ==================== EVENT HANDLERS ====================

        private void OnModelLoaded(object sender, ModelLoadedEventArgs e)
        {
            try
            {
                Debug.WriteLine($"[MAINWINDOW] Model loaded event: {e.FilePath}");
                TxtModelFile.Text = Path.GetFileName(e.FilePath);

                if (ViewportHost != null)
                {
                    TxtVertexCount.Text = ViewportHost.GetVertexCount().ToString("N0");
                    TxtFaceCount.Text = ViewportHost.GetTriangleCount().ToString("N0");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in OnModelLoaded: {ex.Message}");
            }
        }

        private void OnModelLoadError(object sender, ModelLoadErrorEventArgs e)
        {
            try
            {
                Debug.WriteLine($"[MAINWINDOW] Model load error: {e.Exception?.Message}");
                UpdateStatusBar($"Error loading model: {e.Exception?.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in OnModelLoadError: {ex.Message}");
            }
        }

        private void OnViewportRenderError(object sender, RenderErrorEventArgs e)
        {
            try
            {
                Debug.WriteLine($"[MAINWINDOW] Viewport render error: {e.Exception?.Message}");
                UpdateStatusBar($"Render error: {e.Exception?.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in OnViewportRenderError: {ex.Message}");
            }
        }

        // ==================== STATS UPDATE ====================

        private void InitializeStatsTimer()
        {
            try
            {
                _statsUpdateTimer = new DispatcherTimer();
                _statsUpdateTimer.Interval = TimeSpan.FromMilliseconds(500);
                _statsUpdateTimer.Tick += OnStatsUpdateTick;
                _statsUpdateTimer.Start();

                Debug.WriteLine("[MAINWINDOW] Stats timer started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] ❌ Exception in InitializeStatsTimer: {ex.Message}");
            }
        }

        private void OnStatsUpdateTick(object sender, object e)
        {
            try
            {
                if (ViewportHost == null || !_isInitialized)
                    return;

                float fps = ViewportHost.GetFrameRate();
                TxtFPS.Text = fps.ToString("F1");

                uint vertexCount = ViewportHost.GetVertexCount();
                uint faceCount = ViewportHost.GetTriangleCount();

                if (vertexCount > 0)
                {
                    TxtVertexCount.Text = vertexCount.ToString("N0");
                    TxtFaceCount.Text = faceCount.ToString("N0");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] Exception in OnStatsUpdateTick: {ex.Message}");
            }
        }

        // ==================== UI UTILITIES ====================

        private void UpdateStatusBar(string message)
        {
            try
            {
                StatusText.Text = message;
                Debug.WriteLine($"[MAINWINDOW] Status: {message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] Exception in UpdateStatusBar: {ex.Message}");
            }
        }
    }
}
