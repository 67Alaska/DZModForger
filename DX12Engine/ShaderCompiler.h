#pragma once

#include <d3dcompiler.h>
#include <wrl/client.h>

using Microsoft::WRL::ComPtr;

class ShaderCompiler
{
public:
    // Compile HLSL shader from file
    static HRESULT CompileShaderFromFile(
        const wchar_t* fileName,
        const char* entryPoint,
        const char* shaderModel,
        ID3DBlob** ppBlobOut)
    {
        if (!fileName || !entryPoint || !shaderModel || !ppBlobOut)
            return E_INVALIDARG;

        ComPtr<ID3DBlob> pErrorBlob;
        HRESULT hr = D3DCompileFromFile(
            fileName,
            nullptr,                  // pDefines
            D3D_COMPILE_STANDARD_FILE_INCLUDE,  // pInclude
            entryPoint,
            shaderModel,
            D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION,
            0,                        // Flags2
            ppBlobOut,
            &pErrorBlob);

        if (FAILED(hr))
        {
            if (pErrorBlob)
            {
                OutputDebugStringA("[SHADER] Compilation Error:\n");
                OutputDebugStringA((const char*)pErrorBlob->GetBufferPointer());
                OutputDebugStringA("\n");
                pErrorBlob->Release();
            }
            return hr;
        }

        return S_OK;
    }

    // Compile HLSL shader from memory
    static HRESULT CompileShaderFromMemory(
        const char* shaderCode,
        size_t codeLength,
        const char* entryPoint,
        const char* shaderModel,
        ID3DBlob** ppBlobOut)
    {
        if (!shaderCode || !entryPoint || !shaderModel || !ppBlobOut)
            return E_INVALIDARG;

        ComPtr<ID3DBlob> pErrorBlob;
        HRESULT hr = D3DCompile(
            shaderCode,
            codeLength,
            "InMemoryShader",
            nullptr,                  // pDefines
            D3D_COMPILE_STANDARD_FILE_INCLUDE,  // pInclude
            entryPoint,
            shaderModel,
            D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION,
            0,                        // Flags2
            ppBlobOut,
            &pErrorBlob);

        if (FAILED(hr))
        {
            if (pErrorBlob)
            {
                OutputDebugStringA("[SHADER] Compilation Error:\n");
                OutputDebugStringA((const char*)pErrorBlob->GetBufferPointer());
                OutputDebugStringA("\n");
                pErrorBlob->Release();
            }
            return hr;
        }

        return S_OK;
    }
};
