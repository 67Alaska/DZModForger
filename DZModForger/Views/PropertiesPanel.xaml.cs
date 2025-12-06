using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace DZModForger
{
    public sealed partial class PropertiesPanel : UserControl
    {
        public PropertiesViewModel ViewModel { get; private set; }

        public PropertiesPanel()
        {
            this.InitializeComponent();
            ViewModel = new PropertiesViewModel();
            this.DataContext = ViewModel;
        }

        private async void DiffuseColor_Click(object sender, RoutedEventArgs e)
        {
            await ShowColorPicker("Diffuse Color", (color) =>
            {
                ViewModel.DiffuseColorHex = color.ToString();
            });
        }

        private async void SpecularColor_Click(object sender, RoutedEventArgs e)
        {
            await ShowColorPicker("Specular Color", (color) =>
            {
                ViewModel.SpecularColorHex = color.ToString();
            });
        }

        private async System.Threading.Tasks.Task ShowColorPicker(string title, System.Action<Color> onColorSelected)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                CloseButtonText = "Cancel",
                PrimaryButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            // Simple implementation - could use actual color picker control
            dialog.Content = new TextBlock { Text = "Color picker placeholder" };

            await dialog.ShowAsync();
        }
    }
}
