#include "pch.h"
#include "Engine.h"
#include <iostream>
#include <sstream>
#include <vector>
#include <chrono>

using Microsoft::WRL::ComPtr;

// ==================== VIEWPORT IMPLEMENTATION ====================

class CDX12Viewport : public IDX12Viewport {
public:
    CDX12Viewport();
    ~CDX12Viewport();

    // IUnknown
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppvObject) override;
    ULONG STDMETHODCALLTYPE AddRef() override;
    ULONG STDMETHODCALLTYPE Release() override;

    // IDX12Viewport
    HRESULT STDMETHODCALLTYPE Initialize(HWND hwnd, UINT width, UINT height) override;
    HRESULT STDMETHODCALLTYPE LoadModelFromFile(const wchar_t* filePath) override;
    HRESULT STDMETHODCALLTYPE UpdateModelData(Vertex* vertices, UINT vertexCount, UINT* indices, UINT indexCount) override;
    HRESULT STDMETHODCALLTYPE Render() override;
    HRESULT STDMETHODCALLTYPE Present() override;
    HRESULT STDMETHODCALLTYPE SetCameraOrbital(float yaw, float pitch, float distance) override;
    HRESULT STDMETHODCALLTYPE SetCameraPan(float panX, float panY) override;
    HRESULT STDMETHODCALLTYPE SetCameraZoom(float zoomFactor) override;
    HRESULT STDMETHODCALLTYPE ResetCamera() override;
    HRESULT STDMETHODCALLTYPE SetShadingMode(int mode) override;
    HRESULT STDMETHODCALLTYPE ShowGrid(BOOL visible) override;
    HRESULT STDMETHODCALLTYPE ShowAxes(BOOL visible) override;
    HRESULT STDMETHODCALLTYPE ShowBounds(BOOL visible) override;
    HRESULT STDMETHODCALLTYPE SetMaterialMetallic(float metallic) override;
    HRESULT STDMETHODCALLTYPE SetMaterialRoughness(float roughness) override;
    HRESULT STDMETHODCALLTYPE SetMaterialAO(float ao) override;
    HRESULT STDMETHODCALLTYPE AddLight(Light* light) override;
    HRESULT STDMETHODCALLTYPE ClearLights() override;
    HRESULT STDMETHODCALLTYPE GetFrameRate(float* outFPS) override;
    HRESULT STDMETHODCALLTYPE GetGPUMemoryUsage(UINT64* outBytes) override;
    HRESULT STDMETHODCALLTYPE GetLastError(wchar_t* outError, UINT errorLength) override;
    HRESULT STDMETHODCALLTYPE Shutdown() override;

private:
    ULONG m_refCount = 1;
    HWND m_hwnd = nullptr;
    UINT m_width = 0;
    UINT m_height = 0;
    bool m_initialized = false;

    // DirectX 12 objects
    ComPtr<IDXGIFactory6> m_factory;
    ComPtr<IDXGIAdapter1> m_adapter;
    ComPtr<ID3D12Device> m_device;
    ComPtr<ID3D12CommandQueue> m_commandQueue;
    ComPtr<IDXGISwapChain4> m_swapChain;
    ComPtr<ID3D12DescriptorHeap> m_rtvHeap;
    ComPtr<ID3D12DescriptorHeap> m_dsvHeap;
    ComPtr<ID3D12Resource> m_renderTargets[3];
    ComPtr<ID3D12Resource> m_depthStencilBuffer;
    ComPtr<ID3D12CommandAllocator> m_commandAllocators[3];
    ComPtr<ID3D12GraphicsCommandList> m_commandList;
    ComPtr<ID3D12Fence> m_fence;
    HANDLE m_fenceEvent = nullptr;
    UINT64 m_fenceValues[3] = { 0, 0, 0 };
    UINT m_frameIndex = 0;

    // Pipeline state
    ComPtr<ID3D12PipelineState> m_pipelineState;
    ComPtr<ID3D12RootSignature> m_rootSignature;

    // Buffers
    ComPtr<ID3D12Resource> m_vertexBuffer;
    ComPtr<ID3D12Resource> m_indexBuffer;
    ComPtr<ID3D12Resource> m_constantBuffer;
    ComPtr<ID3D12DescriptorHeap> m_cbvHeap;
    D3D12_VERTEX_BUFFER_VIEW m_vertexBufferView;
    D3D12_INDEX_BUFFER_VIEW m_indexBufferView;

    // Data
    UINT m_indexCount = 0;
    Camera m_camera;
    Material m_material;
    std::vector<Light> m_lights;
    int m_shadingMode = 1; // Solid
    bool m_showGrid = true;
    bool m_showAxes = true;
    bool m_showBounds = false;
    std::wstring m_lastError;

    // Timing
    std::chrono::high_resolution_clock::time_point m_lastFrameTime;
    float m_fps = 0.0f;
    int m_frameCount = 0;

    // Initialization helpers
    HRESULT InitializeDevice();
    HRESULT InitializePipeline();
    HRESULT CreateRootSignature();
    HRESULT CreatePipelineState();
    HRESULT CreateBuffers();
    HRESULT WaitForGPU();
    HRESULT MoveToNextFrame();

    // Rendering helpers
    void UpdateConstantBuffer();
    void RenderModel();
    void RenderGrid();
    void RenderAxes();
    void RenderBounds();
    void UpdateCamera();

    // Utility
    void SetError(const wchar_t* error);
};

