// DX12Viewport.cpp - COMPLETE FILE

#include "pch.h"
#include "DX12Viewport.h"
#include <cmath>
#include <algorithm>

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
    DebugPrint("[DX12VIEWPORT] DX12Viewport constructor");
    ZeroMemory(_renderTargets, sizeof(_renderTargets));
    ZeroMemory(&_viewport, sizeof(_viewport));
    ZeroMemory(&_scissorRect, sizeof(_scissorRect));
    ZeroMemory(&_vertexBufferView, sizeof(_vertexBufferView));
    ZeroMemory(&_indexBufferView, sizeof(_indexBufferView));
}

DX12Viewport::~DX12Viewport()
{
    Shutdown();
}

STDMETHODIMP DX12Viewport::QueryInterface(REFIID riid, void** ppvObject)
{
    if (!ppvObject) return E_INVALIDARG;

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
    LONG refCount = InterlockedDecrement(&_refCount);
    if (refCount == 0)
    {
        delete this;
    }
    return refCount;
}

HRESULT DX12Viewport::Initialize(HWND hwnd, UINT width, UINT height)
{
    try
    {
        DebugPrint("[DX12VIEWPORT] Initializing with hwnd=%p, %ux%u", hwnd, width, height);

        if (!hwnd || width == 0 || height == 0)
        {
            DebugPrint("[DX12VIEWPORT] Invalid parameters");
            return E_INVALIDARG;
        }

        _hwnd = hwnd;
        _width = width;
        _height = height;

#ifdef _DEBUG
        {
            ComPtr<ID3D12Debug> debugController;
            if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController))))
            {
                debugController->EnableDebugLayer();
                DebugPrint("[DX12VIEWPORT] Debug layer enabled");
            }
        }
#endif

        ThrowIfFailed(D3D12CreateDevice(nullptr, D3D_FEATURE_LEVEL_12_1, IID_PPV_ARGS(&_device)));
        DebugPrint("[DX12VIEWPORT] Device created");

        D3D12_COMMAND_QUEUE_DESC queueDesc = {};
        queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
        queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
        ThrowIfFailed(_device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&_commandQueue)));
        DebugPrint("[DX12VIEWPORT] Command queue created");

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
        ThrowIfFailed(factory->CreateSwapChainForHwnd(_commandQueue.Get(), hwnd, &swapChainDesc, nullptr, nullptr, &swapChain1));
        ThrowIfFailed(swapChain1.As(&_swapChain));
        DebugPrint("[DX12VIEWPORT] Swap chain created");

        D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
        rtvHeapDesc.NumDescriptors = FRAME_COUNT;
        rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
        ThrowIfFailed(_device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&_rtvHeap)));
        DebugPrint("[DX12VIEWPORT] RTV descriptor heap created");

        D3D12_DESCRIPTOR_HEAP_DESC dsvHeapDesc = {};
        dsvHeapDesc.NumDescriptors = 1;
        dsvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_DSV;
        ThrowIfFailed(_device->CreateDescriptorHeap(&dsvHeapDesc, IID_PPV_ARGS(&_dsvHeap)));
        DebugPrint("[DX12VIEWPORT] DSV descriptor heap created");

        D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = _rtvHeap->GetCPUDescriptorHandleForHeapStart();
        _rtvDescriptorSize = _device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);

        for (UINT i = 0; i < FRAME_COUNT; i++)
        {
            ThrowIfFailed(_swapChain->GetBuffer(i, IID_PPV_ARGS(&_renderTargets[i])));
            _device->CreateRenderTargetView(_renderTargets[i].Get(), nullptr, rtvHandle);
            rtvHandle.ptr += _rtvDescriptorSize;
        }
        DebugPrint("[DX12VIEWPORT] Render target views created");

        ThrowIfFailed(_device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&_commandAllocator)));
        DebugPrint("[DX12VIEWPORT] Command allocator created");

        ThrowIfFailed(_device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, _commandAllocator.Get(), nullptr, IID_PPV_ARGS(&_commandList)));
        ThrowIfFailed(_commandList->Close());
        DebugPrint("[DX12VIEWPORT] Command list created");

        _fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        ThrowIfFailed(_device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&_fence)));
        _fenceValue = 1;
        DebugPrint("[DX12VIEWPORT] Fence created");

        _isInitialized = true;
        DebugPrint("[DX12VIEWPORT] Initialization successful");
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        DebugPrint("[DX12VIEWPORT] Exception in Initialize: %s", ex.what());
        return E_FAIL;
    }
}

HRESULT DX12Viewport::Shutdown()
{
    try
    {
        if (_commandQueue)
        {
            WaitForGpu();
        }

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
        _swapChain = nullptr;
        _commandQueue = nullptr;
        _device = nullptr;

        if (_fenceEvent)
        {
            CloseHandle(_fenceEvent);
            _fenceEvent = nullptr;
        }

        _isInitialized = false;
        DebugPrint("[DX12VIEWPORT] Shutdown complete");
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        DebugPrint("[DX12VIEWPORT] Exception in Shutdown: %s", ex.what());
        return E_FAIL;
    }
}

