#pragma once

#ifndef DX12ENGINE_H
#define DX12ENGINE_H

#include <d3d12.h>
#include <dxgi1_6.h>
#include <d3dcompiler.h>
#include <DirectXMath.h>
#include <wrl.h>

using namespace Microsoft::WRL;
using namespace DirectX;

#pragma comment(lib, "d3d12.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3dcompiler.lib")

// Forward declarations
class CDX12Viewport;

// ==================== STRUCTURES ====================

// Vertex structure matching FBX loader output
struct Vertex {
    XMFLOAT3 position;
    XMFLOAT3 normal;
    XMFLOAT2 texCoord;
};

// Camera structure
struct Camera {
    XMFLOAT3 position;
    XMFLOAT3 target;
    XMFLOAT3 up;
    float fov;
    float aspectRatio;
    float nearPlane;
    float farPlane;
    float yaw;
    float pitch;
    float distance;
};

// Material structure
struct Material {
    XMFLOAT4 diffuse;
    XMFLOAT4 specular;
    float metallic;
    float roughness;
    float aoFactor;
    float padding;
};

// Light structure
struct Light {
    XMFLOAT4 position;
    XMFLOAT4 direction;
    XMFLOAT4 color;
    float intensity;
    float range;
    int type; // 0=Directional, 1=Point, 2=Spot
    float padding;
};

// Constant Buffer Structure
struct ConstantBuffer {
    XMMATRIX world;
    XMMATRIX view;
    XMMATRIX projection;
    XMFLOAT4 cameraPos;
    Material material;
    Light lights[4];
    int lightCount;
    int pad1, pad2, pad3;
};

// ==================== COM INTERFACE ====================

// {12345678-1234-1234-1234-123456789ABC}
DEFINE_GUID(IID_IDX12Viewport,
    0x12345678, 0x1234, 0x1234, 0x12, 0x34, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC);

// {87654321-4321-4321-4321-CBACBACBACBA}
DEFINE_GUID(CLSID_DX12Viewport,
    0x87654321, 0x4321, 0x4321, 0x43, 0x21, 0xCB, 0xAC, 0xBA, 0xCB, 0xAC, 0xBA);

interface IDX12Viewport : public IUnknown{
    // Initialization
    virtual HRESULT STDMETHODCALLTYPE Initialize(HWND hwnd, UINT width, UINT height) = 0;

// Model management
virtual HRESULT STDMETHODCALLTYPE LoadModelFromFile(const wchar_t* filePath) = 0;
virtual HRESULT STDMETHODCALLTYPE UpdateModelData(Vertex* vertices, UINT vertexCount, UINT* indices, UINT indexCount) = 0;

// Rendering
virtual HRESULT STDMETHODCALLTYPE Render() = 0;
virtual HRESULT STDMETHODCALLTYPE Present() = 0;

// Camera control
virtual HRESULT STDMETHODCALLTYPE SetCameraOrbital(float yaw, float pitch, float distance) = 0;
virtual HRESULT STDMETHODCALLTYPE SetCameraPan(float panX, float panY) = 0;
virtual HRESULT STDMETHODCALLTYPE SetCameraZoom(float zoomFactor) = 0;
virtual HRESULT STDMETHODCALLTYPE ResetCamera() = 0;

// Viewport state
virtual HRESULT STDMETHODCALLTYPE SetShadingMode(int mode) = 0; // 0=Wire, 1=Solid, 2=Material, 3=Render
virtual HRESULT STDMETHODCALLTYPE ShowGrid(BOOL visible) = 0;
virtual HRESULT STDMETHODCALLTYPE ShowAxes(BOOL visible) = 0;
virtual HRESULT STDMETHODCALLTYPE ShowBounds(BOOL visible) = 0;

// Material control
virtual HRESULT STDMETHODCALLTYPE SetMaterialMetallic(float metallic) = 0;
virtual HRESULT STDMETHODCALLTYPE SetMaterialRoughness(float roughness) = 0;
virtual HRESULT STDMETHODCALLTYPE SetMaterialAO(float ao) = 0;

// Lighting
virtual HRESULT STDMETHODCALLTYPE AddLight(Light* light) = 0;
virtual HRESULT STDMETHODCALLTYPE ClearLights() = 0;

// Query information
virtual HRESULT STDMETHODCALLTYPE GetFrameRate(float* outFPS) = 0;
virtual HRESULT STDMETHODCALLTYPE GetGPUMemoryUsage(UINT64* outBytes) = 0;
virtual HRESULT STDMETHODCALLTYPE GetLastError(wchar_t* outError, UINT errorLength) = 0;

// Cleanup
virtual HRESULT STDMETHODCALLTYPE Shutdown() = 0;
};

// Class factory for COM object creation
class DX12ViewportClassFactory : public IClassFactory {
public:
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppvObject) override;
    ULONG STDMETHODCALLTYPE AddRef() override;
    ULONG STDMETHODCALLTYPE Release() override;
    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppvObject) override;
    HRESULT STDMETHODCALLTYPE LockServer(BOOL fLock) override;

private:
    ULONG m_refCount = 1;
};

// ==================== HELPER FUNCTIONS ====================

#endif // DX12ENGINE_H
