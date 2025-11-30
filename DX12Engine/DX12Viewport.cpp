#include "pch.h"
#include "DX12Viewport.h"
#include <cmath>
#include <algorithm>

#pragma warning(disable: 4566)  // Disable unicode character warnings

// ==================== GUID DEFINITIONS ====================

// {12345678-1234-1234-1234-123456789012}
extern "C" const IID IID_ID3D12Viewport =
{ 0x12345678, 0x1234, 0x1234, { 0x12, 0x34, 0x12, 0x34, 0x56, 0x78, 0x90, 0x12 } };

// {87654321-4321-4321-4321-210987654321}
extern "C" const CLSID CLSID_DX12Viewport =
{ 0x87654321, 0x4321, 0x4321, { 0x43, 0x21, 0x21, 0x09, 0x87, 0x65, 0x43, 0x21 } };

#define FRAME_COUNT 3

// Debug helper
inline void DebugPrint(const char* format, ...)
{
    char buffer[256];
    va_list args;
    va_start(args, format);
    vsprintf_s(buffer, sizeof(buffer), format, args);
    va_end(args);
    OutputDebugStringA(buffer);
    OutputDebugStringA("\n");
}

// ==================== CONSTRUCTOR / DESTRUCTOR ====================

DX12Viewport::DX12Viewport()
    : _refCount(1)
    , _hwnd(nullptr)
    , _width(0)
    , _height(0)
    , _device(nullptr)
    , _commandQueue(nullptr)
    , _commandAllocator(nullptr)
    , _commandList(nullptr)
    , _swapChain(nullptr)
    , _rtvHeap(nullptr)
    , _dsvHeap(nullptr)
    , _cbvSrvHeap(nullptr)
    , _depthStencilBuffer(nullptr)
    , _rtvDescriptorSize(0)
    , _currentFrameIndex(0)
    , _fence(nullptr)
    , _fenceEvent(nullptr)
    , _fenceValue(0)
    , _rootSignature(nullptr)
    , _pipelineState(nullptr)
    , _cbvDataBegin(nullptr)
    , _cameraDistance(5.0f)
    , _cameraTheta(0.0f)
    , _cameraPhi(30.0f)
    , _cameraTargetX(0.0f)
    , _cameraTargetY(0.0f)
    , _cameraTargetZ(0.0f)
    , _vertexCount(0)
    , _triangleCount(0)
    , _frameTime(0.0f)
    , _isInitialized(false)
{
    DebugPrint("[DX12VIEWPORT] Constructor called");
    ZeroMemory(_renderTargets, sizeof(_renderTargets));
    ZeroMemory(&_viewport, sizeof(_viewport));
    ZeroMemory(&_scissorRect, sizeof(_scissorRect));
    ZeroMemory(&_vertexBufferView, sizeof(_vertexBufferView));
    ZeroMemory(&_indexBufferView, sizeof(_indexBufferView));
}

DX12Viewport::~DX12Viewport()
{
    DebugPrint("[DX12VIEWPORT] Destructor called");
    Shutdown();
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

// ==================== CORE INITIALIZATION ====================

STDMETHODIMP DX12Viewport::Initialize(HWND hwnd, UINT width, UINT height)
{
    try
    {
        if (!hwnd || width == 0 || height == 0)
        {
            LogMessage("[DX12VIEWPORT] [ERROR] Invalid parameters: hwnd=%p, width=%u, height=%u", hwnd, width, height);
            return E_INVALIDARG;
        }

        LogMessage("[DX12VIEWPORT] Initializing: hwnd=%p, %ux%u", hwnd, width, height);

        _hwnd = hwnd;
        _width = width;
        _height = height;

        // Enable debug layer
#ifdef _DEBUG
        {
            ComPtr<ID3D12Debug> debugController;
            if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController))))
            {
                debugController->EnableDebugLayer();
                LogMessage("[DX12VIEWPORT] [OK] Debug layer enabled");
            }
        }
