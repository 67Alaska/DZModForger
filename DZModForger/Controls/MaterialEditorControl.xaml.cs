using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DZModForger.Models;
using System;

namespace DZModForger.Controls
{
    public sealed partial class MaterialEditorControl : UserControl
    {
        public event EventHandler<MaterialChangedEventArgs>? MaterialChanged;

        private MaterialData? _currentMaterial;
        private bool _isUpdatingUI = false;

        public MaterialEditorControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Set material to edit
        /// </summary>
        public void SetMaterial(MaterialData material)
        {
            if (material == null)
                return;

            _currentMaterial = material;
            _isUpdatingUI = true;

            MaterialNameInput.Text = material.Name;

            DiffuseColorPicker.SetColorRGBA(
                (int)(material.DiffuseColor * 255),
                (int)(material.DiffuseColor * 255),
                (int)(material.DiffuseColor * 255),
                (int)(material.DiffuseColor * 255)
            );

            SpecularColorPicker.SetColorRGBA(
                (int)(material.SpecularColor * 255),
                (int)(material.SpecularColor * 255),
                (int)(material.SpecularColor * 255),
                (int)(material.SpecularColor * 255)
            );

            EmissiveColorPicker.SetColorRGBA(
                (int)(material.EmissiveColor * 255),
                (int)(material.EmissiveColor * 255),
                (int)(material.EmissiveColor * 255),
                (int)(material.EmissiveColor * 255)
            );

            ShininessSlider.Value = material.Shininess;
            TransparencySlider.Value = material.Transparency;

            _isUpdatingUI = false;
        }

        /// <summary>
        /// Get current material
        /// </summary>
        public MaterialData GetMaterial()
        {
            if (_currentMaterial == null)
                return new MaterialData();

            _currentMaterial.Name = MaterialNameInput.Text;
            _currentMaterial.Shininess = (float)ShininessSlider.Value;
            _currentMaterial.Transparency = (float)TransparencySlider.Value;

            var diffuseColor = DiffuseColorPicker.GetColorNormalized();
            Array.Copy(diffuseColor, _currentMaterial.DiffuseColor, 4);

            var specularColor = SpecularColorPicker.GetColorNormalized();
            Array.Copy(specularColor, _currentMaterial.SpecularColor, 4);

            var emissiveColor = EmissiveColorPicker.GetColorNormalized();
            Array.Copy(emissiveColor, _currentMaterial.EmissiveColor, 4);

            return _currentMaterial;
        }

        private void MaterialNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingUI || _currentMaterial == null)
                return;

            _currentMaterial.Name = MaterialNameInput.Text;
            OnMaterialChanged();
        }

        private void DiffuseColorPicker_ColorChanged(object sender, ColorPickerControl.ColorChangedEventArgs e)
        {
            if (_isUpdatingUI || _currentMaterial == null)
                return;

            Array.Copy(e.NormalizedColor, _currentMaterial.DiffuseColor, 4);
            OnMaterialChanged();
        }

        private void SpecularColorPicker_ColorChanged(object sender, ColorPickerControl.ColorChangedEventArgs e)
        {
            if (_isUpdatingUI || _currentMaterial == null)
                return;

            Array.Copy(e.NormalizedColor, _currentMaterial.SpecularColor, 4);
            OnMaterialChanged();
        }

        private void EmissiveColorPicker_ColorChanged(object sender, ColorPickerControl.ColorChangedEventArgs e)
        {
            if (_isUpdatingUI || _currentMaterial == null)
                return;

            Array.Copy(e.NormalizedColor, _currentMaterial.EmissiveColor, 4);
            OnMaterialChanged();
        }

        private void ShininessSlider_ValueChanged(object sender, SliderWithValueControl.SliderValueChangedEventArgs e)
        {
            if (_isUpdatingUI || _currentMaterial == null)
                return;

            _currentMaterial.Shininess = (float)e.Value;
            OnMaterialChanged();
        }

        private void TransparencySlider_ValueChanged(object sender, SliderWithValueControl.SliderValueChangedEventArgs e)
        {
            if (_isUpdatingUI || _currentMaterial == null)
                return;

            _currentMaterial.Transparency = (float)e.Value;
            OnMaterialChanged();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMaterial != null)
            {
                _currentMaterial = new MaterialData(_currentMaterial.Name);
                SetMaterial(_currentMaterial);
                OnMaterialChanged();
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMaterial != null)
            {
                var cloned = _currentMaterial.Clone();
                SetMaterial(cloned);
                OnMaterialChanged();
            }
        }

        private void OnMaterialChanged()
        {
            MaterialChanged?.Invoke(this, new MaterialChangedEventArgs(GetMaterial()));
        }
    }

    public class MaterialChangedEventArgs : EventArgs
    {
        public MaterialData Material { get; }

        public MaterialChangedEventArgs(MaterialData material)
        {
            Material = material;
        }
    }
}
