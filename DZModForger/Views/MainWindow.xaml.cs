using DZModForger.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WinUI;

namespace DZModForger
{
    public sealed partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private ViewportControl _viewportControl;

        // DirectX resources (from previous implementation)
        private Vortice.WinUI.ISwapChainPanelNative? _swapChainPanelNative;
        private IDXGISwapChain1? _swapChain;
        private ID3D12Device? _device;
        private ID3D12CommandQueue? _commandQueue;
        private ID3D12CommandAllocator? _commandAllocator;
        private ID3D12Resource[] _renderTargets = new ID3D12Resource;
        private ID3D12DescriptorHeap? _rtvHeap;
        private uint _rtvDescriptorSize;
        private int _frameIndex = 0;
        private bool _isRendering = false;

        // Synchronization
        private ID3D12Fence? _fence;
        private ulong _fenceValue = 0;
        private System.Threading.ManualResetEvent? _fenceEvent;

        // Services
        private FbxImportService? _fbxImportService;
        private FbxExportService? _fbxExportService;
        private ModelLibraryService? _modelLibrary;

        public MainWindow()
        {
            this.InitializeComponent();

            // Initialize view model
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            // Get viewport control reference
            _viewportControl = this.FindName("ViewportPanel") as ViewportControl;

            // Wire up events
            this.Activated += MainWindow_Activated;
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == WindowActivationState.CodeActivated)
            {
                // Initialize services
                _fbxImportService = new FbxImportService();
                _fbxExportService = new FbxExportService();
                _modelLibrary = new ModelLibraryService();

                // Initialize DirectX
                await InitializeDirectX();

                if (_device != null)
                {
                    StartRenderLoop();
                    UpdateStatusMessage("Ready to import FBX files");
                }
            }
        }

        private async System.Threading.Tasks.Task InitializeDirectX()
        {
            try
            {
                if (_viewportControl == null)
                    return;

                _swapChainPanelNative = _viewportControl.SwapChainPanel.As<Vortice.WinUI.ISwapChainPanelNative>();

                using var factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false);
                factory.EnumAdapters(0, out var adapter);

                _device = D3D12.D3D12CreateDevice<ID3D12Device>(adapter, Vortice.Direct3D.FeatureLevel.Level_11_0);
                adapter.Dispose();

                var queueDesc = new CommandQueueDescription(CommandListType.Direct, 0);
                _commandQueue = _device.CreateCommandQueue(queueDesc);
                _commandAllocator = _device.CreateCommandAllocator(CommandListType.Direct);

                var rtvHeapDesc = new DescriptorHeapDescription(DescriptorHeapType.RenderTargetView, 2);
                _rtvHeap = _device.CreateDescriptorHeap(rtvHeapDesc);
                _rtvDescriptorSize = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);

                _fence = _device.CreateFence(0, FenceFlags.None);
                _fenceEvent = new System.Threading.ManualResetEvent(false);

                CreateSwapChain();
                Debug.WriteLine("DirectX 12 initialization complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DirectX initialization failed: {ex.Message}");
                throw;
            }
        }

        private void CreateSwapChain()
        {
            if (_device == null || _viewportControl?.SwapChainPanel.ActualWidth <= 1)
                return;

            _swapChain?.Dispose();
            foreach (var rt in _renderTargets)
                rt?.Dispose();

            var width = (uint)Math.Max(8, _viewportControl.SwapChainPanel.ActualWidth);
            var height = (uint)Math.Max(8, _viewportControl.SwapChainPanel.ActualHeight);

            var swapChainDesc = new SwapChainDescription1
            {
                Width = width,
                Height = height,
                Format = Format.R8G8B8A8_UNorm,
                BufferCount = 2,
                SampleDescription = new SampleDescription(1, 0),
                BufferUsage = Usage.RenderTargetOutput,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore
            };

            using var factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false);
            _swapChain = factory.CreateSwapChainForComposition(_commandQueue, swapChainDesc);

            var rtvHandle = _rtvHeap!.GetCPUDescriptorHandleForHeapStart();
            for (int i = 0; i < 2; i++)
            {
                var backBuffer = _swapChain.GetBuffer<ID3D12Resource>((uint)i);
                var handle = rtvHandle;
                handle.Offset((int)(i * _rtvDescriptorSize));
                _device!.CreateRenderTargetView(backBuffer, null, handle);
                _renderTargets[i] = backBuffer;
            }

            Debug.WriteLine($"SwapChain created: {width}x{height}");
        }

        private void StartRenderLoop()
        {
            _isRendering = true;
            _ = RenderLoopAsync();
        }

        private async System.Threading.Tasks.Task RenderLoopAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            int frameCount = 0;

            while (_isRendering && _device != null && _swapChain != null)
            {
                try
                {
                    _commandAllocator!.Reset();
                    Render3D();
                    _swapChain.Present(1, 0);

                    _frameIndex = (_frameIndex + 1) % 2;

                    // Update FPS every second
                    frameCount++;
                    if (stopwatch.ElapsedMilliseconds >= 1000)
                    {
                        _viewModel.FpsValue = frameCount;
                        frameCount = 0;
                        stopwatch.Restart();
                    }

                    await System.Threading.Tasks.Task.Delay(1);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Render error: {ex.Message}");
                }
            }
        }

        private void Render3D()
        {
            if (_renderTargets[_frameIndex] == null || _device == null || _rtvHeap == null)
                return;

            var rtvHandle = _rtvHeap.GetCPUDescriptorHandleForHeapStart();
            rtvHandle.Offset((int)(_frameIndex * _rtvDescriptorSize));

            using var commandList = _device.CreateCommandList<ID3D12GraphicsCommandList>(
                CommandListType.Direct, _commandAllocator, null);

            var clearColor = new Color4(0.1f, 0.15f, 0.2f, 1.0f);

            commandList.ResourceBarrierTransition(
                _renderTargets[_frameIndex],
                ResourceStates.Present,
                ResourceStates.RenderTarget
            );

            commandList.ClearRenderTargetView(rtvHandle, clearColor);

            var widthf = (float)_viewportControl.SwapChainPanel.ActualWidth;
            var heightf = (float)_viewportControl.SwapChainPanel.ActualHeight;
            commandList.RSSetViewport(0, 0, widthf, heightf, 0, 1);
            commandList.RSSetScissorRect(0, 0, (int)widthf, (int)heightf);
            commandList.OMSetRenderTargets(1, rtvHandle);

            // Render grid (when implemented)
            // GridRenderer.Render(commandList);

            // Render loaded models (when implemented)
            // foreach (var model in _modelLibrary.GetAllModels())
            //     RenderModel(commandList, model);

            commandList.ResourceBarrierTransition(
                _renderTargets[_frameIndex],
                ResourceStates.RenderTarget,
                ResourceStates.Present
            );

            commandList.Close();
            _commandQueue!.ExecuteCommandList(commandList);
        }

        private void UpdateStatusMessage(string message)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _viewModel.StatusMessage = message;
            });
        }

        ~MainWindow()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            _isRendering = false;

            _fence?.Dispose();
            _fenceEvent?.Dispose();

            foreach (var rt in _renderTargets)
                rt?.Dispose();

            _rtvHeap?.Dispose();
            _commandAllocator?.Dispose();
            _commandQueue?.Dispose();
            _device?.Dispose();
            _swapChain?.Dispose();
        }
    }
}
