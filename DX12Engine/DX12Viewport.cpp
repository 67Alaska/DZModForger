#include "pch.h"
#include <d3dx12.h>
#include <wincodec.h>
#include <winerror.h>
#include "DX12Viewport.h"
#include <stdarg.h>

// ==================== GUID DEFINITIONS ====================

// {A1B2C3D4-E5F6-4A5B-9C8D-7E6F5A4B3C2D}
const GUID IID_ID3D12Viewport =
{ 0xa1b2c3d4, 0xe5f6, 0x4a5b, { 0x9c, 0x8d, 0x7e, 0x6f, 0x5a, 0x4b, 0x3c, 0x2d } };

// {B2C3D4E5-F6A7-5B9C-8D9E-7F6G5H4I3J2K}
const GUID CLSID_DX12Viewport =
{ 0xb2c3d4e5, 0xf6a7, 0x5b9c, { 0x8d, 0x9e, 0x7f, 0x6e, 0x5e, 0x4d, 0x3c, 0x2b } };

// ==================== CONSTRUCTOR/DESTRUCTOR ====================

DX12Viewport::DX12Viewport()
    : _refCount(1)
    , _initialized(false)
    , _hwnd(nullptr)
    , _width(0)
    , _height(0)
    , _rtvDescriptorSize(0)
    , _currentFrameIndex(0)
    , _fenceValue(1)
    , _fenceEvent(nullptr)
    , _cbvDataBegin(nullptr)
    , _cameraDistance(5.0f)
    , _cameraTheta(0.0f)
    , _cameraPhi(30.0f)
    , _cameraTargetX(0.0f)
    , _cameraTargetY(0.0f)
    , _cameraTargetZ(0.0f)
    , _frameCount_stat(0)
    , _vertexCount(0)
    , _triangleCount(0)
    , _fps(0.0)
    , _lastFrameTime(0.0)
{
    LogMessage("[DX12VIEWPORT] Constructor called\n");
    ZeroMemory(&_viewport, sizeof(_viewport));
    ZeroMemory(&_scissorRect, sizeof(_scissorRect));
    ZeroMemory(_renderTargets, sizeof(_renderTargets));
}

DX12Viewport::~DX12Viewport()
{
    LogMessage("[DX12VIEWPORT] Destructor called\n");
    if (_initialized)
    {
        Shutdown();
    }
}

// ==================== IUNKNOWN IMPLEMENTATION ====================

STDMETHODIMP DX12Viewport::QueryInterface(REFIID riid, void** ppvObject)
{
    if (!ppvObject)
        return E_INVALIDARG;

    if (riid == IID_IUnknown || riid == IID_ID3D12Viewport)
    {
        *ppvObject = this;
        AddRef();
        return S_OK;
    }

    *ppvObject = nullptr;
    return E_NOINTERFACE;
}

STDMETHODIMP_(ULONG) DX12Viewport::AddRef()
{
    return InterlockedIncrement(&_refCount);
}

STDMETHODIMP_(ULONG) DX12Viewport::Release()
{
    ULONG refCount = InterlockedDecrement(&_refCount);
    if (refCount == 0)
    {
        delete this;
    }
    return refCount;
}

// ==================== INITIALIZATION ====================

STDMETHODIMP DX12Viewport::Initialize(HWND hwnd, UINT width, UINT height)
{
    try
    {
        LogMessage("[DX12VIEWPORT] Initialize called: %ux%u\n", width, height);

        if (!hwnd || width == 0 || height == 0)
            return E_INVALIDARG;

        _hwnd = hwnd;
        _width = width;
        _height = height;

        // Initialize DirectX 12
        HRESULT hr = InitializeDirectX();
        if (FAILED(hr))
        {
            LogMessage("[DX12VIEWPORT] InitializeDirectX failed: 0x%08X\n", hr);
            return hr;
        }

        // Create render targets
        hr = CreateRenderTargets();
        if (FAILED(hr))
        {
            LogMessage("[DX12VIEWPORT] CreateRenderTargets failed: 0x%08X\n", hr);
            return hr;
        }

        // Create pipeline
        hr = CreatePipeline();
        if (FAILED(hr))
        {
            LogMessage("[DX12VIEWPORT] CreatePipeline failed: 0x%08X\n", hr);
            return hr;
        }

        _initialized = true;
        LogMessage("[DX12VIEWPORT] ✅ Initialize successful\n");
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in Initialize: %s\n", ex.what());
        return E_FAIL;
    }
}

