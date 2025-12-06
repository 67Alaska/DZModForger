// C# FBX Import Service for DZModForger
// This bridges native C++ FbxWrapper with your C# WinUI app

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

namespace DZModForger.Services
{
    // ============================================================================
    // C# Data Models (matches C++ structures)
    // ============================================================================

    public class MeshData
    {
        public string Name { get; set; } = "Mesh";
        public List<float> VertexData { get; set; } = new();        // Flattened vertices
        public List<uint> IndexData { get; set; } = new();
        public uint MaterialIndex { get; set; } = 0;
        public uint VertexCount { get; set; } = 0;
        public uint IndexCount { get; set; } = 0;
    }

    public class MaterialData
    {
        public string Name { get; set; } = "Material";
        public float[] DiffuseColor { get; set; } = { 0.8f, 0.8f, 0.8f, 1.0f };
        public float[] SpecularColor { get; set; } = { 0.5f, 0.5f, 0.5f, 1.0f };
        public float Shininess { get; set; } = 32.0f;
    }

    public class ModelData
    {
        public string Name { get; set; } = "Model";
        public string FilePath { get; set; } = "";
        public List<MeshData> Meshes { get; set; } = new();
        public List<MaterialData> Materials { get; set; } = new();
        public float[] BoundingBoxMin { get; set; } = { 0, 0, 0 };
        public float[] BoundingBoxMax { get; set; } = { 1, 1, 1 };
    }

    // ============================================================================
    // P/Invoke Interop Layer
    // ============================================================================

    public static class FbxInterop
    {
        // Use relative path from your DXEngine DLL location
        private const string DllName = "DXEngine.dll";

        // FBX Importer P/Invoke declarations
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateImporter();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LoadFbxModel(IntPtr importer, [MarshalAs(UnmanagedType.LPStr)] string filePath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetMeshCount(IntPtr importer);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetMaterialCount(IntPtr importer);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetMeshData(
            IntPtr importer,
            int meshIndex,
            [MarshalAs(UnmanagedType.LPStr)] out string meshName,
            out uint vertexCount,
            out uint indexCount,
            [In, Out] float[] vertices,
            [In, Out] uint[] indices,
            out uint materialIndex
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetMaterialData(
            IntPtr importer,
            int materialIndex,
            [MarshalAs(UnmanagedType.LPStr)] out string materialName,
            [In, Out] float[] diffuseColor,
            [In, Out] float[] specularColor,
            out float shininess
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetBoundingBox(
            IntPtr importer,
            [In, Out] float[] minBounds,
            [In, Out] float[] maxBounds
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetLastError(IntPtr importer);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyImporter(IntPtr importer);

        // FBX Exporter P/Invoke declarations
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateExporter();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ExportFbxModel(
            IntPtr exporter,
            [MarshalAs(UnmanagedType.LPStr)] string outputPath,
            int meshCount,
            int materialCount,
            [In] float[] allVertices,
            [In] uint[] allIndices,
            [In] float[] allMaterials,
            int vertexStride,
            int indexStride
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyExporter(IntPtr exporter);
    }

    // ============================================================================
    // C# FBX Import Service
    // ============================================================================

    public class FbxImportService
    {
        public ModelData ImportFbx(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var model = new ModelData
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath
            };

            IntPtr importer = FbxInterop.CreateImporter();
            try
            {
                // Load the model
                int result = FbxInterop.LoadFbxModel(importer, filePath);
                if (result != 1)
                {
                    string error = Marshal.PtrToStringAnsi(FbxInterop.GetLastError(importer)) ?? "Unknown error";
                    throw new InvalidOperationException($"Failed to load FBX: {error}");
                }

                // Get mesh and material counts
                int meshCount = FbxInterop.GetMeshCount(importer);
                int materialCount = FbxInterop.GetMaterialCount(importer);

                // Load materials
                for (int i = 0; i < materialCount; i++)
                {
                    var material = new MaterialData();
                    float[] diffuse = new float[4];
                    float[] specular = new float[4];

                    FbxInterop.GetMaterialData(
                        importer,
                        i,
                        out string materialName,
                        diffuse,
                        specular,
                        out float shininess
                    );

                    material.Name = materialName;
                    material.DiffuseColor = diffuse;
                    material.SpecularColor = specular;
                    material.Shininess = shininess;

                    model.Materials.Add(material);
                }

                // Load meshes
                for (int i = 0; i < meshCount; i++)
                {
                    FbxInterop.GetMeshData(
                        importer,
                        i,
                        out string meshName,
                        out uint vertexCount,
                        out uint indexCount,
                        null,  // Query for size first
                        null,
                        out uint materialIndex
                    );

                    var mesh = new MeshData
                    {
                        Name = meshName,
                        VertexCount = vertexCount,
                        IndexCount = indexCount,
                        MaterialIndex = materialIndex
                    };

                    // Allocate buffers
                    float[] vertices = new float[vertexCount * 12]; // 3 pos + 3 normal + 2 uv + 4 color
                    uint[] indices = new uint[indexCount];

                    FbxInterop.GetMeshData(
                        importer,
                        i,
                        out meshName,
                        out vertexCount,
                        out indexCount,
                        vertices,
                        indices,
                        out materialIndex
                    );

                    mesh.VertexData = vertices.ToList();
                    mesh.IndexData = indices.ToList();

                    model.Meshes.Add(mesh);
                }

                // Get bounding box
                float[] minBounds = new float[3];
                float[] maxBounds = new float[3];
                FbxInterop.GetBoundingBox(importer, minBounds, maxBounds);
                model.BoundingBoxMin = minBounds;
                model.BoundingBoxMax = maxBounds;

                return model;
            }
            finally
            {
                FbxInterop.DestroyImporter(importer);
            }
        }
    }

    // ============================================================================
    // C# FBX Export Service
    // ============================================================================

    public class FbxExportService
    {
        public void ExportFbx(string outputPath, ModelData model)
        {
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentException("Output path required");

            if (model.Meshes.Count == 0)
                throw new InvalidOperationException("No meshes to export");

            IntPtr exporter = FbxInterop.CreateExporter();
            try
            {
                // Flatten all vertex and index data
                var allVertices = new List<float>();
                var allIndices = new List<uint>();

                foreach (var mesh in model.Meshes)
                {
                    allVertices.AddRange(mesh.VertexData);

                    // Offset indices by current vertex count
                    uint vertexOffset = (uint)(allVertices.Count / 12); // 12 floats per vertex
                    foreach (var idx in mesh.IndexData)
                    {
                        allIndices.Add(idx + vertexOffset);
                    }
                }

                // Flatten material data
                var allMaterials = new List<float>();
                foreach (var material in model.Materials)
                {
                    allMaterials.AddRange(material.DiffuseColor);
                    allMaterials.AddRange(material.SpecularColor);
                    allMaterials.Add(material.Shininess);
                }

                FbxInterop.ExportFbxModel(
                    exporter,
                    outputPath,
                    model.Meshes.Count,
                    model.Materials.Count,
                    allVertices.ToArray(),
                    allIndices.ToArray(),
                    allMaterials.ToArray(),
                    12 * sizeof(float), // vertex stride in bytes
                    sizeof(uint)         // index stride in bytes
                );
            }
            finally
            {
                FbxInterop.DestroyExporter(exporter);
            }
        }
    }
}