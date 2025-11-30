using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DZModForger.Interop
{
    public static class DX12InteropHelper
    {
        public const string DX12EngineDLL = "DX12Engine.dll";

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        private const uint CLSCTX_INPROC_SERVER = 1;

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

                Debug.WriteLine("[DX12INTEROP] DX12Engine.dll loaded successfully");
                return hModule;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12INTEROP] Exception: {ex.Message}");
                throw;
            }
        }

        public static void ReleaseCOMObject(object? obj)
        {
            try
            {
                if (obj != null)
                {
                    Marshal.ReleaseComObject(obj);
                    Debug.WriteLine("[DX12INTEROP] COM object released");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12INTEROP] Exception in ReleaseCOMObject: {ex.Message}");
            }
        }
    }

    [ComImport]
    [Guid("A1B2C3D4-E5F6-4A5B-9C8D-7E6F5A4B3C2D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ID3D12Viewport
    {
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

    public class RenderErrorEventArgs : EventArgs
    {
        public Exception? Exception { get; set; }
        public string? FilePath { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
