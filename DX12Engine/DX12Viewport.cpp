#include "pch.h"
#include "DX12Viewport.h"

// ========================================================
// CLASS IMPLEMENTATION
// ========================================================

DX12Viewport::DX12Viewport()
    : m_isInitialized(false), m_hwnd(nullptr), m_width(0), m_height(0),
    m_fenceEvent(nullptr), m_fenceValue(0), m_frameIndex(0), m_rtvDescriptorSize(0)
{
}

DX12Viewport::~DX12Viewport()
{
    Shutdown();
}

bool DX12Viewport::Initialize(void* hwnd, int width, int height)
{
    if (m_isInitialized) return true;
    m_hwnd = hwnd;
    m_width = width;
    m_height = height;

    if (!CreateDevice()) return false;
    if (!CreateSwapChain()) return false;
    if (!CreateResources()) return false;

    m_isInitialized = true;
    return true;
}

void DX12Viewport::Shutdown()
{
    WaitForPreviousFrame();
    if (m_fenceEvent) CloseHandle(m_fenceEvent);
    m_isInitialized = false;
}

void DX12Viewport::Render()
{
    if (!m_isInitialized) return;

    // 1. Reset Command Allocators
    m_commandAllocator->Reset();
    m_commandList->Reset(m_commandAllocator.Get(), nullptr);

    // 2. Barrier: Present -> RenderTarget
    auto barrier = CD3DX12_RESOURCE_BARRIER::Transition(
        m_renderTargets[m_frameIndex].Get(),
        D3D12_RESOURCE_STATE_PRESENT,
        D3D12_RESOURCE_STATE_RENDER_TARGET);
    m_commandList->ResourceBarrier(1, &barrier);

    // 3. Clear & Set Render Target
    CD3DX12_CPU_DESCRIPTOR_HANDLE rtvHandle(
        m_rtvHeap->GetCPUDescriptorHandleForHeapStart(),
        m_frameIndex,
        m_rtvDescriptorSize);

    const float clearColor[] = { 0.1f, 0.1f, 0.1f, 1.0f }; // Dark Grey Background
    m_commandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);
    m_commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, nullptr);

    // 4. Barrier: RenderTarget -> Present
    barrier = CD3DX12_RESOURCE_BARRIER::Transition(
        m_renderTargets[m_frameIndex].Get(),
        D3D12_RESOURCE_STATE_RENDER_TARGET,
        D3D12_RESOURCE_STATE_PRESENT);
    m_commandList->ResourceBarrier(1, &barrier);

    // 5. Execute
    m_commandList->Close();
    ID3D12CommandList* ppCommandLists[] = { m_commandList.Get() };
    m_commandQueue->ExecuteCommandLists(1, ppCommandLists);

    // 6. Present
    m_swapChain->Present(1, 0);
    WaitForPreviousFrame();
}

void DX12Viewport::Resize(int width, int height)
{
    if (!m_isInitialized || (width == m_width && height == m_height)) return;

    WaitForPreviousFrame();
    m_width = width;
    m_height = height;

    // Release buffers
    for (int i = 0; i < 2; i++) m_renderTargets[i].Reset();

    // Resize SwapChain
    m_swapChain->ResizeBuffers(2, width, height, DXGI_FORMAT_R8G8B8A8_UNORM, 0);
    m_frameIndex = 0;

    // Recreate Views
    CD3DX12_CPU_DESCRIPTOR_HANDLE rtvHandle(m_rtvHeap->GetCPUDescriptorHandleForHeapStart());
    for (UINT i = 0; i < 2; i++)
    {
        m_swapChain->GetBuffer(i, IID_PPV_ARGS(&m_renderTargets[i]));
        m_device->CreateRenderTargetView(m_renderTargets[i].Get(), nullptr, rtvHandle);
        rtvHandle.Offset(1, m_rtvDescriptorSize);
    }
}

// --- Helpers (Minimal implementation for brevity) ---
void DX12Viewport::WaitForPreviousFrame() {
    // (Keep your existing synchronization logic here)
    const UINT64 fence = m_fenceValue;
    m_commandQueue->Signal(m_fence.Get(), fence);
    m_fenceValue++;
    if (m_fence->GetCompletedValue() < fence) {
        m_fence->SetEventOnCompletion(fence, m_fenceEvent);
        WaitForSingleObject(m_fenceEvent, INFINITE);
    }
    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();
}
bool DX12Viewport::CreateDevice() {
    // (Basic Device + Command Queue creation - Standard DX12)
    D3D12CreateDevice(nullptr, D3D_FEATURE_LEVEL_11_0, IID_PPV_ARGS(&m_device));
    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
    m_device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&m_commandQueue));
    m_device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_commandAllocator));
    m_device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, m_commandAllocator.Get(), nullptr, IID_PPV_ARGS(&m_commandList));
    m_commandList->Close();
    m_device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_fence));
    m_fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
    m_fenceValue = 1;
    return true;
}
bool DX12Viewport::CreateSwapChain() {
    // (Swap Chain creation)
    Microsoft::WRL::ComPtr<IDXGIFactory4> factory;
    CreateDXGIFactory1(IID_PPV_ARGS(&factory));
    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
    swapChainDesc.BufferCount = 2;
    swapChainDesc.Width = m_width;
    swapChainDesc.Height = m_height;
    swapChainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    swapChainDesc.SampleDesc.Count = 1;
    Microsoft::WRL::ComPtr<IDXGISwapChain1> swapChain;
    factory->CreateSwapChainForHwnd(m_commandQueue.Get(), (HWND)m_hwnd, &swapChainDesc, nullptr, nullptr, &swapChain);
    swapChain.As(&m_swapChain);
    return true;
}
bool DX12Viewport::CreateResources() {
    // (RTV Heap creation)
    D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
    rtvHeapDesc.NumDescriptors = 2;
    rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
    rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
    m_device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&m_rtvHeap));
    m_rtvDescriptorSize = m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
    Resize(m_width, m_height); // Create views
    return true;
}
void DX12Viewport::SetCamera(float x, float y, float z, float yaw, float pitch) {
    // Store camera data for rendering
}

// ========================================================
// EXPORT FUNCTIONS implementation
// ========================================================
extern "C" {
    void* CreateViewport(void* hwnd, int width, int height) {
        auto* viewport = new DX12Viewport();
        if (viewport->Initialize(hwnd, width, height)) return viewport;
        delete viewport;
        return nullptr;
    }

    void DestroyViewport(void* instance) {
        if (instance) {
            delete static_cast<DX12Viewport*>(instance);
        }
    }

    void ResizeViewport(void* instance, int width, int height) {
        if (instance) static_cast<DX12Viewport*>(instance)->Resize(width, height);
    }

    void RenderViewport(void* instance) {
        if (instance) static_cast<DX12Viewport*>(instance)->Render();
    }

    void SetCamera(void* instance, float x, float y, float z, float yaw, float pitch) {
        if (instance) static_cast<DX12Viewport*>(instance)->SetCamera(x, y, z, yaw, pitch);
    }
}
