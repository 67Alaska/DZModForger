using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using Windows.Storage.Pickers;

namespace UltimateDZForge
{
    /// <summary>
    /// Main window for UltimateDZForge 3D Model Editor
    /// Implements Blender-like viewport with FBX model loading and camera controls
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private CameraController cameraController;
        private ModelLoaderService modelLoader;
        private DX12RenderingService renderingService;

        // Mouse state
        private bool isMouseDragging = false;
        private Windows.Foundation.Point lastMousePos;
        private bool isShiftPressed = false;

        // Editor state
        private EditorMode currentMode = EditorMode.Object;
        private ShadeMode currentShadeMode = ShadeMode.Solid;
        private string currentModelPath = "";

        public MainWindow()
        {
            this.InitializeComponent();
            Debug.WriteLine("[MAINWINDOW] Constructor called");

            // Initialize services
            cameraController = new CameraController();
            modelLoader = new ModelLoaderService();
            renderingService = new DX12RenderingService();

            // Set window title with version
            this.Title = $"UltimateDZForge - 3D Model Editor v1.0";

            // Attach window events
            this.Activated += MainWindow_Activated;
            this.Closed += MainWindow_Closed;

            Debug.WriteLine("[MAINWINDOW] Initialization complete");
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            Debug.WriteLine("[MAINWINDOW] Window activated");
            UpdateStatus("Ready");
        }

        private void MainWindow_Closed(object sender, WindowClosedEventArgs args)
        {
            Debug.WriteLine("[MAINWINDOW] Window closing");
            try
            {
                renderingService?.Dispose();
                Debug.WriteLine("[MAINWINDOW] Rendering service disposed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] Exception during shutdown: {ex.Message}");
            }
        }

        // ==================== FILE MENU HANDLERS ====================

        private async void MenuItemOpenModel_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MAINWINDOW] Open Model clicked");
            try
            {
                var picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add(".fbx");
                picker.FileTypeFilter.Add(".dae");
                picker.FileTypeFilter.Add(".obj");
                picker.FileTypeFilter.Add("*");

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    Debug.WriteLine($"[MAINWINDOW] File selected: {file.Path}");
                    await LoadModelFromPath(file.Path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAINWINDOW] Exception in OpenModel: {ex.Message}");
                ShowErrorDialog("Error", $"Failed to open file: {ex.Message}");
            }
        }

        private async void MenuItemSaveProject_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[MAINWINDOW] Save Project clicked");
            try
            {
                var picker = new FileSavePicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker
