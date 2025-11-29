// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.

#ifndef PCH_H
#define PCH_H

// ==================== WINDOWS SDK ====================
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <windowsx.h>
#include <sdkddkver.h>
#include <Comdef.h>
#include <wrl.h>
#include <winerror.h>

// ==================== DIRECTX 12 ====================
#include <d3d12.h>
#include <dxgi1_6.h>
#include <d3dcompiler.h>
#include <d3dx12.h>
#include <DirectXMath.h>
#include <DirectXPackedVector.h>

// ==================== STANDARD LIBRARY ====================
#include <iostream>
#include <vector>
#include <array>
#include <memory>
#include <string>
#include <sstream>
#include <algorithm>
#include <stdexcept>
#include <math.h>
#include <string.h>
#include <stdint.h>
#include <stdarg.h>
#include <stdio.h>
#include <map>
#include <unordered_map>
#include <queue>
#include <thread>
#include <mutex>
#include <atomic>
#include <assert.h>

// ==================== DIRECTX HELPERS ====================
#pragma comment(lib, "d3d12.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3dcompiler.lib")
#pragma comment(lib, "winmm.lib")

// ==================== USING DECLARATIONS ====================
using Microsoft::WRL::ComPtr;
using DirectX::XMFLOAT2;
using DirectX::XMFLOAT3;
using DirectX::XMFLOAT4;
using DirectX::XMFLOAT4X4;
using DirectX::XMMATRIX;
using DirectX::XMVECTOR;
using DirectX::XMMatrixIdentity;

// ==================== DEBUG HELPERS ====================
#if defined(DEBUG) || defined(_DEBUG)
    #define ASSERT(x) assert(x)
    #define TRACE(x) OutputDebugStringA(x)
#else
    #define ASSERT(x) ((void)0)
    #define TRACE(x) ((void)0)
#endif

// ==================== HRESULT HELPERS ====================
inline void ThrowIfFailed(HRESULT hr)
{
    if (FAILED(hr))
    {
        _com_error err(hr);
        LPCTSTR errMsg = err.ErrorMessage();
        char buffer[256];
        sprintf_s(buffer, sizeof(buffer), "DX12 Error: 0x%08X - %s\n", hr, (const char*)errMsg);
        TRACE(buffer);
        throw std::runtime_error(buffer);
    }
}

#endif //PCH_H
