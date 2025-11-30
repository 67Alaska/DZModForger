#include "pch.h"
#include "DX12Viewport.h"
#include "ShaderCompiler.h"
#include <cmath>
#include <algorithm>
#include <DirectXMath.h>

using namespace DirectX;

#pragma warning(disable: 4566)  // Disable unicode character warnings

// ==================== GUID DEFINITIONS ====================

extern "C" const IID IID_ID3D12Viewport =
{ 0x12345678, 0x1234, 0x1234, { 0x12, 0x34, 0x12, 0x34, 0x56, 0x78, 0x90, 0x12 } };

extern "C" const CLSID CLSID_DX12Viewport =
{ 0x87654321, 0x4321, 0x4321, { 0x43, 0x21, 0x21, 0x09, 0x87, 0x65, 0x43, 0x21 } };

#define FRAME_COUNT 3

// Vertex structure
struct Vertex
{
    XMFLOAT3 position;
    XMFLOAT3 normal;
    XMFLOAT2 texCoord;
};

// Constant buffer structure
struct TransformBuffer
{
    XMFLOAT4X4 worldViewProj;
    XMFLOAT4X4 world;
    XMFLOAT4X4 view;
    XMFLOAT4X4 projection;
    XMFLOAT4 cameraPosition;
    XMFLOAT4 lightDirection;
};

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

        D3D12_DESCRIPTOR_HEAP_DESC cbvHeapDesc = {};
        cbvHeapDesc.NumDescriptors = 1;
        cbvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
        cbvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
        ThrowIfFailed(_device->CreateDescriptorHeap(&cbvHeapDesc, IID_PPV_ARGS(&_cbvSrvHeap)));
        LogMessage("[DX12VIEWPORT] [OK] CBV heap created");

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

        // Create root signature
        ThrowIfFailed(CreateRootSignature());
        LogMessage("[DX12VIEWPORT] [OK] Root signature created");

        // Create pipeline state
        ThrowIfFailed(CreatePipelineState());
        LogMessage("[DX12VIEWPORT] [OK] Pipeline state created");

        // Create geometry
        ThrowIfFailed(CreateCubeGeometry());
        LogMessage("[DX12VIEWPORT] [OK] Cube geometry created");

        // Create constant buffer
        ThrowIfFailed(CreateConstantBuffer());
        LogMessage("[DX12VIEWPORT] [OK] Constant buffer created");

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

HRESULT DX12Viewport::CreateRootSignature()
{
    // Create root signature with one constant buffer view
    D3D12_ROOT_PARAMETER rootParam = {};
    rootParam.ParameterType = D3D12_ROOT_PARAMETER_TYPE_CBV;
    rootParam.Descriptor.ShaderRegister = 0;
    rootParam.Descriptor.RegisterSpace = 0;
    rootParam.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;

    D3D12_ROOT_SIGNATURE_DESC rootSigDesc = {};
    rootSigDesc.NumParameters = 1;
    rootSigDesc.pParameters = &rootParam;
    rootSigDesc.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

    ComPtr<ID3DBlob> signature, error;
    HRESULT hr = D3D12SerializeRootSignature(&rootSigDesc, D3D_ROOT_SIGNATURE_VERSION_1, &signature, &error);
    if (FAILED(hr))
    {
        if (error)
        {
            OutputDebugStringA((const char*)error->GetBufferPointer());
            error->Release();
        }
        return hr;
    }

    return _device->CreateRootSignature(0, signature->GetBufferPointer(), signature->GetBufferSize(), IID_PPV_ARGS(&_rootSignature));
}

