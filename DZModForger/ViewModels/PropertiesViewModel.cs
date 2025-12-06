using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace DZModForger
{
    public class PropertiesViewModel : INotifyPropertyChanged
    {
        private float _positionX, _positionY, _positionZ;
        private float _rotationX, _rotationY, _rotationZ;
        private float _scaleX = 1, _scaleY = 1, _scaleZ = 1;
        private string _diffuseColorHex = "#FF6B5B";
        private string _specularColorHex = "#FFFFFF";
        private float _shininess = 32;
        private float _opacity = 1.0f;
        private bool _isVisible = true;
        private bool _showWireframe = false;
        private bool _showNormals = false;
        private bool _showTangents = false;
        private bool _smoothShading = true;

        public float PositionX
        {
            get => _positionX;
            set => SetProperty(ref _positionX, value);
        }

        public float PositionY
        {
            get => _positionY;
            set => SetProperty(ref _positionY, value);
        }

        public float PositionZ
        {
            get => _positionZ;
            set => SetProperty(ref _positionZ, value);
        }

        public float RotationX
        {
            get => _rotationX;
            set => SetProperty(ref _rotationX, value);
        }

        public float RotationY
        {
            get => _rotationY;
            set => SetProperty(ref _rotationY, value);
        }

        public float RotationZ
        {
            get => _rotationZ;
            set => SetProperty(ref _rotationZ, value);
        }

        public float ScaleX
        {
            get => _scaleX;
            set => SetProperty(ref _scaleX, value);
        }

        public float ScaleY
        {
            get => _scaleY;
            set => SetProperty(ref _scaleY, value);
        }

        public float ScaleZ
        {
            get => _scaleZ;
            set => SetProperty(ref _scaleZ, value);
        }

        public string DiffuseColorHex
        {
            get => _diffuseColorHex;
            set => SetProperty(ref _diffuseColorHex, value);
        }

        public Brush DiffuseColorBrush
        {
            get
            {
                if (Color.TryParse(_diffuseColorHex, out var color))
                    return new SolidColorBrush(color);
                return new SolidColorBrush(Colors.Red);
            }
        }

        public string SpecularColorHex
        {
            get => _specularColorHex;
            set => SetProperty(ref _specularColorHex, value);
        }

        public Brush SpecularColorBrush
        {
            get
            {
                if (Color.TryParse(_specularColorHex, out var color))
                    return new SolidColorBrush(color);
                return new SolidColorBrush(Colors.White);
            }
        }

        public float Shininess
        {
            get => _shininess;
            set => SetProperty(ref _shininess, value);
        }

        public float Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool ShowWireframe
        {
            get => _showWireframe;
            set => SetProperty(ref _showWireframe, value);
        }

        public bool ShowNormals
        {
            get => _showNormals;
            set => SetProperty(ref _showNormals, value);
        }

        public bool ShowTangents
        {
            get => _showTangents;
            set => SetProperty(ref _showTangents, value);
        }

        public bool SmoothShading
        {
            get => _smoothShading;
            set => SetProperty(ref _smoothShading, value);
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
