using DZModForger.Services;
using System;
using System.Runtime.InteropServices;

namespace DZModForger.Interop
{
    public class DX12ViewportHost
    {
        private ID3D12Viewport? _viewport;

        public event EventHandler<RenderErrorEventArgs>? RenderError;

        public DX12ViewportHost()
        {
            _viewport = null;
        }

        /// <summary>
        /// Initialize the viewport with window handle and dimensions
        /// </summary>
        public void Initialize(IntPtr hwnd, uint width, uint height)
        {
            try
            {
                if (_viewport != null)
                {
                    throw new InvalidOperationException("Viewport already initialized");
                }

                // Create the viewport using the interop helper
                _viewport = DX12InteropHelper.CreateViewport();

                if (_viewport == null)
                {
                    throw new InvalidOperationException("Failed to create viewport");
                }

                // Initialize the viewport
                int hr = _viewport.Initialize(hwnd, width, height);
                if (hr != 0) // S_OK = 0
                {
                    throw new COMException($"Initialize failed with HRESULT: 0x{hr:X8}", hr);
                }
            }
            catch (Exception ex)
            {
                RenderError?.Invoke(this, new RenderErrorEventArgs(ex, $"Initialize failed: {ex.Message}"));
                throw;
            }
        }

        /// <summary>
        /// Load a model file (FBX, OBJ, etc.)
        /// </summary>
        public void LoadModel(string filePath)
        {
            try
            {
                if (_viewport == null)
                {
                    throw new InvalidOperationException("Viewport not initialized");
                }

                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentNullException(nameof(filePath));
                }

                // Determine file type and load accordingly
                if (filePath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                {
                    int hr = _viewport.LoadFBX(filePath);
                    if (hr != 0)
                        throw new COMException($"LoadFBX failed: 0x{hr:X8}", hr);
                }
                else if (filePath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
                {
                    int hr = _viewport.LoadOBJ(filePath);
                    if (hr != 0)
                        throw new COMException($"LoadOBJ failed: 0x{hr:X8}", hr);
                }
                else
                {
                    throw new NotSupportedException($"File format not supported: {filePath}");
                }
            }
            catch (Exception ex)
            {
                RenderError?.Invoke(this, new RenderErrorEventArgs(ex, $"LoadModel failed: {ex.Message}"));
                throw;
            }
        }

        /// <summary>
        /// Set camera position and target
        /// </summary>
        public void SetCamera(float radius, float theta, float phi, float targetX, float targetY, float targetZ)
        {
            try
            {
                if (_viewport == null)
                {
                    throw new InvalidOperationException("Viewport not initialized");
                }

                int hr = _viewport.SetCamera(radius, theta, phi, targetX, targetY, targetZ);
                if (hr != 0)
                {
                    throw new COMException($"SetCamera failed: 0x{hr:X8}", hr);
                }
            }
            catch (Exception ex)
            {
                RenderError?.Invoke(this, new RenderErrorEventArgs(ex, $"SetCamera failed: {ex.Message}"));
                throw;
            }
        }

        /// <summary>
        /// Get the current frame rate
        /// </summary>
        public float GetFrameRate()
        {
            try
            {
                if (_viewport == null)
                    return 0.0f;

                float fps = 0.0f;
                int hr = _viewport.GetFrameRate(out fps);
                if (hr != 0)
                    return 0.0f;

                return fps;
            }
            catch
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// Get the vertex count of the loaded model
        /// </summary>
        public uint GetVertexCount()
        {
            try
            {
                if (_viewport == null)
                    return 0;

                uint count = 0;
                int hr = _viewport.GetVertexCount(out count);
                if (hr != 0)
                    return 0;

                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the triangle count of the loaded model
        /// </summary>
        public uint GetTriangleCount()
        {
            try
            {
                if (_viewport == null)
                    return 0;

                uint count = 0;
                int hr = _viewport.GetTriangleCount(out count);
                if (hr != 0)
                    return 0;

                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Render frame
        /// </summary>
        public void Render()
        {
            try
            {
                if (_viewport != null)
                {
                    int hr = _viewport.Render();
                    if (hr != 0)
                    {
                        throw new COMException($"Render failed: 0x{hr:X8}", hr);
                    }
                }
            }
            catch (Exception ex)
            {
                RenderError?.Invoke(this, new RenderErrorEventArgs(ex, $"Render failed: {ex.Message}"));
            }
        }

        /// <summary>
        /// Shutdown the viewport
        /// </summary>
        public void Shutdown()
        {
            try
            {
                if (_viewport != null)
                {
                    _viewport.Shutdown();
                    Marshal.ReleaseComObject(_viewport);
                    _viewport = null;
                }
            }
            catch (Exception ex)
            {
                RenderError?.Invoke(this, new RenderErrorEventArgs(ex, $"Shutdown failed: {ex.Message}"));
            }
        }
    }
}
