using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation;

namespace DZModForger.Interop
{
    /// <summary>
    /// XAML container for DirectX 12 viewport
    /// Hosts ID3D12Viewport COM interface from DX12Engine.dll
    /// Integrates D3D rendering into WinUI 3 window hierarchy
    /// </summary>
    public sealed class DX12ViewportHost : SwapChainPanel
    {
        private D3D12ViewportWrapper _viewportWrapper;
        private IntPtr _nativeWindow = IntPtr.Zero;
        private bool _isInitialized = false;
        private float _desiredFrameRate = 120.0f;

        public event EventHandler<RenderErrorEventArgs> RenderError;

        public DX12ViewportHost()
        {
            Debug.WriteLine("[DX12HOST] DX12ViewportHost created");

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            this.SizeChanged += OnSizeChanged;
        }

        /// <summary>
        /// Initializes DirectX 12 viewport
        /// </summary>
        public async void InitializeViewport()
        {
            try
            {
                if (_isInitialized)
                {
                    Debug.WriteLine("[DX12HOST] ⚠️  Viewport already initialized");
                    return;
                }

                Debug.WriteLine("[DX12HOST] Initializing DirectX 12 viewport");

                // Get native window handle
                _nativeWindow = GetWindowHandle();
                if (_nativeWindow == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to get native window handle");
                }

                Debug.WriteLine($"[DX12HOST] Window handle: 0x{_nativeWindow:X}");

                // Get swap chain panel native interface
                var nativePanel = Marshal.GetIUnknownForObject(this);
                Debug.WriteLine("[DX12HOST] ✓ Got SwapChainPanel native interface");

                // Create COM viewport instance
                var comObject = DX12InteropHelper.CreateD3D12Viewport();
                if (comObject == null)
                {
                    throw new InvalidOperationException("Failed to create COM viewport instance");
                }

                _viewportWrapper = new D3D12ViewportWrapper((ID3D12Viewport)comObject);
                _viewportWrapper.RenderError += OnViewportRenderError;

                // Get viewport dimensions
                uint width = (uint)this.ActualWidth;
                uint height = (uint)this.ActualHeight;

                if (width == 0 || height == 0)
                {
                    width = 1920;
                    height = 1080;
                }

                Debug.WriteLine($"[DX12HOST] Viewport dimensions: {width}x{height}");

                // Initialize viewport
                _viewportWrapper.Initialize(_nativeWindow, width, height);

                _isInitialized = true;
                Debug.WriteLine("[DX12HOST] ✅ Viewport initialized successfully");

                // Start render loop
                StartRenderLoop();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12HOST] ❌ Exception in InitializeViewport: {ex.Message}");
                Debug.WriteLine($"[DX12HOST] Stack trace: {ex.StackTrace}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex });
            }
        }

        /// <summary>
        /// Loads model from file path
        /// </summary>
        public void LoadModel(string filePath)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("Viewport not initialized");
                }

                Debug.WriteLine($"[DX12HOST] Loading model: {filePath}");

                string extension = System.IO.Path.GetExtension(filePath).ToLower();

                switch (extension)
                {
                    case ".fbx":
                        _viewportWrapper.LoadFBX(filePath);
                        break;
                    case ".obj":
                        _viewportWrapper.LoadOBJ(filePath);
                        break;
                    default:
                        throw new NotSupportedException($"File format not supported: {extension}");
                }

                Debug.WriteLine("[DX12HOST] ✓ Model loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12HOST] ❌ Exception in LoadModel: {ex.Message}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex, FilePath = filePath });
            }
        }

        /// <summary>
        /// Sets camera position and orientation
        /// </summary>
        public void SetCamera(float distance, float yaw, float pitch, float targetX, float targetY, float targetZ)
        {
            try
            {
                if (!_isInitialized)
                {
                    return;
                }

                _viewportWrapper.SetCamera(distance, yaw, pitch, targetX, targetY, targetZ);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12HOST] ❌ Exception in SetCamera: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets current frame rate
        /// </summary>
        public float GetFrameRate()
        {
            if (!_isInitialized || _viewportWrapper == null)
            {
                return 0.0f;
            }

            return _viewportWrapper.GetFrameRate();
        }

        /// <summary>
        /// Gets vertex count of current model
        /// </summary>
        public uint GetVertexCount()
        {
            if (!_isInitialized || _viewportWrapper == null)
            {
                return 0;
            }

            return _viewportWrapper.GetVertexCount();
        }

        /// <summary>
        /// Gets triangle count of current model
        /// </summary>
        public uint GetTriangleCount()
        {
            if (!_isInitialized || _viewportWrapper == null)
            {
                return 0;
            }

            return _viewportWrapper.GetTriangleCount();
        }

        /// <summary>
        /// Resizes viewport
        /// </summary>
        public void ResizeViewport(uint width, uint height)
        {
            try
            {
                if (!_isInitialized || _viewportWrapper == null)
                {
                    return;
                }

                Debug.WriteLine($"[DX12HOST] Resizing viewport to {width}x{height}");
                _viewportWrapper.Resize(width, height);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12HOST] ❌ Exception in ResizeViewport: {ex.Message}");
            }
        }

        // ==================== EVENT HANDLERS ====================

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[DX12HOST] OnLoaded event");
            InitializeViewport();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[DX12HOST] OnUnloaded event");
            Shutdown();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine($"[DX12HOST] Size changed: {e.NewSize.Width}x{e.NewSize.Height}");

            if (_isInitialized && e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                ResizeViewport((uint)e.NewSize.Width, (uint)e.NewSize.Height);
            }
        }

        private void OnViewportRenderError(object sender, RenderErrorEventArgs e)
        {
            Debug.WriteLine($"[DX12HOST] Viewport render error: {e.Exception?.Message}");
            OnRenderError(e);
        }

        protected virtual void OnRenderError(RenderErrorEventArgs e)
        {
            RenderError?.Invoke(this, e);
        }

        // ==================== RENDERING LOOP ====================

        private async void StartRenderLoop()
        {
            try
            {
                Debug.WriteLine("[DX12HOST] Starting render loop");

                while (_isInitialized)
                {
                    // Render frame
                    _viewportWrapper.Render();
                    _viewportWrapper.Present();

                    // Frame rate limiting
                    int frameTimeMs = (int)(1000.0f / _desiredFrameRate);
                    await Task.Delay(frameTimeMs);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12HOST] ❌ Exception in render loop: {ex.Message}");
                OnRenderError(new RenderErrorEventArgs { Exception = ex });
            }
        }

        // ==================== NATIVE INTEROP ====================

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetActiveWindow();

        /// <summary>
        /// Gets native window handle for XAML control
        /// </summary>
        private IntPtr GetWindowHandle()
        {
            try
            {
                // For WinUI 3, get the window handle from the XamlRoot
                if (this.XamlRoot != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this.XamlRoot.Content as Window);
                    return hwnd;
                }

                // Fallback: get active window
                return GetActiveWindow();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12HOST] Exception getting window handle: {ex.Message}");
                throw;
            }
        }

        // ==================== CLEANUP ====================

        public void Shutdown()
        {
            try
            {
                Debug.WriteLine("[DX12HOST] Shutting down viewport");

                _isInitialized = false;

                if (_viewportWrapper != null)
                {
                    _viewportWrapper.RenderError -= OnViewportRenderError;
                    _viewportWrapper.Dispose();
                    _viewportWrapper = null;
                }

                Debug.WriteLine("[DX12HOST] ✓ Shutdown complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DX12HOST] Exception in Shutdown: {ex.Message}");
            }
        }

        ~DX12ViewportHost()
        {
            Shutdown();
        }
    }
}