HRESULT DX12Viewport::Render()
{
    try
    {
        if (!_isInitialized)
            return E_FAIL;

        ThrowIfFailed(_commandAllocator->Reset());
        ThrowIfFailed(_commandList->Reset(_commandAllocator.Get(), nullptr));

        D3D12_VIEWPORT viewport = { 0.0f, 0.0f, (float)_width, (float)_height, 0.0f, 1.0f };
        D3D12_RECT scissorRect = { 0, 0, (LONG)_width, (LONG)_height };
        _commandList->RSSetViewports(1, &viewport);
        _commandList->RSSetScissorRects(1, &scissorRect);

        D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = _rtvHeap->GetCPUDescriptorHandleForHeapStart();
        rtvHandle.ptr += (_currentFrameIndex * _rtvDescriptorSize);

        float clearColor[] = { 0.2f, 0.2f, 0.2f, 1.0f };
        _commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);

        ThrowIfFailed(_commandList->Close());

        ID3D12CommandList* ppCommandLists[] = { _commandList.Get() };
        _commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);

        ThrowIfFailed(_swapChain->Present(1, 0));

        WaitForGpu();
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        DebugPrint("[DX12VIEWPORT] Exception in Render: %s", ex.what());
        return E_FAIL;
    }
}

HRESULT DX12Viewport::Present()
{
    return Render();
}

HRESULT DX12Viewport::Resize(UINT width, UINT height)
{
    try
    {
        if (width == 0 || height == 0)
            return E_INVALIDARG;

        _width = width;
        _height = height;

        WaitForGpu();

        for (int i = 0; i < FRAME_COUNT; i++)
        {
            _renderTargets[i] = nullptr;
        }

        ThrowIfFailed(_swapChain->ResizeBuffers(FRAME_COUNT, width, height, DXGI_FORMAT_R8G8B8A8_UNORM, 0));

        D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = _rtvHeap->GetCPUDescriptorHandleForHeapStart();
        for (UINT i = 0; i < FRAME_COUNT; i++)
        {
            ThrowIfFailed(_swapChain->GetBuffer(i, IID_PPV_ARGS(&_renderTargets[i])));
            _device->CreateRenderTargetView(_renderTargets[i].Get(), nullptr, rtvHandle);
            rtvHandle.ptr += _rtvDescriptorSize;
        }

        _currentFrameIndex = _swapChain->GetCurrentBackBufferIndex();
        DebugPrint("[DX12VIEWPORT] Resized to %ux%u", width, height);
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        DebugPrint("[DX12VIEWPORT] Exception in Resize: %s", ex.what());
        return E_FAIL;
    }
}

HRESULT DX12Viewport::SetCamera(float radius, float theta, float phi, float targetX, float targetY, float targetZ)
{
    _cameraDistance = radius;
    _cameraTheta = theta;
    _cameraPhi = phi;
    _cameraTargetX = targetX;
    _cameraTargetY = targetY;
    _cameraTargetZ = targetZ;
    DebugPrint("[DX12VIEWPORT] Camera set: r=%.2f, theta=%.2f, phi=%.2f", radius, theta, phi);
    return S_OK;
}

HRESULT DX12Viewport::LoadFBX(const char* filePath)
{
    DebugPrint("[DX12VIEWPORT] Loading FBX: %s", filePath);
    return S_OK;
}

HRESULT DX12Viewport::LoadOBJ(const char* filePath)
{
    DebugPrint("[DX12VIEWPORT] Loading OBJ: %s", filePath);
    return S_OK;
}

HRESULT DX12Viewport::GetFrameRate(float* pFps)
{
    if (!pFps) return E_INVALIDARG;
    *pFps = _frameTime > 0 ? 1000.0f / _frameTime : 0.0f;
    return S_OK;
}

HRESULT DX12Viewport::GetVertexCount(UINT* pCount)
{
    if (!pCount) return E_INVALIDARG;
    *pCount = _vertexCount;
    return S_OK;
}

HRESULT DX12Viewport::GetTriangleCount(UINT* pCount)
{
    if (!pCount) return E_INVALIDARG;
    *pCount = _triangleCount;
    return S_OK;
}

void DX12Viewport::WaitForGpu()
{
    if (_commandQueue && _fence && _fenceEvent)
    {
        try
        {
            ThrowIfFailed(_commandQueue->Signal(_fence.Get(), _fenceValue));
            ThrowIfFailed(_fence->SetEventOnCompletion(_fenceValue, _fenceEvent));
            WaitForSingleObject(_fenceEvent, INFINITE);
            _fenceValue++;
        }
        catch (const std::exception& ex)
        {
            DebugPrint("[DX12VIEWPORT] Exception in WaitForGpu: %s", ex.what());
        }
    }
}

void DX12Viewport::LogMessage(const char* format, ...)
{
    char buffer[512];
    va_list args;
    va_start(args, format);
    vsprintf_s(buffer, sizeof(buffer), format, args);
    va_end(args);
    OutputDebugStringA(buffer);
    OutputDebugStringA("\n");
}
