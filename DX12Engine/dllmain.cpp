// dllmain.cpp - COMPLETE FILE

#include "pch.h"
#include "DX12Viewport.h"

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C"
{
    // Class factory for DX12Viewport
    HRESULT STDMETHODCALLTYPE DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
    {
        if (!ppv) return E_INVALIDARG;
        *ppv = nullptr;

        if (rclsid == CLSID_DX12Viewport)
        {
            DX12Viewport* pViewport = new DX12Viewport();
            if (!pViewport) return E_OUTOFMEMORY;

            HRESULT hr = pViewport->QueryInterface(riid, ppv);
            pViewport->Release();
            return hr;
        }

        return CLASS_E_CLASSNOTAVAILABLE;
    }

    HRESULT STDMETHODCALLTYPE DllCanUnloadNow()
    {
        return S_OK;
    }
}

// GUIDs
const GUID IID_ID3D12Viewport = { 0xA1B2C3D4, 0xE5F6, 0x4A5B, { 0x9C, 0x8D, 0x7E, 0x6F, 0x5A, 0x4B, 0x3C, 0x2D } };
const GUID CLSID_DX12Viewport = { 0xB2C3D4E5, 0xF6A7, 0x5B9C, { 0x8D, 0x9E, 0x7F, 0x6E, 0x5D, 0x4C, 0x3B, 0x2A } };
