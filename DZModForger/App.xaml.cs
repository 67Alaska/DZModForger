using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;

namespace DZModForger
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            Debug.WriteLine("[APP] Initializing DZModForger");
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                m_window = new MainWindow();
                m_window.Activate();
                Debug.WriteLine("[APP] MainWindow activated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APP] Launch error: {ex.Message}");
            }
        }

        private Window? m_window;
    }
}
