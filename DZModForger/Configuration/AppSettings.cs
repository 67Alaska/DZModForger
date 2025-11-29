using System;
using System.Diagnostics;
using Windows.Storage;

namespace DZModForger.Configuration
{
    public static class AppSettings
    {
        private static readonly ApplicationDataContainer LocalSettings =
            ApplicationData.Current.LocalSettings;

        private const string SettingsContainerName = "DZModForger";

        public static int TargetFPS
        {
            get => GetInt("TargetFPS", 120);
            set => SetInt("TargetFPS", value);
        }

        public static string LastOpenedPath
        {
            get => GetString("LastOpenedPath", "");
            set => SetString("LastOpenedPath", value);
        }

        public static bool WindowMaximized
        {
            get => GetBool("WindowMaximized", false);
            set => SetBool("WindowMaximized", value);
        }

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

        private static string GetString(string key, string defaultValue)
        {
            var value = GetSetting(key, defaultValue);
            return value as string ?? defaultValue;
        }

        private static void SetString(string key, string value)
        {
            SetSetting(key, value ?? "");
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APPSETTINGS] Exception setting '{key}': {ex.Message}");
            }
        }

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

        public static void ResetToDefaults()
        {
            try
            {
                var container = GetSettingsContainer();
                container.Values.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APPSETTINGS] Exception in ResetToDefaults: {ex.Message}");
            }
        }
    }

    public static class AppConstants
    {
        public const string AppVersion = "1.0.0";
        public const string FBXSDKVersion = "2020.3.7";
        public const int DefaultTargetFPS = 120;
        public const int MinWindowWidth = 1024;
        public const int MinWindowHeight = 768;
    }
}
