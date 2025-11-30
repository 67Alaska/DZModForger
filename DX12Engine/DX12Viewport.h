// DX12Viewport.h - COMPLETE CORRECTED HEADER

#pragma once

#include "framework.h"
#include <vector>
#include <cstdarg>

#define FRAME_COUNT 3

// ==================== COM INTERFACE ====================

extern const GUID IID_ID3D12Viewport;
extern const GUID CLSID_DX12Viewport;

// ==================== VERTEX STRUCTURE ====================

struct Vertex
{
    XMFLOAT3 position;
    XMFLOAT3 normal;
    XMFLOAT2 texCoord;
};

// ==================== CONSTANT BUFFERS ====================

struct ConstantBuffer
{
    XMMATRIX worldViewProj;
    XMMATRIX world;
    XMMATRIX view;
    XMMATRIX projection;
    XMFLOAT4 cameraPosition;
    XMFLOAT4 lightDirection;
};

// ==================== COM INTERFACE DEFINITION ====================

MIDL_INTERFACE("A1B2C3D4-E5F6-4A5B-9C8D-7E6F5A4B3C2D")
ID3D12Viewport : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE Initialize(HWND hwnd, UINT width, UINT height) = 0;
    virtual HRESULT STDMETHODCALLTYPE Shutdown() = 0;
    virtual HRESULT STDMETHODCALLTYPE Render() = 0;
    virtual HRESULT STDMETHODCALLTYPE Present() = 0;
    virtual HRESULT STDMETHODCALLTYPE Resize(UINT width, UINT height) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetCamera(float radius, float theta, float phi, float targetX, float targetY, float targetZ) = 0;
    virtual HRESULT STDMETHODCALLTYPE LoadFBX(const char* filePath) = 0;
    virtual HRESULT STDMETHODCALLTYPE LoadOBJ(const char* filePath) = 0;
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

    // Window
    HWND _hwnd;
    UINT _width;
    UINT _height;

    // DirectX 12 Core
    ComPtr<ID3D12Device> _device;
    ComPtr<ID3D12CommandQueue> _commandQueue;
    ComPtr<ID3D12CommandAllocator> _commandAllocator;
    ComPtr<ID3D12GraphicsCommandList> _commandList;
    ComPtr<IDXGISwapChain4> _swapChain;
    ComPtr<ID3D12DescriptorHeap> _rtvHeap;
    ComPtr<ID3D12DescriptorHeap> _dsvHeap;
    ComPtr<ID3D12DescriptorHeap> _cbvSrvHeap;

    // Render Targets
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

    // Constant Buffers
    ComPtr<ID3D12Resource> _constantBuffer;
    ConstantBuffer* _cbvDataBegin;

    // Model Data
    std::vector<Vertex> _vertices;
    std::vector<UINT> _indices;
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
    bool _isInitialized;

    // Private Methods
    HRESULT InitializeDirectX();
    HRESULT CreateRenderTargets();
    HRESULT CreatePipeline();
    void UpdateCamera();
    void WaitForGpu();
    void MoveToNextFrame();
    void LogMessage(const char* format, ...);
};