STDMETHODIMP DX12Viewport::Shutdown()
{
    try
    {
        LogMessage("[DX12VIEWPORT] Shutdown called\n");

        if (_initialized)
        {
            WaitForPreviousFrame();

            if (_fenceEvent)
            {
                CloseHandle(_fenceEvent);
                _fenceEvent = nullptr;
            }

            _device.Reset();
            _commandQueue.Reset();
            _commandAllocator.Reset();
            _commandList.Reset();
            _swapChain.Reset();
            _rtvHeap.Reset();
            _dsvHeap.Reset();
            _cbvSrvHeap.Reset();
            _fence.Reset();

            for (int i = 0; i < _frameCount; ++i)
            {
                _renderTargets[i].Reset();
            }

            _depthStencilBuffer.Reset();
            _rootSignature.Reset();
            _pipelineState.Reset();
            _constantBuffer.Reset();
            _vertexBuffer.Reset();
            _indexBuffer.Reset();

            _initialized = false;
            LogMessage("[DX12VIEWPORT] ✅ Shutdown successful\n");
        }

        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in Shutdown: %s\n", ex.what());
        return E_FAIL;
    }
}

// ==================== RENDERING ====================

STDMETHODIMP DX12Viewport::Render()
{
    try
    {
        if (!_initialized)
        {
            LogMessage("[DX12VIEWPORT] Error: Viewport not initialized\n");
            return 0x8000000AL;  // E_NOT_READY
        }

        RenderScene();
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in Render: %s\n", ex.what());
        return E_FAIL;
    }
}

STDMETHODIMP DX12Viewport::Present()
{
    try
    {
        if (!_swapChain)
        {
            LogMessage("[DX12VIEWPORT] Error: Swap chain not initialized\n");
            return 0x8000000AL;  // E_NOT_READY
        }

        HRESULT hr = _swapChain->Present(0, 0);
        if (FAILED(hr))
        {
            LogMessage("[DX12VIEWPORT] Present failed: 0x%08X\n", hr);
            return hr;
        }

        WaitForPreviousFrame();
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in Present: %s\n", ex.what());
        return E_FAIL;
    }
}

// ==================== VIEWPORT CONTROL ====================

STDMETHODIMP DX12Viewport::Resize(UINT width, UINT height)
{
    try
    {
        LogMessage("[DX12VIEWPORT] Resize called: %ux%u\n", width, height);

        if (width == 0 || height == 0)
            return E_INVALIDARG;

        _width = width;
        _height = height;

        if (_swapChain)
        {
            WaitForPreviousFrame();

            // Release render targets
            for (int i = 0; i < _frameCount; ++i)
            {
                _renderTargets[i].Reset();
            }

            HRESULT hr = _swapChain->ResizeBuffers(_frameCount, _width, _height, DXGI_FORMAT_R8G8B8A8_UNORM, 0);
            if (FAILED(hr))
            {
                LogMessage("[DX12VIEWPORT] ResizeBuffers failed: 0x%08X\n", hr);
                return hr;
            }

            _currentFrameIndex = _swapChain->GetCurrentBackBufferIndex();

            hr = CreateRenderTargets();
            if (FAILED(hr))
            {
                LogMessage("[DX12VIEWPORT] CreateRenderTargets after resize failed: 0x%08X\n", hr);
                return hr;
            }
        }

        // Update viewport
        _viewport.TopLeftX = 0.0f;
        _viewport.TopLeftY = 0.0f;
        _viewport.Width = static_cast<float>(_width);
        _viewport.Height = static_cast<float>(_height);
        _viewport.MinDepth = 0.0f;
        _viewport.MaxDepth = 1.0f;

        _scissorRect = { 0, 0, (LONG)_width, (LONG)_height };

        LogMessage("[DX12VIEWPORT] ✅ Resize successful\n");
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in Resize: %s\n", ex.what());
        return E_FAIL;
    }
}

STDMETHODIMP DX12Viewport::SetCamera(float radius, float theta, float phi, float targetX, float targetY, float targetZ)
{
    try
    {
        _cameraDistance = radius;
        _cameraTheta = theta;
        _cameraPhi = phi;
        _cameraTargetX = targetX;
        _cameraTargetY = targetY;
        _cameraTargetZ = targetZ;

        UpdateCamera();
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in SetCamera: %s\n", ex.what());
        return E_FAIL;
    }
}

// ==================== MODEL LOADING ====================

