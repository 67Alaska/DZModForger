#include "pch.h"
#include "FbxImporter.h"
#include <algorithm>
#include <cmath>

namespace DXEngine
{
    FbxImporter::FbxImporter()
        : _fbxManager(nullptr), _fbxScene(nullptr), _isLoaded(false)
    {
        InitializeFBX();
    }

    FbxImporter::~FbxImporter()
    {
        ShutdownFBX();
    }

    void FbxImporter::InitializeFBX()
    {
        _fbxManager = FbxManager::Create();
        if (!_fbxManager)
        {
            _lastError = "Failed to create FBX Manager";
            return;
        }

        FbxIOSettings* ios = FbxIOSettings::Create(_fbxManager, IOSN_IMPORT);
        _fbxManager->SetIOSettings(ios);
    }

    void FbxImporter::ShutdownFBX()
    {
        if (_fbxScene)
        {
            _fbxScene->Destroy();
            _fbxScene = nullptr;
        }

        if (_fbxManager)
        {
            _fbxManager->Destroy();
            _fbxManager = nullptr;
        }

        _isLoaded = false;
    }

    Model FbxImporter::LoadModel(const std::string& filePath)
    {
        Model model;
        model.FilePath = filePath;

        // Extract filename from path
        size_t lastSlash = filePath.find_last_of("/\\");
        model.FileName = (lastSlash == std::string::npos) ? filePath : filePath.substr(lastSlash + 1);

        if (!_fbxManager)
        {
            _lastError = "FBX Manager not initialized";
            return model;
        }

        // Create importer
        FbxImporter* fbxImporter = FbxImporter::Create(_fbxManager, "");
        if (!fbxImporter)
        {
            _lastError = "Failed to create FBX Importer";
            return model;
        }

        // Initialize importer with file
        int fileFormat = -1;
        if (!fbxImporter->Initialize(filePath.c_str(), fileFormat, _fbxManager->GetIOSettings()))
        {
            _lastError = "Failed to initialize importer: " + std::string(fbxImporter->GetStatus().GetErrorString());
            fbxImporter->Destroy();
            return model;
        }

        // Create scene
        _fbxScene = FbxScene::Create(_fbxManager, "ImportedScene");
        if (!_fbxScene)
        {
            _lastError = "Failed to create FBX Scene";
            fbxImporter->Destroy();
            return model;
        }

        // Import scene
        if (!fbxImporter->Import(_fbxScene))
        {
            _lastError = "Failed to import scene: " + std::string(fbxImporter->GetStatus().GetErrorString());
            fbxImporter->Destroy();
            return model;
        }

        // Process scene hierarchy
        FbxNode* rootNode = _fbxScene->GetRootNode();
        if (rootNode)
        {
            ProcessNode(rootNode, model);
        }

        // Process materials
        int materialCount = _fbxScene->GetMaterialCount();
        for (int i = 0; i < materialCount; ++i)
        {
            FbxSurfaceMaterial* material = _fbxScene->GetMaterial(i);
            ProcessMaterial(material, model);
        }

        // Calculate bounding box
        CalculateBoundingBox(model);

        fbxImporter->Destroy();
        _isLoaded = true;
        _lastError = "";

        return model;
    }

    void FbxImporter::ProcessNode(FbxNode* node, Model& model)
    {
        if (!node) return;

        FbxNodeAttribute::EType attributeType = node->GetNodeAttribute()
            ? node->GetNodeAttribute()->GetAttributeType()
            : FbxNodeAttribute::eUnknown;

        if (attributeType == FbxNodeAttribute::eMesh)
        {
            FbxMesh* mesh = static_cast<FbxMesh*>(node->GetNodeAttribute());
            if (mesh && !mesh->IsTriangleMesh())
            {
                FbxGeometryConverter converter(_fbxManager);
                converter.TriangulateInPlace(node);
                mesh = static_cast<FbxMesh*>(node->GetNodeAttribute());
            }
            ProcessMesh(mesh, model);
        }

        // Recursively process child nodes
        for (int i = 0; i < node->GetChildCount(); ++i)
        {
            ProcessNode(node->GetChild(i), model);
        }
    }

    void FbxImporter::ProcessMesh(FbxMesh* mesh, Model& model)
    {
        if (!mesh || mesh->GetPolygonCount() == 0)
            return;

        Mesh fbxMesh;
        fbxMesh.Name = mesh->GetName();
        fbxMesh.MaterialIndex = 0;

        int controlPointCount = mesh->GetControlPointsCount();
        const FbxVector4* controlPoints = mesh->GetControlPoints();

        // Ensure normals are computed
        if (mesh->GetElementNormalCount() == 0)
        {
            mesh->GenerateNormals(true);
        }

        // Process polygons
        int polygonCount = mesh->GetPolygonCount();
        int vertexIndex = 0;

        for (int polyIndex = 0; polyIndex < polygonCount; ++polyIndex)
        {
            int polygonSize = mesh->GetPolygonSize(polyIndex);

            for (int i = 0; i < polygonSize; ++i)
            {
                int controlPointIndex = mesh->GetPolygonVertex(polyIndex, i);
                Vertex vertex = {};

                // Extract position
                ExtractVertexPosition(mesh, controlPointIndex, vertex);

                // Extract normal
                ExtractVertexNormal(mesh, vertexIndex, controlPointIndex, vertex);

                // Extract UV
                ExtractVertexUV(mesh, vertexIndex, 0, vertex);

                fbxMesh.Vertices.push_back(vertex);
                fbxMesh.Indices.push_back(static_cast<uint32_t>(fbxMesh.Vertices.size() - 1));

                ++vertexIndex;
            }
        }

        // Get material index from polygon group
        FbxLayerElementMaterial* leMaterial = mesh->GetLayer(0)->GetMaterials();
        if (leMaterial)
        {
            int polyGroupMatIndex = leMaterial->GetIndexArray().GetAt(0);
            fbxMesh.MaterialIndex = static_cast<uint32_t>(polyGroupMatIndex);
        }

        model.Meshes.push_back(fbxMesh);
    }

