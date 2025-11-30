using System;
using System.Runtime.InteropServices;

namespace DZModForger.Interop
{
    public static class DX12InteropHelper
    {
        private const string DllName = "DX12Engine.dll";
        private static IntPtr _viewportInstance = IntPtr.Zero;

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateViewport(IntPtr hwnd, int width, int height);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyViewport(IntPtr instance);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ResizeViewport(IntPtr instance, int width, int height);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RenderViewport(IntPtr instance);

        public static bool Initialize(IntPtr hwnd, uint width, uint height)
        {
            if (_viewportInstance != IntPtr.Zero) return true;

            _viewportInstance = CreateViewport(hwnd, (int)width, (int)height);
            return _viewportInstance != IntPtr.Zero;
        }

        public static void Render()
        {
            if (_viewportInstance != IntPtr.Zero)
                RenderViewport(_viewportInstance);
        }

        public static void Resize(uint width, uint height)
        {
            if (_viewportInstance != IntPtr.Zero)
                ResizeViewport(_viewportInstance, (int)width, (int)height);
        }

        public static void Shutdown()
        {
            if (_viewportInstance != IntPtr.Zero)
            {
                DestroyViewport(_viewportInstance);
                _viewportInstance = IntPtr.Zero;
            }
        }

        public static string GetEngineVersion() => "DX12 Flat API v1.0";
    }
}
