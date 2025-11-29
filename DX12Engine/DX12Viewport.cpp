#include "pch.h"
#include "DX12Viewport.h"

struct Vertex
{
    float x, y, z;
    float r, g, b, a;
};

DX12Viewport::DX12Viewport()
    : m_refCount(1),
    m_fenceValue(1),
    m_frameIndex(0),
    m_fenceEvent(nullptr),
    m_rtvDescriptorSize(0)
{
    ZeroMemory(&m_vertexBufferView, sizeof(m_vertexBufferView));
}

DX12Viewport::~DX12Viewport()
{
    if (m_fenceEvent)
    {
        CloseHandle(m_fenceEvent);
    }
}

HRESULT __stdcall DX12Viewport::QueryInterface(REFIID riid, void** ppvObject)
{
    if (!ppvObject) return E_INVALIDARG;

    if (riid == IID_IUnknown || riid == __uuidof(ID3D12Viewport))
    {
        *ppvObject = this;
        AddRef();
        return S_OK;
    }

    *ppvObject = nullptr;
    return E_NOINTERFACE;
}

ULONG __stdcall DX12Viewport::AddRef()
{
    return InterlockedIncrement(&m_refCount);
}

ULONG __stdcall DX12Viewport::Release()
{
    LONG count = InterlockedDecrement(&m_refCount);
    if (count == 0)
    {
        delete this;
    }
    return count;
}

HRESULT DX12Viewport::Initialize(HWND hwnd, UINT width, UINT height)
{
    if (!hwnd || width == 0 || height == 0) return E_INVALIDARG;

    HRESULT hr = CreateDevice();
    if (FAILED(hr)) return hr;

    hr = CreateCommandInfrastructure();
    if (FAILED(hr)) return hr;

    hr = CreateSwapChain(hwnd, width, height);
    if (FAILED(hr)) return hr;

    hr = CreateRenderTargets();
    if (FAILED(hr)) return hr;

    hr = CreateVertexBuffer();
    if (FAILED(hr)) return hr;

    hr = CreatePipelineState();
    if (FAILED(hr)) return hr;

    hr = m_device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_fence));
    if (FAILED(hr)) return hr;

    m_fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
    if (!m_fenceEvent) return E_FAIL;

    return S_OK;
}

HRESULT DX12Viewport::CreateDevice()
{
    Microsoft::WRL::ComPtr<IDXGIFactory4> factory;
    HRESULT hr = CreateDXGIFactory1(IID_PPV_ARGS(&factory));
    if (FAILED(hr)) return hr;

    Microsoft::WRL::ComPtr<IDXGIAdapter1> adapter;
    BOOL foundAdapter = FALSE;

    for (UINT adapterIndex = 0; SUCCEEDED(factory->EnumAdapters1(adapterIndex, &adapter)); ++adapterIndex)
    {
        DXGI_ADAPTER_DESC1 desc;
        adapter->GetDesc1(&desc);
        if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE) continue;
        foundAdapter = TRUE;
        break;
    }

    if (!foundAdapter)
    {
        adapter.Reset();
    }

    hr = D3D12CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_0, IID_PPV_ARGS(&m_device));
    return hr;
}

HRESULT DX12Viewport::CreateCommandInfrastructure()
{
    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
    queueDesc.Priority = D3D12_COMMAND_QUEUE_PRIORITY_NORMAL;
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;

    HRESULT hr = m_device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&m_commandQueue));
    if (FAILED(hr)) return hr;

    hr = m_device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_commandAllocator));
    if (FAILED(hr)) return hr;

    hr = m_device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, m_commandAllocator.Get(), nullptr, IID_PPV_ARGS(&m_commandList));
    if (FAILED(hr)) return hr;

    return m_commandList->Close();
}

HRESULT DX12Viewport::CreateSwapChain(HWND hwnd, UINT width, UINT height)
{
    Microsoft::WRL::ComPtr<IDXGIFactory4> factory;
    HRESULT hr = CreateDXGIFactory1(IID_PPV_ARGS(&factory));
    if (FAILED(hr)) return hr;

    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
    swapChainDesc.BufferCount = 2;
    swapChainDesc.Width = width;
    swapChainDesc.Height = height;
    swapChainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    swapChainDesc.SampleDesc.Count = 1;
    swapChainDesc.SampleDesc.Quality = 0;

    Microsoft::WRL::ComPtr<IDXGISwapChain1> swapChain1;
    hr = factory->CreateSwapChainForHwnd(m_commandQueue.Get(), hwnd, &swapChainDesc, nullptr, nullptr, &swapChain1);
    if (FAILED(hr)) return hr;

    hr = swapChain1.As(&m_swapChain);
    if (FAILED(hr)) return hr;

    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();
    return S_OK;
}

