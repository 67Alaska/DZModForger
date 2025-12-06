#pragma once
#include "FbxModelData.h"
#include <fbxsdk.h>
#include <string>

namespace DXEngine
{
    class FbxExporter
    {
    public:
        FbxExporter();
        ~FbxExporter();

        void ExportModel(
            const std::string& outputPath,
            const std::vector<Mesh>& meshes,
            const std::vector<Material>& materials,
            bool exportNormals = true,
            bool exportUVs = true,
            bool exportMaterials = true
        );

        std::string GetLastError() const { return _lastError; }

    private:
        FbxManager* _fbxManager;
        FbxScene* _fbxScene;
        std::string _lastError;

        void InitializeFBX();
        void ShutdownFBX();

        void AddMeshToScene(
            const Mesh& mesh,
            const Material& material,
            bool exportNormals,
            bool exportUVs
        );

        void AddMaterialToScene(const Material& material);
    };
}
