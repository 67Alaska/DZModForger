using System;
using System.Diagnostics;

namespace DZModForger.Interop
{
    public class DX12ViewportHost
    {
        private ID3D12Viewport? _viewport;
        private IntPtr _hModule = IntPtr.Zero;

        public event EventHandler<RenderErrorEventArgs>? RenderError;

        public DX12ViewportHost()
        {
            try
            {
                Debug.WriteLine("[DX12ViewportHost] Loading DX12Engine");
                _hModule = DX12InteropHelper.LoadDX12Engine();
                Debug.WriteLine("[DX12ViewportHost] DX12Engine loaded");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12ViewportHost] Error: {ex.Message}");
                RenderError?.Invoke(this, new RenderErrorEventArgs { Exception = ex, FilePath = "DX12Engine.dll" });
            }
        }

        public void Initialize(IntPtr hwnd, uint width, uint height)
        {
            try
            {
                Debug.WriteLine($"[DX12ViewportHost] Initialize: {width}x{height}");
                Guid clsid = new Guid("B2C3D4E5-F6A7-5B9C-8D9E-7F6E5D4C3B2A");
                object? obj = Activator.CreateInstance(Type.GetTypeFromCLSID(clsid));
                _viewport = obj as ID3D12Viewport;
                if (_viewport == null) throw new InvalidOperationException("Failed to create viewport");
                _viewport.Initialize(hwnd, width, height);
                Debug.WriteLine("[DX12ViewportHost] Initialize OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12ViewportHost] Initialize error: {ex.Message}");
                RenderError?.Invoke(this, new RenderErrorEventArgs { Exception = ex, FilePath = "Initialize" });
            }
        }

        public void Shutdown()
        {
            try
            {
                _viewport?.Shutdown();
                if (_viewport != null) DX12InteropHelper.ReleaseCOMObject(_viewport);
                _viewport = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12ViewportHost] Shutdown error: {ex.Message}");
            }
        }

        public void Render() => _viewport?.Render();
        public void Resize(uint width, uint height) => _viewport?.Resize(width, height);
        public void SetCamera(float r, float theta, float phi, float tx, float ty, float tz)
            => _viewport?.SetCamera(r, theta, phi, tx, ty, tz);
    }
}
