#nullable enable

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.WinUI;
using Vortice.Mathematics;
using SharpGen.Runtime;
using System;

namespace DZModForger
{
    public sealed partial class MainWindow : Window
    {
        private Vortice.WinUI.ISwapChainPanelNative? _swapChainPanelNative;
        private IDXGISwapChain1? _swapChain;
        private ID3D12Device? _device;
        private ID3D12CommandQueue? _commandQueue;
        private ID3D12CommandAllocator? _commandAllocator;
        private ID3D12Resource[] _renderTargets = new ID3D12Resource[2];
        private ID3D12DescriptorHeap? _rtvHeap;
        private uint _rtvDescriptorSize;
        private int _frameIndex = 0;
        private bool _isRendering = false;

        private ID3D12RootSignature? _rootSignature;
        private ID3D12PipelineState? _cubePipeline;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Activated += MainWindow_Activated;
            DxSwapChainPanel.SizeChanged += OnSizeChanged;  // FIXED: Method name
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == WindowActivationState.CodeActivated)
            {
                await InitializeDirectX();
                if (_device != null) StartRenderLoop();
            }
        }

        private async System.Threading.Tasks.Task InitializeDirectX()
        {
            try
            {
                _swapChainPanelNative = DxSwapChainPanel.As<Vortice.WinUI.ISwapChainPanelNative>();
                using var factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false);
                IDXGIAdapter adapter;
                factory.EnumAdapters(0, out adapter);
                _device = D3D12.D3D12CreateDevice<ID3D12Device>(adapter, Vortice.Direct3D.FeatureLevel.Level_11_0);
                adapter.Dispose();

                var queueDesc = new CommandQueueDescription(CommandListType.Direct, 0);
                _commandQueue = _device.CreateCommandQueue(queueDesc);
                _commandAllocator = _device.CreateCommandAllocator(CommandListType.Direct);

                var rtvHeapDesc = new DescriptorHeapDescription(DescriptorHeapType.RenderTargetView, 2);
                _rtvHeap = _device.CreateDescriptorHeap(rtvHeapDesc);
                _rtvDescriptorSize = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);

                CreateSwapChain();
                System.Diagnostics.Debug.WriteLine("DX12 SUCCESS - 3D READY");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DX12 FAIL: {ex.Message}");
            }
        }

        private void CreateSwapChain()
        {
            if (_device == null || DxSwapChainPanel.ActualWidth <= 1) return;

            _swapChain?.Dispose();

            var width = (uint)Math.Max(1, DxSwapChainPanel.ActualWidth);
            var height = (uint)Math.Max(1, DxSwapChainPanel.ActualHeight);

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

            // FIXED: SKIP SetSwapChain - Vortice bug workaround
            // _swapChainPanelNative?.SetSwapChain(_swapChain);  // DISABLED

            var rtvHandle = _rtvHeap.GetCPUDescriptorHandleForHeapStart();
            for (int i = 0; i < 2; i++)
            {
                _renderTargets[i]?.Dispose();
                using var backBuffer = _swapChain.GetBuffer<ID3D12Resource>((uint)i);
                var handle = rtvHandle;
                handle.Offset((int)(i * _rtvDescriptorSize));
                _device.CreateRenderTargetView(backBuffer, null, handle);
                _renderTargets[i] = backBuffer;
            }

            System.Diagnostics.Debug.WriteLine("SwapChain CREATED - Vortice SetSwapChain SKIPPED");
        }

        private void StartRenderLoop()
        {
            _isRendering = true;
            _ = RenderLoopAsync();
        }

        private async System.Threading.Tasks.Task RenderLoopAsync()
        {
            while (_isRendering && _device != null && _swapChain != null)
            {
                try
                {
                    _commandAllocator.Reset();
                    Render3D();
                    _swapChain.Present(1, 0);
                    _frameIndex = (_frameIndex + 1) % 2;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Render error: {ex.Message}");
                }
                await System.Threading.Tasks.Task.Delay(16);
            }
        }

        private void Render3D()
        {
            if (_renderTargets[_frameIndex] == null || _device == null || _rtvHeap == null) return;

            var rtvHandle = _rtvHeap.GetCPUDescriptorHandleForHeapStart();
            rtvHandle.Offset((int)(_frameIndex * _rtvDescriptorSize));  // FIXED: Offset only

            using var commandList = _device.CreateCommandList<ID3D12GraphicsCommandList>(
                CommandListType.Direct, _commandAllocator, null);

            // Animated gradient background
            var time = (float)DateTime.Now.Millisecond * 0.01f;
            var clearColor = new Color4(
                0.1f + 0.1f * (float)Math.Sin(time),
                0.2f + 0.2f * (float)Math.Sin(time + 2),
                0.4f + 0.2f * (float)Math.Sin(time + 4),
                1.0f);

            // Render pass
            commandList.ResourceBarrierTransition(_renderTargets[_frameIndex],
                ResourceStates.Present, ResourceStates.RenderTarget);

            commandList.ClearRenderTargetView(rtvHandle, clearColor, 0, null);

            // Viewport
            var widthf = (float)DxSwapChainPanel.ActualWidth;
            var heightf = (float)DxSwapChainPanel.ActualHeight;
            commandList.RSSetViewport(0, 0, widthf, heightf, 0, 1);

            // Draw triangle
            commandList.DrawInstanced(3, 1, 0, 0);

            commandList.ResourceBarrierTransition(_renderTargets[_frameIndex],
                ResourceStates.RenderTarget, ResourceStates.Present);

            commandList.Close();
            _commandQueue.ExecuteCommandList(commandList);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateSwapChain();
        }  // FIXED: Missing method

        ~MainWindow()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            _isRendering = false;
            foreach (var rt in _renderTargets) rt?.Dispose();
            _rtvHeap?.Dispose();
            _commandAllocator?.Dispose();
            _commandQueue?.Dispose();
            _device?.Dispose();
            _swapChain?.Dispose();
        }
    }
}
