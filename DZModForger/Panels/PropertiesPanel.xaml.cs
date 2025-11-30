using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

namespace DZModForger.Panels
{
    public sealed partial class PropertiesPanel : UserControl
    {
        public PropertiesPanel()
        {
            this.InitializeComponent();
            Debug.WriteLine("[PROPERTIES_PANEL] Initialized");
        }

        private void BtnResetTransform_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PropPosX.Value = 0;
                PropRotX.Value = 0;
                PropScale.Value = 1;
                Debug.WriteLine("[PROPERTIES_PANEL] Transform reset");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROPERTIES_PANEL] Reset error: {ex.Message}");
            }
        }

        public void UpdateModelInfo(string filename, int vertices, int faces)
        {
            try
            {
                PropModelFile.Text = filename;
                PropVertexCount.Text = vertices.ToString();
                PropFaceCount.Text = faces.ToString();
                Debug.WriteLine($"[PROPERTIES_PANEL] Model info updated: {filename}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROPERTIES_PANEL] UpdateModelInfo error: {ex.Message}");
            }
        }

        public void UpdateStats(float fps, ulong gpuMemoryMB)
        {
            try
            {
                PropFPS.Text = fps.ToString("F1");
                PropGPUMemory.Text = $"{gpuMemoryMB} MB";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROPERTIES_PANEL] UpdateStats error: {ex.Message}");
            }
        }
    }
}