HRESULT DX12Viewport::CreateRenderTargets()
{
    D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
    heapDesc.NumDescriptors = 2;
    heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
    heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;

    HRESULT hr = m_device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&m_rtvHeap));
    if (FAILED(hr)) return hr;

    m_rtvDescriptorSize = m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);

    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = m_rtvHeap->GetCPUDescriptorHandleForHeapStart();
    for (int i = 0; i < 2; ++i)
    {
        hr = m_swapChain->GetBuffer(i, IID_PPV_ARGS(&m_renderTargets[i]));
        if (FAILED(hr)) return hr;

        m_device->CreateRenderTargetView(m_renderTargets[i].Get(), nullptr, rtvHandle);
        rtvHandle.ptr += m_rtvDescriptorSize;
    }

    return S_OK;
}

HRESULT DX12Viewport::CreateVertexBuffer()
{
    Vertex vertices[] =
    {
        { 0.0f,  0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f },
        { 0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f },
        {-0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f }
    };

    const UINT vertexBufferSize = sizeof(vertices);

    D3D12_HEAP_PROPERTIES heapProps = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD);
    D3D12_RESOURCE_DESC resourceDesc = CD3DX12_RESOURCE_DESC::Buffer(vertexBufferSize);

    HRESULT hr = m_device->CreateCommittedResource(
        &heapProps,
        D3D12_HEAP_FLAG_NONE,
        &resourceDesc,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nullptr,
        IID_PPV_ARGS(&m_vertexBuffer));
    if (FAILED(hr)) return hr;

    UINT8* pVertexDataBegin = nullptr;
    D3D12_RANGE readRange = { 0, 0 };
    hr = m_vertexBuffer->Map(0, &readRange, reinterpret_cast<void**>(&pVertexDataBegin));
    if (FAILED(hr)) return hr;

    memcpy(pVertexDataBegin, vertices, sizeof(vertices));
    m_vertexBuffer->Unmap(0, nullptr);

    m_vertexBufferView.BufferLocation = m_vertexBuffer->GetGPUVirtualAddress();
    m_vertexBufferView.StrideInBytes = sizeof(Vertex);
    m_vertexBufferView.SizeInBytes = vertexBufferSize;

    return S_OK;
}

HRESULT DX12Viewport::CreatePipelineState()
{
    const char* vertexShader = R"(
        struct VS_INPUT { float3 pos : POSITION; float4 col : COLOR; };
        struct VS_OUTPUT { float4 pos : SV_POSITION; float4 col : COLOR; };
        VS_OUTPUT main(VS_INPUT input) { 
            VS_OUTPUT output;
            output.pos = float4(input.pos, 1.0f);
            output.col = input.col;
            return output;
        }
    )";

    const char* pixelShader = R"(
        struct PS_INPUT { float4 pos : SV_POSITION; float4 col : COLOR; };
        float4 main(PS_INPUT input) : SV_TARGET { return input.col; }
    )";

    Microsoft::WRL::ComPtr<ID3DBlob> vsBlob, psBlob, errorBlob;

    HRESULT hr = D3DCompile(vertexShader, strlen(vertexShader), nullptr, nullptr, nullptr, "main", "vs_5_0", 0, 0, &vsBlob, &errorBlob);
    if (FAILED(hr)) return hr;

    hr = D3DCompile(pixelShader, strlen(pixelShader), nullptr, nullptr, nullptr, "main", "ps_5_0", 0, 0, &psBlob, &errorBlob);
    if (FAILED(hr)) return hr;

    D3D12_ROOT_SIGNATURE_DESC rootSignDesc = {};
    rootSignDesc.NumParameters = 0;
    rootSignDesc.pParameters = nullptr;
    rootSignDesc.NumStaticSamplers = 0;
    rootSignDesc.pStaticSamplers = nullptr;
    rootSignDesc.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

    Microsoft::WRL::ComPtr<ID3DBlob> signatureBlob;
    hr = D3D12SerializeRootSignature(&rootSignDesc, D3D_ROOT_SIGNATURE_VERSION_1, &signatureBlob, &errorBlob);
    if (FAILED(hr)) return hr;

    hr = m_device->CreateRootSignature(0, signatureBlob->GetBufferPointer(), signatureBlob->GetBufferSize(), IID_PPV_ARGS(&m_rootSignature));
    if (FAILED(hr)) return hr;

    D3D12_INPUT_ELEMENT_DESC inputLayout[] =
    {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
        { "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 }
    };

    D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
    psoDesc.pRootSignature = m_rootSignature.Get();
    psoDesc.VS = { vsBlob->GetBufferPointer(), vsBlob->GetBufferSize() };
    psoDesc.PS = { psBlob->GetBufferPointer(), psBlob->GetBufferSize() };
    psoDesc.InputLayout = { inputLayout, _countof(inputLayout) };
    psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
    psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
    psoDesc.NumRenderTargets = 1;
    psoDesc.SampleMask = UINT_MAX;
    psoDesc.SampleDesc.Count = 1;
    psoDesc.SampleDesc.Quality = 0;
    psoDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
    psoDesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);
    psoDesc.DepthStencilState.DepthEnable = FALSE;

    return m_device->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&m_pipelineState));
}