HRESULT DX12Viewport::CreatePipelineState()
{
    // Compile shaders
    ComPtr<ID3DBlob> vsBlob, psBlob;

    // For now, use inline shader compilation
    const char* shaderCode = R"(
cbuffer TransformBuffer : register(b0)
{
    float4x4 worldViewProj;
    float4x4 world;
    float4x4 view;
    float4x4 projection;
    float4 cameraPosition;
    float4 lightDirection;
};

struct VS_INPUT
{
    float3 position : POSITION;
    float3 normal : NORMAL;
    float2 texCoord : TEXCOORD0;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 normal : NORMAL;
    float3 worldPos : TEXCOORD1;
    float2 texCoord : TEXCOORD0;
};

PS_INPUT VS_Main(VS_INPUT input)
{
    PS_INPUT output;
    float4 worldPos = mul(float4(input.position, 1.0f), world);
    output.worldPos = worldPos.xyz;
    output.position = mul(float4(input.position, 1.0f), worldViewProj);
    output.normal = mul(input.normal, (float3x3)world);
    output.texCoord = input.texCoord;
    return output;
}

float4 PS_Main(PS_INPUT input) : SV_TARGET
{
    float3 normal = normalize(input.normal);
    float3 lightDir = normalize(lightDirection.xyz);
    float diffuse = max(dot(normal, lightDir), 0.0f);
    float3 baseColor = float3(0.3f, 0.6f, 1.0f);
    float3 finalColor = baseColor * (0.3f + 0.7f * diffuse);
    float3 viewDir = normalize(cameraPosition.xyz - input.worldPos);
    float3 reflectDir = reflect(-lightDir, normal);
    float specular = pow(max(dot(viewDir, reflectDir), 0.0f), 32.0f);
    finalColor += specular * 0.5f;
    return float4(finalColor, 1.0f);
}
)";

    ThrowIfFailed(ShaderCompiler::CompileShaderFromMemory(shaderCode, strlen(shaderCode), "VS_Main", "vs_5_1", &vsBlob));
    ThrowIfFailed(ShaderCompiler::CompileShaderFromMemory(shaderCode, strlen(shaderCode), "PS_Main", "ps_5_1", &psBlob));

    // Input layout
    D3D12_INPUT_ELEMENT_DESC inputLayout[] =
    {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
        { "NORMAL", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
        { "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 24, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
    };

    D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
    psoDesc.InputLayout = { inputLayout, _countof(inputLayout) };
    psoDesc.pRootSignature = _rootSignature.Get();
    psoDesc.VS = { vsBlob->GetBufferPointer(), vsBlob->GetBufferSize() };
    psoDesc.PS = { psBlob->GetBufferPointer(), psBlob->GetBufferSize() };
    psoDesc.RasterizerState.FillMode = D3D12_FILL_MODE_SOLID;
    psoDesc.RasterizerState.CullMode = D3D12_CULL_MODE_BACK;
    psoDesc.BlendState.AlphaToCoverageEnable = FALSE;
    psoDesc.BlendState.IndependentBlendEnable = FALSE;
    psoDesc.BlendState.RenderTarget[0].RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE_ALL;
    psoDesc.SampleMask = UINT_MAX;
    psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
    psoDesc.NumRenderTargets = 1;
    psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
    psoDesc.SampleDesc.Count = 1;

    return _device->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&_pipelineState));
}