#endif

        // Create device
        ThrowIfFailed(D3D12CreateDevice(nullptr, D3D_FEATURE_LEVEL_12_1, IID_PPV_ARGS(&_device)));
        LogMessage("[DX12VIEWPORT] [OK] Device created");

        // Create command queue
        D3D12_COMMAND_QUEUE_DESC queueDesc = {};
        queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
        queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
        ThrowIfFailed(_device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&_commandQueue)));
        LogMessage("[DX12VIEWPORT] [OK] Command queue created");

        // Create DXGI factory and swap chain
        ComPtr<IDXGIFactory4> factory;
        ThrowIfFailed(CreateDXGIFactory1(IID_PPV_ARGS(&factory)));

        DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
        swapChainDesc.BufferCount = FRAME_COUNT;
        swapChainDesc.Width = width;
        swapChainDesc.Height = height;
        swapChainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
        swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
        swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
        swapChainDesc.SampleDesc.Count = 1;

        ComPtr<IDXGISwapChain1> swapChain1;
        ThrowIfFailed(factory->CreateSwapChainForHwnd(
            _commandQueue.Get(),
            hwnd,
            &swapChainDesc,
            nullptr,
            nullptr,
            &swapChain1));
        ThrowIfFailed(swapChain1.As(&_swapChain));
        LogMessage("[DX12VIEWPORT] [OK] Swap chain created");

        // Create descriptor heaps
        D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
        rtvHeapDesc.NumDescriptors = FRAME_COUNT;
        rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
        ThrowIfFailed(_device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&_rtvHeap)));
        LogMessage("[DX12VIEWPORT] [OK] RTV heap created");

        D3D12_DESCRIPTOR_HEAP_DESC dsvHeapDesc = {};
        dsvHeapDesc.NumDescriptors = 1;
        dsvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_DSV;
        ThrowIfFailed(_device->CreateDescriptorHeap(&dsvHeapDesc, IID_PPV_ARGS(&_dsvHeap)));
        LogMessage("[DX12VIEWPORT] [OK] DSV heap created");

        // Create render target views
        D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = _rtvHeap->GetCPUDescriptorHandleForHeapStart();
        _rtvDescriptorSize = _device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);

        for (UINT i = 0; i < FRAME_COUNT; i++)
        {
            ThrowIfFailed(_swapChain->GetBuffer(i, IID_PPV_ARGS(&_renderTargets[i])));
            _device->CreateRenderTargetView(_renderTargets[i].Get(), nullptr, rtvHandle);
            rtvHandle.ptr += _rtvDescriptorSize;
        }
        LogMessage("[DX12VIEWPORT] [OK] Render targets created");

        // Create command allocator and list
        ThrowIfFailed(_device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&_commandAllocator)));
        ThrowIfFailed(_device->CreateCommandList(
            0,
            D3D12_COMMAND_LIST_TYPE_DIRECT,
            _commandAllocator.Get(),
            nullptr,
            IID_PPV_ARGS(&_commandList)));
        ThrowIfFailed(_commandList->Close());
        LogMessage("[DX12VIEWPORT] [OK] Command allocator and list created");

        // Create fence
        _fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        if (!_fenceEvent)
        {
            LogMessage("[DX12VIEWPORT] [ERROR] Failed to create fence event");
            return E_FAIL;
        }

        ThrowIfFailed(_device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&_fence)));
        _fenceValue = 1;
        LogMessage("[DX12VIEWPORT] [OK] Fence created");

        _isInitialized = true;
        LogMessage("[DX12VIEWPORT] [SUCCESS] Initialization SUCCESSFUL");
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] [ERROR] Exception in Initialize: %s", ex.what());
        return E_FAIL;
    }
}

STDMETHODIMP DX12Viewport::Shutdown()
{
    try
    {
        LogMessage("[DX12VIEWPORT] Shutting down...");

        if (_commandQueue && _fence && _fenceEvent)
        {
            WaitForGpu();
        }

        // Release all COM objects
        _commandList = nullptr;
        _commandAllocator = nullptr;
        _fence = nullptr;

        for (int i = 0; i < FRAME_COUNT; i++)
        {
            _renderTargets[i] = nullptr;
        }

        _depthStencilBuffer = nullptr;
        _rtvHeap = nullptr;
        _dsvHeap = nullptr;
        _cbvSrvHeap = nullptr;
        _pipelineState = nullptr;
        _rootSignature = nullptr;
        _swapChain = nullptr;
        _commandQueue = nullptr;
        _device = nullptr;

        if (_fenceEvent)
        {
            CloseHandle(_fenceEvent);
            _fenceEvent = nullptr;
        }

        _isInitialized = false;
        LogMessage("[DX12VIEWPORT] [OK] Shutdown complete");
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] [WARNING] Exception in Shutdown: %s", ex.what());
        return E_FAIL;
    }
}

// ==================== RENDERING ====================

STDMETHODIMP DX12Viewport::Render()
{
    try
    {
        if (!_isInitialized)
        {
            LogMessage("[DX12VIEWPORT] [ERROR] Not initialized");
            return E_FAIL;
        }

        // Reset allocator and list
        ThrowIfFailed(_commandAllocator->Reset());
        ThrowIfFailed(_commandList->Reset(_commandAllocator.Get(), nullptr));

        // Set viewport and scissor
        D3D12_VIEWPORT viewport = { 0.0f, 0.0f, (float)_width, (float)_height, 0.0f, 1.0f };
        D3D12_RECT scissorRect = { 0, 0, (LONG)_width, (LONG)_height };
        _commandList->RSSetViewports(1, &viewport);
        _commandList->RSSetScissorRects(1, &scissorRect);

        // Get current RTV handle
        D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = _rtvHeap->GetCPUDescriptorHandleForHeapStart();
        rtvHandle.ptr += (_currentFrameIndex * _rtvDescriptorSize);

        // Clear render target
        float clearColor[] = { 0.2f, 0.2f, 0.2f, 1.0f };
        _commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);

        // Close and execute command list
        ThrowIfFailed(_commandList->Close());

        ID3D12CommandList* ppCommandLists[] = { _commandList.Get() };
        _commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);

        // Present
        ThrowIfFailed(_swapChain->Present(1, 0));

        // Update frame index
        _currentFrameIndex = _swapChain->GetCurrentBackBufferIndex();

        WaitForGpu();
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] [ERROR] Exception in Render: %s", ex.what());
        return E_FAIL;
    }
}

