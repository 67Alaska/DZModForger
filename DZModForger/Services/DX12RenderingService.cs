using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DZModForger.Services
{
    /// <summary>
    /// C# COM Interop wrapper for DirectX 12 rendering engine
    /// Communicates with native DX12Engine.dll via COM interfaces
    /// </summary>
    public class DX12RenderingService : IDisposable
    {
        // COM Interface GUIDs
        private static readonly Guid IID_IDX12Viewport = new Guid("12345678-1234-1234-1234-123456789ABC");
        private static readonly Guid CLSID_DX12Viewport = new Guid("87654321-4321-4321-4321-CBACBACBACBA");

        // Native COM interface
        [ComImport]
        [Guid("12345678-1234-1234-1234-123456789ABC")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDX12Viewport
        {
            [PreserveSig]
            int Initialize(IntPtr hwnd, uint width, uint height);

            [PreserveSig]
            int LoadModelFromFile([MarshalAs(UnmanagedType.LPWStr)] string filePath);

            [PreserveSig]
            int UpdateModelData(IntPtr vertices, uint vertexCount, IntPtr indices, uint indexCount);

            [PreserveSig]
            int Render();

            [PreserveSig]
            int Present();

            [PreserveSig]
            int SetCameraOrbital(float yaw, float pitch, float distance);

            [PreserveSig]
            int SetCameraPan(float panX, float panY);

            [PreserveSig]
            int SetCameraZoom(float zoomFactor);

            [PreserveSig]
            int ResetCamera();

            [PreserveSig]
            int SetShadingMode(int mode);

            [PreserveSig]
            int ShowGrid(bool visible);

            [PreserveSig]
            int ShowAxes(bool visible);

            [PreserveSig]
            int ShowBounds(bool visible);

            [PreserveSig]
            int SetMaterialMetallic(float metallic);

            [PreserveSig]
            int SetMaterialRoughness(float roughness);

            [PreserveSig]
            int SetMaterialAO(float ao);

            [PreserveSig]
            int AddLight(IntPtr light);

            [PreserveSig]
            int ClearLights();

            [PreserveSig]
            int GetFrameRate(out float fps);

            [PreserveSig]
            int GetGPUMemoryUsage(out ulong bytes);

            [PreserveSig]
            int GetLastError([MarshalAs(UnmanagedType.LPWStr)] out string error);

            [PreserveSig]
            int Shutdown();
        }

        // Vertex structure matching C++
        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public float X, Y, Z;
            public float NX, NY, NZ;
            public float U, V;
        }

        // Material structure
        [StructLayout(LayoutKind.Sequential)]
        public struct Material
        {
            public float DR, DG, DB, DA;
            public float SR, SG, SB, SA;
            public float Metallic;
            public float Roughness;
            public float AO;
            public float Padding;
        }

        // Light structure
        [StructLayout(LayoutKind.Sequential)]
        public struct Light
        {
            public float PX, PY, PZ, PW;
            public float DX, DY, DZ, DW;
            public float CR, CG, CB, CA;
            public float Intensity;
            public float Range;
            public int Type;
            public float Padding;
        }

        private IDX12Viewport _dx12Viewport;
        private bool _isInitialized = false;
        private bool _isDisposed = false;

        public DX12RenderingService()
        {
            Debug.WriteLine("[DX12RENDERING] Initializing COM wrapper");

            try
            {
                // Create COM object (DX12Engine.dll)
                Type dx12Type = Type.GetTypeFromCLSID(CLSID_DX12Viewport);
                if (dx12Type == null)
                {
                    throw new Exception("DX12Engine COM class not found. Ensure DX12Engine.dll is registered.");
                }

                _dx12Viewport = (IDX12Viewport)Activator.CreateInstance(dx12Type);
                if (_dx12Viewport == null)
                {
                    throw new Exception("Failed to create DX12Engine COM object");
                }

                Debug.WriteLine("[DX12RENDERING] ✅ COM wrapper initialized");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] ❌ Initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initializes the DirectX 12 viewport
        /// </summary>
        public bool Initialize(IntPtr hwnd, uint width, uint height)
        {
            try
            {
                Debug.WriteLine($"[DX12RENDERING] Initializing viewport: {width}x{height}");

                int result = _dx12Viewport.Initialize(hwnd, width, height);
                if (result != 0)
                {
                    string error;
                    _dx12Viewport.GetLastError(out error);
                    Debug.WriteLine($"[DX12RENDERING] ❌ Initialize failed: {error}");
                    return false;
                }

                _isInitialized = true;
                Debug.WriteLine("[DX12RENDERING] ✅ Viewport initialized");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in Initialize: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads a model from file
        /// </summary>
        public bool LoadModelFromFile(string filePath)
        {
            try
            {
                if (!_isInitialized) return false;

                Debug.WriteLine($"[DX12RENDERING] Loading model: {filePath}");

                int result = _dx12Viewport.LoadModelFromFile(filePath);
                if (result != 0)
                {
                    string error;
                    _dx12Viewport.GetLastError(out error);
                    Debug.WriteLine($"[DX12RENDERING] ❌ Load failed: {error}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in LoadModelFromFile: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates model data
        /// </summary>
        public bool UpdateModelData(Vertex[] vertices, uint[] indices)
        {
            try
            {
                if (!_isInitialized || vertices == null || indices == null) return false;

                Debug.WriteLine($"[DX12RENDERING] Updating model: {vertices.Length} vertices, {indices.Length} indices");

                // Pin arrays in memory
                var vertexHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
                var indexHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);

                try
                {
                    int result = _dx12Viewport.UpdateModelData(
                        vertexHandle.AddrOfPinnedObject(),
                        (uint)vertices.Length,
                        indexHandle.AddrOfPinnedObject(),
                        (uint)indices.Length
                    );

                    if (result != 0)
                    {
                        string error;
                        _dx12Viewport.GetLastError(out error);
                        Debug.WriteLine($"[DX12RENDERING] ❌ Update failed: {error}");
                        return false;
                    }

                    return true;
                }
                finally
                {
                    vertexHandle.Free();
                    indexHandle.Free();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in UpdateModelData: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Renders the scene
        /// </summary>
        public bool Render()
        {
            try
            {
                if (!_isInitialized) return false;

                int result = _dx12Viewport.Render();
                if (result != 0) return false;

                result = _dx12Viewport.Present();
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in Render: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets camera orbital parameters
        /// </summary>
        public bool SetCameraOrbital(float yaw, float pitch, float distance)
        {
            try
            {
                if (!_isInitialized) return false;
                int result = _dx12Viewport.SetCameraOrbital(yaw, pitch, distance);
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in SetCameraOrbital: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets camera pan
        /// </summary>
        public bool SetCameraPan(float panX, float panY)
        {
            try
            {
                if (!_isInitialized) return false;
                int result = _dx12Viewport.SetCameraPan(panX, panY);
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in SetCameraPan: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets camera zoom
        /// </summary>
        public bool SetCameraZoom(float zoomFactor)
        {
            try
            {
                if (!_isInitialized) return false;
                int result = _dx12Viewport.SetCameraZoom(zoomFactor);
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in SetCameraZoom: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Resets camera to default
        /// </summary>
        public bool ResetCamera()
        {
            try
            {
                if (!_isInitialized) return false;
                int result = _dx12Viewport.ResetCamera();
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in ResetCamera: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets shading mode
        /// </summary>
        public bool SetShadingMode(int mode)
        {
            try
            {
                if (!_isInitialized) return false;
                int result = _dx12Viewport.SetShadingMode(mode);
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in SetShadingMode: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Shows or hides grid
        /// </summary>
        public bool ShowGrid(bool visible)
        {
            try
            {
                if (!_isInitialized) return false;
                int result = _dx12Viewport.ShowGrid(visible);
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in ShowGrid: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Shows or hides axes
        /// </summary>
        public bool ShowAxes(bool visible)
        {
            try
            {
                if (!_isInitialized) return false;
                int result = _dx12Viewport.ShowAxes(visible);
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in ShowAxes: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Shows or hides bounds
        /// </summary>
        public bool ShowBounds(bool visible)
        {
            try
            {
                if (!_isInitialized) return false;
                int result = _dx12Viewport.ShowBounds(visible);
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in ShowBounds: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets current frame rate
        /// </summary>
        public float GetFrameRate()
        {
            try
            {
                if (!_isInitialized) return 0.0f;
                float fps;
                int result = _dx12Viewport.GetFrameRate(out fps);
                return result == 0 ? fps : 0.0f;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in GetFrameRate: {ex.Message}");
                return 0.0f;
            }
        }

        /// <summary>
        /// Gets GPU memory usage
        /// </summary>
        public ulong GetGPUMemoryUsage()
        {
            try
            {
                if (!_isInitialized) return 0;
                ulong bytes;
                int result = _dx12Viewport.GetGPUMemoryUsage(out bytes);
                return result == 0 ? bytes : 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in GetGPUMemoryUsage: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets last error message
        /// </summary>
        public string GetLastError()
        {
            try
            {
                if (_dx12Viewport == null) return "Not initialized";
                string error;
                _dx12Viewport.GetLastError(out error);
                return error ?? "Unknown error";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in GetLastError: {ex.Message}");
                return ex.Message;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                Debug.WriteLine("[DX12RENDERING] Disposing");

                if (_dx12Viewport != null && _isInitialized)
                {
                    _dx12Viewport.Shutdown();
                }

                if (_dx12Viewport != null)
                {
                    Marshal.ReleaseComObject(_dx12Viewport);
                    _dx12Viewport = null;
                }

                _isInitialized = false;
                _isDisposed = true;

                Debug.WriteLine("[DX12RENDERING] ✅ Disposed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12RENDERING] Exception in Dispose: {ex.Message}");
            }
        }

        ~DX12RenderingService()
        {
            Dispose();
        }

        /// <summary>
        /// Gets whether service is initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;
    }
}