HRESULT DX12Viewport::CreateCubeGeometry()
{
    // Create cube vertices
    Vertex vertices[] = {
        // Front face
        { XMFLOAT3(-1, -1, 1), XMFLOAT3(0, 0, 1), XMFLOAT2(0, 1) },
        { XMFLOAT3(1, -1, 1), XMFLOAT3(0, 0, 1), XMFLOAT2(1, 1) },
        { XMFLOAT3(1, 1, 1), XMFLOAT3(0, 0, 1), XMFLOAT2(1, 0) },
        { XMFLOAT3(-1, 1, 1), XMFLOAT3(0, 0, 1), XMFLOAT2(0, 0) },
        // Back face
        { XMFLOAT3(1, -1, -1), XMFLOAT3(0, 0, -1), XMFLOAT2(0, 1) },
        { XMFLOAT3(-1, -1, -1), XMFLOAT3(0, 0, -1), XMFLOAT2(1, 1) },
        { XMFLOAT3(-1, 1, -1), XMFLOAT3(0, 0, -1), XMFLOAT2(1, 0) },
        { XMFLOAT3(1, 1, -1), XMFLOAT3(0, 0, -1), XMFLOAT2(0, 0) },
        // Top face
        { XMFLOAT3(-1, 1, 1), XMFLOAT3(0, 1, 0), XMFLOAT2(0, 1) },
        { XMFLOAT3(1, 1, 1), XMFLOAT3(0, 1, 0), XMFLOAT2(1, 1) },
        { XMFLOAT3(1, 1, -1), XMFLOAT3(0, 1, 0), XMFLOAT2(1, 0) },
        { XMFLOAT3(-1, 1, -1), XMFLOAT3(0, 1, 0), XMFLOAT2(0, 0) },
        // Bottom face
        { XMFLOAT3(-1, -1, -1), XMFLOAT3(0, -1, 0), XMFLOAT2(0, 1) },
        { XMFLOAT3(1, -1, -1), XMFLOAT3(0, -1, 0), XMFLOAT2(1, 1) },
        { XMFLOAT3(1, -1, 1), XMFLOAT3(0, -1, 0), XMFLOAT2(1, 0) },
        { XMFLOAT3(-1, -1, 1), XMFLOAT3(0, -1, 0), XMFLOAT2(0, 0) },
        // Right face
        { XMFLOAT3(1, -1, 1), XMFLOAT3(1, 0, 0), XMFLOAT2(0, 1) },
        { XMFLOAT3(1, -1, -1), XMFLOAT3(1, 0, 0), XMFLOAT2(1, 1) },
        { XMFLOAT3(1, 1, -1), XMFLOAT3(1, 0, 0), XMFLOAT2(1, 0) },
        { XMFLOAT3(1, 1, 1), XMFLOAT3(1, 0, 0), XMFLOAT2(0, 0) },
        // Left face
        { XMFLOAT3(-1, -1, -1), XMFLOAT3(-1, 0, 0), XMFLOAT2(0, 1) },
        { XMFLOAT3(-1, -1, 1), XMFLOAT3(-1, 0, 0), XMFLOAT2(1, 1) },
        { XMFLOAT3(-1, 1, 1), XMFLOAT3(-1, 0, 0), XMFLOAT2(1, 0) },
        { XMFLOAT3(-1, 1, -1), XMFLOAT3(-1, 0, 0), XMFLOAT2(0, 0) },
    };
    _vertexCount = 24;

    // Create indices (6 faces * 2 triangles * 3 indices)
    UINT indices[] = {
        0, 1, 2, 0, 2, 3,      // Front
        4, 5, 6, 4, 6, 7,      // Back
        8, 9, 10, 8, 10, 11,   // Top
        12, 13, 14, 12, 14, 15, // Bottom
        16, 17, 18, 16, 18, 19, // Right
        20, 21, 22, 20, 22, 23  // Left
    };
    _triangleCount = 12;

    // Create vertex buffer
    D3D12_HEAP_PROPERTIES heapProps = {};
    heapProps.Type = D3D12_HEAP_TYPE_UPLOAD;

    D3D12_RESOURCE_DESC bufDesc = {};
    bufDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
    bufDesc.Width = sizeof(vertices);
    bufDesc.Height = 1;
    bufDesc.DepthOrArraySize = 1;
    bufDesc.MipLevels = 1;
    bufDesc.Format = DXGI_FORMAT_UNKNOWN;
    bufDesc.SampleDesc.Count = 1;
    bufDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;

    ThrowIfFailed(_device->CreateCommittedResource(
        &heapProps,
        D3D12_HEAP_FLAG_NONE,
        &bufDesc,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nullptr,
        IID_PPV_ARGS(&_vertexBuffer)));

    void* data;
    ThrowIfFailed(_vertexBuffer->Map(0, nullptr, &data));
    memcpy(data, vertices, sizeof(vertices));
    _vertexBuffer->Unmap(0, nullptr);

    _vertexBufferView.BufferLocation = _vertexBuffer->GetGPUVirtualAddress();
    _vertexBufferView.StrideInBytes = sizeof(Vertex);
    _vertexBufferView.SizeInBytes = sizeof(vertices);

    // Create index buffer
    bufDesc.Width = sizeof(indices);
    ThrowIfFailed(_device->CreateCommittedResource(
        &heapProps,
        D3D12_HEAP_FLAG_NONE,
        &bufDesc,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nullptr,
        IID_PPV_ARGS(&_indexBuffer)));

    ThrowIfFailed(_indexBuffer->Map(0, nullptr, &data));
    memcpy(data, indices, sizeof(indices));
    _indexBuffer->Unmap(0, nullptr);

    _indexBufferView.BufferLocation = _indexBuffer->GetGPUVirtualAddress();
    _indexBufferView.Format = DXGI_FORMAT_R32_UINT;
    _indexBufferView.SizeInBytes = sizeof(indices);

    return S_OK;
}