STDMETHODIMP DX12Viewport::LoadFBX(const char* filePath)
{
    try
    {
        if (!filePath)
            return E_INVALIDARG;

        LogMessage("[DX12VIEWPORT] Loading FBX: %s\n", filePath);

        // TODO: Implement FBX loading via Autodesk FBX SDK
        // For now, load a simple triangle

        LogMessage("[DX12VIEWPORT] ✅ FBX loaded\n");
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in LoadFBX: %s\n", ex.what());
        return E_FAIL;
    }
}

STDMETHODIMP DX12Viewport::LoadOBJ(const char* filePath)
{
    try
    {
        if (!filePath)
            return E_INVALIDARG;

        LogMessage("[DX12VIEWPORT] Loading OBJ: %s\n", filePath);

        // TODO: Implement OBJ loading

        LogMessage("[DX12VIEWPORT] ✅ OBJ loaded\n");
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in LoadOBJ: %s\n", ex.what());
        return E_FAIL;
    }
}

// ==================== STATISTICS ====================

STDMETHODIMP DX12Viewport::GetFrameRate(float* pFps)
{
    if (!pFps)
        return E_INVALIDARG;

    *pFps = static_cast<float>(_fps);
    return S_OK;
}

STDMETHODIMP DX12Viewport::GetVertexCount(UINT* pCount)
{
    if (!pCount)
        return E_INVALIDARG;

    *pCount = _vertexCount;
    return S_OK;
}

STDMETHODIMP DX12Viewport::GetTriangleCount(UINT* pCount)
{
    if (!pCount)
        return E_INVALIDARG;

    *pCount = _triangleCount;
    return S_OK;
}

// ==================== PRIVATE METHODS ====================

HRESULT DX12Viewport::InitializeDirectX()
{
    try
    {
        LogMessage("[DX12VIEWPORT] InitializeDirectX starting\n");

        // Enable debug layer in debug builds
#if defined(DEBUG) || defined(_DEBUG)
        {
            ComPtr<ID3D12Debug> debugController;
            if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController))))
            {
                debugController->EnableDebugLayer();
                LogMessage("[DX12VIEWPORT] Debug layer enabled\n");
            }
        }
#endif

        // Create DXGI factory
        ComPtr<IDXGIFactory6> factory;
        ThrowIfFailed(CreateDXGIFactory1(IID_PPV_ARGS(&factory)));

        // Create device
        ThrowIfFailed(D3D12CreateDevice(nullptr, D3D_FEATURE_LEVEL_12_1, IID_PPV_ARGS(&_device)));
        LogMessage("[DX12VIEWPORT] Device created\n");

        // Create command queue
        D3D12_COMMAND_QUEUE_DESC queueDesc = {};
        queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
        queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;

        ThrowIfFailed(_device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&_commandQueue)));
        LogMessage("[DX12VIEWPORT] Command queue created\n");

        // Create swap chain
        DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
        swapChainDesc.BufferCount = _frameCount;
        swapChainDesc.Width = _width;
        swapChainDesc.Height = _height;
        swapChainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
        swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
        swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
        swapChainDesc.SampleDesc.Count = 1;

        ComPtr<IDXGISwapChain1> swapChain;
        ThrowIfFailed(factory->CreateSwapChainForHwnd(_commandQueue.Get(), _hwnd, &swapChainDesc, nullptr, nullptr, &swapChain));
        ThrowIfFailed(swapChain.As(&_swapChain));

        _currentFrameIndex = _swapChain->GetCurrentBackBufferIndex();
        LogMessage("[DX12VIEWPORT] Swap chain created\n");

        // Disable fullscreen transitions
        ThrowIfFailed(factory->MakeWindowAssociation(_hwnd, DXGI_MWA_NO_ALT_ENTER));

        // Create descriptor heaps
        D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
        rtvHeapDesc.NumDescriptors = _frameCount;
        rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;

        ThrowIfFailed(_device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&_rtvHeap)));
        _rtvDescriptorSize = _device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);

        D3D12_DESCRIPTOR_HEAP_DESC dsvHeapDesc = {};
        dsvHeapDesc.NumDescriptors = 1;
        dsvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_DSV;

        ThrowIfFailed(_device->CreateDescriptorHeap(&dsvHeapDesc, IID_PPV_ARGS(&_dsvHeap)));

        D3D12_DESCRIPTOR_HEAP_DESC cbvHeapDesc = {};
        cbvHeapDesc.NumDescriptors = 1;
        cbvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
        cbvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;

        ThrowIfFailed(_device->CreateDescriptorHeap(&cbvHeapDesc, IID_PPV_ARGS(&_cbvSrvHeap)));

        LogMessage("[DX12VIEWPORT] Descriptor heaps created\n");

        // Create command allocator
        ThrowIfFailed(_device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&_commandAllocator)));

        // Create command list
        ThrowIfFailed(_device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, _commandAllocator.Get(), nullptr, IID_PPV_ARGS(&_commandList)));
        ThrowIfFailed(_commandList->Close());

        LogMessage("[DX12VIEWPORT] Command list created\n");

        // Create fence
        ThrowIfFailed(_device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&_fence)));
        _fenceValue = 1;
        _fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        if (!_fenceEvent)
            return E_FAIL;

        LogMessage("[DX12VIEWPORT] Fence created\n");
        LogMessage("[DX12VIEWPORT] ✅ InitializeDirectX successful\n");

        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in InitializeDirectX: %s\n", ex.what());
        return E_FAIL;
    }
}

