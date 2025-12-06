using System;
using System.Collections.Generic;

namespace DZModForger.Models
{
    /// <summary>
    /// Application-wide settings and preferences
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Default output directory for exports
        /// </summary>
        public string DefaultExportDirectory { get; set; } =
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// Recent files list
        /// </summary>
        public List<string> RecentFiles { get; set; } = new();

        /// <summary>
        /// Maximum recent files to keep
        /// </summary>
        public int MaxRecentFiles { get; set; } = 10;

        /// <summary>
        /// Auto-save enabled
        /// </summary>
        public bool AutoSaveEnabled { get; set; } = true;

        /// <summary>
        /// Auto-save interval in seconds
        /// </summary>
        public int AutoSaveInterval { get; set; } = 300; // 5 minutes

        /// <summary>
        /// Backup directory
        /// </summary>
        public string BackupDirectory { get; set; } =
            System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "DZModForger", "Backups"
            );

        /// <summary>
        /// Viewport grid size
        /// </summary>
        public float GridSize { get; set; } = 1.0f;

        /// <summary>
        /// Viewport grid subdivisions
        /// </summary>
        public int GridSubdivisions { get; set; } = 10;

        /// <summary>
        /// Camera field of view (degrees)
        /// </summary>
        public float CameraFieldOfView { get; set; } = 45.0f;

        /// <summary>
        /// Default material shininess
        /// </summary>
        public float DefaultShininess { get; set; } = 32.0f;

        /// <summary>
        /// Show performance stats
        /// </summary>
        public bool ShowPerformanceStats { get; set; } = true;

        /// <summary>
        /// Vertical sync enabled
        /// </summary>
        public bool VsyncEnabled { get; set; } = true;

        /// <summary>
        /// Target frame rate
        /// </summary>
        public int TargetFrameRate { get; set; } = 60;

        /// <summary>
        /// Window width
        /// </summary>
        public int WindowWidth { get; set; } = 1280;

        /// <summary>
        /// Window height
        /// </summary>
        public int WindowHeight { get; set; } = 800;

        /// <summary>
        /// Window maximized
        /// </summary>
        public bool WindowMaximized { get; set; } = false;

        /// <summary>
        /// Theme (Light, Dark, System)
        /// </summary>
        public string Theme { get; set; } = "Dark";

        public AppSettings()
        {
        }

        /// <summary>
        /// Add a recent file
        /// </summary>
        public void AddRecentFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            // Remove if already exists
            RecentFiles.Remove(filePath);

            // Add to beginning
            RecentFiles.Insert(0, filePath);

            // Keep only MaxRecentFiles
            while (RecentFiles.Count > MaxRecentFiles)
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }

        /// <summary>
        /// Get recent files
        /// </summary>
        public List<string> GetRecentFiles()
        {
            return new List<string>(RecentFiles);
        }

        /// <summary>
        /// Clear recent files
        /// </summary>
        public void ClearRecentFiles()
        {
            RecentFiles.Clear();
        }

        /// <summary>
        /// Reset to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            DefaultExportDirectory =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            RecentFiles.Clear();
            AutoSaveEnabled = true;
            AutoSaveInterval = 300;
            GridSize = 1.0f;
            GridSubdivisions = 10;
            CameraFieldOfView = 45.0f;
            DefaultShininess = 32.0f;
            ShowPerformanceStats = true;
            VsyncEnabled = true;
            TargetFrameRate = 300;
            WindowWidth = 1920;
            WindowHeight = 1080;
            WindowMaximized = false;
            Theme = "Dark";
        }

        public override string ToString()
        {
            return $"AppSettings: " +
                   $"GridSize={GridSize} GridSubs={GridSubdivisions} " +
                   $"FOV={CameraFieldOfView} AutoSave={AutoSaveEnabled} " +
                   $"VSync={VsyncEnabled} FPS={TargetFrameRate} " +
                   $"Theme={Theme}";
        }
    }
}