// ==================== CDX12VIEWPORT IMPLEMENTATION ====================

CDX12Viewport::CDX12Viewport() {
    m_lastFrameTime = std::chrono::high_resolution_clock::now();

    // Initialize camera
    m_camera.position = { 0.0f, 0.0f, 5.0f };
    m_camera.target = { 0.0f, 0.0f, 0.0f };
    m_camera.up = { 0.0f, 1.0f, 0.0f };
    m_camera.fov = 45.0f;
    m_camera.nearPlane = 0.1f;
    m_camera.farPlane = 1000.0f;
    m_camera.yaw = 0.0f;
    m_camera.pitch = 30.0f;
    m_camera.distance = 5.0f;

    // Initialize material
    m_material.diffuse = { 0.8f, 0.8f, 0.8f, 1.0f };
    m_material.specular = { 1.0f, 1.0f, 1.0f, 1.0f };
    m_material.metallic = 0.0f;
    m_material.roughness = 0.5f;
    m_material.aoFactor = 1.0f;
}

CDX12Viewport::~CDX12Viewport() {
    if (m_initialized) {
        Shutdown();
    }
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::QueryInterface(REFIID riid, void** ppvObject) {
    if (!ppvObject) return E_INVALIDARG;

    if (riid == IID_IUnknown || riid == IID_IDX12Viewport) {
        *ppvObject = static_cast<IDX12Viewport*>(this);
        AddRef();
        return S_OK;
    }

    *ppvObject = nullptr;
    return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE CDX12Viewport::AddRef() {
    return ++m_refCount;
}

ULONG STDMETHODCALLTYPE CDX12Viewport::Release() {
    ULONG refCount = --m_refCount;
    if (refCount == 0) {
        delete this;
    }
    return refCount;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::Initialize(HWND hwnd, UINT width, UINT height) {
    if (!hwnd || width == 0 || height == 0) {
        SetError(L"Invalid parameters");
        return E_INVALIDARG;
    }

    m_hwnd = hwnd;
    m_width = width;
    m_height = height;
    m_camera.aspectRatio = static_cast<float>(width) / static_cast<float>(height);

    // Initialize DirectX 12
    if (FAILED(InitializeDevice())) {
        SetError(L"Failed to initialize device");
        return E_FAIL;
    }

    if (FAILED(InitializePipeline())) {
        SetError(L"Failed to initialize pipeline");
        return E_FAIL;
    }

    m_initialized = true;
    return S_OK;
}

HRESULT CDX12Viewport::InitializeDevice() {
    // Create DXGI Factory
    if (FAILED(CreateDXGIFactory2(0, IID_PPV_ARGS(&m_factory)))) {
        return E_FAIL;
    }

    // Find suitable adapter
    for (UINT i = 0; SUCCEEDED(m_factory->EnumAdapterByGpuPreference(
        i, DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE, IID_PPV_ARGS(&m_adapter))); ++i) {
        break; // Use first suitable adapter
    }

    // Create device
    if (FAILED(D3D12CreateDevice(m_adapter.Get(), D3D_FEATURE_LEVEL_12_1, IID_PPV_ARGS(&m_device)))) {
        return E_FAIL;
    }

    // Create command queue
    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
    queueDesc.Priority = D3D12_COMMAND_QUEUE_PRIORITY_NORMAL;
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;

    if (FAILED(m_device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&m_commandQueue)))) {
        return E_FAIL;
    }

    // Create swap chain
    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
    swapChainDesc.BufferCount = 3; // Triple buffering
    swapChainDesc.Width = m_width;
    swapChainDesc.Height = m_height;
    swapChainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    swapChainDesc.SampleDesc.Count = 1;

    ComPtr<IDXGISwapChain1> swapChain;
    if (FAILED(m_factory->CreateSwapChainForHwnd(
        m_commandQueue.Get(), m_hwnd, &swapChainDesc, nullptr, nullptr, &swapChain))) {
        return E_FAIL;
    }

    if (FAILED(swapChain.As(&m_swapChain))) {
        return E_FAIL;
    }

    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();

    // Create RTV descriptor heap
    D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
    rtvHeapDesc.NumDescriptors = 3;
    rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
    rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;

    if (FAILED(m_device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&m_rtvHeap)))) {
        return E_FAIL;
    }

    // Get render targets
    UINT rtvDescriptorSize = m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = m_rtvHeap->GetCPUDescriptorHandleForHeapStart();

    for (UINT i = 0; i < 3; i++) {
        if (FAILED(m_swapChain->GetBuffer(i, IID_PPV_ARGS(&m_renderTargets[i])))) {
            return E_FAIL;
        }
        m_device->CreateRenderTargetView(m_renderTargets[i].Get(), nullptr, rtvHandle);
        rtvHandle.ptr += rtvDescriptorSize;
    }

    // Create DSV descriptor heap
    D3D12_DESCRIPTOR_HEAP_DESC dsvHeapDesc = {};
    dsvHeapDesc.NumDescriptors = 1;
    dsvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_DSV;

    if (FAILED(m_device->CreateDescriptorHeap(&dsvHeapDesc, IID_PPV_ARGS(&m_dsvHeap)))) {
        return E_FAIL;
    }

    // Create depth stencil buffer
    D3D12_RESOURCE_DESC depthStencilDesc = {};
    depthStencilDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
    depthStencilDesc.Width = m_width;
    depthStencilDesc.Height = m_height;
    depthStencilDesc.DepthOrArraySize = 1;
    depthStencilDesc.MipLevels = 1;
    depthStencilDesc.Format = DXGI_FORMAT_D32_FLOAT;
    depthStencilDesc.SampleDesc.Count = 1;
    depthStencilDesc.Flags = D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;

    D3D12_CLEAR_VALUE clearValue = {};
    clearValue.Format = DXGI_FORMAT_D32_FLOAT;
    clearValue.DepthStencil.Depth = 1.0f;
    clearValue.DepthStencil.Stencil = 0;

    D3D12_HEAP_PROPERTIES heapProps = {};
    heapProps.Type = D3D12_HEAP_TYPE_DEFAULT;

    if (FAILED(m_device->CreateCommittedResource(
        &heapProps, D3D12_HEAP_FLAG_NONE, &depthStencilDesc,
        D3D12_RESOURCE_STATE_DEPTH_WRITE, &clearValue, IID_PPV_ARGS(&m_depthStencilBuffer)))) {
        return E_FAIL;
    }

    m_device->CreateDepthStencilView(m_depthStencilBuffer.Get(), nullptr,
        m_dsvHeap->GetCPUDescriptorHandleForHeapStart());

    // Create command allocators
    for (UINT i = 0; i < 3; i++) {
        if (FAILED(m_device->CreateCommandAllocator(
            D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_commandAllocators[i])))) {
            return E_FAIL;
        }
    }

    // Create command list
    if (FAILED(m_device->CreateCommandList(
        0, D3D12_COMMAND_LIST_TYPE_DIRECT, m_commandAllocators[0].Get(),
        nullptr, IID_PPV_ARGS(&m_commandList)))) {
        return E_FAIL;
    }

    m_commandList->Close();

    // Create fence
    if (FAILED(m_device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_fence)))) {
        return E_FAIL;
    }

    m_fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
    if (!m_fenceEvent) {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CDX12Viewport::InitializePipeline() {
    if (FAILED(CreateRootSignature())) {
        return E_FAIL;
    }

    if (FAILED(CreatePipelineState())) {
        return E_FAIL;
    }

    if (FAILED(CreateBuffers())) {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CDX12Viewport::CreateRootSignature() {
    D3D12_ROOT_PARAMETER rootParams[1] = {};
    D3D12_DESCRIPTOR_RANGE cbvRange = {};
    cbvRange.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
    cbvRange.NumDescriptors = 1;
    cbvRange.BaseShaderRegister = 0;

    rootParams[0].ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
    rootParams[0].DescriptorTable.NumDescriptorRanges = 1;
    rootParams[0].DescriptorTable.pDescriptorRanges = &cbvRange;
    rootParams[0].ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;

    D3D12_ROOT_SIGNATURE_DESC rootSigDesc = {};
    rootSigDesc.NumParameters = 1;
    rootSigDesc.pParameters = rootParams;
    rootSigDesc.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

    ComPtr<ID3DBlob> signature, error;
    if (FAILED(D3D12SerializeRootSignature(&rootSigDesc, D3D_ROOT_SIGNATURE_VERSION_1, &signature, &error))) {
        return E_FAIL;
    }

    if (FAILED(m_device->CreateRootSignature(0, signature->GetBufferPointer(), signature->GetBufferSize(),
        IID_PPV_ARGS(&m_rootSignature)))) {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CDX12Viewport::CreatePipelineState() {
    // Compile shaders (simplified - in production load precompiled)
    ComPtr<ID3DBlob> vsBlob, psBlob;

    // Simple vertex shader
    const char* vsCode = R"(
        cbuffer ConstantBuffer : register(b0) {
            float4x4 world;
            float4x4 view;
            float4x4 projection;
        };

        struct VS_INPUT {
            float3 position : POSITION;
            float3 normal : NORMAL;
            float2 texCoord : TEXCOORD0;
        };

        struct PS_INPUT {
            float4 position : SV_POSITION;
            float3 normal : NORMAL;
            float2 texCoord : TEXCOORD0;
        };

        PS_INPUT main(VS_INPUT input) {
            PS_INPUT output;
            float4 worldPos = mul(float4(input.position, 1.0f), world);
            float4 viewPos = mul(worldPos, view);
            output.position = mul(viewPos, projection);
            output.normal = mul(input.normal, (float3x3)world);
            output.texCoord = input.texCoord;
            return output;
        }
    )";

    // Simple pixel shader
    const char* psCode = R"(
        struct PS_INPUT {
            float4 position : SV_POSITION;
            float3 normal : NORMAL;
            float2 texCoord : TEXCOORD0;
        };

        float4 main(PS_INPUT input) : SV_TARGET {
            float3 normal = normalize(input.normal);
            float lighting = max(dot(normal, float3(1, 1, 1)), 0.3f);
            return float4(lighting, lighting, lighting, 1.0f);
        }
    )";

    if (FAILED(D3DCompile(vsCode, strlen(vsCode), nullptr, nullptr, nullptr, "main", "vs_5_0", 0, 0, &vsBlob, nullptr))) {
        return E_FAIL;
    }

    if (FAILED(D3DCompile(psCode, strlen(psCode), nullptr, nullptr, nullptr, "main", "ps_5_0", 0, 0, &psBlob, nullptr))) {
        return E_FAIL;
    }

    // Input layout
    D3D12_INPUT_ELEMENT_DESC inputLayout[] = {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
        { "NORMAL", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
        { "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 24, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 }
    };

    D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
    psoDesc.pRootSignature = m_rootSignature.Get();
    psoDesc.VS = { vsBlob->GetBufferPointer(), vsBlob->GetBufferSize() };
    psoDesc.PS = { psBlob->GetBufferPointer(), psBlob->GetBufferSize() };
    psoDesc.InputLayout = { inputLayout, _countof(inputLayout) };
    psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
    psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
    psoDesc.NumRenderTargets = 1;
    psoDesc.DSVFormat = DXGI_FORMAT_D32_FLOAT;
    psoDesc.SampleDesc.Count = 1;
    psoDesc.RasterizerState.FillMode = D3D12_FILL_MODE_SOLID;
    psoDesc.RasterizerState.CullMode = D3D12_CULL_MODE_BACK;
    psoDesc.BlendState.RenderTarget[0].RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE_ALL;
    psoDesc.DepthStencilState.DepthEnable = TRUE;
    psoDesc.DepthStencilState.DepthFunc = D3D12_COMPARISON_FUNC_LESS;
    psoDesc.DepthStencilState.DepthWriteMask = D3D12_DEPTH_WRITE_MASK_ALL;

    if (FAILED(m_device->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&m_pipelineState)))) {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CDX12Viewport::CreateBuffers() {
    // Create CBV descriptor heap
    D3D12_DESCRIPTOR_HEAP_DESC cbvHeapDesc = {};
    cbvHeapDesc.NumDescriptors = 1;
    cbvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
    cbvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;

    if (FAILED(m_device->CreateDescriptorHeap(&cbvHeapDesc, IID_PPV_ARGS(&m_cbvHeap)))) {
        return E_FAIL;
    }

    // Create constant buffer
    D3D12_HEAP_PROPERTIES heapProps = {};
    heapProps.Type = D3D12_HEAP_TYPE_UPLOAD;

    D3D12_RESOURCE_DESC resourceDesc = {};
    resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
    resourceDesc.Width = (sizeof(ConstantBuffer) + 255) & ~255; // Align to 256 bytes
    resourceDesc.Height = 1;
    resourceDesc.DepthOrArraySize = 1;
    resourceDesc.MipLevels = 1;
    resourceDesc.Format = DXGI_FORMAT_UNKNOWN;
    resourceDesc.SampleDesc.Count = 1;
    resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;

    if (FAILED(m_device->CreateCommittedResource(&heapProps, D3D12_HEAP_FLAG_NONE,
        &resourceDesc, D3D12_RESOURCE_STATE_GENERIC_READ, nullptr, IID_PPV_ARGS(&m_constantBuffer)))) {
        return E_FAIL;
    }

    // Create CBV
    D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc = {};
    cbvDesc.BufferLocation = m_constantBuffer->GetGPUVirtualAddress();
    cbvDesc.SizeInBytes = (sizeof(ConstantBuffer) + 255) & ~255;

    m_device->CreateConstantBufferView(&cbvDesc, m_cbvHeap->GetCPUDescriptorHandleForHeapStart());

    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::LoadModelFromFile(const wchar_t* filePath) {
    if (!filePath) {
        SetError(L"Invalid file path");
        return E_INVALIDARG;
    }
    // TODO: Load from file and call UpdateModelData
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::UpdateModelData(Vertex* vertices, UINT vertexCount, UINT* indices, UINT indexCount) {
    if (!vertices || !indices || vertexCount == 0 || indexCount == 0) {
        SetError(L"Invalid model data");
        return E_INVALIDARG;
    }

    m_indexCount = indexCount;

    // Create vertex buffer
    D3D12_HEAP_PROPERTIES heapProps = {};
    heapProps.Type = D3D12_HEAP_TYPE_UPLOAD;

    D3D12_RESOURCE_DESC resourceDesc = {};
    resourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
    resourceDesc.Width = vertexCount * sizeof(Vertex);
    resourceDesc.Height = 1;
    resourceDesc.DepthOrArraySize = 1;
    resourceDesc.MipLevels = 1;
    resourceDesc.Format = DXGI_FORMAT_UNKNOWN;
    resourceDesc.SampleDesc.Count = 1;
    resourceDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;

    if (FAILED(m_device->CreateCommittedResource(&heapProps, D3D12_HEAP_FLAG_NONE,
        &resourceDesc, D3D12_RESOURCE_STATE_GENERIC_READ, nullptr, IID_PPV_ARGS(&m_vertexBuffer)))) {
        return E_FAIL;
    }

    // Copy vertex data
    void* mappedData = nullptr;
    if (FAILED(m_vertexBuffer->Map(0, nullptr, &mappedData))) {
        return E_FAIL;
    }
    memcpy(mappedData, vertices, vertexCount * sizeof(Vertex));
    m_vertexBuffer->Unmap(0, nullptr);

    m_vertexBufferView.BufferLocation = m_vertexBuffer->GetGPUVirtualAddress();
    m_vertexBufferView.StrideInBytes = sizeof(Vertex);
    m_vertexBufferView.SizeInBytes = vertexCount * sizeof(Vertex);

    // Create index buffer
    resourceDesc.Width = indexCount * sizeof(UINT);

    if (FAILED(m_device->CreateCommittedResource(&heapProps, D3D12_HEAP_FLAG_NONE,
        &resourceDesc, D3D12_RESOURCE_STATE_GENERIC_READ, nullptr, IID_PPV_ARGS(&m_indexBuffer)))) {
        return E_FAIL;
    }

    // Copy index data
    if (FAILED(m_indexBuffer->Map(0, nullptr, &mappedData))) {
        return E_FAIL;
    }
    memcpy(mappedData, indices, indexCount * sizeof(UINT));
    m_indexBuffer->Unmap(0, nullptr);

    m_indexBufferView.BufferLocation = m_indexBuffer->GetGPUVirtualAddress();
    m_indexBufferView.Format = DXGI_FORMAT_R32_UINT;
    m_indexBufferView.SizeInBytes = indexCount * sizeof(UINT);

    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::Render() {
    if (!m_initialized) {
        return E_FAIL;
    }

    WaitForGPU();

    HRESULT hr = m_commandAllocators[m_frameIndex]->Reset();
    if (FAILED(hr)) return hr;

    hr = m_commandList->Reset(m_commandAllocators[m_frameIndex].Get(), m_pipelineState.Get());
    if (FAILED(hr)) return hr;

    UpdateCamera();
    UpdateConstantBuffer();

    // Set viewport and scissor
    D3D12_VIEWPORT viewport = { 0.0f, 0.0f, (float)m_width, (float)m_height, 0.0f, 1.0f };
    D3D12_RECT scissorRect = { 0, 0, (LONG)m_width, (LONG)m_height };
    m_commandList->RSSetViewports(1, &viewport);
    m_commandList->RSSetScissorRects(1, &scissorRect);

    // Set render targets
    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = m_rtvHeap->GetCPUDescriptorHandleForHeapStart();
    rtvHandle.ptr += m_frameIndex * m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);

    D3D12_CPU_DESCRIPTOR_HANDLE dsvHandle = m_dsvHeap->GetCPUDescriptorHandleForHeapStart();

    m_commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, &dsvHandle);

    // Clear render target
    float clearColor[] = { 0.1f, 0.1f, 0.1f, 1.0f };
    m_commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);
    m_commandList->ClearDepthStencilView(dsvHandle, D3D12_CLEAR_FLAG_DEPTH, 1.0f, 0, 0, nullptr);

    // Render model if available
    if (m_indexCount > 0) {
        RenderModel();
    }

    // Render debug elements
    if (m_showGrid) RenderGrid();
    if (m_showAxes) RenderAxes();
    if (m_showBounds) RenderBounds();

    // Close command list and execute
    m_commandList->Close();
    ID3D12CommandList* ppCommandLists[] = { m_commandList.Get() };
    m_commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);

    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::Present() {
    HRESULT hr = m_swapChain->Present(1, 0);
    if (FAILED(hr)) return hr;

    return MoveToNextFrame();
}

void CDX12Viewport::UpdateCamera() {
    // Calculate camera position based on orbital values
    float radYaw = m_camera.yaw * XM_PI / 180.0f;
    float radPitch = m_camera.pitch * XM_PI / 180.0f;

    m_camera.position.x = m_camera.distance * cosf(radPitch) * sinf(radYaw);
    m_camera.position.y = m_camera.distance * sinf(radPitch);
    m_camera.position.z = m_camera.distance * cosf(radPitch) * cosf(radYaw);
}

void CDX12Viewport::UpdateConstantBuffer() {
    ConstantBuffer cb = {};

    // Camera matrices
    XMVECTOR pos = XMLoadFloat3(&m_camera.position);
    XMVECTOR target = XMLoadFloat3(&m_camera.target);
    XMVECTOR up = XMLoadFloat3(&m_camera.up);

    cb.world = XMMatrixIdentity();
    cb.view = XMMatrixLookAtLH(pos, target, up);
    cb.projection = XMMatrixPerspectiveFovLH(
        m_camera.fov * XM_PI / 180.0f,
        m_camera.aspectRatio,
        m_camera.nearPlane,
        m_camera.farPlane
    );
    cb.cameraPos = { m_camera.position.x, m_camera.position.y, m_camera.position.z, 1.0f };

    // Material
    cb.material = m_material;

    // Lights
    cb.lightCount = static_cast<int>(m_lights.size());
    for (size_t i = 0; i < m_lights.size() && i < 4; i++) {
        cb.lights[i] = m_lights[i];
    }

    // Update constant buffer
    void* mappedData = nullptr;
    if (SUCCEEDED(m_constantBuffer->Map(0, nullptr, &mappedData))) {
        memcpy(mappedData, &cb, sizeof(ConstantBuffer));
        m_constantBuffer->Unmap(0, nullptr);
    }
}

void CDX12Viewport::RenderModel() {
    m_commandList->SetGraphicsRootSignature(m_rootSignature.Get());
    m_commandList->SetDescriptorHeaps(1, m_cbvHeap.GetAddressOf());
    m_commandList->SetGraphicsRootDescriptorTable(0, m_cbvHeap->GetGPUDescriptorHandleForHeapStart());

    m_commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
    m_commandList->IASetVertexBuffers(0, 1, &m_vertexBufferView);
    m_commandList->IASetIndexBuffer(&m_indexBufferView);

    m_commandList->DrawIndexedInstanced(m_indexCount, 1, 0, 0, 0);
}

void CDX12Viewport::RenderGrid() {
    // TODO: Implement grid rendering
}

void CDX12Viewport::RenderAxes() {
    // TODO: Implement axes rendering
}

void CDX12Viewport::RenderBounds() {
    // TODO: Implement bounds rendering
}

HRESULT CDX12Viewport::WaitForGPU() {
    if (FAILED(m_commandQueue->Signal(m_fence.Get(), m_fenceValues[m_frameIndex]))) {
        return E_FAIL;
    }

    if (FAILED(m_fence->SetEventOnCompletion(m_fenceValues[m_frameIndex], m_fenceEvent))) {
        return E_FAIL;
    }

    WaitForSingleObjectEx(m_fenceEvent, INFINITE, FALSE);
    m_fenceValues[m_frameIndex]++;

    return S_OK;
}

HRESULT CDX12Viewport::MoveToNextFrame() {
    m_fenceValues[m_frameIndex]++;
    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();

    if (m_fence->GetCompletedValue() < m_fenceValues[m_frameIndex]) {
        if (FAILED(m_fence->SetEventOnCompletion(m_fenceValues[m_frameIndex], m_fenceEvent))) {
            return E_FAIL;
        }
        WaitForSingleObjectEx(m_fenceEvent, INFINITE, FALSE);
    }

    // Update FPS
    auto now = std::chrono::high_resolution_clock::now();
    auto elapsed = std::chrono::duration_cast<std::chrono::milliseconds>(now - m_lastFrameTime).count();

    m_frameCount++;
    if (elapsed >= 1000) {
        m_fps = (m_frameCount * 1000.0f) / elapsed;
        m_frameCount = 0;
        m_lastFrameTime = now;
    }

    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::SetCameraOrbital(float yaw, float pitch, float distance) {
    m_camera.yaw = yaw;
    m_camera.pitch = pitch;
    m_camera.distance = distance;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::SetCameraPan(float panX, float panY) {
    m_camera.target.x += panX;
    m_camera.target.y += panY;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::SetCameraZoom(float zoomFactor) {
    m_camera.distance *= zoomFactor;
    m_camera.distance = max(0.1f, min(1000.0f, m_camera.distance));
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::ResetCamera() {
    m_camera.position = { 0.0f, 0.0f, 5.0f };
    m_camera.target = { 0.0f, 0.0f, 0.0f };
    m_camera.yaw = 0.0f;
    m_camera.pitch = 30.0f;
    m_camera.distance = 5.0f;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::SetShadingMode(int mode) {
    m_shadingMode = mode;
    // TODO: Switch between wireframe, solid, material, render modes
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::ShowGrid(BOOL visible) {
    m_showGrid = visible ? true : false;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::ShowAxes(BOOL visible) {
    m_showAxes = visible ? true : false;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::ShowBounds(BOOL visible) {
    m_showBounds = visible ? true : false;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::SetMaterialMetallic(float metallic) {
    m_material.metallic = metallic;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::SetMaterialRoughness(float roughness) {
    m_material.roughness = roughness;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::SetMaterialAO(float ao) {
    m_material.aoFactor = ao;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::AddLight(Light* light) {
    if (light && m_lights.size() < 4) {
        m_lights.push_back(*light);
        return S_OK;
    }
    return E_FAIL;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::ClearLights() {
    m_lights.clear();
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::GetFrameRate(float* outFPS) {
    if (!outFPS) return E_INVALIDARG;
    *outFPS = m_fps;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::GetGPUMemoryUsage(UINT64* outBytes) {
    if (!outBytes) return E_INVALIDARG;
    // TODO: Query GPU memory
    *outBytes = 0;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::GetLastError(wchar_t* outError, UINT errorLength) {
    if (!outError || errorLength == 0) return E_INVALIDARG;
    wcsncpy_s(outError, errorLength, m_lastError.c_str(), _TRUNCATE);
    return S_OK;
}

void CDX12Viewport::SetError(const wchar_t* error) {
    m_lastError = error ? error : L"Unknown error";
}

HRESULT STDMETHODCALLTYPE CDX12Viewport::Shutdown() {
    if (m_fenceEvent) {
        CloseHandle(m_fenceEvent);
        m_fenceEvent = nullptr;
    }

    WaitForGPU();

    m_renderTargets[0].Reset();
    m_renderTargets[1].Reset();
    m_renderTargets[2].Reset();
    m_depthStencilBuffer.Reset();
    m_vertexBuffer.Reset();
    m_indexBuffer.Reset();
    m_constantBuffer.Reset();
    m_commandList.Reset();
    m_rtvHeap.Reset();
    m_dsvHeap.Reset();
    m_cbvHeap.Reset();
    m_pipelineState.Reset();
    m_rootSignature.Reset();
    m_commandQueue.Reset();
    m_swapChain.Reset();
    m_device.Reset();
    m_adapter.Reset();
    m_factory.Reset();

    m_initialized = false;
    return S_OK;
}

// ==================== CLASS FACTORY ====================

HRESULT STDMETHODCALLTYPE DX12ViewportClassFactory::QueryInterface(REFIID riid, void** ppvObject) {
    if (!ppvObject) return E_INVALIDARG;

    if (riid == IID_IUnknown || riid == IID_IClassFactory) {
        *ppvObject = static_cast<IClassFactory*>(this);
        AddRef();
        return S_OK;
    }

    *ppvObject = nullptr;
    return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE DX12ViewportClassFactory::AddRef() {
    return ++m_refCount;
}

ULONG STDMETHODCALLTYPE DX12ViewportClassFactory::Release() {
    ULONG refCount = --m_refCount;
    if (refCount == 0) {
        delete this;
    }
    return refCount;
}

HRESULT STDMETHODCALLTYPE DX12ViewportClassFactory::CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppvObject) {
    if (pUnkOuter) return CLASS_E_NOAGGREGATION;
    if (!ppvObject) return E_INVALIDARG;

    auto* pViewport = new CDX12Viewport();
    if (!pViewport) return E_OUTOFMEMORY;

    HRESULT hr = pViewport->QueryInterface(riid, ppvObject);
    pViewport->Release();
    return hr;
}

HRESULT STDMETHODCALLTYPE DX12ViewportClassFactory::LockServer(BOOL fLock) {
    return S_OK;
}

// ==================== DLL EXPORTS ====================

static DX12ViewportClassFactory* g_classFactory = nullptr;

HRESULT STDAPICALLTYPE DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv) {
    if (rclsid != CLSID_DX12Viewport) {
        return CLASS_E_CLASSNOTAVAILABLE;
    }

    if (!g_classFactory) {
        g_classFactory = new DX12ViewportClassFactory();
    }

    return g_classFactory->QueryInterface(riid, ppv);
}

HRESULT STDAPICALLTYPE DllCanUnloadNow() {
    return S_OK;
}

HRESULT STDAPICALLTYPE DllRegisterServer() {
    return S_OK;
}

HRESULT STDAPICALLTYPE DllUnregisterServer() {
    return S_OK;
}
