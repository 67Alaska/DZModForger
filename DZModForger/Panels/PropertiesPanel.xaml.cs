using Microsoft.UI.Xaml.Controls;

namespace DZModForger.Panels
{
    public sealed partial class PropertiesPanel : UserControl
    {
        public PropertiesPanel()
        {
            this.InitializeComponent();
        }

        public void UpdateStats(float fps, ulong memory)
        {
            PropFPS.Text = $"{fps:F0}";
            PropGPUMemory.Text = $"{memory / 1024 / 1024} MB";
        }
    }
}