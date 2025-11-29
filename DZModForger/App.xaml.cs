using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using DZModForger.Configuration;

namespace DZModForger
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// Main entry point for DZModForger with FBX SDK validation and initialization
    /// </summary>
    public partial class App : Application
    {
        private Window _mainWindow;

        /// <summary>
        /// Initializes the singleton application object.
        /// This is the first line of authored code executed, and as such is the logical
        /// equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;

            Debug.WriteLine("[APP] ====================================");
            Debug.WriteLine("[APP] Initializing DZModForger 1.0.0");
            Debug.WriteLine("[APP] ====================================");
            Debug.WriteLine($"[APP] Build Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Debug.WriteLine($"[APP] Platform: {Package.Current.Id.Architecture}");

            // Log FBX SDK configuration
            Debug.WriteLine("[APP] ====================================");
            FBXSDKConfiguration.LogConfiguration();
            Debug.WriteLine("[APP] ====================================");

            // Validate FBX SDK installation
            var (isValid, error) = FBXSDKConfiguration.ValidateInstallation();
            if (!isValid)
            {
                Debug.WriteLine($"[APP] ⚠️  WARNING: FBX SDK validation failed!");
                Debug.WriteLine($"[APP] Error: {error}");
                Debug.WriteLine($"[APP] Expected path: {FBXSDKConfiguration.InstallPath}");
                Debug.WriteLine("[APP] Application will continue with limited model loading support");
            }
            else
            {
                Debug.WriteLine("[APP] ✅ FBX SDK validated successfully");
                var sdkInfo = FBXSDKConfiguration.GetSDKInfo();
                Debug.WriteLine($"[APP] SDK Version: {sdkInfo.Version}");
                Debug.WriteLine($"[APP] DLLs Found: {sdkInfo.DLLCount}");
                Debug.WriteLine($"[APP] Headers Found: {sdkInfo.HeaderCount}");
            }

            Debug.WriteLine("[APP] Application initialization complete");
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                Debug.WriteLine("[APP] ====================================");
                Debug.WriteLine("[APP] Application launched");
                Debug.WriteLine("[APP] ====================================");

                if (_mainWindow == null)
                {
                    _mainWindow = new MainWindow();
                    Debug.WriteLine("[APP] MainWindow created successfully");
                }

                // Ensure the window is active
                _mainWindow.Activate();
                Debug.WriteLine("[APP] MainWindow activated");

                // Log window info
                Debug.WriteLine($"[APP] Window Title: {_mainWindow.Title}");
                Debug.WriteLine("[APP] Application startup complete");

                Debug.WriteLine("[APP] ====================================");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APP] ❌ Exception in OnLaunched: {ex.Message}");
                Debug.WriteLine($"[APP] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Invoked when application execution is being suspended.
        /// Application state is saved without knowing whether the application will be
        /// terminated or resumed with the contents of memory still
