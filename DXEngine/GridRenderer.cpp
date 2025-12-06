#include "pch.h"
#include "GridRenderer.h"
#include <d3dcompiler.h>

namespace DXEngine
{
    GridRenderer::GridRenderer(
        ID3D12Device* device,
        ID3D12CommandQueue* commandQueue,
        uint32_t width,
        uint32_t height)
        : _device(device)
        , _commandQueue(commandQueue)
        , _windowWidth(width)
        , _windowHeight(height)
    {
        // Default grid config (Blender-like)
        _gridConfig.GridSize = 1.0f;
        _gridConfig.MainGridScale = 10.0f;
        _gridConfig.MinorGridScale = 1.0f;
        _gridConfig.GridExtent = 1000.0f;
        _gridConfig.FadeStartDistance = 500.0f;
        _gridConfig.FadeEndDistance = 1000.0f;
        _gridConfig.AxisLineWidth = 2.0f;
        _gridConfig.GridLineWidth = 1.0f;

        // Colors
        _gridConfig.GridColor = DirectX::XMFLOAT4(0.3f, 0.3f, 0.3f, 0.5f);      // Grey
        _gridConfig.MinorGridColor = DirectX::XMFLOAT4(0.2f, 0.2f, 0.2f, 0.3f); // Darker grey
        _gridConfig.XAxisColor = DirectX::XMFLOAT4(1.0f, 0.0f, 0.0f, 1.0f);     // Red
        _gridConfig.YAxisColor = DirectX::XMFLOAT4(0.0f, 1.0f, 0.0f, 1.0f);     // Green
        _gridConfig.ZAxisColor = DirectX::XMFLOAT4(0.0f, 0.0f, 1.0f, 1.0f);     // Blue
    }

    GridRenderer::~GridRenderer()
    {
    }

    void GridRenderer::Initialize()
    {
        CompileShaders();
        CreateRootSignature();
        CreatePipelineState();
        CreateBuffers();
    }

    void GridRenderer::CompileShaders()
    {
        // Compile vertex shader
        HRESULT hr = D3DCompileFromFile(
            L"GridShaders.hlsl",
            nullptr,
            nullptr,
            "VS_Main",
            "vs_5_0",
            D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION,
            0,
            &_vertexShaderBlob,
            nullptr
        );

        if (FAILED(hr))
        {
            throw std::runtime_error("Failed to compile vertex shader");
        }

        // Compile pixel shader
        hr = D3DCompileFromFile(
            L"GridShaders.hlsl",
            nullptr,
            nullptr,
            "PS_Main",
            "ps_5_0",
            D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION,
            0,
            &_pixelShaderBlob,
            nullptr
        );

        if (FAILED(hr))
        {
            throw std::runtime_error("Failed to compile pixel shader");
        }
    }

    void GridRenderer::CreateRootSignature()
    {
        D3D12_ROOT_PARAMETER rootParams[1];

        // Constant buffer parameter
        D3D12_ROOT_DESCRIPTOR_TABLE descriptorTable;
        D3D12_DESCRIPTOR_RANGE range;
        range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
        range.NumDescriptors = 1;
        range.BaseShaderRegister = 0;
        range.RegisterSpace = 0;
        range.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;

        descriptorTable.NumDescriptorRanges = 1;
        descriptorTable.pDescriptorRanges = &range;

        rootParams[0].ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
        rootParams[0].DescriptorTable = descriptorTable;
        rootParams[0].ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;

        D3D12_ROOT_SIGNATURE_DESC rootSigDesc;
        rootSigDesc.NumParameters = 1;
        rootSigDesc.pParameters = rootParams;
        rootSigDesc.NumStaticSamplers = 0;
        rootSigDesc.pStaticSamplers = nullptr;
        rootSigDesc.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

        Microsoft::WRL::ComPtr<ID3DBlob> sigBlob;
        Microsoft::WRL::ComPtr<ID3DBlob> errorBlob;

        HRESULT hr = D3D12SerializeRootSignature(&rootSigDesc, D3D_ROOT_SIGNATURE_VERSION_1_0, &sigBlob, &errorBlob);
        if (FAILED(hr))
        {
            throw std::runtime_error("Failed to serialize root signature");
        }

        hr = _device->CreateRootSignature(0, sigBlob->GetBufferPointer(), sigBlob->GetBufferSize(), IID_PPV_ARGS(&_rootSignature));
        if (FAILED(hr))
        {
            throw std::runtime_error("Failed to create root signature");
        }
    }

