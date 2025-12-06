#pragma once
#include "FbxModelData.h"
#include <fbxsdk.h>
#include <string>
#include <sstream>

namespace DXEngine
{
    class FbxImporter
    {
    public:
        FbxImporter();
        ~FbxImporter();

        Model LoadModel(const std::string& filePath);
        bool IsModelLoaded() const { return _isLoaded; }
        std::string GetLastError() const { return _lastError; }

    private:
        FbxManager* _fbxManager;
        FbxScene* _fbxScene;
        bool _isLoaded;
        std::string _lastError;

        void InitializeFBX();
        void ShutdownFBX();
        void ProcessNode(FbxNode* node, Model& model);
        void ProcessMesh(FbxMesh* mesh, Model& model);
        void ProcessMaterial(FbxSurfaceMaterial* material, Model& model);
        void CalculateBoundingBox(Model& model);

        // Vertex data extraction
        void ExtractVertexPosition(FbxMesh* mesh, int vertexIndex, Vertex& vertex);
        void ExtractVertexNormal(FbxMesh* mesh, int vertexIndex, int controlPointIndex, Vertex& vertex);
        void ExtractVertexUV(FbxMesh* mesh, int vertexIndex, int uvIndex, Vertex& vertex);
    };
}
