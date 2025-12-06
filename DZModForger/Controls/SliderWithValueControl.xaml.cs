using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;

namespace DZModForger.Controls
{
    public sealed partial class SliderWithValueControl : UserControl
    {
        public event EventHandler<SliderValueChangedEventArgs>? ValueChanged;

        private bool _isUpdatingUI = false;

        public SliderWithValueControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Get or set label text
        /// </summary>
        public string LabelText
        {
            get => LabelText;
            set
            {
                LabelText = value;
            }
        }

        /// <summary>
        /// Get or set minimum value
        /// </summary>
        public double Minimum
        {
            get => MainSlider.Minimum;
            set => MainSlider.Minimum = value;
        }

        /// <summary>
        /// Get or set maximum value
        /// </summary>
        public double Maximum
        {
            get => MainSlider.Maximum;
            set => MainSlider.Maximum = value;
        }

        /// <summary>
        /// Get or set current value
        /// </summary>
        public double Value
        {
            get => MainSlider.Value;
            set
            {
                _isUpdatingUI = true;
                MainSlider.Value = value;
                _isUpdatingUI = false;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Get or set step frequency
        /// </summary>
        public double StepFrequency
        {
            get => MainSlider.StepFrequency;
            set => MainSlider.StepFrequency = value;
        }

        private void MainSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            UpdateDisplay();
            OnValueChanged();
        }

        private void ValueInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            if (double.TryParse(ValueInput.Text, out double value))
            {
                value = Math.Clamp(value, Minimum, Maximum);
                _isUpdatingUI = true;
                MainSlider.Value = value;
                _isUpdatingUI = false;
                UpdateDisplay();
                OnValueChanged();
            }
        }

        private void UpdateDisplay()
        {
            _isUpdatingUI = true;
            ValueDisplay.Text = MainSlider.Value.ToString("F2");
            ValueInput.Text = MainSlider.Value.ToString("F2");
            _isUpdatingUI = false;
        }

        private void OnValueChanged()
        {
            ValueChanged?.Invoke(this, new SliderValueChangedEventArgs(MainSlider.Value));
        }
    }

    public class SliderValueChangedEventArgs : EventArgs
    {
        public double Value { get; }

        public SliderValueChangedEventArgs(double value)
        {
            Value = value;
        }
    }
}
