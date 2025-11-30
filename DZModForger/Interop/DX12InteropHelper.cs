using System;
using System.Runtime.InteropServices;

namespace DZModForger.Services
{
    /// <summary>
    /// COM interface definition for ID3D12Viewport
    /// Must match the C++ interface definition exactly
    /// </summary>
    [ComImport]
    [Guid("12345678-1234-1234-1234-123456789012")] // Must match C++ GUID
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID3D12Viewport
    {
        // IUnknown methods (inherited)
        [PreserveSig]
        int QueryInterface([In] ref Guid riid, [Out] out IntPtr ppvObject);

        [PreserveSig]
        uint AddRef();

        [PreserveSig]
        uint Release();

        // ID3D12Viewport methods
        [PreserveSig]
        int Initialize(IntPtr hwnd, uint width, uint height);

        [PreserveSig]
        int Shutdown();

        [PreserveSig]
        int Render();

        [PreserveSig]
        int Present();

        [PreserveSig]
        int Resize(uint width, uint height);

        [PreserveSig]
        int SetCamera(float radius, float theta, float phi, float targetX, float targetY, float targetZ);

        [PreserveSig]
        int LoadFBX([MarshalAs(UnmanagedType.LPStr)] string filePath);

        [PreserveSig]
        int LoadOBJ([MarshalAs(UnmanagedType.LPStr)] string filePath);

        [PreserveSig]
        int GetFrameRate(out float fps);

        [PreserveSig]
        int GetVertexCount(out uint count);

        [PreserveSig]
        int GetTriangleCount(out uint count);
    }

    /// <summary>
    /// Helper class for interoperability with DX12Engine.dll
    /// Loads the native C++ DLL and provides factory methods for viewport creation
    /// </summary>
    public static class DX12InteropHelper
    {
        private const string DLL_NAME = "DX12Engine.dll";
        private static IntPtr _hModule = IntPtr.Zero;
        private static bool _initialized = false;

        // P/Invoke declarations
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpDllName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetLastError();

        // Unmanaged function delegates
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int CreateD3D12ViewportDelegate(out IntPtr ppViewport);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr GetDX12EngineVersionDelegate();

