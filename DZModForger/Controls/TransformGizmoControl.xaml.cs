using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DZModForger.Models;
using System;

namespace DZModForger.Controls
{
    public sealed partial class TransformGizmoControl : UserControl
    {
        public event EventHandler<TransformChangedEventArgs>? TransformChanged;
        public event EventHandler<GizmoModeChangedEventArgs>? GizmoModeChanged;

        private string _currentGizmoMode = "None";
        private bool _isUpdatingUI = false;

        public TransformGizmoControl()
        {
            this.InitializeComponent();
            SelectGizmoMode("None");
        }

        /// <summary>
        /// Set transform data to display
        /// </summary>
        public void SetTransform(TransformData transform)
        {
            if (transform == null)
                return;

            _isUpdatingUI = true;

            PositionInput.SetValues(transform.Position, transform.Position, transform.Position);

            var rotationDegrees = transform.GetRotationDegrees();
            RotationInput.SetValues(rotationDegrees, rotationDegrees, rotationDegrees);

            ScaleInput.SetValues(transform.Scale, transform.Scale, transform.Scale);

            _isUpdatingUI = false;
        }

        /// <summary>
        /// Get current transform data
        /// </summary>
        public TransformData GetTransform()
        {
            var transform = new TransformData();

            transform.SetPosition(
                PositionInput.ValueX,
                PositionInput.ValueY,
                PositionInput.ValueZ
            );

            transform.SetRotationDegrees(
                RotationInput.ValueX,
                RotationInput.ValueY,
                RotationInput.ValueZ
            );

            transform.SetScale(
                ScaleInput.ValueX,
                ScaleInput.ValueY,
                ScaleInput.ValueZ
            );

            return transform;
        }

        /// <summary>
        /// Get current gizmo mode
        /// </summary>
        public string GetGizmoMode()
        {
            return _currentGizmoMode;
        }

        /// <summary>
        /// Set gizmo mode
        /// </summary>
        public void SetGizmoMode(string mode)
        {
            SelectGizmoMode(mode);
        }

        private void SelectGizmoMode(string mode)
        {
            // Deselect all buttons
            NoneButton.IsEnabled = true;
            MoveButton.IsEnabled = true;
            RotateButton.IsEnabled = true;
            ScaleButton.IsEnabled = true;

            // Select the appropriate button
            switch (mode)
            {
                case "None":
                    NoneButton.IsEnabled = false;
                    break;
                case "Move":
                    MoveButton.IsEnabled = false;
                    break;
                case "Rotate":
                    RotateButton.IsEnabled = false;
                    break;
                case "Scale":
                    ScaleButton.IsEnabled = false;
                    break;
            }

            _currentGizmoMode = mode;
            OnGizmoModeChanged();
        }

        private void NoneButton_Click(object sender, RoutedEventArgs e)
        {
            SelectGizmoMode("None");
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            SelectGizmoMode("Move");
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            SelectGizmoMode("Rotate");
        }

        private void ScaleButton_Click(object sender, RoutedEventArgs e)
        {
            SelectGizmoMode("Scale");
        }

        private void PositionInput_Vector3Changed(object sender, Vector3ChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            OnTransformChanged();
        }

        private void RotationInput_Vector3Changed(object sender, Vector3ChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            OnTransformChanged();
        }

        private void ScaleInput_Vector3Changed(object sender, Vector3ChangedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            if (UniformScaleCheckBox.IsChecked == true)
            {
                _isUpdatingUI = true;
                ScaleInput.SetValues(e.X, e.X, e.X);
                _isUpdatingUI = false;
            }

            OnTransformChanged();
        }

        private void OnTransformChanged()
        {
            TransformChanged?.Invoke(this, new TransformChangedEventArgs(GetTransform()));
        }

        private void OnGizmoModeChanged()
        {
            GizmoModeChanged?.Invoke(this, new GizmoModeChangedEventArgs(_currentGizmoMode));
        }
    }

    public class TransformChangedEventArgs : EventArgs
    {
        public TransformData Transform { get; }

        public TransformChangedEventArgs(TransformData transform)
        {
            Transform = transform;
        }
    }

    public class GizmoModeChangedEventArgs : EventArgs
    {
        public string Mode { get; }

        public GizmoModeChangedEventArgs(string mode)
        {
            Mode = mode;
        }
    }
}
