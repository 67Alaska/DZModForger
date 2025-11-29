using Microsoft.UI.Xaml;
using DZModForger.Controls;
using DZModForger.Services;

namespace DZModForger
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Set window title
            this.Title = "DZModForger - Professional 3D Editor";

            // Initialize status
            StatusText.Text = "System Ready";
        }
    }
}