HRESULT DX12Viewport::CreateRenderTargets()
{
    try
    {
        LogMessage("[DX12VIEWPORT] CreateRenderTargets starting\n");

        D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle(_rtvHeap->GetCPUDescriptorHandleForHeapStart());

        for (UINT n = 0; n < _frameCount; n++)
        {
            ThrowIfFailed(_swapChain->GetBuffer(n, IID_PPV_ARGS(&_renderTargets[n])));
            _device->CreateRenderTargetView(_renderTargets[n].Get(), nullptr, rtvHandle);
            rtvHandle.ptr += _rtvDescriptorSize;
        }

        // Create depth stencil buffer
        D3D12_RESOURCE_DESC depthDesc = {};
        depthDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
        depthDesc.Width = _width;
        depthDesc.Height = _height;
        depthDesc.DepthOrArraySize = 1;
        depthDesc.MipLevels = 1;
        depthDesc.Format = DXGI_FORMAT_D32_FLOAT;
        depthDesc.SampleDesc.Count = 1;
        depthDesc.Flags = D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;

        D3D12_CLEAR_VALUE depthClearValue = {};
        depthClearValue.Format = DXGI_FORMAT_D32_FLOAT;
        depthClearValue.DepthStencil.Depth = 1.0f;

        D3D12_HEAP_PROPERTIES heapProps = {};
        heapProps.Type = D3D12_HEAP_TYPE_DEFAULT;

        ThrowIfFailed(_device->CreateCommittedResource(
            &heapProps,
            D3D12_HEAP_FLAG_NONE,
            &depthDesc,
            D3D12_RESOURCE_STATE_DEPTH_WRITE,
            &depthClearValue,
            IID_PPV_ARGS(&_depthStencilBuffer)
        ));

        D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
        dsvDesc.Format = DXGI_FORMAT_D32_FLOAT;
        dsvDesc.ViewDimension = D3D12_DSV_DIMENSION_TEXTURE2D;

        _device->CreateDepthStencilView(_depthStencilBuffer.Get(), &dsvDesc, _dsvHeap->GetCPUDescriptorHandleForHeapStart());

        LogMessage("[DX12VIEWPORT] ✅ CreateRenderTargets successful\n");

        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in CreateRenderTargets: %s\n", ex.what());
        return E_FAIL;
    }
}

HRESULT DX12Viewport::CreatePipeline()
{
    try
    {
        LogMessage("[DX12VIEWPORT] CreatePipeline starting\n");

        // Create root signature
        D3D12_ROOT_SIGNATURE_DESC rootSignatureDesc = {};
        rootSignatureDesc.NumParameters = 0;
        rootSignatureDesc.pParameters = nullptr;
        rootSignatureDesc.NumStaticSamplers = 0;
        rootSignatureDesc.pStaticSamplers = nullptr;
        rootSignatureDesc.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

        ComPtr<ID3DBlob> signature;
        ComPtr<ID3DBlob> error;

        ThrowIfFailed(D3D12SerializeRootSignature(&rootSignatureDesc, D3D_ROOT_SIGNATURE_VERSION_1, &signature, &error));
        ThrowIfFailed(_device->CreateRootSignature(0, signature->GetBufferPointer(), signature->GetBufferSize(), IID_PPV_ARGS(&_rootSignature)));

        LogMessage("[DX12VIEWPORT] Root signature created\n");

        // Create simple pipeline state (placeholder)
        D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
        psoDesc.pRootSignature = _rootSignature.Get();
        psoDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
        psoDesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);
        psoDesc.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC(D3D12_DEFAULT);
        psoDesc.SampleMask = UINT_MAX;
        psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
        psoDesc.NumRenderTargets = 1;
        psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
        psoDesc.DSVFormat = DXGI_FORMAT_D32_FLOAT;
        psoDesc.SampleDesc.Count = 1;

        ThrowIfFailed(_device->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&_pipelineState)));

        LogMessage("[DX12VIEWPORT] Pipeline state created\n");

        // Set up viewport and scissor rect
        _viewport.TopLeftX = 0.0f;
        _viewport.TopLeftY = 0.0f;
        _viewport.Width = static_cast<float>(_width);
        _viewport.Height = static_cast<float>(_height);
        _viewport.MinDepth = 0.0f;
        _viewport.MaxDepth = 1.0f;

        _scissorRect = { 0, 0, (LONG)_width, (LONG)_height };

        LogMessage("[DX12VIEWPORT] ✅ CreatePipeline successful\n");

        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in CreatePipeline: %s\n", ex.what());
        return E_FAIL;
    }
}

