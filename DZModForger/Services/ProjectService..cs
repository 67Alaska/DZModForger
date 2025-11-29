using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UltimateDZForge.ViewModels;

namespace DZModForger.Services
{
    /// <summary>
    /// Service for managing DZForge projects (save/load/export)
    /// Handles JSON serialization via Newtonsoft.Json
    /// </summary>
    public class ProjectService
    {
        private const string ProjectExtension = ".dzproj";
        private const string ProjectVersion = "1.0";
        private readonly JsonSerializerSettings _jsonSettings;

        public ProjectService()
        {
            Debug.WriteLine("[PROJECTSERVICE] Initializing");

            // Configure JSON serialization
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
                TypeNameHandling = TypeNameHandling.Auto
            };

            Debug.WriteLine("[PROJECTSERVICE] Initialization complete");
        }

        // ==================== PROJECT CREATION ====================

        /// <summary>
        /// Creates a new blank project
        /// </summary>
        /// <returns>New DZProject instance</returns>
        public DZProject CreateNewProject(string projectName = "Untitled Project")
        {
            try
            {
                Debug.WriteLine($"[PROJECTSERVICE] Creating new project: {projectName}");

                var project = new DZProject
                {
                    Name = projectName,
                    Path = "",
                    Objects = new System.Collections.ObjectModel.ObservableCollection<SceneObject>(),
                    Models = new System.Collections.ObjectModel.ObservableCollection<ModelData>(),
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    ProjectVersion = ProjectVersion
                };

                Debug.WriteLine($"[PROJECTSERVICE] New project created successfully");
                return project;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in CreateNewProject: {ex.Message}");
                throw;
            }
        }

        // ==================== PROJECT SAVING ====================

