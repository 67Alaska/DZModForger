#pragma once
#include <DirectXMath.h>
#include <cstdint>

namespace DXEngine
{
    // Grid configuration
    struct GridConfig
    {
        float GridSize;                 // Base grid unit size (e.g., 1.0f = 1 unit)
        float MainGridScale;            // Major grid line scale multiplier (e.g., 10.0f)
        float MinorGridScale;           // Minor grid line scale multiplier (e.g., 1.0f)
        float GridExtent;               // How far grid extends from origin (world units)
        float FadeStartDistance;        // Distance where grid starts fading
        float FadeEndDistance;          // Distance where grid fully fades
        float AxisLineWidth;            // Width of X, Y, Z axis lines
        float GridLineWidth;            // Width of grid lines
        DirectX::XMFLOAT4 GridColor;    // Grid line color (grey)
        DirectX::XMFLOAT4 MinorGridColor; // Minor grid color
        DirectX::XMFLOAT4 XAxisColor;   // Red for X axis
        DirectX::XMFLOAT4 YAxisColor;   // Green for Y axis
        DirectX::XMFLOAT4 ZAxisColor;   // Blue for Z axis
    };

    // Constants buffer for shaders
    struct GridConstants
    {
        DirectX::XMMATRIX ViewMatrix;
        DirectX::XMMATRIX ProjectionMatrix;
        DirectX::XMFLOAT4 CameraPosition;
        float GridSize;
        float MainGridScale;
        float MinorGridScale;
        float GridExtent;
        float FadeStartDistance;
        float FadeEndDistance;
        float AxisLineWidth;
        float GridLineWidth;
        DirectX::XMFLOAT4 GridColor;
        DirectX::XMFLOAT4 MinorGridColor;
        DirectX::XMFLOAT4 XAxisColor;
        DirectX::XMFLOAT4 YAxisColor;
        DirectX::XMFLOAT4 ZAxisColor;
    };

    // Simple grid vertex (full-screen quad)
    struct GridVertex
    {
        DirectX::XMFLOAT3 Position;
        DirectX::XMFLOAT2 TexCoord;
    };
}
