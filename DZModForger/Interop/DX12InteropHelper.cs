using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DZModForger.Interop
{
    /// <summary>
    /// COM Interop helper for DX12Engine.dll communication
    /// Provides marshaling and interface management
    /// </summary>
    public static class DX12InteropHelper
    {
        public const string DX12EngineDLL = "DX12Engine.dll";

        // COM GUIDs for DX12Engine interfaces
        public const string IID_ID3D12Viewport = "A1B2C3D4-E5F6-4A5B-9C8D-7E6F5A4B3C2D";
        public const string CLSID_DX12Viewport = "B2C3D4E5-F6A7-5B9C-8D9E-7F6G5H4I3J2K";

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("ole32.dll")]
        private static extern int CoCreateInstance(
            ref Guid rclsid,
            IntPtr pUnkOuter,
            uint dwClsContext,
            ref Guid riid,
            out IntPtr ppv);

        private const uint CLSCTX_INPROC_SERVER = 1;
        private const uint CLSCTX_LOCAL_SERVER = 4;

        /// <summary>
        /// Loads DX12Engine.dll from application directory
        /// </summary>
        public static IntPtr LoadDX12Engine()
        {
            try
            {
                Debug.WriteLine("[DX12INTEROP] Loading DX12Engine.dll");

                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string dllPath = System.IO.Path.Combine(appPath, DX12EngineDLL);

                Debug.WriteLine($"[DX12INTEROP] DLL path: {dllPath}");

                if (!System.IO.File.Exists(dllPath))
                {
                    throw new DllNotFoundException($"DX12Engine.dll not found at: {dllPath}");
                }

                IntPtr hModule = LoadLibrary(dllPath);
                if (hModule == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new DllNotFoundException($"Failed to load DX12Engine.dll. Error: {error}");
                }

                Debug.WriteLine($"[DX12INTEROP] ✓ DX12Engine.dll loaded successfully");
                return hModule;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12INTEROP] ❌ Exception in LoadDX12Engine: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates COM instance of ID3D12Viewport
        /// </summary>
        public static object CreateD3D12Viewport()
        {
            try
            {
                Debug.WriteLine("[DX12INTEROP] Creating ID3D12Viewport COM instance");

                Guid clsid = Guid.Parse(CLSID_DX12Viewport);
                Guid iid = Guid.Parse(IID_ID3D12Viewport);

                int hr = CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX_INPROC_SERVER, ref iid, out IntPtr ppv);

                if (hr != 0)
                {
                    throw new COMException($"CoCreateInstance failed with HRESULT: 0x{hr:X8}");
                }

                if (ppv == IntPtr.Zero)
                {
                    throw new COMException("COM object pointer is null");
                }

                object comObject = Marshal.GetObjectForIUnknown(ppv);
                Marshal.Release(ppv);

                Debug.WriteLine("[DX12INTEROP] ✓ ID3D12Viewport COM instance created successfully");
                return comObject;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12INTEROP] ❌ Exception in CreateD3D12Viewport: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Releases COM object
        /// </summary>
        public static void ReleaseCOMObject(object obj)
        {
            try
            {
                if (obj != null)
                {
                    Marshal.ReleaseComObject(obj);
                    Debug.WriteLine("[DX12INTEROP] ✓ COM object released");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12INTEROP] Exception in ReleaseCOMObject: {ex.Message}");
            }
        }

        /// <summary>
        /// Unloads DX12Engine.dll
        /// </summary>
        public static void UnloadDX12Engine(IntPtr hModule)
        {
            try
            {
                if (hModule != IntPtr.Zero)
                {
                    if (FreeLibrary(hModule))
                    {
                        Debug.WriteLine("[DX12INTEROP] ✓ DX12Engine.dll unloaded");
                    }
                    else
                    {
                        int error = Marshal.GetLastWin32Error();
                        Debug.WriteLine($"[DX12INTEROP] Warning: Failed to unload DX12Engine.dll. Error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12INTEROP] Exception in UnloadDX12Engine: {ex.Message}");
            }
        }
    }

    // ==================== COM INTERFACE DEFINITIONS ====================

    /// <summary>
    /// COM interface for DirectX 12 Viewport
    /// Maps to ID3D12Viewport in DX12Engine.dll
    /// </summary>
    [ComImport]
    [Guid("A1B2C3D4-E5F6-4A5B-9C8D-7E6F5A4B3C2D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID3D12Viewport
    {
        // Initialization
        [PreserveSig]
        int Initialize(IntPtr hwnd, uint width, uint height);

        [PreserveSig]
        int Shutdown();

        // Rendering
        [PreserveSig]
        int Render();

        [PreserveSig]
        int Present();

        // Viewport Control
        [PreserveSig]
        int Resize(uint width, uint height);

        [PreserveSig]
        int SetCamera(float radius, float theta, float phi, float targetX, float targetY, float targetZ);

        // Model Loading
        [PreserveSig]
        int LoadFBX([MarshalAs(UnmanagedType.LPStr)] string filePath);

        [PreserveSig]
        int LoadOBJ([MarshalAs(UnmanagedType.LPStr)] string filePath);

        // Statistics
        [PreserveSig]
        int GetFrameRate(out float fps);

        [PreserveSig]
        int GetVertexCount(out uint count);

        [PreserveSig]
        int GetTriangleCount(out uint count);
    }

    /// <summary>
    /// Wrapper for ID3D12Viewport COM interface
    /// Provides managed access to DX12Engine functionality
    /// </summary>
    public class D3D12ViewportWrapper : IDisposable
    {
        private ID3D12Viewport _viewport;
        private bool _disposed = false;
        private IntPtr _hwnd = IntPtr.Zero;

        public event EventHandler<RenderErrorEventArgs> RenderError;

        public D3D12ViewportWrapper(ID3D12Viewport viewport)
        {
            if (viewport == null)
                throw new ArgumentNullException(nameof(viewport));

            _viewport = viewport;
            Debug.WriteLine("[D3D12WRAPPER] ID3D12Viewport wrapper created");
        }

        /// <summary>
        /// Initializes the viewport with window handle
        /// </summary>
        public void Initialize(IntPtr hwnd, uint width, uint height)
        {
            try
            {
                Debug.WriteLine($"[D3D12WRAPPER] Initializing viewport: {width}x{height}");

                _hwnd = hwnd;
                int hr = _viewport.Initialize(hwnd, width, height);

                if (hr != 0)
                {
                    throw new COMException($"Initialize failed with HRESULT: 0x{hr:X8}");
                }

                Debug.WriteLine("[D3D12WRAPPER] ✓ Viewport initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in Initialize: {ex.Message}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex });
                throw;
            }
        }

        /// <summary>
        /// Renders current frame
        /// </summary>
        public void Render()
        {
            try
            {
                int hr = _viewport.Render();
                if (hr != 0)
                {
                    throw new COMException($"Render failed with HRESULT: 0x{hr:X8}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in Render: {ex.Message}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex });
            }
        }

        /// <summary>
        /// Presents frame to screen
        /// </summary>
        public void Present()
        {
            try
            {
                int hr = _viewport.Present();
                if (hr != 0)
                {
                    throw new COMException($"Present failed with HRESULT: 0x{hr:X8}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in Present: {ex.Message}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex });
            }
        }

        /// <summary>
        /// Resizes viewport
        /// </summary>
        public void Resize(uint width, uint height)
        {
            try
            {
                Debug.WriteLine($"[D3D12WRAPPER] Resizing to {width}x{height}");

                int hr = _viewport.Resize(width, height);
                if (hr != 0)
                {
                    throw new COMException($"Resize failed with HRESULT: 0x{hr:X8}");
                }

                Debug.WriteLine("[D3D12WRAPPER] ✓ Resized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in Resize: {ex.Message}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex });
            }
        }

        /// <summary>
        /// Sets camera position and orientation
        /// </summary>
        public void SetCamera(float radius, float theta, float phi, float targetX, float targetY, float targetZ)
        {
            try
            {
                int hr = _viewport.SetCamera(radius, theta, phi, targetX, targetY, targetZ);
                if (hr != 0)
                {
                    throw new COMException($"SetCamera failed with HRESULT: 0x{hr:X8}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in SetCamera: {ex.Message}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex });
            }
        }

        /// <summary>
        /// Loads FBX model
        /// </summary>
        public void LoadFBX(string filePath)
        {
            try
            {
                Debug.WriteLine($"[D3D12WRAPPER] Loading FBX: {filePath}");

                int hr = _viewport.LoadFBX(filePath);
                if (hr != 0)
                {
                    throw new COMException($"LoadFBX failed with HRESULT: 0x{hr:X8}");
                }

                Debug.WriteLine("[D3D12WRAPPER] ✓ FBX loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in LoadFBX: {ex.Message}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex, FilePath = filePath });
                throw;
            }
        }

        /// <summary>
        /// Loads OBJ model
        /// </summary>
        public void LoadOBJ(string filePath)
        {
            try
            {
                Debug.WriteLine($"[D3D12WRAPPER] Loading OBJ: {filePath}");

                int hr = _viewport.LoadOBJ(filePath);
                if (hr != 0)
                {
                    throw new COMException($"LoadOBJ failed with HRESULT: 0x{hr:X8}");
                }

                Debug.WriteLine("[D3D12WRAPPER] ✓ OBJ loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in LoadOBJ: {ex.Message}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex, FilePath = filePath });
                throw;
            }
        }

        /// <summary>
        /// Gets current frame rate
        /// </summary>
        public float GetFrameRate()
        {
            try
            {
                int hr = _viewport.GetFrameRate(out float fps);
                if (hr != 0)
                {
                    throw new COMException($"GetFrameRate failed with HRESULT: 0x{hr:X8}");
                }
                return fps;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in GetFrameRate: {ex.Message}");
                return 0.0f;
            }
        }

        /// <summary>
        /// Gets vertex count of loaded model
        /// </summary>
        public uint GetVertexCount()
        {
            try
            {
                int hr = _viewport.GetVertexCount(out uint count);
                if (hr != 0)
                {
                    throw new COMException($"GetVertexCount failed with HRESULT: 0x{hr:X8}");
                }
                return count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in GetVertexCount: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets triangle count of loaded model
        /// </summary>
        public uint GetTriangleCount()
        {
            try
            {
                int hr = _viewport.GetTriangleCount(out uint count);
                if (hr != 0)
                {
                    throw new COMException($"GetTriangleCount failed with HRESULT: 0x{hr:X8}");
                }
                return count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[D3D12WRAPPER] ❌ Exception in GetTriangleCount: {ex.Message}");
                return 0;
            }
        }

        protected virtual void OnRenderError(RenderErrorEventArgs e)
        {
            RenderError?.Invoke(this, e);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                try
                {
                    Debug.WriteLine("[D3D12WRAPPER] Disposing");

                    if (_viewport != null)
                    {
                        int hr = _viewport.Shutdown();
                        if (hr != 0)
                        {
                            Debug.WriteLine($"[D3D12WRAPPER] Warning: Shutdown failed with 0x{hr:X8}");
                        }

                        Marshal.ReleaseComObject(_viewport);
                        _viewport = null;
                    }

                    Debug.WriteLine("[D3D12WRAPPER] ✓ Disposed successfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[D3D12WRAPPER] Exception in Dispose: {ex.Message}");
                }
            }

            _disposed = true;
        }

        ~D3D12ViewportWrapper()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Event args for rendering errors
    /// </summary>
    public class RenderErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public string FilePath { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
