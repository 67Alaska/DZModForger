using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using UltimateDZForge.Configuration;

namespace UltimateDZForge
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window m_window;

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.DebugSettings.EnableRedrawRegions = false;

            // Validate FBX SDK on initialization
            Debug.WriteLine("[APP] Initializing UltimateDZForge 1.0.0");
            Debug.WriteLine("[APP] ====================================");
            FBXSDKConfiguration.LogConfiguration();
            Debug.WriteLine("[APP] ====================================");

            var (isValid, error) = FBXSDKConfiguration.ValidateInstallation();
            if (!isValid)
            {
                Debug.WriteLine($"[APP] ⚠️  WARNING: FBX SDK validation failed!");
                Debug.WriteLine($"[APP] Error: {error}");
                Debug.WriteLine($"[APP] Expected path: {FBXSDKConfiguration.InstallPath}");
            }
            else
            {
                Debug.WriteLine("[APP] ✅ FBX SDK validated successfully");
                var sdkInfo = FBXSDKConfiguration.GetSDKInfo();
                Debug.WriteLine($"[APP] Version: {sdkInfo.Version}");
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

                if (m_window == null)
                {
                    m_window = new MainWindow();
                    Debug.WriteLine("[APP] MainWindow created successfully");
                }

                // Ensure the window is active
                m_window.Activate();
                Debug.WriteLine("[APP] MainWindow activated");
                Debug.WriteLine("[APP] Application startup complete");
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
        /// </summary>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Debug.WriteLine("[APP] Application suspending");
            var deferral = e.SuspendingOperation.GetDeferral();

            try
            {
                // Save application state if needed
                Debug.WriteLine("[APP] Saving application state");

                // Save settings
                AppSettings.WindowMaximized = m_window.Content is Frame;
                Debug.WriteLine("[APP] Application state saved");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APP] ❌ Exception during suspend: {ex.Message}");
            }
            finally
            {
                deferral.Complete();
                Debug.WriteLine("[APP] Application suspended");
            }
        }

        /// <summary>
        /// Invoked when application execution is being resumed.
        /// </summary>
        private void OnResuming(object sender, object e)
        {
            Debug.WriteLine("[APP] Application resuming");
            Debug.WriteLine("[APP] Application resumed");
        }

        /// <summary>
        /// Gets the current application instance
        /// </summary>
        public static new App Current => (App)Application.Current;

        /// <summary>
        /// Gets the main application window
        /// </summary>
        public Window MainWindow => m_window;

        /// <summary>
        /// Gets FBX SDK configuration info
        /// </summary>
        public SDKInfo GetFBXSDKInfo()
        {
            return FBXSDKConfiguration.GetSDKInfo();
        }

        /// <summary>
        /// Validates FBX SDK installation
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateFBXSDK()
        {
            return FBXSDKConfiguration.ValidateInstallation();
        }
    }
}
