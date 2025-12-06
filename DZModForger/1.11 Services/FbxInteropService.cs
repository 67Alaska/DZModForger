using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace DZModForger.Services
{
    /// <summary>
    /// P/Invoke bridge to native DXEngine.dll for FBX operations
    /// </summary>
    public static class FbxInteropService
    {
        private const string DllName = "DXEngine.dll";

        #region Vertex Structure

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public float PositionX, PositionY, PositionZ;           // 12 bytes
            public float NormalX, NormalY, NormalZ;                 // 12 bytes
            public float TexCoordU, TexCoordV;                      // 8 bytes
            public float ColorR, ColorG, ColorB, ColorA;            // 16 bytes
            // Total: 48 bytes per vertex
        }

        #endregion

        #region Material Structure

        [StructLayout(LayoutKind.Sequential)]
        public struct Material
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Name;

            public float DiffuseR, DiffuseG, DiffuseB, DiffuseA;
            public float SpecularR, SpecularG, SpecularB, SpecularA;
            public float Shininess;
            public float Transparency;
        }

        #endregion

        #region Mesh Structure

        [StructLayout(LayoutKind.Sequential)]
        public struct Mesh
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Name;

            public IntPtr VertexData;           // Pointer to Vertex array
            public uint VertexCount;
            public uint VertexStride;

            public IntPtr IndexData;            // Pointer to uint array
            public uint IndexCount;

            public int MaterialIndex;

            public float BoundingBoxMinX, BoundingBoxMinY, BoundingBoxMinZ;
            public float BoundingBoxMaxX, BoundingBoxMaxY, BoundingBoxMaxZ;
        }

        #endregion

        #region Model Structure

        [StructLayout(LayoutKind.Sequential)]
        public struct Model
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Name;

            public IntPtr MeshArray;            // Pointer to Mesh array
            public uint MeshCount;

            public IntPtr MaterialArray;        // Pointer to Material array
            public uint MaterialCount;

            public float BoundingBoxMinX, BoundingBoxMinY, BoundingBoxMinZ;
            public float BoundingBoxMaxX, BoundingBoxMaxY, BoundingBoxMaxZ;
        }

        #endregion

        #region P/Invoke Declarations

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateImporter();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyImporter(IntPtr importer);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void LoadFbxModel(IntPtr importer, string filePath, out Model model);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetLastError(IntPtr importer);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateExporter();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyExporter(IntPtr exporter);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void ExportFbxModel(IntPtr exporter, string outputPath, in Model model);

        #endregion

        #region Public Methods

        /// <summary>
        /// Load an FBX file and return model data
        /// </summary>
        public static Model LoadFbxFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            IntPtr importer = IntPtr.Zero;
            try
            {
                importer = CreateImporter();
                if (importer == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to create FBX importer");

                LoadFbxModel(importer, filePath, out Model model);

                // Check for errors
                IntPtr errorPtr = GetLastError(importer);
                if (errorPtr != IntPtr.Zero)
                {
                    string error = Marshal.PtrToStringAnsi(errorPtr);
                    if (!string.IsNullOrEmpty(error))
                        throw new InvalidOperationException($"FBX loading error: {error}");
                }

                return model;
            }
            finally
            {
                if (importer != IntPtr.Zero)
                    DestroyImporter(importer);
            }
        }

        /// <summary>
        /// Export model data to an FBX file
        /// </summary>
        public static void SaveFbxFile(string outputPath, Model model)
        {
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));

            IntPtr exporter = IntPtr.Zero;
            try
            {
                exporter = CreateExporter();
                if (exporter == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to create FBX exporter");

                ExportFbxModel(exporter, outputPath, model);
            }
            finally
            {
                if (exporter != IntPtr.Zero)
                    DestroyExporter(exporter);
            }
        }

        /// <summary>
        /// Convert native model to managed ModelData
        /// </summary>
        public static ModelData MarshalModel(Model nativeModel)
        {
            var modelData = new ModelData
            {
                Name = nativeModel.Name,
                BoundingBoxMin = new float[]
                {
                    nativeModel.BoundingBoxMinX,
                    nativeModel.BoundingBoxMinY,
                    nativeModel.BoundingBoxMinZ
                },
                BoundingBoxMax = new float[]
                {
                    nativeModel.BoundingBoxMaxX,
                    nativeModel.BoundingBoxMaxY,
                    nativeModel.BoundingBoxMaxZ
                }
            };

            // Marshal meshes
            if (nativeModel.MeshCount > 0 && nativeModel.MeshArray != IntPtr.Zero)
            {
                int meshSize = Marshal.SizeOf<Mesh>();
                for (int i = 0; i < nativeModel.MeshCount; i++)
                {
                    IntPtr meshPtr = new IntPtr(nativeModel.MeshArray.ToInt64() + (i * meshSize));
                    Mesh nativeMesh = Marshal.PtrToStructure<Mesh>(meshPtr);

                    var meshData = MarshalMesh(nativeMesh);
                    modelData.Meshes.Add(meshData);
                }
            }

            // Marshal materials
            if (nativeModel.MaterialCount > 0 && nativeModel.MaterialArray != IntPtr.Zero)
            {
                int materialSize = Marshal.SizeOf<Material>();
                for (int i = 0; i < nativeModel.MaterialCount; i++)
                {
                    IntPtr materialPtr = new IntPtr(nativeModel.MaterialArray.ToInt64() + (i * materialSize));
                    Material nativeMaterial = Marshal.PtrToStructure<Material>(materialPtr);

                    var materialData = MarshalMaterial(nativeMaterial);
                    modelData.Materials.Add(materialData);
                }
            }

            return modelData;
        }

        private static MeshData MarshalMesh(Mesh nativeMesh)
        {
            var meshData = new MeshData
            {
                Name = nativeMesh.Name,
                VertexCount = nativeMesh.VertexCount,
                IndexCount = nativeMesh.IndexCount,
                MaterialIndex = nativeMesh.MaterialIndex,
                BoundingBoxMin = new float[]
                {
                    nativeMesh.BoundingBoxMinX,
                    nativeMesh.BoundingBoxMinY,
                    nativeMesh.BoundingBoxMinZ
                },
                BoundingBoxMax = new float[]
                {
                    nativeMesh.BoundingBoxMaxX,
                    nativeMesh.BoundingBoxMaxY,
                    nativeMesh.BoundingBoxMaxZ
                }
            };

            // Marshal vertices
            if (nativeMesh.VertexCount > 0 && nativeMesh.VertexData != IntPtr.Zero)
            {
                int vertexSize = Marshal.SizeOf<Vertex>();
                for (int i = 0; i < nativeMesh.VertexCount; i++)
                {
                    IntPtr vertexPtr = new IntPtr(nativeMesh.VertexData.ToInt64() + (i * vertexSize));
                    Vertex vertex = Marshal.PtrToStructure<Vertex>(vertexPtr);

                    meshData.Vertices.Add(new VertexData
                    {
                        Position = new float[] { vertex.PositionX, vertex.PositionY, vertex.PositionZ },
                        Normal = new float[] { vertex.NormalX, vertex.NormalY, vertex.NormalZ },
                        TexCoord = new float[] { vertex.TexCoordU, vertex.TexCoordV },
                        Color = new float[] { vertex.ColorR, vertex.ColorG, vertex.ColorB, vertex.ColorA }
                    });
                }
            }

            // Marshal indices
            if (nativeMesh.IndexCount > 0 && nativeMesh.IndexData != IntPtr.Zero)
            {
                uint[] indices = new uint[nativeMesh.IndexCount];
                Marshal.Copy(nativeMesh.IndexData, indices, 0, (int)nativeMesh.IndexCount);
                meshData.Indices.AddRange(indices);
            }

            return meshData;
        }

        private static MaterialData MarshalMaterial(Material nativeMaterial)
        {
            return new MaterialData
            {
                Name = nativeMaterial.Name,
                DiffuseColor = new float[]
                {
                    nativeMaterial.DiffuseR,
                    nativeMaterial.DiffuseG,
                    nativeMaterial.DiffuseB,
                    nativeMaterial.DiffuseA
                },
                SpecularColor = new float[]
                {
                    nativeMaterial.SpecularR,
                    nativeMaterial.SpecularG,
                    nativeMaterial.SpecularB,
                    nativeMaterial.SpecularA
                },
                Shininess = nativeMaterial.Shininess,
                Transparency = nativeMaterial.Transparency
            };
        }

        #endregion
    }
}
