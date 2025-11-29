using System;
using System.Diagnostics;
using Windows.Storage;

namespace DZModForger.Configuration
{
    /// <summary>
    /// Application settings and preferences management
    /// Persists user preferences to Windows.Storage.ApplicationData
    /// </summary>
    public static class AppSettings
    {
        private static readonly ApplicationDataContainer LocalSettings =
            ApplicationData.Current.LocalSettings;

        private const string SettingsContainerName = "DZModForger";

        // ==================== VIEWPORT SETTINGS ====================

        public static bool GridVisible
        {
            get => GetBool("GridVisible", true);
            set => SetBool("GridVisible", value);
        }

        public static bool AxesVisible
        {
            get => GetBool("AxesVisible", true);
            set => SetBool("AxesVisible", value);
        }

        public static bool BoundsVisible
        {
            get => GetBool("BoundsVisible", false);
            set => SetBool("BoundsVisible", value);
        }

        public static int ShadingMode
        {
            get => GetInt("ShadingMode", 1); // 0=Wire, 1=Solid, 2=Material, 3=Render
            set => SetInt("ShadingMode", value);
        }

        public static int ViewMode
        {
            get => GetInt("ViewMode", 0); // 0=Front, 1=Top, 2=Side, 3=Iso
            set => SetInt("ViewMode", value);
        }

        // ==================== CAMERA SETTINGS ====================

        public static float CameraDistance
        {
            get => GetFloat("CameraDistance", 5.0f);
            set => SetFloat("CameraDistance", value);
        }

        public static float CameraYaw
        {
            get => GetFloat("CameraYaw", 0.0f);
            set => SetFloat("CameraYaw", value);
        }

        public static float CameraPitch
        {
            get => GetFloat("CameraPitch", 30.0f);
            set => SetFloat("CameraPitch", value);
        }

        // ==================== RENDERING SETTINGS ====================

        public static int TargetFPS
        {
            get => GetInt("TargetFPS", 120);
            set => SetInt("TargetFPS", Math.Max(30, Math.Min(240, value)));
        }

        public static bool VSync
        {
            get => GetBool("VSync", false);
            set => SetBool("VSync", value);
        }

        public static int AnisotropicFiltering
        {
            get => GetInt("AnisotropicFiltering", 16);
            set => SetInt("AnisotropicFiltering", value);
        }

        public static bool MSAA
        {
            get => GetBool("MSAA", true);
            set => SetBool("MSAA", value);
        }

        // ==================== MODEL LOADING ====================

        public static string LastOpenedPath
        {
            get => GetString("LastOpenedPath", "");
            set => SetString("LastOpenedPath", value);
        }

        public static int RecentFilesCount
        {
            get => GetInt("RecentFilesCount", 10);
            set => SetInt("RecentFilesCount", Math.Max(5, Math.Min(20, value)));
        }

        public static bool AutoLoadLastModel
        {
            get => GetBool("AutoLoadLastModel", false);
            set => SetBool("AutoLoadLastModel", value);
        }

        public static bool AutoCalculateNormals
        {
            get => GetBool("AutoCalculateNormals", true);
            set => SetBool("AutoCalculateNormals", value);
        }

        // ==================== UI SETTINGS ====================

        public static int WindowWidth
        {
            get => GetInt("WindowWidth", 1920);
            set => SetInt("WindowWidth", value);
        }

        public static int WindowHeight
        {
            get => GetInt("WindowHeight", 1080);
            set => SetInt("WindowHeight", value);
        }

        public static bool WindowMaximized
        {
            get => GetBool("WindowMaximized", false);
            set => SetBool("WindowMaximized", value);
        }

        public static string Theme
        {
            get => GetString("Theme", "Dark");
            set => SetString("Theme", value);
        }

        public static int SidebarWidth
        {
            get => GetInt("SidebarWidth", 280);
            set => SetInt("SidebarWidth", Math.Max(200, Math.Min(600, value)));
        }

        public static bool ShowStatusBar
        {
            get => GetBool("ShowStatusBar", true);
            set => SetBool("ShowStatusBar", value);
        }

        // ==================== EXPORT SETTINGS ====================

        public static string LastExportPath
        {
            get => GetString("LastExportPath", "");
            set => SetString("LastExportPath", value);
        }

        public static int DefaultExportFormat
        {
            get => GetInt("DefaultExportFormat", 0); // 0=FBX, 1=OBJ, 2=DAE
            set => SetInt("DefaultExportFormat", value);
        }

        public static bool ExportNormals
        {
            get => GetBool("ExportNormals", true);
            set => SetBool("ExportNormals", value);
        }

        public static bool ExportUVs
        {
            get => GetBool("ExportUVs", true);
            set => SetBool("ExportUVs", value);
        }

        // ==================== PERFORMANCE SETTINGS ====================

        public static int LODLevels
        {
            get => GetInt("LODLevels", 3);
            set => SetInt("LODLevels", Math.Max(1, Math.Min(5, value)));
        }

        public static bool CullingEnabled
        {
            get => GetBool("CullingEnabled", true);
            set => SetBool("CullingEnabled", value);
        }

        public static int MaxDrawCalls
        {
            get => GetInt("MaxDrawCalls", 1000);
            set => SetInt("MaxDrawCalls", value);
        }

        // ==================== DEBUG SETTINGS ====================

        public static bool DebugMode
        {
            get => GetBool("DebugMode", false);
            set => SetBool("DebugMode", value);
        }

        public static bool ShowDebugInfo
        {
            get => GetBool("ShowDebugInfo", false);
            set => SetBool("ShowDebugInfo", value);
        }

        public static bool EnableLogging
        {
            get => GetBool("EnableLogging", true);
            set => SetBool("EnableLogging", value);
        }