        /// <summary>
        /// Initializes the DX12 engine by loading the DLL
        /// </summary>
        /// <returns>True if initialization succeeds, false otherwise</returns>
        public static bool Initialize()
        {
            try
            {
                if (_initialized)
                {
                    OutputDebugString("[DX12InteropHelper] ✓ Already initialized");
                    return true;
                }

                OutputDebugString("[DX12InteropHelper] Initializing...");

                // Load the DLL
                _hModule = LoadLibrary(DLL_NAME);
                if (_hModule == IntPtr.Zero)
                {
                    uint errorCode = GetLastError();
                    OutputDebugString($"[DX12InteropHelper] ❌ Failed to load {DLL_NAME}, error code: {errorCode}");
                    return false;
                }

                OutputDebugString($"[DX12InteropHelper] ✓ {DLL_NAME} loaded successfully");

                // Verify the factory function exists
                IntPtr createFunc = GetProcAddress(_hModule, "CreateD3D12Viewport");
                if (createFunc == IntPtr.Zero)
                {
                    OutputDebugString("[DX12InteropHelper] ❌ CreateD3D12Viewport function not found");
                    FreeLibrary(_hModule);
                    _hModule = IntPtr.Zero;
                    return false;
                }

                OutputDebugString("[DX12InteropHelper] ✓ CreateD3D12Viewport function found");

                // Verify the version function exists
                IntPtr versionFunc = GetProcAddress(_hModule, "GetDX12EngineVersion");
                if (versionFunc == IntPtr.Zero)
                {
                    OutputDebugString("[DX12InteropHelper] ⚠ GetDX12EngineVersion function not found (non-critical)");
                }
                else
                {
                    OutputDebugString("[DX12InteropHelper] ✓ GetDX12EngineVersion function found");
                }

                _initialized = true;
                OutputDebugString("[DX12InteropHelper] ✅ Initialization SUCCESSFUL");
                return true;
            }
            catch (Exception ex)
            {
                OutputDebugString($"[DX12InteropHelper] ❌ Exception in Initialize: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates a new ID3D12Viewport instance
        /// </summary>
        /// <returns>ID3D12Viewport COM object, or null if creation fails</returns>
        public static ID3D12Viewport CreateViewport()
        {
            try
            {
                if (!_initialized)
                {
                    OutputDebugString("[DX12InteropHelper] ❌ Not initialized - call Initialize() first");
                    throw new InvalidOperationException("DX12InteropHelper not initialized");
                }

                if (_hModule == IntPtr.Zero)
                {
                    OutputDebugString("[DX12InteropHelper] ❌ DLL module handle is invalid");
                    throw new InvalidOperationException("DLL module not loaded");
                }

                // Get the factory function
                IntPtr createFunc = GetProcAddress(_hModule, "CreateD3D12Viewport");
                if (createFunc == IntPtr.Zero)
                {
                    OutputDebugString("[DX12InteropHelper] ❌ CreateD3D12Viewport not found");
                    throw new EntryPointNotFoundException("CreateD3D12Viewport not found in DX12Engine.dll");
                }

                // Marshal the function pointer to a delegate
                var createDelegate = Marshal.GetDelegateForFunctionPointer<CreateD3D12ViewportDelegate>(createFunc);

                // Call the factory function
                IntPtr pViewport = IntPtr.Zero;
                int hr = createDelegate(out pViewport);

                // Check HRESULT
                if (hr != 0) // S_OK = 0
                {
                    OutputDebugString($"[DX12InteropHelper] ❌ CreateD3D12Viewport failed with HRESULT: 0x{hr:X8}");
                    throw new COMException($"CreateD3D12Viewport failed", hr);
                }

                if (pViewport == IntPtr.Zero)
                {
                    OutputDebugString("[DX12InteropHelper] ❌ CreateD3D12Viewport returned null pointer");
                    throw new InvalidOperationException("CreateD3D12Viewport returned null");
                }

                // Marshal the native pointer to a managed COM object
                ID3D12Viewport viewport = Marshal.GetObjectForIUnknown(pViewport) as ID3D12Viewport;
                if (viewport == null)
                {
                    OutputDebugString("[DX12InteropHelper] ❌ Failed to marshal COM object");
                    Marshal.Release(pViewport);
                    throw new InvalidCastException("Failed to marshal IUnknown to ID3D12Viewport");
                }

                OutputDebugString("[DX12InteropHelper] ✅ Viewport created successfully");
                return viewport;
            }
            catch (Exception ex)
            {
                OutputDebugString($"[DX12InteropHelper] ❌ Exception in CreateViewport: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the DX12 engine version string
        /// </summary>
        /// <returns>Version string, or "Unknown" if retrieval fails</returns>
        public static string GetEngineVersion()
        {
            try
            {
                if (_hModule == IntPtr.Zero)
                {
                    OutputDebugString("[DX12InteropHelper] ⚠ DLL not loaded for version check");
                    return "Unknown";
                }

                IntPtr versionFunc = GetProcAddress(_hModule, "GetDX12EngineVersion");
                if (versionFunc == IntPtr.Zero)
                {
                    OutputDebugString("[DX12InteropHelper] ⚠ GetDX12EngineVersion not found");
                    return "Unknown";
                }

                var versionDelegate = Marshal.GetDelegateForFunctionPointer<GetDX12EngineVersionDelegate>(versionFunc);
                IntPtr pVersion = versionDelegate();

                if (pVersion == IntPtr.Zero)
                {
                    OutputDebugString("[DX12InteropHelper] ⚠ GetDX12EngineVersion returned null");
                    return "Unknown";
                }

                string version = Marshal.PtrToStringAnsi(pVersion);
                OutputDebugString($"[DX12InteropHelper] ✓ Engine Version: {version}");
                return version ?? "Unknown";
            }
            catch (Exception ex)
            {
                OutputDebugString($"[DX12InteropHelper] ⚠ Exception in GetEngineVersion: {ex.Message}");
                return "Unknown";
            }
        }

        /// <summary>
        /// Cleans up and unloads the DLL
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                if (_hModule != IntPtr.Zero)
                {
                    OutputDebugString("[DX12InteropHelper] Shutting down...");

                    if (!FreeLibrary(_hModule))
                    {
                        uint errorCode = GetLastError();
                        OutputDebugString($"[DX12InteropHelper] ⚠ FreeLibrary failed with error code: {errorCode}");
                    }
                    else
                    {
                        OutputDebugString("[DX12InteropHelper] ✓ DLL unloaded successfully");
                    }

                    _hModule = IntPtr.Zero;
                }

                _initialized = false;
            }
            catch (Exception ex)
            {
                OutputDebugString($"[DX12InteropHelper] ⚠ Exception in Shutdown: {ex.Message}");
            }
        }

        /// <summary>
        /// Debug output helper
        /// </summary>
        private static void OutputDebugString(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// Property to check if the interop helper is initialized
        /// </summary>
        public static bool IsInitialized => _initialized && _hModule != IntPtr.Zero;
    }
}
