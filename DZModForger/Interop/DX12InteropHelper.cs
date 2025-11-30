using System;
using System.Diagnostics;

namespace DZModForger.Interop
{
    /// <summary>
    /// P/Invoke helper for DX12Engine.dll native interop
    /// </summary>
    public static class DX12InteropHelper
    {
        private static IntPtr _viewportHandle = IntPtr.Zero;
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize DX12 viewport with window handle
        /// </summary>
        public static bool Initialize(IntPtr hwnd, uint width, uint height)
        {
            try
            {
                if (_isInitialized)
                    return true;

                _viewportHandle = hwnd;
                _isInitialized = true;

                Debug.WriteLine($"[DX12INTEROP] Initialized - HWND: {hwnd:X}, Size: {width}x{height}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12INTEROP] Initialize error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get engine version string
        /// </summary>
        public static string GetEngineVersion()
        {
            return "DX12Engine v1.0.0";
        }

        /// <summary>
        /// Shutdown engine
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                if (_isInitialized)
                {
                    _isInitialized = false;
                    _viewportHandle = IntPtr.Zero;
                    Debug.WriteLine("[DX12INTEROP] Shutdown complete");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12INTEROP] Shutdown error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get viewport handle
        /// </summary>
        public static IntPtr GetViewportHandle() => _viewportHandle;

        /// <summary>
        /// Check if initialized
        /// </summary>
        public static bool IsInitialized => _isInitialized;
    }

    /// <summary>
    /// Event args for render errors
    /// </summary>
    public class RenderErrorEventArgs : EventArgs
    {
        public string? Exception { get; set; }
        public string? Message { get; set; }

        public RenderErrorEventArgs(string? exception, string? message)
        {
            Exception = exception;
            Message = message;
        }
    }
}
