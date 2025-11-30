// pch.h
#ifndef PCH_H
#define PCH_H

#include "framework.h"

#define WIN32_LEAN_AND_MEAN
#define NOMINMAX

#include <windows.h>
#include <winuser.h>
#include <wrl.h>
#include <wrl/client.h>
#include <dxgi.h>
#include <dxgi1_6.h>
#include <d3d12.h>
#include <d3dcompiler.h>
#include <dxcapi.h>
#include <comdef.h>

#include <DirectXMath.h>
#include <DirectXColors.h>

#include <cstdio>
#include <cstdint>
#include <cmath>
#include <cstring>
#include <cassert>
#include <stdexcept>
#include <string>
#include <vector>
#include <array>
#include <memory>
#include <algorithm>
#include <functional>
#include <unordered_map>
#include <map>
#include <queue>
#include <deque>
#include <set>
#include <unordered_set>
#include <thread>
#include <mutex>
#include <chrono>
#include <optional>
#include <any>

#pragma comment(lib, "d3d12.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3dcompiler.lib")
#pragma comment(lib, "winmm.lib")

using Microsoft::WRL::ComPtr;
using DirectX::XMFLOAT2;
using DirectX::XMFLOAT3;
using DirectX::XMFLOAT4;
using DirectX::XMFLOAT4X4;
using DirectX::XMMATRIX;
using DirectX::XMVECTOR;
using DirectX::XMMatrixIdentity;

#if defined(DEBUG) || defined(_DEBUG)
#define ASSERT(x) assert(x)
#define TRACE(x) OutputDebugStringA(x)
#else
#define ASSERT(x) ((void)0)
#define TRACE(x) ((void)0)
#endif

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