    void FbxImporter::ExtractVertexPosition(FbxMesh* mesh, int vertexIndex, Vertex& vertex)
    {
        const FbxVector4* controlPoints = mesh->GetControlPoints();
        if (vertexIndex < mesh->GetControlPointsCount())
        {
            FbxVector4 pos = controlPoints[vertexIndex];
            vertex.X = static_cast<float>(pos[0]);
            vertex.Y = static_cast<float>(pos[1]);
            vertex.Z = static_cast<float>(pos[2]);
        }
    }

    void FbxImporter::ExtractVertexNormal(FbxMesh* mesh, int vertexIndex, int controlPointIndex, Vertex& vertex)
    {
        FbxLayerElementNormal* leNormal = mesh->GetLayer(0)->GetNormals();
        if (leNormal)
        {
            int normalIndex = (leNormal->GetReferenceMode() == FbxLayerElement::eDirect)
                ? vertexIndex
                : leNormal->GetIndexArray().GetAt(vertexIndex);

            FbxVector4 normal = leNormal->GetDirectArray().GetAt(normalIndex);
            vertex.NX = static_cast<float>(normal[0]);
            vertex.NY = static_cast<float>(normal[1]);
            vertex.NZ = static_cast<float>(normal[2]);
        }
    }

    void FbxImporter::ExtractVertexUV(FbxMesh* mesh, int vertexIndex, int uvIndex, Vertex& vertex)
    {
        FbxLayerElementUV* leUV = mesh->GetLayer(0)->GetUVs(uvIndex);
        if (leUV)
        {
            int uvArrayIndex = (leUV->GetReferenceMode() == FbxLayerElement::eDirect)
                ? vertexIndex
                : leUV->GetIndexArray().GetAt(vertexIndex);

            FbxVector2 uv = leUV->GetDirectArray().GetAt(uvArrayIndex);
            vertex.U = static_cast<float>(uv[0]);
            vertex.V = static_cast<float>(uv[1]);
        }
    }

    void FbxImporter::ProcessMaterial(FbxSurfaceMaterial* material, Model& model)
    {
        if (!material) return;

        Material mat;
        mat.Name = material->GetName();

        // Default values
        mat.DiffuseColor = { 0.8f, 0.8f, 0.8f, 1.0f };
        mat.SpecularColor = { 0.5f, 0.5f, 0.5f, 1.0f };
        mat.Shininess = 32.0f;

        // Extract diffuse color
        if (material->GetClassId().Is(FbxSurfaceLambert::ClassId))
        {
            FbxSurfaceLambert* lambert = static_cast<FbxSurfaceLambert*>(material);
            FbxDouble3 diffuse = lambert->Diffuse.Get();
            mat.DiffuseColor = {
                static_cast<float>(diffuse[0]),
                static_cast<float>(diffuse[1]),
                static_cast<float>(diffuse[2]),
                1.0f
            };
        }

        // Extract specular if Phong material
        if (material->GetClassId().Is(FbxSurfacePhong::ClassId))
        {
            FbxSurfacePhong* phong = static_cast<FbxSurfacePhong*>(material);
            FbxDouble3 specular = phong->Specular.Get();
            mat.SpecularColor = {
                static_cast<float>(specular[0]),
                static_cast<float>(specular[1]),
                static_cast<float>(specular[2]),
                1.0f
            };
            mat.Shininess = static_cast<float>(phong->Shininess.Get());
        }

        model.Materials.push_back(mat);
    }

    void FbxImporter::CalculateBoundingBox(Model& model)
    {
        if (model.Meshes.empty())
        {
            model.BoundingBoxMin = { 0.0f, 0.0f, 0.0f };
            model.BoundingBoxMax = { 0.0f, 0.0f, 0.0f };
            return;
        }

        float minX = FLT_MAX, minY = FLT_MAX, minZ = FLT_MAX;
        float maxX = -FLT_MAX, maxY = -FLT_MAX, maxZ = -FLT_MAX;

        for (const auto& mesh : model.Meshes)
        {
            for (const auto& vertex : mesh.Vertices)
            {
                minX = std::min(minX, vertex.X);
                minY = std::min(minY, vertex.Y);
                minZ = std::min(minZ, vertex.Z);

                maxX = std::max(maxX, vertex.X);
                maxY = std::max(maxY, vertex.Y);
                maxZ = std::max(maxZ, vertex.Z);
            }
        }

        model.BoundingBoxMin = { minX, minY, minZ };
        model.BoundingBoxMax = { maxX, maxY, maxZ };
    }
}