        public static string FBXSDKPath
        {
            get => GetString("FBXSDKPath", @"C:\Program Files\Autodesk\FBX\FBX SDK\2020.3.7");
            set => SetString("FBXSDKPath", value);
        }

        public static string FBXSDKVersion
        {
            get => GetString("FBXSDKVersion", "2020.3.7");
            set => SetString("FBXSDKVersion", value);
        }

        public static bool FBXLoggingEnabled
        {
            get => GetBool("FBXLoggingEnabled", true);
            set => SetBool("FBXLoggingEnabled", value);
        }

        // ==================== HELPER METHODS ====================

        private static ApplicationDataContainer GetSettingsContainer()
        {
            try
            {
                if (!LocalSettings.Containers.ContainsKey(SettingsContainerName))
                {
                    LocalSettings.CreateContainer(SettingsContainerName,
                        ApplicationDataCreateDisposition.Always);
                }

                return LocalSettings.Containers[SettingsContainerName];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APPSETTINGS] Exception getting settings container: {ex.Message}");
                return LocalSettings;
            }
        }

        private static object GetSetting(string key, object defaultValue)
        {
            try
            {
                var container = GetSettingsContainer();
                if (container.Values.ContainsKey(key))
                {
                    return container.Values[key];
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APPSETTINGS] Exception getting setting '{key}': {ex.Message}");
                return defaultValue;
            }
        }

        private static void SetSetting(string key, object value)
        {
            try
            {
                var container = GetSettingsContainer();
                container.Values[key] = value;
                Debug.WriteLine($"[APPSETTINGS] Setting '{key}' = {value}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APPSETTINGS] Exception setting '{key}': {ex.Message}");
            }
        }

        // ==================== TYPE-SPECIFIC GETTERS/SETTERS ====================

        private static bool GetBool(string key, bool defaultValue)
        {
            var value = GetSetting(key, defaultValue);
            return value is bool ? (bool)value : defaultValue;
        }

        private static void SetBool(string key, bool value)
        {
            SetSetting(key, value);
        }

        private static int GetInt(string key, int defaultValue)
        {
            var value = GetSetting(key, defaultValue);
            if (value is int intValue)
                return intValue;
            if (int.TryParse(value?.ToString(), out int parsed))
                return parsed;
            return defaultValue;
        }

        private static void SetInt(string key, int value)
        {
            SetSetting(key, value);
        }

        private static float GetFloat(string key, float defaultValue)
        {
            var value = GetSetting(key, defaultValue);
            if (value is float floatValue)
                return floatValue;
            if (float.TryParse(value?.ToString(), out float parsed))
                return parsed;
            return defaultValue;
        }

        private static void SetFloat(string key, float value)
        {
            SetSetting(key, value);
        }

        private static string GetString(string key, string defaultValue)
        {
            var value = GetSetting(key, defaultValue);
            return value as string ?? defaultValue;
        }

        private static void SetString(string key, string value)
        {
            SetSetting(key, value ?? "");
        }

        // ==================== UTILITY METHODS ====================

        /// <summary>
        /// Resets all settings to defaults
        /// </summary>
        public static void ResetToDefaults()
        {
            try
            {
                Debug.WriteLine("[APPSETTINGS] Resetting to defaults");

                var container = GetSettingsContainer();
                container.Values.Clear();

                Debug.WriteLine("[APPSETTINGS] Reset complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APPSETTINGS] Exception in ResetToDefaults: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports settings to JSON-formatted string
        /// </summary>
        public static string ExportSettings()
        {
            try
            {
                var container = GetSettingsContainer();
                var settings = new System.Collections.Generic.Dictionary<string, object>();

                foreach (var kvp in container.Values)
                {
                    settings[kvp.Key] = kvp.Value;
                }

                return Newtonsoft.Json.JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APPSETTINGS] Exception in ExportSettings: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// Imports settings from JSON-formatted string
        /// </summary>
        public static void ImportSettings(string json)
        {
            try
            {
                Debug.WriteLine("[APPSETTINGS] Importing settings from JSON");

                var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<
                    System.Collections.Generic.Dictionary<string, object>>(json);

                var container = GetSettingsContainer();

                foreach (var kvp in settings)
                {
                    container.Values[kvp.Key] = kvp.Value;
                }

                Debug.WriteLine("[APPSETTINGS] Import complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APPSETTINGS] Exception in ImportSettings: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Application constants
    /// </summary>
    public static class AppConstants
    {
        // Version
        public const string AppVersion = "1.0.0";
        public const string FBXSDKVersion = "2020.3.7";
        public const string FBXSDKPath = @"C:\Program Files\Autodesk\FBX\FBX SDK\2020.3.7";
        public const string AppName = "DZModForger";
        public const string AppDescription = "Professional 3D Model Editor for DayZ";

        // Limits
        public const int MaxRecentFiles = 20;
        public const int MinWindowWidth = 1024;
        public const int MinWindowHeight = 768;
        public const float MinCameraDistance = 0.1f;
        public const float MaxCameraDistance = 1000.0f;
        public const int MaxDrawCalls = 10000;

        // Defaults
        public const int DefaultViewportWidth = 1600;
        public const int DefaultViewportHeight = 980;
        public const float DefaultCameraFOV = 45.0f;
        public const float DefaultNearPlane = 0.1f;
        public const float DefaultFarPlane = 1000.0f;

        // File Extensions
        public const string FBXExtension = ".fbx";
        public const string OBJExtension = ".obj";
        public const string DAEExtension = ".dae";
        public const string ProjectExtension = ".dzmod";

        // Performance
        public const int DefaultTargetFPS = 120;
        public const bool DefaultVSync = false;
        public const int DefaultAnisotropicFiltering = 16;
    }
}
