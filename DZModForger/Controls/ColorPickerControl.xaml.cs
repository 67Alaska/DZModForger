using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace DZModForger.Controls
{
    public sealed partial class ColorPickerControl : UserControl
    {
        public event EventHandler<ColorChangedEventArgs>? ColorChanged;

        private bool _isUpdatingUI = false;

        public ColorPickerControl()
        {
            this.InitializeComponent();
            UpdatePreview();
        }

        /// <summary>
        /// Set color from RGBA values (0-255)
        /// </summary>
        public void SetColorRGBA(int r, int g, int b, int a = 255)
        {
            _isUpdatingUI = true;
            RedSlider.Value = Math.Clamp(r, 0, 255);
            GreenSlider.Value = Math.Clamp(g, 0, 255);
            BlueSlider.Value = Math.Clamp(b, 0, 255);
            AlphaSlider.Value = Math.Clamp(a, 0, 255);
            _isUpdatingUI = false;

            UpdatePreview();
            OnColorChanged();
        }

        /// <summary>
        /// Set color from hex string (#RRGGBBAA)
        /// </summary>
        public void SetColorFromHex(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return;

            if (hexColor.StartsWith("#"))
                hexColor = hexColor.Substring(1);

            try
            {
                int r = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                int a = hexColor.Length >= 8
                    ? int.Parse(hexColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)
                    : 255;

                SetColorRGBA(r, g, b, a);
            }
            catch
            {
                // Invalid hex color
            }
        }

        /// <summary>
        /// Get current color as hex string
        /// </summary>
        public string GetColorHex()
        {
            int r = (int)RedSlider.Value;
            int g = (int)GreenSlider.Value;
            int b = (int)BlueSlider.Value;
            int a = (int)AlphaSlider.Value;

            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        }

        /// <summary>
        /// Get current color as normalized float array (0-1)
        /// </summary>
        public float[] GetColorNormalized()
        {
            return new[]
            {
                (float)RedSlider.Value / 255f,
                (float)GreenSlider.Value / 255f,
                (float)BlueSlider.Value / 255f,
                (float)AlphaSlider.Value / 255f
            };
        }

        /// <summary>
        /// Get current color as Color struct
        /// </summary>
        public Color GetColor()
        {
            return Color.FromArgb(
                (byte)AlphaSlider.Value,
                (byte)RedSlider.Value,
                (byte)GreenSlider.Value,
                (byte)BlueSlider.Value
            );
        }

        private void RedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            RedValue.Text = ((int)e.NewValue).ToString();
            UpdatePreview();
            OnColorChanged();
        }

        private void GreenSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            GreenValue.Text = ((int)e.NewValue).ToString();
            UpdatePreview();
            OnColorChanged();
        }

        private void BlueSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            BlueValue.Text = ((int)e.NewValue).ToString();
            UpdatePreview();
            OnColorChanged();
        }

        private void AlphaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            AlphaValue.Text = ((int)e.NewValue).ToString();
            UpdatePreview();
            OnColorChanged();
        }

        private void HexInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            SetColorFromHex(HexInput.Text);
        }

        private void ColorPreviewButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Could open a more advanced color picker dialog here
        }

        private void UpdatePreview()
        {
            Color color = GetColor();
            ColorPreviewButton.Background = new SolidColorBrush(color);

            _isUpdatingUI = true;
            HexInput.Text = GetColorHex();
            _isUpdatingUI = false;
        }

        private void OnColorChanged()
        {
            ColorChanged?.Invoke(this, new ColorChangedEventArgs(GetColor(), GetColorNormalized()));
        }
    }

    public class ColorChangedEventArgs : EventArgs
    {
        public Color Color { get; }
        public float[] NormalizedColor { get; }

        public ColorChangedEventArgs(Color color, float[] normalizedColor)
        {
            Color = color;
            NormalizedColor = normalizedColor;
        }
    }
}
