#pragma once

#include <d3d12.h>
#include <dxgi1_4.h>
#include <wrl/client.h>
#include <DirectXMath.h>
#include "GridData.h"

namespace DXEngine
{
    class GridRenderer
    {
    public:
        GridRenderer(
            ID3D12Device* device,
            ID3D12CommandQueue* commandQueue,
            uint32_t width,
            uint32_t height
        );

        ~GridRenderer();

        // Initialize grid renderer
        void Initialize();

        // Update grid parameters
        void UpdateGridConfig(const GridConfig& config);

        // Render the grid
        void Render(
            ID3D12GraphicsCommandList* commandList,
            const DirectX::XMMATRIX& viewMatrix,
            const DirectX::XMMATRIX& projMatrix,
            const DirectX::XMFLOAT3& cameraPos
        );

        // Window resize
        void OnWindowSizeChanged(uint32_t width, uint32_t height);

        // Get current grid config
        const GridConfig& GetGridConfig() const { return _gridConfig; }

    private:
        Microsoft::WRL::ComPtr<ID3D12Device> _device;
        Microsoft::WRL::ComPtr<ID3D12CommandQueue> _commandQueue;

        // Shaders
        Microsoft::WRL::ComPtr<ID3DBlob> _vertexShaderBlob;
        Microsoft::WRL::ComPtr<ID3DBlob> _pixelShaderBlob;

        // Pipeline
        Microsoft::WRL::ComPtr<ID3D12RootSignature> _rootSignature;
        Microsoft::WRL::ComPtr<ID3D12PipelineState> _pipelineState;

        // Buffers
        Microsoft::WRL::ComPtr<ID3D12Resource> _vertexBuffer;
        Microsoft::WRL::ComPtr<ID3D12Resource> _constantBuffer;
        Microsoft::WRL::ComPtr<ID3D12DescriptorHeap> _cbvHeap;

        // Grid configuration
        GridConfig _gridConfig;
        GridConstants _gridConstants;

        uint32_t _windowWidth;
        uint32_t _windowHeight;

        // Initialization helpers
        void CompileShaders();
        void CreateRootSignature();
        void CreatePipelineState();
        void CreateBuffers();

        // Update constant buffer
        void UpdateConstantBuffer(
            const DirectX::XMMATRIX& viewMatrix,
            const DirectX::XMMATRIX& projMatrix,
            const DirectX::XMFLOAT3& cameraPos
        );
    };
}
