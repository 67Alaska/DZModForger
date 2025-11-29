using System;
using System.Diagnostics;
using System.IO;

namespace DZModForger.Configuration
{
    /// <summary>
    /// FBX SDK Configuration and Validation
    /// Verifies Autodesk FBX SDK 2020.3.7 installation and compatibility
    /// </summary>
    public static class FBXSDKConfiguration
    {
        public const string RequiredVersion = "2020.3.7";
        public const string InstallPath = @"C:\Program Files\Autodesk\FBX\FBX SDK\2020.3.7";
        public const string LibPath = @"lib\vs2015\x64\release\libfbxsdk.lib";
        public const string BinPath = @"bin\x64\release";
        public const string IncludePath = @"include";

        /// <summary>
        /// Validates FBX SDK installation
        /// </summary>
        /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
        public static (bool IsValid, string ErrorMessage) ValidateInstallation()
        {
            try
            {
                Debug.WriteLine("[FBX-CONFIG] Validating FBX SDK installation...");

                // Check if SDK path exists
                if (!Directory.Exists(InstallPath))
                {
                    var errorMsg = $"FBX SDK not found at: {InstallPath}";
                    Debug.WriteLine($"[FBX-CONFIG] ❌ {errorMsg}");
                    return (false, errorMsg);
                }

                Debug.WriteLine($"[FBX-CONFIG] ✓ SDK path exists: {InstallPath}");

                // Check lib path
                var libFullPath = Path.Combine(InstallPath, LibPath);
                if (!File.Exists(libFullPath))
                {
                    var errorMsg = $"FBX SDK library not found at: {libFullPath}";
                    Debug.WriteLine($"[FBX-CONFIG] ❌ {errorMsg}");
                    return (false, errorMsg);
                }

                Debug.WriteLine($"[FBX-CONFIG] ✓ SDK library found: {libFullPath}");

                // Check include path
                var includeFullPath = Path.Combine(InstallPath, IncludePath);
                if (!Directory.Exists(includeFullPath))
                {
                    var errorMsg = $"FBX SDK include directory not found at: {includeFullPath}";
                    Debug.WriteLine($"[FBX-CONFIG] ❌ {errorMsg}");
                    return (false, errorMsg);
                }

                Debug.WriteLine($"[FBX-CONFIG] ✓ SDK include directory found: {includeFullPath}");

                // Check bin path for DLLs
                var binFullPath = Path.Combine(InstallPath, BinPath);
                if (!Directory.Exists(binFullPath))
                {
                    var errorMsg = $"FBX SDK bin directory not found at: {binFullPath}";
                    Debug.WriteLine($"[FBX-CONFIG] ❌ {errorMsg}");
                    return (false, errorMsg);
                }

                var dllCount = Directory.GetFiles(binFullPath, "*.dll").Length;
                Debug.WriteLine($"[FBX-CONFIG] ✓ SDK bin directory found with {dllCount} DLL files");

                // All checks passed
                Debug.WriteLine("[FBX-CONFIG] ✅ FBX SDK validation passed!");
                return (true, "");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Exception validating FBX SDK: {ex.Message}";
                Debug.WriteLine($"[FBX-CONFIG] ❌ {errorMsg}");
                return (false, errorMsg);
            }
        }

        /// <summary>
        /// Gets detailed SDK information
        /// </summary>
        public static SDKInfo GetSDKInfo()
        {
            try
            {
                var (isValid, error) = ValidateInstallation();

                var info = new SDKInfo
                {
                    Version = RequiredVersion,
                    InstallPath = InstallPath,
                    IsInstalled = isValid,
                    ErrorMessage = error,
                    LibPath = Path.Combine(InstallPath, LibPath),
                    BinPath = Path.Combine(InstallPath, BinPath),
                    IncludePath = Path.Combine(InstallPath, IncludePath)
                };

                if (isValid)
                {
                    // Count DLLs
                    var dlls = Directory.GetFiles(info.BinPath, "*.dll");
                    info.DLLCount = dlls.Length;

                    // Count header files
                    var headers = Directory.GetFiles(info.IncludePath, "*.h", SearchOption.AllDirectories);
                    info.HeaderCount = headers.Length;
                }

                return info;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FBX-CONFIG] Exception in GetSDKInfo: {ex.Message}");
                return new SDKInfo
                {
                    Version = RequiredVersion,
                    InstallPath = InstallPath,
                    IsInstalled = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Logs FBX SDK configuration details
        /// </summary>
        public static void LogConfiguration()
        {
            Debug.WriteLine("========================================");
            Debug.WriteLine("FBX SDK Configuration Details");
            Debug.WriteLine("========================================");
            Debug.WriteLine($"Required Version: {RequiredVersion}");
            Debug.WriteLine($"Install Path: {InstallPath}");
            Debug.WriteLine($"Lib Path: {Path.Combine(InstallPath, LibPath)}");
            Debug.WriteLine($"Bin Path: {Path.Combine(InstallPath, BinPath)}");
            Debug.WriteLine($"Include Path: {Path.Combine(InstallPath, IncludePath)}");

            var info = GetSDKInfo();
            Debug.WriteLine("----------------------------------------");
            Debug.WriteLine($"Installation Status: {(info.IsInstalled ? "✅ INSTALLED" : "❌ NOT INSTALLED")}");

            if (info.IsInstalled)
            {
                Debug.WriteLine($"DLL Count: {info.DLLCount}");
                Debug.WriteLine($"Header Count: {info.HeaderCount}");
            }
            else
            {
                Debug.WriteLine($"Error: {info.ErrorMessage}");
            }

            Debug.WriteLine("========================================");
        }
    }

    /// <summary>
    /// FBX SDK Information
    /// </summary>
    public class SDKInfo
    {
        public string Version { get; set; }
        public string InstallPath { get; set; }
        public string LibPath { get; set; }
        public string BinPath { get; set; }
        public string IncludePath { get; set; }
        public bool IsInstalled { get; set; }
        public string ErrorMessage { get; set; }
        public int DLLCount { get; set; }
        public int HeaderCount { get; set; }

        public override string ToString()
        {
            return $"FBX SDK {Version} - {(IsInstalled ? "✅ Installed" : "❌ Not Installed")} at {InstallPath}";
        }
    }
}
