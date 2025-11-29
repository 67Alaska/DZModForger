#pragma once
#include "pch.h"

interface __declspec(uuid("A1B2C3D4-E5F6-4A5B-9C8D-7E6F5A4B3C2D")) ID3D12Viewport : public IUnknown
{
    virtual HRESULT __stdcall Initialize(HWND hwnd, UINT width, UINT height) = 0;
    virtual HRESULT __stdcall Render() = 0;
};

class DX12Viewport : public ID3D12Viewport
{
public:
    DX12Viewport();
    ~DX12Viewport();

    HRESULT __stdcall QueryInterface(REFIID riid, void** ppvObject) override;
    ULONG __stdcall AddRef() override;
    ULONG __stdcall Release() override;

    HRESULT __stdcall Initialize(HWND hwnd, UINT width, UINT height) override;
    HRESULT __stdcall Render() override;

private:
    LONG m_refCount;

    Microsoft::WRL::ComPtr<ID3D12Device> m_device;
    Microsoft::WRL::ComPtr<ID3D12CommandQueue> m_commandQueue;
    Microsoft::WRL::ComPtr<ID3D12CommandAllocator> m_commandAllocator;
    Microsoft::WRL::ComPtr<ID3D12GraphicsCommandList> m_commandList;
    Microsoft::WRL::ComPtr<IDXGISwapChain3> m_swapChain;
    Microsoft::WRL::ComPtr<ID3D12Fence> m_fence;
    Microsoft::WRL::ComPtr<ID3D12DescriptorHeap> m_rtvHeap;
    Microsoft::WRL::ComPtr<ID3D12Resource> m_renderTargets[2];
    Microsoft::WRL::ComPtr<ID3D12RootSignature> m_rootSignature;
    Microsoft::WRL::ComPtr<ID3D12PipelineState> m_pipelineState;
    Microsoft::WRL::ComPtr<ID3D12Resource> m_vertexBuffer;

    HANDLE m_fenceEvent;
    UINT64 m_fenceValue;
    UINT m_frameIndex;
    UINT m_rtvDescriptorSize;
    D3D12_VERTEX_BUFFER_VIEW m_vertexBufferView;

    HRESULT CreateDevice();
    HRESULT CreateCommandInfrastructure();
    HRESULT CreateSwapChain(HWND hwnd, UINT width, UINT height);
    HRESULT CreateRenderTargets();
    HRESULT CreatePipelineState();
    HRESULT CreateVertexBuffer();
    void PopulateCommandList();
    void WaitForPreviousFrame();
};

extern "C" __declspec(dllexport) HRESULT __stdcall CreateDX12Viewport(
    HWND hwnd, UINT width, UINT height, ID3D12Viewport** ppViewport);