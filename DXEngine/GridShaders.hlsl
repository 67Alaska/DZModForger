// GridShaders.hlsl - Blender-style grid rendering

cbuffer GridConstants : register(b0)
{
    float4x4 ViewMatrix;
    float4x4 ProjectionMatrix;
    float3 CameraPosition;
    float GridSize;
    float MainGridScale;
    float MinorGridScale;
    float GridExtent;
    float FadeStartDistance;
    float FadeEndDistance;
    float AxisLineWidth;
    float GridLineWidth;
    float4 GridColor;
    float4 MinorGridColor;
    float4 XAxisColor;
    float4 YAxisColor;
    float4 ZAxisColor;
};

// ============================================================================
// VERTEX SHADER
// ============================================================================

struct VS_Input
{
    float3 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
};

struct VS_Output
{
    float4 Position : SV_POSITION;
    float2 ScreenCoord : TEXCOORD0;
};

VS_Output VS_Main(VS_Input input)
{
    VS_Output output;
    
    // Full-screen quad - no transformation needed
    output.Position = float4(input.Position, 1.0f);
    output.ScreenCoord = input.TexCoord;
    
    return output;
}

// ============================================================================
// PIXEL SHADER
// ============================================================================

// Unproject screen coordinates to world space
float3 UnprojectPoint(float2 screenCoord, float depth)
{
    // Normalize screen coordinates to [-1, 1]
    float2 ndc = screenCoord * 2.0f - 1.0f;
    ndc.y = -ndc.y; // Flip Y for DirectX convention
    
    // Create ray in view space
    float3 viewRay = float3(ndc, 1.0f);
    
    // Unproject to world space
    float4x4 invProj = transpose(ProjectionMatrix);
    float4 worldPos = mul(float4(viewRay, 1.0f), invProj);
    worldPos.xyz /= worldPos.w;
    
    // Extend ray from camera
    float3 cameraDir = normalize(worldPos.xyz);
    float3 rayStart = CameraPosition;
    float3 rayEnd = CameraPosition + cameraDir * 10000.0f;
    
    return rayStart + normalize(rayEnd - rayStart) * (depth * 100.0f);
}

// Compute grid lines
float4 GridPattern(float3 fragPos3D, float scale, bool drawAxis)
{
    // Use XZ plane for horizontal grid (Y is up)
    float2 coord = fragPos3D.xz * scale / GridSize;
    float2 derivative = fwidth(coord);
    float2 grid = abs(fract(coord - 0.5f) - 0.5f) / derivative;
    float line = min(grid.x, grid.y);
    
    // Line rendering with anti-aliasing
    float minimumZ = min(derivative.y, 1.0f);
    float minimumX = min(derivative.x, 1.0f);
    
    float4 color = GridColor;
    color.a = (1.0f - min(line, 1.0f)) * GridLineWidth;
    
    // Draw X axis (red) - parallel to X
    if (abs(fragPos3D.z) < 0.1f * minimumZ * AxisLineWidth)
    {
        color = XAxisColor;
        color.a = 1.0f;
    }
    
    // Draw Z axis (blue) - parallel to Z
    if (abs(fragPos3D.x) < 0.1f * minimumX * AxisLineWidth)
    {
        color = ZAxisColor;
        color.a = 1.0f;
    }
    
    return color;
}

// Main pixel shader
float4 PS_Main(VS_Output input) : SV_TARGET
{
    // Get ray from camera through screen pixel
    float3 rayDir = normalize(mul(float3(input.ScreenCoord, 1.0f), (float3x3) transpose(ProjectionMatrix)).xyz);
    float3 rayStart = CameraPosition;
    
    // Intersect with ground plane (Y = 0)
    float t = -rayStart.y / rayDir.y;
    
    // Only render if ray intersects plane in front of camera
    if (t <= 0.0f)
        discard;
    
    float3 fragPos3D = rayStart + rayDir * t;
    
    // Clamp to grid extent
    if (abs(fragPos3D.x) > GridExtent || abs(fragPos3D.z) > GridExtent)
        discard;
    
    // Calculate distance for fading
    float dist = length(fragPos3D - CameraPosition);
    float fade = smoothstep(FadeEndDistance, FadeStartDistance, dist);
    
    // Composite grids at different scales
    float4 majorGrid = GridPattern(fragPos3D, MainGridScale, true);
    float4 minorGrid = GridPattern(fragPos3D, MinorGridScale, false);
    minorGrid.a *= 0.3f; // Make minor grid more subtle
    
    // Blend grids
    float4 finalColor = lerp(minorGrid, majorGrid, majorGrid.a);
    
    // Apply fade
    finalColor.a *= fade;
    
    return finalColor;
}