        /// <summary>
        /// Saves a project to disk as JSON
        /// </summary>
        /// <param name="project">Project to save</param>
        /// <param name="filePath">Full path to save location</param>
        /// <returns>True if successful</returns>
        public async Task<bool> SaveProjectAsync(DZProject project, string filePath)
        {
            try
            {
                Debug.WriteLine($"[PROJECTSERVICE] Saving project to: {filePath}");

                // Ensure file path ends with .dzproj
                if (!filePath.EndsWith(ProjectExtension))
                {
                    filePath = Path.ChangeExtension(filePath, ProjectExtension);
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.WriteLine($"[PROJECTSERVICE] Created directory: {directory}");
                }

                // Update modification date
                project.LastModifiedDate = DateTime.UtcNow;
                project.Path = filePath;

                // Serialize to JSON
                var json = JsonConvert.SerializeObject(project, _jsonSettings);

                // Write to file asynchronously
                await File.WriteAllTextAsync(filePath, json);

                Debug.WriteLine($"[PROJECTSERVICE] Project saved successfully: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in SaveProjectAsync: {ex.Message}");
                Debug.WriteLine($"[PROJECTSERVICE] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // ==================== PROJECT LOADING ====================

        /// <summary>
        /// Loads a project from disk
        /// </summary>
        /// <param name="filePath">Full path to project file</param>
        /// <returns>Loaded DZProject instance</returns>
        public async Task<DZProject> LoadProjectAsync(string filePath)
        {
            try
            {
                Debug.WriteLine($"[PROJECTSERVICE] Loading project from: {filePath}");

                // Verify file exists
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Project file not found: {filePath}");
                }

                // Read file asynchronously
                var json = await File.ReadAllTextAsync(filePath);

                // Deserialize from JSON
                var project = JsonConvert.DeserializeObject<DZProject>(json, _jsonSettings);

                if (project == null)
                {
                    throw new InvalidOperationException("Failed to deserialize project file");
                }

                // Ensure path is set
                project.Path = filePath;

                Debug.WriteLine($"[PROJECTSERVICE] Project loaded successfully: {project.Name}");
                Debug.WriteLine($"[PROJECTSERVICE] Objects: {project.Objects?.Count ?? 0}, Models: {project.Models?.Count ?? 0}");

                return project;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in LoadProjectAsync: {ex.Message}");
                Debug.WriteLine($"[PROJECTSERVICE] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // ==================== PROJECT BACKUP ====================

        /// <summary>
        /// Creates a backup of the project file
        /// </summary>
        /// <param name="projectFilePath">Path to project file to backup</param>
        /// <returns>Path to backup file</returns>
        public async Task<string> CreateBackupAsync(string projectFilePath)
        {
            try
            {
                Debug.WriteLine($"[PROJECTSERVICE] Creating backup of: {projectFilePath}");

                if (!File.Exists(projectFilePath))
                {
                    throw new FileNotFoundException($"Project file not found: {projectFilePath}");
                }

                // Create backup filename with timestamp
                var directory = Path.GetDirectoryName(projectFilePath);
                var filename = Path.GetFileNameWithoutExtension(projectFilePath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(directory, $"{filename}_backup_{timestamp}{ProjectExtension}");

                // Copy file asynchronously
                var fileContent = await File.ReadAllBytesAsync(projectFilePath);
                await File.WriteAllBytesAsync(backupPath, fileContent);

                Debug.WriteLine($"[PROJECTSERVICE] Backup created: {backupPath}");
                return backupPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in CreateBackupAsync: {ex.Message}");
                throw;
            }
        }

        // ==================== PROJECT EXPORT ====================

        /// <summary>
        /// Exports project models to FBX format
        /// </summary>
        /// <param name="project">Project to export</param>
        /// <param name="outputPath">Path to save exported file</param>
        /// <returns>True if successful</returns>
        public async Task<bool> ExportToFBXAsync(DZProject project, string outputPath)
        {
            try
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exporting project to FBX: {outputPath}");

                if (project?.Models == null || project.Models.Count == 0)
                {
                    throw new InvalidOperationException("Project contains no models to export");
                }

                // Ensure output path ends with .fbx
                if (!outputPath.EndsWith(".fbx"))
                {
                    outputPath = Path.ChangeExtension(outputPath, ".fbx");
                }

                // TODO: Implement FBX export logic
                // This would use the Autodesk FBX SDK
                Debug.WriteLine($"[PROJECTSERVICE] FBX export logic to be implemented");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in ExportToFBXAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exports project models to OBJ format
        /// </summary>
        /// <param name="project">Project to export</param>
        /// <param name="outputPath">Path to save exported file</param>
        /// <returns>True if successful</returns>
        public async Task<bool> ExportToOBJAsync(DZProject project, string outputPath)
        {
            try
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exporting project to OBJ: {outputPath}");

                if (project?.Models == null || project.Models.Count == 0)
                {
                    throw new InvalidOperationException("Project contains no models to export");
                }

                // Ensure output path ends with .obj
                if (!outputPath.EndsWith(".obj"))
                {
                    outputPath = Path.ChangeExtension(outputPath, ".obj");
                }

                // TODO: Implement OBJ export logic
                Debug.WriteLine($"[PROJECTSERVICE] OBJ export logic to be implemented");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in ExportToOBJAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exports project to DayZ config.cpp format
        /// </summary>
        /// <param name="project">Project to export</param>
        /// <param name="outputPath">Path to save exported file</param>
        /// <returns>True if successful</returns>
        public async Task<bool> ExportToDayZConfigAsync(DZProject project, string outputPath)
        {
            try
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exporting project to DayZ config: {outputPath}");

                if (project?.Models == null || project.Models.Count == 0)
                {
                    throw new InvalidOperationException("Project contains no models to export");
                }

                // TODO: Implement DayZ config export logic
                Debug.WriteLine($"[PROJECTSERVICE] DayZ config export logic to be implemented");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in ExportToDayZConfigAsync: {ex.Message}");
                throw;
            }
        }

        // ==================== RECENT FILES ====================

        /// <summary>
        /// Gets list of recent project files
        /// </summary>
        /// <returns>List of recent file paths</returns>
        public List<string> GetRecentProjects()
        {
            try
            {
                Debug.WriteLine("[PROJECTSERVICE] Getting recent projects");

                var recentProjects = new List<string>();

                // TODO: Implement reading from registry or settings file
                Debug.WriteLine("[PROJECTSERVICE] Recent projects retrieval logic to be implemented");

                return recentProjects;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in GetRecentProjects: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Adds a project to recent files
        /// </summary>
        /// <param name="filePath">Path to project file</param>
        public void AddToRecentProjects(string filePath)
        {
            try
            {
                Debug.WriteLine($"[PROJECTSERVICE] Adding to recent projects: {filePath}");

                // TODO: Implement adding to registry or settings file
                Debug.WriteLine("[PROJECTSERVICE] Recent projects add logic to be implemented");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in AddToRecentProjects: {ex.Message}");
            }
        }

        // ==================== VALIDATION ====================

        /// <summary>
        /// Validates a project file
        /// </summary>
        /// <param name="filePath">Path to project file</param>
        /// <returns>True if valid</returns>
        public async Task<bool> ValidateProjectFileAsync(string filePath)
        {
            try
            {
                Debug.WriteLine($"[PROJECTSERVICE] Validating project file: {filePath}");

                if (!File.Exists(filePath))
                {
                    Debug.WriteLine("[PROJECTSERVICE] Project file not found");
                    return false;
                }

                // Try to load and deserialize
                var json = await File.ReadAllTextAsync(filePath);
                var project = JsonConvert.DeserializeObject<DZProject>(json, _jsonSettings);

                if (project == null)
                {
                    Debug.WriteLine("[PROJECTSERVICE] Failed to deserialize project");
                    return false;
                }

                // Validate required fields
                if (string.IsNullOrEmpty(project.Name))
                {
                    Debug.WriteLine("[PROJECTSERVICE] Project name is empty");
                    return false;
                }

                Debug.WriteLine("[PROJECTSERVICE] Project file is valid");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in ValidateProjectFileAsync: {ex.Message}");
                return false;
            }
        }

        // ==================== CLEANUP ====================

        /// <summary>
        /// Cleanup temporary files
        /// </summary>
        public void Cleanup()
        {
            try
            {
                Debug.WriteLine("[PROJECTSERVICE] Cleaning up");
                // TODO: Implement cleanup logic
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PROJECTSERVICE] Exception in Cleanup: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Extension class for DZProject to add helper methods
    /// </summary>
    public static class DZProjectExtensions
    {
        public static bool HasUnsavedChanges(this DZProject project)
        {
            return project.LastModifiedDate > project.CreatedDate;
        }

        public static int GetTotalObjectCount(this DZProject project)
        {
            return project.Objects?.Count ?? 0;
        }

        public static int GetTotalModelCount(this DZProject project)
        {
            return project.Models?.Count ?? 0;
        }
    }
}
