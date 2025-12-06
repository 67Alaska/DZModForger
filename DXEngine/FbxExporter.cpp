#include "pch.h"
#include "FbxExporter.h"

namespace DXEngine
{
    FbxExporter::FbxExporter()
        : _fbxManager(nullptr), _fbxScene(nullptr)
    {
        InitializeFBX();
    }

    FbxExporter::~FbxExporter()
    {
        ShutdownFBX();
    }

    void FbxExporter::InitializeFBX()
    {
        _fbxManager = FbxManager::Create();
        if (!_fbxManager)
        {
            _lastError = "Failed to create FBX Manager for export";
        }
    }

    void FbxExporter::ShutdownFBX()
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
    }

    void FbxExporter::ExportModel(
        const std::string& outputPath,
        const std::vector<Mesh>& meshes,
        const std::vector<Material>& materials,
        bool exportNormals,
        bool exportUVs,
        bool exportMaterials)
    {
        if (!_fbxManager)
        {
            _lastError = "FBX Manager not initialized for export";
            return;
        }

        if (meshes.empty())
        {
            _lastError = "No meshes to export";
            return;
        }

        // Create scene
        _fbxScene = FbxScene::Create(_fbxManager, "ExportScene");
        if (!_fbxScene)
        {
            _lastError = "Failed to create export scene";
            return;
        }

        // Add materials to scene
        if (exportMaterials)
        {
            for (const auto& material : materials)
            {
                AddMaterialToScene(material);
            }
        }

        // Add meshes
        for (const auto& mesh : meshes)
        {
            const Material& material = (mesh.MaterialIndex < materials.size())
                ? materials[mesh.MaterialIndex]
                : materials.front();

            AddMeshToScene(mesh, material, exportNormals, exportUVs);
        }

        // Create exporter
        FbxExporter* fbxExporter = FbxExporter::Create(_fbxManager, "");
        if (!fbxExporter)
        {
            _lastError = "Failed to create FBX Exporter";
            return;
        }

        // Initialize exporter
        int fileFormat = _fbxManager->GetIOPluginRegistry()->FindWriterIDByDescription("Autodesk FBX 2020");
        if (!fbxExporter->Initialize(outputPath.c_str(), fileFormat, _fbxManager->GetIOSettings()))
        {
            _lastError = "Failed to initialize exporter: " + std::string(fbxExporter->GetStatus().GetErrorString());
            fbxExporter->Destroy();
            return;
        }

        // Export scene
        if (!fbxExporter->Export(_fbxScene))
        {
            _lastError = "Failed to export scene: " + std::string(fbxExporter->GetStatus().GetErrorString());
            fbxExporter->Destroy();
            return;
        }

        fbxExporter->Destroy();
        _lastError = "";
    }

    void FbxExporter::AddMeshToScene(
        const Mesh& mesh,
        const Material& material,
        bool exportNormals,
        bool exportUVs)
    {
        if (mesh.Vertices.empty() || mesh.Indices.empty())
            return;

        // Create mesh
        FbxMesh* fbxMesh = FbxMesh::Create(_fbxManager, mesh.Name.c_str());

        // Initialize mesh with vertices
        fbxMesh->InitControlPoints(static_cast<int>(mesh.Vertices.size()));
        FbxVector4* controlPoints = fbxMesh->GetControlPoints();

        for (size_t i = 0; i < mesh.Vertices.size(); ++i)
        {
            const Vertex& vertex = mesh.Vertices[i];
            controlPoints[i] = FbxVector4(vertex.X, vertex.Y, vertex.Z, 1.0);
        }

        // Add polygon indices
        for (size_t i = 0; i < mesh.Indices.size(); i += 3)
        {
            fbxMesh->BeginPolygon();
            fbxMesh->AddPolygon(mesh.Indices[i]);
            fbxMesh->AddPolygon(mesh.Indices[i + 1]);
            fbxMesh->AddPolygon(mesh.Indices[i + 2]);
            fbxMesh->EndPolygon();
        }

        // Add normals
        if (exportNormals)
        {
            FbxLayerElementNormal* lNormal = FbxLayerElementNormal::Create(fbxMesh, "");
            lNormal->SetMappingMode(FbxLayerElement::eByControlPoint);
            lNormal->SetReferenceMode(FbxLayerElement::eDirect);

            for (const auto& vertex : mesh.Vertices)
            {
                lNormal->GetDirectArray().Add(FbxVector4(vertex.NX, vertex.NY, vertex.NZ));
            }

            fbxMesh->GetLayer(0)->SetNormals(lNormal);
        }

        // Add UVs
        if (exportUVs)
        {
            FbxLayerElementUV* lUVs = FbxLayerElementUV::Create(fbxMesh, "DiffuseUV");
            lUVs->SetMappingMode(FbxLayerElement::eByControlPoint);
            lUVs->SetReferenceMode(FbxLayerElement::eDirect);

            for (const auto& vertex : mesh.Vertices)
            {
                lUVs->GetDirectArray().Add(FbxVector2(vertex.U, vertex.V));
            }

            fbxMesh->GetLayer(0)->SetUVs(lUVs, FbxLayerElement::eTextureDiffuse);
        }

        // Create node
        FbxNode* meshNode = FbxNode::Create(_fbxManager, mesh.Name.c_str());
        meshNode->SetNodeAttribute(fbxMesh);

        // Set material
        if (!material.Name.empty())
        {
            FbxSurfacePhong* phongMaterial = FbxSurfacePhong::Create(_fbxManager, material.Name.c_str());
            phongMaterial->Diffuse.Set(FbxDouble3(
                material.DiffuseColor[0],
                material.DiffuseColor[1],
                material.DiffuseColor[2]
            ));
            phongMaterial->Specular.Set(FbxDouble3(
                material.SpecularColor[0],
                material.SpecularColor[1],
                material.SpecularColor[2]
            ));
            phongMaterial->Shininess.Set(material.Shininess);

            meshNode->AddMaterial(phongMaterial);
        }

        // Add to scene
        _fbxScene->GetRootNode()->AddChild(meshNode);
    }

    void FbxExporter::AddMaterialToScene(const Material& material)
    {
        FbxSurfacePhong* phongMaterial = FbxSurfacePhong::Create(_fbxManager, material.Name.c_str());

        phongMaterial->Diffuse.Set(FbxDouble3(
            material.DiffuseColor[0],
            material.DiffuseColor[1],
            material.DiffuseColor[2]
        ));

        phongMaterial->Specular.Set(FbxDouble3(
            material.SpecularColor[0],
            material.SpecularColor[1],
            material.SpecularColor[2]
        ));

        phongMaterial->Shininess.Set(material.Shininess);
        phongMaterial->Transparency.Set(1.0 - material.DiffuseColor[3]);
    }
}