    void GridRenderer::CreatePipelineState()
    {
        D3D12_INPUT_ELEMENT_DESC inputLayout[] =
        {
            { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
            { "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 }
        };

        D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
        psoDesc.InputLayout = { inputLayout, _countof(inputLayout) };
        psoDesc.pRootSignature = _rootSignature.Get();
        psoDesc.VS = { _vertexShaderBlob->GetBufferPointer(), _vertexShaderBlob->GetBufferSize() };
        psoDesc.PS = { _pixelShaderBlob->GetBufferPointer(), _pixelShaderBlob->GetBufferSize() };
        psoDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
        psoDesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);
        psoDesc.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC(D3D12_DEFAULT);
        psoDesc.SampleMask = UINT_MAX;
        psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
        psoDesc.NumRenderTargets = 1;
        psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
        psoDesc.SampleDesc.Count = 1;

        // Enable blending for grid transparency
        psoDesc.BlendState.RenderTarget[0].BlendEnable = TRUE;
        psoDesc.BlendState.RenderTarget[0].SrcBlend = D3D12_BLEND_SRC_ALPHA;
        psoDesc.BlendState.RenderTarget[0].DestBlend = D3D12_BLEND_INV_SRC_ALPHA;
        psoDesc.BlendState.RenderTarget[0].BlendOp = D3D12_BLEND_OP_ADD;

        HRESULT hr = _device->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&_pipelineState));
        if (FAILED(hr))
        {
            throw std::runtime_error("Failed to create pipeline state");
        }
    }

    void GridRenderer::CreateBuffers()
    {
        // Create full-screen quad vertex buffer
        GridVertex vertices[] =
        {
            { DirectX::XMFLOAT3(-1.0f, -1.0f, 0.0f), DirectX::XMFLOAT2(0.0f, 1.0f) },
            { DirectX::XMFLOAT3(-1.0f,  1.0f, 0.0f), DirectX::XMFLOAT2(0.0f, 0.0f) },
            { DirectX::XMFLOAT3(1.0f, -1.0f, 0.0f), DirectX::XMFLOAT2(1.0f, 1.0f) },
            { DirectX::XMFLOAT3(1.0f,  1.0f, 0.0f), DirectX::XMFLOAT2(1.0f, 0.0f) }
        };

        auto vertexBufferDesc = CD3DX12_RESOURCE_DESC::Buffer(sizeof(vertices));
        HRESULT hr = _device->CreateCommittedResource(
            &CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD),
            D3D12_HEAP_FLAG_NONE,
            &vertexBufferDesc,
            D3D12_RESOURCE_STATE_GENERIC_READ,
            nullptr,
            IID_PPV_ARGS(&_vertexBuffer)
        );

        if (FAILED(hr))
        {
            throw std::runtime_error("Failed to create vertex buffer");
        }

        // Copy vertex data
        void* mappedData;
        _vertexBuffer->Map(0, nullptr, &mappedData);
        memcpy(mappedData, vertices, sizeof(vertices));
        _vertexBuffer->Unmap(0, nullptr);

        // Create constant buffer
        auto cbDesc = CD3DX12_RESOURCE_DESC::Buffer((sizeof(GridConstants) + 255) & ~255);
        hr = _device->CreateCommittedResource(
            &CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD),
            D3D12_HEAP_FLAG_NONE,
            &cbDesc,
            D3D12_RESOURCE_STATE_GENERIC_READ,
            nullptr,
            IID_PPV_ARGS(&_constantBuffer)
        );

        if (FAILED(hr))
        {
            throw std::runtime_error("Failed to create constant buffer");
        }

        // Create CBV heap
        D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
        heapDesc.NumDescriptors = 1;
        heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
        heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;

        hr = _device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&_cbvHeap));
        if (FAILED(hr))
        {
            throw std::runtime_error("Failed to create CBV heap");
        }

        // Create CBV
        D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc = {};
        cbvDesc.BufferLocation = _constantBuffer->GetGPUVirtualAddress();
        cbvDesc.SizeInBytes = (sizeof(GridConstants) + 255) & ~255;

        _device->CreateConstantBufferView(&cbvDesc, _cbvHeap->GetCPUDescriptorHandleForHeapStart());
    }

    void GridRenderer::UpdateGridConfig(const GridConfig& config)
    {
        _gridConfig = config;
    }

    void GridRenderer::UpdateConstantBuffer(
        const DirectX::XMMATRIX& viewMatrix,
        const DirectX::XMMATRIX& projMatrix,
        const DirectX::XMFLOAT3& cameraPos)
    {
        _gridConstants.ViewMatrix = DirectX::XMMatrixTranspose(viewMatrix);
        _gridConstants.ProjectionMatrix = DirectX::XMMatrixTranspose(projMatrix);
        _gridConstants.CameraPosition = DirectX::XMFLOAT4(cameraPos.x, cameraPos.y, cameraPos.z, 1.0f);
        _gridConstants.GridSize = _gridConfig.GridSize;
        _gridConstants.MainGridScale = _gridConfig.MainGridScale;
        _gridConstants.MinorGridScale = _gridConfig.MinorGridScale;
        _gridConstants.GridExtent = _gridConfig.GridExtent;
        _gridConstants.FadeStartDistance = _gridConfig.FadeStartDistance;
        _gridConstants.FadeEndDistance = _gridConfig.FadeEndDistance;
        _gridConstants.AxisLineWidth = _gridConfig.AxisLineWidth;
        _gridConstants.GridLineWidth = _gridConfig.GridLineWidth;
        _gridConstants.GridColor = _gridConfig.GridColor;
        _gridConstants.MinorGridColor = _gridConfig.MinorGridColor;
        _gridConstants.XAxisColor = _gridConfig.XAxisColor;
        _gridConstants.YAxisColor = _gridConfig.YAxisColor;
        _gridConstants.ZAxisColor = _gridConfig.ZAxisColor;

        // Update buffer
        void* mappedData;
        _constantBuffer->Map(0, nullptr, &mappedData);
        memcpy(mappedData, &_gridConstants, sizeof(GridConstants));
        _constantBuffer->Unmap(0, nullptr);
    }

    void GridRenderer::Render(
        ID3D12GraphicsCommandList* commandList,
        const DirectX::XMMATRIX& viewMatrix,
        const DirectX::XMMATRIX& projMatrix,
        const DirectX::XMFLOAT3& cameraPos)
    {
        if (!_pipelineState)
            return;

        UpdateConstantBuffer(viewMatrix, projMatrix, cameraPos);

        // Set pipeline
        commandList->SetPipelineState(_pipelineState.Get());
        commandList->SetGraphicsRootSignature(_rootSignature.Get());

        // Set CBV heap and descriptor
        ID3D12DescriptorHeap* heaps[] = { _cbvHeap.Get() };
        commandList->SetDescriptorHeaps(_countof(heaps), heaps);
        commandList->SetGraphicsRootDescriptorTable(0, _cbvHeap->GetGPUDescriptorHandleForHeapStart());

        // Set vertex buffer
        D3D12_VERTEX_BUFFER_VIEW vbView;
        vbView.BufferLocation = _vertexBuffer->GetGPUVirtualAddress();
        vbView.StrideInBytes = sizeof(GridVertex);
        vbView.SizeInBytes = 4 * sizeof(GridVertex);
        commandList->IASetVertexBuffers(0, 1, &vbView);

        // Set primitive topology
        commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP);

        // Draw full-screen quad
        commandList->DrawInstanced(4, 1, 0, 0);
    }

    void GridRenderer::OnWindowSizeChanged(uint32_t width, uint32_t height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }
}
