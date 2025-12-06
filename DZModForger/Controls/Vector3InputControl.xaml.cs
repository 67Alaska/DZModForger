using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace DZModForger.Controls
{
    public sealed partial class Vector3InputControl : UserControl
    {
        public event EventHandler<Vector3ChangedEventArgs>? Vector3Changed;

        private float _valueX = 0f;
        private float _valueY = 0f;
        private float _valueZ = 0f;
        private bool _isUpdatingUI = false;

        public Vector3InputControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Get the label text
        /// </summary>
        public string LabelText
        {
            get => LabelText;
            set => LabelText = value;
        }

        /// <summary>
        /// Get current X value
        /// </summary>
        public float ValueX
        {
            get => _valueX;
            set
            {
                if (_valueX != value)
                {
                    _valueX = value;
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Get current Y value
        /// </summary>
        public float ValueY
        {
            get => _valueY;
            set
            {
                if (_valueY != value)
                {
                    _valueY = value;
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Get current Z value
        /// </summary>
        public float ValueZ
        {
            get => _valueZ;
            set
            {
                if (_valueZ != value)
                {
                    _valueZ = value;
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Set all values at once
        /// </summary>
        public void SetValues(float x, float y, float z)
        {
            _valueX = x;
            _valueY = y;
            _valueZ = z;
            UpdateUI();
        }

        /// <summary>
        /// Get current vector as array
        /// </summary>
        public float[] GetVector()
        {
            return new[] { _valueX, _valueY, _valueZ };
        }

        private void UpdateUI()
        {
            _isUpdatingUI = true;
            XInput.Text = _valueX.ToString("F2");
            YInput.Text = _valueY.ToString("F2");
            ZInput.Text = _valueZ.ToString("F2");
            _isUpdatingUI = false;
        }

        private void XInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            if (float.TryParse(XInput.Text, out float value))
            {
                _valueX = value;
                OnVector3Changed();
            }
        }

        private void YInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            if (float.TryParse(YInput.Text, out float value))
            {
                _valueY = value;
                OnVector3Changed();
            }
        }

        private void ZInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            if (float.TryParse(ZInput.Text, out float value))
            {
                _valueZ = value;
                OnVector3Changed();
            }
        }

        private void OnVector3Changed()
        {
            Vector3Changed?.Invoke(this, new Vector3ChangedEventArgs(_valueX, _valueY, _valueZ));
        }
    }

    public class Vector3ChangedEventArgs : EventArgs
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Vector3ChangedEventArgs(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