HRESULT DX12Viewport::CreateConstantBuffer()
{
    // Create constant buffer for transforms
    D3D12_HEAP_PROPERTIES heapProps = {};
    heapProps.Type = D3D12_HEAP_TYPE_UPLOAD;

    D3D12_RESOURCE_DESC bufDesc = {};
    bufDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
    bufDesc.Width = ((sizeof(TransformBuffer) + 255) / 256) * 256; // CB must be 256-byte aligned
    bufDesc.Height = 1;
    bufDesc.DepthOrArraySize = 1;
    bufDesc.MipLevels = 1;
    bufDesc.Format = DXGI_FORMAT_UNKNOWN;
    bufDesc.SampleDesc.Count = 1;
    bufDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;

    return _device->CreateCommittedResource(
        &heapProps,
        D3D12_HEAP_FLAG_NONE,
        &bufDesc,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nullptr,
        IID_PPV_ARGS(&_constantBuffer));
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
        _vertexBuffer = nullptr;
        _indexBuffer = nullptr;
        _constantBuffer = nullptr;

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
        ThrowIfFailed(_commandList->Reset(_commandAllocator.Get(), _pipelineState.Get()));

        // Set viewport and scissor
        D3D12_VIEWPORT viewport = { 0.0f, 0.0f, (float)_width, (float)_height, 0.0f, 1.0f };
        D3D12_RECT scissorRect = { 0, 0, (LONG)_width, (LONG)_height };
        _commandList->RSSetViewports(1, &viewport);
        _commandList->RSSetScissorRects(1, &scissorRect);

        // Get current RTV handle
        D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = _rtvHeap->GetCPUDescriptorHandleForHeapStart();
        rtvHandle.ptr += (_currentFrameIndex * _rtvDescriptorSize);

        // Transition render target to render state
        D3D12_RESOURCE_BARRIER barrier = {};
        barrier.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
        barrier.Flags = D3D12_RESOURCE_BARRIER_FLAG_NONE;
        barrier.Transition.pResource = _renderTargets[_currentFrameIndex].Get();
        barrier.Transition.StateBefore = D3D12_RESOURCE_STATE_PRESENT;
        barrier.Transition.StateAfter = D3D12_RESOURCE_STATE_RENDER_TARGET;
        barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
        _commandList->ResourceBarrier(1, &barrier);

        // Clear render target
        float clearColor[] = { 0.2f, 0.2f, 0.2f, 1.0f };
        _commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);

        // Set render target
        _commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, nullptr);

        // Set pipeline state and root signature
        _commandList->SetPipelineState(_pipelineState.Get());
        _commandList->SetGraphicsRootSignature(_rootSignature.Get());

        // Set vertex/index buffers
        _commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
        _commandList->IASetVertexBuffers(0, 1, &_vertexBufferView);
        _commandList->IASetIndexBuffer(&_indexBufferView);

        // Update and set constant buffer
        if (_constantBuffer)
        {
            TransformBuffer cb = {};

            // Create matrices
            XMMATRIX world = XMMatrixIdentity();
            XMMATRIX view = XMMatrixLookAtLH(
                XMVectorSet(3.0f, 3.0f, 3.0f, 0.0f),
                XMVectorSet(0.0f, 0.0f, 0.0f, 0.0f),
                XMVectorSet(0.0f, 1.0f, 0.0f, 0.0f));
            XMMATRIX projection = XMMatrixPerspectiveFovLH(
                XM_PIDIV4,
                (float)_width / (float)_height,
                0.1f, 1000.0f);

            XMMATRIX worldViewProj = world * view * projection;

            XMStoreFloat4x4(&cb.worldViewProj, XMMatrixTranspose(worldViewProj));
            XMStoreFloat4x4(&cb.world, XMMatrixTranspose(world));
            XMStoreFloat4x4(&cb.view, XMMatrixTranspose(view));
            XMStoreFloat4x4(&cb.projection, XMMatrixTranspose(projection));
            cb.cameraPosition = XMFLOAT4(3.0f, 3.0f, 3.0f, 0.0f);
            cb.lightDirection = XMFLOAT4(1.0f, 1.0f, 1.0f, 0.0f);

            void* data;
            ThrowIfFailed(_constantBuffer->Map(0, nullptr, &data));
            memcpy(data, &cb, sizeof(cb));
            _constantBuffer->Unmap(0, nullptr);

            _commandList->SetGraphicsRootConstantBufferView(0, _constantBuffer->GetGPUVirtualAddress());
        }

        // Draw indexed
        _commandList->DrawIndexedInstanced(_triangleCount * 3, 1, 0, 0, 0);

        // Transition render target to present state
        barrier.Transition.StateBefore = D3D12_RESOURCE_STATE_RENDER_TARGET;
        barrier.Transition.StateAfter = D3D12_RESOURCE_STATE_PRESENT;
        _commandList->ResourceBarrier(1, &barrier);

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