void DX12Viewport::PopulateCommandList()
{
    m_commandAllocator->Reset();
    m_commandList->Reset(m_commandAllocator.Get(), m_pipelineState.Get());
    m_commandList->SetGraphicsRootSignature(m_rootSignature.Get());

    D3D12_RESOURCE_BARRIER barrier = CD3DX12_RESOURCE_BARRIER::Transition(
        m_renderTargets[m_frameIndex].Get(),
        D3D12_RESOURCE_STATE_PRESENT,
        D3D12_RESOURCE_STATE_RENDER_TARGET);
    m_commandList->ResourceBarrier(1, &barrier);

    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = m_rtvHeap->GetCPUDescriptorHandleForHeapStart();
    rtvHandle.ptr += m_frameIndex * m_rtvDescriptorSize;
    m_commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, nullptr);

    const float clearColor[] = { 0.0f, 0.0f, 0.0f, 1.0f };
    m_commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);

    D3D12_VIEWPORT viewport = { 0.0f, 0.0f, 1840.0f, 1010.0f, 0.0f, 1.0f };
    D3D12_RECT scissorRect = { 0, 0, 1840, 1010 };
    m_commandList->RSSetViewports(1, &viewport);
    m_commandList->RSSetScissorRects(1, &scissorRect);

    m_commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
    m_commandList->IASetVertexBuffers(0, 1, &m_vertexBufferView);
    m_commandList->DrawInstanced(3, 1, 0, 0);

    barrier = CD3DX12_RESOURCE_BARRIER::Transition(
        m_renderTargets[m_frameIndex].Get(),
        D3D12_RESOURCE_STATE_RENDER_TARGET,
        D3D12_RESOURCE_STATE_PRESENT);
    m_commandList->ResourceBarrier(1, &barrier);

    m_commandList->Close();
}

void DX12Viewport::WaitForPreviousFrame()
{
    const UINT64 fence = m_fenceValue;
    m_commandQueue->Signal(m_fence.Get(), fence);
    m_fenceValue++;

    if (m_fence->GetCompletedValue() < fence)
    {
        m_fence->SetEventOnCompletion(fence, m_fenceEvent);
        WaitForSingleObject(m_fenceEvent, INFINITE);
    }

    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();
}

HRESULT DX12Viewport::Render()
{
    if (!m_device || !m_swapChain) return E_NOT_VALID_STATE;

    PopulateCommandList();

    ID3D12CommandList* ppCommandLists[] = { m_commandList.Get() };
    m_commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);

    HRESULT hr = m_swapChain->Present(1, 0);
    if (FAILED(hr)) return hr;

    WaitForPreviousFrame();

    return S_OK;
}

extern "C" __declspec(dllexport) HRESULT __stdcall CreateDX12Viewport(
    HWND hwnd, UINT width, UINT height, ID3D12Viewport** ppViewport)
{
    if (!ppViewport) return E_INVALIDARG;
    if (!hwnd) return E_INVALIDARG;
    if (width == 0 || height == 0) return E_INVALIDARG;

    DX12Viewport* pViewport = new DX12Viewport();
    if (!pViewport) return E_OUTOFMEMORY;

    HRESULT hr = pViewport->Initialize(hwnd, width, height);
    if (FAILED(hr))
    {
        pViewport->Release();
        return hr;
    }

    *ppViewport = pViewport;
    return S_OK;
}