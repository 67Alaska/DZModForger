// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "DX12Viewport.h"

#pragma warning(disable : 4566) // Disable warning for string literal truncation

#define DX12ENGINE_VERSION "1.0.0"

// ==================== DLL EXPORTS ====================

extern "C" __declspec(dllexport) HRESULT CreateD3D12Viewport(ID3D12Viewport** ppViewport)
{
    if (!ppViewport)
        return E_INVALIDARG;

    try
    {
        *ppViewport = new (std::nothrow) DX12Viewport();
        if (*ppViewport == nullptr)
            return E_OUTOFMEMORY;

        (*ppViewport)->AddRef();
        return S_OK;
    }
    catch (const std::exception& ex)
    {
        OutputDebugStringA("[DX12ENGINE] Exception in CreateD3D12Viewport: ");
        OutputDebugStringA(ex.what());
        OutputDebugStringA("\n");
        return E_FAIL;
    }
}

extern "C" __declspec(dllexport) const char* GetDX12EngineVersion()
{
    return DX12ENGINE_VERSION;
}

// ==================== DLL ENTRY POINT ====================
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        OutputDebugStringA("[DX12ENGINE] ✅ DLL_PROCESS_ATTACH - DX12Engine.dll loaded\n");
        break;
    case DLL_THREAD_ATTACH:
        OutputDebugStringA("[DX12ENGINE] DLL_THREAD_ATTACH\n");
        break;
    case DLL_THREAD_DETACH:
        OutputDebugStringA("[DX12ENGINE] DLL_THREAD_DETACH\n");
        break;
    case DLL_PROCESS_DETACH:
        OutputDebugStringA("[DX12ENGINE] ✅ DLL_PROCESS_DETACH - DX12Engine.dll unloaded\n");
        break;
    }
    return TRUE;
}
