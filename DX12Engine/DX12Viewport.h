#pragma once

#include "framework.h"
#include <wrl/client.h>
#include <d3d12.h>
#include <DirectXMath.h>

using Microsoft::WRL::ComPtr;
using namespace DirectX;

// ==================== GUID DECLARATIONS ====================

extern "C" const IID IID_ID3D12Viewport;
extern "C" const CLSID CLSID_DX12Viewport;

// ==================== INTERFACE DEFINITION ====================

MIDL_INTERFACE("12345678-1234-1234-1234-123456789012")
ID3D12Viewport : public IUnknown
{
public:
    // Initialization
    virtual HRESULT STDMETHODCALLTYPE Initialize(HWND hwnd, UINT width, UINT height) = 0;
    virtual HRESULT STDMETHODCALLTYPE Shutdown() = 0;

    // Rendering
    virtual HRESULT STDMETHODCALLTYPE Render() = 0;
    virtual HRESULT STDMETHODCALLTYPE Present() = 0;

    // Viewport Control
    virtual HRESULT STDMETHODCALLTYPE Resize(UINT width, UINT height) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetCamera(float radius, float theta, float phi,
        float targetX, float targetY, float targetZ) = 0;

    // Model Loading
    virtual HRESULT STDMETHODCALLTYPE LoadFBX(const char* filePath) = 0;
    virtual HRESULT STDMETHODCALLTYPE LoadOBJ(const char* filePath) = 0;

    // Statistics
    virtual HRESULT STDMETHODCALLTYPE GetFrameRate(float* pFps) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetVertexCount(UINT* pCount) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetTriangleCount(UINT* pCount) = 0;
};

// ==================== IMPLEMENTATION CLASS ====================

class DX12Viewport : public ID3D12Viewport
{
public:
    // Constructor/Destructor
    DX12Viewport();
    virtual ~DX12Viewport();

    // IUnknown Implementation
    STDMETHODIMP QueryInterface(REFIID riid, void** ppvObject) override;
    STDMETHODIMP_(ULONG) AddRef() override;
    STDMETHODIMP_(ULONG) Release() override;

    // ID3D12Viewport Implementation
    STDMETHODIMP Initialize(HWND hwnd, UINT width, UINT height) override;
    STDMETHODIMP Shutdown() override;
    STDMETHODIMP Render() override;
    STDMETHODIMP Present() override;
    STDMETHODIMP Resize(UINT width, UINT height) override;
    STDMETHODIMP SetCamera(float radius, float theta, float phi, float targetX, float targetY, float targetZ) override;
    STDMETHODIMP LoadFBX(const char* filePath) override;
    STDMETHODIMP LoadOBJ(const char* filePath) override;
    STDMETHODIMP GetFrameRate(float* pFps) override;
    STDMETHODIMP GetVertexCount(UINT* pCount) override;
    STDMETHODIMP GetTriangleCount(UINT* pCount) override;

private:
    // Reference counting
    LONG _refCount;
    bool _isInitialized;

    // Window
    HWND _hwnd;
    UINT _width;
    UINT _height;

    // DirectX 12 Core
    ComPtr<ID3D12Device> _device;
    ComPtr<ID3D12CommandQueue> _commandQueue;
    ComPtr<ID3D12CommandAllocator> _commandAllocator;
    ComPtr<ID3D12GraphicsCommandList> _commandList;
    ComPtr<IDXGISwapChain3> _swapChain;
    ComPtr<ID3D12DescriptorHeap> _rtvHeap;
    ComPtr<ID3D12DescriptorHeap> _dsvHeap;
    ComPtr<ID3D12DescriptorHeap> _cbvSrvHeap;

    // Render Targets
    static const UINT FRAME_COUNT = 3;
    ComPtr<ID3D12Resource> _renderTargets[FRAME_COUNT];
    ComPtr<ID3D12Resource> _depthStencilBuffer;
    UINT _rtvDescriptorSize;
    UINT _currentFrameIndex;

    // Fence
    ComPtr<ID3D12Fence> _fence;
    HANDLE _fenceEvent;
    UINT64 _fenceValue;

    // Pipeline
    ComPtr<ID3D12RootSignature> _rootSignature;
    ComPtr<ID3D12PipelineState> _pipelineState;
    D3D12_VIEWPORT _viewport;
    D3D12_RECT _scissorRect;

    // Buffers
    ComPtr<ID3D12Resource> _constantBuffer;
    void* _cbvDataBegin;
    ComPtr<ID3D12Resource> _vertexBuffer;
    ComPtr<ID3D12Resource> _indexBuffer;
    D3D12_VERTEX_BUFFER_VIEW _vertexBufferView;
    D3D12_INDEX_BUFFER_VIEW _indexBufferView;

    // Camera
    float _cameraDistance;
    float _cameraTheta;
    float _cameraPhi;
    float _cameraTargetX;
    float _cameraTargetY;
    float _cameraTargetZ;

    // Statistics
    UINT _vertexCount;
    UINT _triangleCount;
    float _frameTime;

    // Private Methods
    HRESULT CreateRootSignature();
    HRESULT CreatePipelineState();
    HRESULT CreateCubeGeometry();
    HRESULT CreateConstantBuffer();
    void WaitForGpu();
    void LogMessage(const char* format, ...);
};