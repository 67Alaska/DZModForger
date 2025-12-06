#pragma once
#include <cstdint>
#include <vector>
#include <string>
#include <array>

namespace DXEngine
{
    struct Vertex
    {
        float X, Y, Z;          // Position
        float NX, NY, NZ;       // Normal
        float U, V;             // Texture Coordinates
    };

    struct Material
    {
        std::string Name;
        std::array<float, 4> DiffuseColor;      // RGBA
        std::array<float, 4> SpecularColor;     // RGBA
        float Shininess;
        std::string TexturePath;
    };

    struct Mesh
    {
        std::string Name;
        std::vector<Vertex> Vertices;
        std::vector<uint32_t> Indices;
        uint32_t MaterialIndex;
    };

    struct Model
    {
        std::string FilePath;
        std::string FileName;
        std::vector<Mesh> Meshes;
        std::vector<Material> Materials;
        std::array<float, 3> BoundingBoxMin;
        std::array<float, 3> BoundingBoxMax;
    };
}
