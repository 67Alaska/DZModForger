using DZModForger.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DZModForger.ViewModels
{
    public sealed partial class ShellViewModel : ViewModelBase
    {
        private ObservableCollection<SceneObject> _sceneObjects;
        private ObservableCollection<ModelReference> _modelReferences;
        private SceneObject? _selectedObject;
        private ModelReference? _selectedModel;

        public ObservableCollection<SceneObject> SceneObjects
        {
            get => _sceneObjects;
            set => SetProperty(ref _sceneObjects, value);
        }

        public ObservableCollection<ModelReference> ModelReferences
        {
            get => _modelReferences;
            set => SetProperty(ref _modelReferences, value);
        }

        public SceneObject? SelectedObject
        {
            get => _selectedObject;
            set => SetProperty(ref _selectedObject, value);
        }

        public ModelReference? SelectedModel
        {
            get => _selectedModel;
            set => SetProperty(ref _selectedModel, value);
        }

        public ICommand DeleteObjectCommand { get; }
        public ICommand DuplicateObjectCommand { get; }
        public ICommand RenameObjectCommand { get; }
        public ICommand ClearSceneCommand { get; }

        public ShellViewModel()
        {
            Debug.WriteLine("[SHELLVIEWMODEL] ShellViewModel created");

            _sceneObjects = new ObservableCollection<SceneObject>();
            _modelReferences = new ObservableCollection<ModelReference>();

            DeleteObjectCommand = new RelayCommand(DeleteObject, CanDeleteObject);
            DuplicateObjectCommand = new RelayCommand(DuplicateObject, CanDuplicateObject);
            RenameObjectCommand = new RelayCommand(RenameObject, CanRenameObject);
            ClearSceneCommand = new RelayCommand(ClearScene, CanClearScene);
        }

        private bool CanDeleteObject(object parameter) => SelectedObject != null;
        private bool CanDuplicateObject(object parameter) => SelectedObject != null;
        private bool CanRenameObject(object parameter) => SelectedObject != null;
        private bool CanClearScene(object parameter) => SceneObjects.Count > 0;

        private void DeleteObject(object parameter)
        {
            try
            {
                if (SelectedObject != null)
                {
                    Debug.WriteLine($"[SHELLVIEWMODEL] Deleting object: {SelectedObject.Name}");
                    SceneObjects.Remove(SelectedObject);
                    SelectedObject = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in DeleteObject: {ex.Message}");
            }
        }

        private void DuplicateObject(object parameter)
        {
            try
            {
                if (SelectedObject != null)
                {
                    Debug.WriteLine($"[SHELLVIEWMODEL] Duplicating object: {SelectedObject.Name}");
                    var duplicate = new SceneObject
                    {
                        Name = $"{SelectedObject.Name}_Copy",
                        Transform = new Transform3D { Position = SelectedObject.Transform.Position },
                        Model = SelectedObject.Model
                    };
                    SceneObjects.Add(duplicate);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in DuplicateObject: {ex.Message}");
            }
        }

        private void RenameObject(object parameter)
        {
            try
            {
                if (SelectedObject != null)
                {
                    Debug.WriteLine($"[SHELLVIEWMODEL] Rename object: {SelectedObject.Name}");
                    // TODO: Show rename dialog
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in RenameObject: {ex.Message}");
            }
        }

        private void ClearScene(object parameter)
        {
            try
            {
                Debug.WriteLine("[SHELLVIEWMODEL] Clearing scene");
                SceneObjects.Clear();
                SelectedObject = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in ClearScene: {ex.Message}");
            }
        }

        public void AddObject(SceneObject sceneObject)
        {
            try
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Adding object: {sceneObject.Name}");
                SceneObjects.Add(sceneObject);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in AddObject: {ex.Message}");
            }
        }

        public void AddModel(ModelReference modelRef)
        {
            try
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Adding model reference: {modelRef.Name}");
                ModelReferences.Add(modelRef);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in AddModel: {ex.Message}");
            }
        }
    }

    public class SceneObject : ViewModelBase
    {
        private string _name = string.Empty;
        private Transform3D _transform = new();
        private ModelData _model = new();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Transform3D Transform
        {
            get => _transform;
            set => SetProperty(ref _transform, value);
        }

        public ModelData Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }
    }

    public class ModelReference : ViewModelBase
    {
        private string _name = string.Empty;
        private string _path = string.Empty;
        private ObservableCollection<SceneObject> _objects = new();
        private ObservableCollection<ModelData> _models = new();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public ObservableCollection<SceneObject> Objects
        {
            get => _objects;
            set => SetProperty(ref _objects, value);
        }

        public ObservableCollection<ModelData> Models
        {
            get => _models;
            set => SetProperty(ref _models, value);
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter!) ?? true;

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
                _execute(parameter!);
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Func<object, bool>? _canExecute;
        private bool _isExecuting;

        public event EventHandler? CanExecuteChanged;

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke(parameter!) ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            try
            {
                await _execute(parameter!);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