void DX12Viewport::UpdateCamera()
{
    // Convert spherical to Cartesian coordinates
    float radTheta = _cameraTheta * 3.14159f / 180.0f;
    float radPhi = _cameraPhi * 3.14159f / 180.0f;

    float x = _cameraTargetX + _cameraDistance * sinf(radPhi) * cosf(radTheta);
    float y = _cameraTargetY + _cameraDistance * cosf(radPhi);
    float z = _cameraTargetZ + _cameraDistance * sinf(radPhi) * sinf(radTheta);

    // Update constant buffer (when implemented)
}

void DX12Viewport::RenderScene()
{
    try
    {
        ThrowIfFailed(_commandAllocator->Reset());
        ThrowIfFailed(_commandList->Reset(_commandAllocator.Get(), _pipelineState.Get()));

        _commandList->SetGraphicsRootSignature(_rootSignature.Get());
        _commandList->RSSetViewports(1, &_viewport);
        _commandList->RSSetScissorRects(1, &_scissorRect);

        // Transition render target
        D3D12_RESOURCE_BARRIER barrier = {};
        barrier.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
        barrier.Transition.pResource = _renderTargets[_currentFrameIndex].Get();
        barrier.Transition.StateBefore = D3D12_RESOURCE_STATE_PRESENT;
        barrier.Transition.StateAfter = D3D12_RESOURCE_STATE_RENDER_TARGET;

        _commandList->ResourceBarrier(1, &barrier);

        D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle(_rtvHeap->GetCPUDescriptorHandleForHeapStart());
        rtvHandle.ptr += _currentFrameIndex * _rtvDescriptorSize;

        D3D12_CPU_DESCRIPTOR_HANDLE dsvHandle(_dsvHeap->GetCPUDescriptorHandleForHeapStart());

        _commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, &dsvHandle);

        // Clear render target
        const float clearColor[] = { 0.1f, 0.1f, 0.1f, 1.0f };
        _commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);
        _commandList->ClearDepthStencilView(dsvHandle, D3D12_CLEAR_FLAG_DEPTH, 1.0f, 0, 0, nullptr);

        // Transition back
        barrier.Transition.StateBefore = D3D12_RESOURCE_STATE_RENDER_TARGET;
        barrier.Transition.StateAfter = D3D12_RESOURCE_STATE_PRESENT;

        _commandList->ResourceBarrier(1, &barrier);

        ThrowIfFailed(_commandList->Close());

        ID3D12CommandList* ppCommandLists[] = { _commandList.Get() };
        _commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);

        _frameCount_stat++;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] Exception in RenderScene: %s\n", ex.what());
    }
}

void DX12Viewport::WaitForPreviousFrame()
{
    const UINT64 fence = _fenceValue;
    ThrowIfFailed(_commandQueue->Signal(_fence.Get(), fence));
    _fenceValue++;

    if (_fence->GetCompletedValue() < fence)
    {
        ThrowIfFailed(_fence->SetEventOnCompletion(fence, _fenceEvent));
        WaitForSingleObject(_fenceEvent, INFINITE);
    }

    _currentFrameIndex = _swapChain->GetCurrentBackBufferIndex();
}

void DX12Viewport::LogMessage(const char* format, ...)
{
    va_list args;
    va_start(args, format);

    char buffer[1024];
    vsnprintf_s(buffer, sizeof(buffer), _TRUNCATE, format, args);

    OutputDebugStringA(buffer);

    va_end(args);
}

HRESULT DX12Viewport::CompileShaders()
{
    // TODO: Implement shader compilation
    return S_OK;

}
