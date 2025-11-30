#pragma once
#include "pch.h"

// Standard C++ Class - NO COM, NO IUNKNOWN
class DX12Viewport
{
public:
    DX12Viewport();
    ~DX12Viewport();

    bool Initialize(void* hwnd, int width, int height);
    void Render();
    void Resize(int width, int height);
    void Shutdown();

    // Camera controls
    void SetCamera(float x, float y, float z, float yaw, float pitch);

private:
    // Standard DirectX 12 Members
    Microsoft::WRL::ComPtr<ID3D12Device> m_device;
    Microsoft::WRL::ComPtr<ID3D12CommandQueue> m_commandQueue;
    Microsoft::WRL::ComPtr<IDXGISwapChain3> m_swapChain;
    Microsoft::WRL::ComPtr<ID3D12CommandAllocator> m_commandAllocator;
    Microsoft::WRL::ComPtr<ID3D12GraphicsCommandList> m_commandList;

    // Synchronization
    Microsoft::WRL::ComPtr<ID3D12Fence> m_fence;
    HANDLE m_fenceEvent;
    UINT64 m_fenceValue;
    int m_frameIndex;

    // Render Targets
    Microsoft::WRL::ComPtr<ID3D12DescriptorHeap> m_rtvHeap;
    Microsoft::WRL::ComPtr<ID3D12Resource> m_renderTargets[2];
    UINT m_rtvDescriptorSize;

    // State
    bool m_isInitialized;
    void* m_hwnd;
    int m_width;
    int m_height;

    // Internal Helpers
    void WaitForPreviousFrame();
    bool CreateDevice();
    bool CreateSwapChain();
    bool CreateResources();
};

// ========================================================
// FLAT C EXPORTS (This is what C# calls)
// ========================================================
extern "C" {
    __declspec(dllexport) void* CreateViewport(void* hwnd, int width, int height);
    __declspec(dllexport) void DestroyViewport(void* instance);
    __declspec(dllexport) void ResizeViewport(void* instance, int width, int height);
    __declspec(dllexport) void RenderViewport(void* instance);
    __declspec(dllexport) void SetCamera(void* instance, float x, float y, float z, float yaw, float pitch);
}
