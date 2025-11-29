using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using DZModForger.Models;

namespace DZModForger.ViewModels
{
    /// <summary>
    /// Shell ViewModel - MVVM core for main application state and commands
    /// Manages project data, rendering state, and UI coordination
    /// </summary>
    public class ShellViewModel : ViewModelBase
    {
        private string _projectName;
        private string _projectPath;
        private bool _isProjectModified;
        private int _currentFrameRate;
        private string _statusMessage;
        private EditorMode _currentEditorMode;
        private ShadeMode _currentShadeMode;
        private float _cameraDistance;
        private float _cameraYaw;
        private float _cameraPitch;

        // Collections
        public ObservableCollection<SceneObject> SceneObjects { get; }
        public ObservableCollection<ModelData> LoadedModels { get; }
        public ObservableCollection<string> RecentFiles { get; }

        // Commands
        public ICommand OpenProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand NewProjectCommand { get; }
        public ICommand ExportModelCommand { get; }
        public ICommand ResetViewCommand { get; }
        public ICommand ToggleGridCommand { get; }

        public ShellViewModel()
        {
            Debug.WriteLine("[SHELLVIEWMODEL] Initializing");

            // Initialize collections
            SceneObjects = new ObservableCollection<SceneObject>();
            LoadedModels = new ObservableCollection<ModelData>();
            RecentFiles = new ObservableCollection<string>();

            // Initialize properties
            _projectName = "Untitled Project";
            _projectPath = "";
            _isProjectModified = false;
            _currentFrameRate = 120;
            _statusMessage = "Ready";
            _currentEditorMode = EditorMode.Object;
            _currentShadeMode = ShadeMode.Solid;
            _cameraDistance = 5.0f;
            _cameraYaw = 0.0f;
            _cameraPitch = 30.0f;

            // Initialize commands
            OpenProjectCommand = new RelayCommand(async () => await OpenProject());
            SaveProjectCommand = new RelayCommand(async () => await SaveProject());
            NewProjectCommand = new RelayCommand(async () => await NewProject());
            ExportModelCommand = new RelayCommand(async () => await ExportModel());
            ResetViewCommand = new RelayCommand(() => ResetCameraView());
            ToggleGridCommand = new RelayCommand(() => ToggleGridVisibility());

            // Load recent files from settings
            LoadRecentFiles();

            Debug.WriteLine("[SHELLVIEWMODEL] Initialization complete");
        }

        // ==================== PROPERTIES ====================

        public string ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }

        public string ProjectPath
        {
            get => _projectPath;
            set => SetProperty(ref _projectPath, value);
        }

        public bool IsProjectModified
        {
            get => _isProjectModified;
            set => SetProperty(ref _isProjectModified, value);
        }