STDMETHODIMP DX12Viewport::Present()
{
    return Render();
}

STDMETHODIMP DX12Viewport::Resize(UINT width, UINT height)
{
    try
    {
        if (width == 0 || height == 0)
        {
            LogMessage("[DX12VIEWPORT] [ERROR] Invalid resize dimensions: %ux%u", width, height);
            return E_INVALIDARG;
        }

        LogMessage("[DX12VIEWPORT] Resizing to %ux%u", width, height);

        _width = width;
        _height = height;

        WaitForGpu();

        // Release render targets
        for (int i = 0; i < FRAME_COUNT; i++)
        {
            _renderTargets[i] = nullptr;
        }

        // Resize swap chain
        ThrowIfFailed(_swapChain->ResizeBuffers(FRAME_COUNT, width, height, DXGI_FORMAT_R8G8B8A8_UNORM, 0));

        // Recreate render target views
        D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = _rtvHeap->GetCPUDescriptorHandleForHeapStart();
        for (UINT i = 0; i < FRAME_COUNT; i++)
        {
            ThrowIfFailed(_swapChain->GetBuffer(i, IID_PPV_ARGS(&_renderTargets[i])));
            _device->CreateRenderTargetView(_renderTargets[i].Get(), nullptr, rtvHandle);
            rtvHandle.ptr += _rtvDescriptorSize;
        }

        _currentFrameIndex = _swapChain->GetCurrentBackBufferIndex();
        LogMessage("[DX12VIEWPORT] [OK] Resize complete");
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] [ERROR] Exception in Resize: %s", ex.what());
        return E_FAIL;
    }
}

// ==================== CAMERA & MODEL CONTROL ====================

STDMETHODIMP DX12Viewport::SetCamera(float radius, float theta, float phi, float targetX, float targetY, float targetZ)
{
    _cameraDistance = radius;
    _cameraTheta = theta;
    _cameraPhi = phi;
    _cameraTargetX = targetX;
    _cameraTargetY = targetY;
    _cameraTargetZ = targetZ;
    LogMessage("[DX12VIEWPORT] Camera set: r=%.2f, theta=%.2f deg, phi=%.2f deg, target=(%.2f,%.2f,%.2f)",
        radius, theta, phi, targetX, targetY, targetZ);
    return S_OK;
}

STDMETHODIMP DX12Viewport::LoadFBX(const char* filePath)
{
    if (!filePath)
        return E_INVALIDARG;

    try
    {
        LogMessage("[DX12VIEWPORT] Loading FBX: %s", filePath);
        // TODO: Implement FBX loading
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] [ERROR] Exception in LoadFBX: %s", ex.what());
        return E_FAIL;
    }
}

STDMETHODIMP DX12Viewport::LoadOBJ(const char* filePath)
{
    if (!filePath)
        return E_INVALIDARG;

    try
    {
        LogMessage("[DX12VIEWPORT] Loading OBJ: %s", filePath);
        // TODO: Implement OBJ loading
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] [ERROR] Exception in LoadOBJ: %s", ex.what());
        return E_FAIL;
    }
}

// ==================== STATISTICS ====================

STDMETHODIMP DX12Viewport::GetFrameRate(float* pFps)
{
    if (!pFps)
        return E_INVALIDARG;

    *pFps = _frameTime > 0.0f ? 1000.0f / _frameTime : 0.0f;
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

// ==================== PRIVATE HELPERS ====================

void DX12Viewport::WaitForGpu()
{
    if (!_commandQueue || !_fence || !_fenceEvent)
        return;

    try
    {
        ThrowIfFailed(_commandQueue->Signal(_fence.Get(), _fenceValue));
        ThrowIfFailed(_fence->SetEventOnCompletion(_fenceValue, _fenceEvent));
        WaitForSingleObject(_fenceEvent, INFINITE);
        _fenceValue++;
    }
    catch (const std::exception& ex)
    {
        LogMessage("[DX12VIEWPORT] [WARNING] Exception in WaitForGpu: %s", ex.what());
    }
}

void DX12Viewport::LogMessage(const char* format, ...)
{
    if (!format)
        return;

    char buffer[512];
    va_list args;
    va_start(args, format);
    vsprintf_s(buffer, sizeof(buffer), format, args);
    va_end(args);
    OutputDebugStringA(buffer);
    OutputDebugStringA("\n");
}