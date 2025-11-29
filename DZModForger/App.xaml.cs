using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using DZModForger.Configuration;

namespace DZModForger
{
    public partial class App : Application
    {
        private Window _mainWindow;

        public App()
        {
            this.InitializeComponent();
            Debug.WriteLine("[APP] DZModForger 1.0.0 initializing");

            // Validate FBX SDK
            var (isValid, error) = FBXSDKConfiguration.ValidateInstallation();
            if (!isValid)
            {
                Debug.WriteLine($"[APP] FBX SDK warning: {error}");
            }
            else
            {
                Debug.WriteLine("[APP] FBX SDK validated successfully");
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                if (_mainWindow == null)
                {
                    _mainWindow = new MainWindow();
                }

                _mainWindow.Activate();
                Debug.WriteLine("[APP] Application launched successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APP] Exception in OnLaunched: {ex.Message}");
                throw;
            }
        }

        public static new App Current => (App)Application.Current;
        public Window MainWindow => _mainWindow;
    }
}