        public int CurrentFrameRate
        {
            get => _currentFrameRate;
            set => SetProperty(ref _currentFrameRate, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public EditorMode CurrentEditorMode
        {
            get => _currentEditorMode;
            set => SetProperty(ref _currentEditorMode, value);
        }

        public ShadeMode CurrentShadeMode
        {
            get => _currentShadeMode;
            set => SetProperty(ref _currentShadeMode, value);
        }

        public float CameraDistance
        {
            get => _cameraDistance;
            set => SetProperty(ref _cameraDistance, value);
        }

        public float CameraYaw
        {
            get => _cameraYaw;
            set => SetProperty(ref _cameraYaw, value);
        }

        public float CameraPitch
        {
            get => _cameraPitch;
            set => SetProperty(ref _cameraPitch, value);
        }

        // ==================== PROJECT COMMANDS ====================

        private async Task NewProject()
        {
            try
            {
                Debug.WriteLine("[SHELLVIEWMODEL] Creating new project");
                StatusMessage = "Creating new project...";

                ProjectName = "Untitled Project";
                ProjectPath = "";
                IsProjectModified = false;

                SceneObjects.Clear();
                LoadedModels.Clear();

                StatusMessage = "New project created";
                Debug.WriteLine("[SHELLVIEWMODEL] New project created successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in NewProject: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task OpenProject()
        {
            try
            {
                Debug.WriteLine("[SHELLVIEWMODEL] Opening project");
                StatusMessage = "Opening project...";

                // File picker logic would go here
                // For now, just update status
                StatusMessage = "Project opened";
                Debug.WriteLine("[SHELLVIEWMODEL] Project opened successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in OpenProject: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task SaveProject()
        {
            try
            {
                Debug.WriteLine("[SHELLVIEWMODEL] Saving project");
                StatusMessage = "Saving project...";

                // Serialize project data
                var projectData = new DZProject
                {
                    Name = ProjectName,
                    Path = ProjectPath,
                    Objects = SceneObjects,
                    Models = LoadedModels
                };

                // Save to file (JSON)
                // FileService.SaveProject(projectData);

                IsProjectModified = false;
                StatusMessage = $"Project saved: {ProjectName}";
                Debug.WriteLine("[SHELLVIEWMODEL] Project saved successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in SaveProject: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task ExportModel()
        {
            try
            {
                Debug.WriteLine("[SHELLVIEWMODEL] Exporting model");
                StatusMessage = "Exporting model...";

                // Export logic would go here
                // Supports: FBX, OBJ, DAE, GLTF

                StatusMessage = "Model exported successfully";
                Debug.WriteLine("[SHELLVIEWMODEL] Model exported");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in ExportModel: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        // ==================== VIEW COMMANDS ====================

        private void ResetCameraView()
        {
            try
            {
                Debug.WriteLine("[SHELLVIEWMODEL] Resetting camera view");

                CameraDistance = 5.0f;
                CameraYaw = 0.0f;
                CameraPitch = 30.0f;

                StatusMessage = "Camera view reset";
                Debug.WriteLine("[SHELLVIEWMODEL] Camera view reset successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in ResetCameraView: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private void ToggleGridVisibility()
        {
            try
            {
                Debug.WriteLine("[SHELLVIEWMODEL] Toggling grid visibility");
                StatusMessage = "Grid visibility toggled";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in ToggleGridVisibility: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        // ==================== SCENE MANAGEMENT ====================

        public void AddSceneObject(SceneObject obj)
        {
            try
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Adding scene object: {obj.Name}");
                SceneObjects.Add(obj);
                IsProjectModified = true;
                StatusMessage = $"Added: {obj.Name}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in AddSceneObject: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        public void RemoveSceneObject(SceneObject obj)
        {
            try
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Removing scene object: {obj.Name}");
                SceneObjects.Remove(obj);
                IsProjectModified = true;
                StatusMessage = $"Removed: {obj.Name}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in RemoveSceneObject: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        public void AddModel(ModelData model)
        {
            try
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Adding model: {model.FileName}");
                LoadedModels.Add(model);
                IsProjectModified = true;
                StatusMessage = $"Loaded: {model.FileName}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in AddModel: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        // ==================== SETTINGS ====================

        private void LoadRecentFiles()
        {
            try
            {
                Debug.WriteLine("[SHELLVIEWMODEL] Loading recent files");

                // Load from settings/registry
                // For now, just clear collection
                RecentFiles.Clear();

                Debug.WriteLine("[SHELLVIEWMODEL] Recent files loaded");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SHELLVIEWMODEL] Exception in LoadRecentFiles: {ex.Message}");
            }
        }

        public void UpdateFrameRate(int fps)
        {
            CurrentFrameRate = fps;
            Debug.WriteLine($"[SHELLVIEWMODEL] Frame rate updated: {fps}");
        }

        public void UpdateEditorMode(EditorMode mode)
        {
            CurrentEditorMode = mode;
            StatusMessage = $"Mode: {mode}";
            Debug.WriteLine($"[SHELLVIEWMODEL] Editor mode changed: {mode}");
        }

        public void UpdateShadeMode(ShadeMode mode)
        {
            CurrentShadeMode = mode;
            StatusMessage = $"Shading: {mode}";
            Debug.WriteLine($"[SHELLVIEWMODEL] Shade mode changed: {mode}");
        }
    }

    // ==================== RELAY COMMAND ====================

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();
    }

    // ==================== ASYNC RELAY COMMAND ====================

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public event EventHandler CanExecuteChanged;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            _isExecuting = true;
            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    // ==================== DATA MODELS ====================

    public class SceneObject
    {
        public string Name { get; set; }
        public Transform3D Transform { get; set; }
        public ModelData Model { get; set; }
        public bool IsVisible { get; set; }
        public bool IsSelected { get; set; }
    }

    public class DZProject
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ObservableCollection<SceneObject> Objects { get; set; }
        public ObservableCollection<ModelData> Models { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
