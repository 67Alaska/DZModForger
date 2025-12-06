using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DZModForger.Services
{
    /// <summary>
    /// Service for exporting models to FBX files
    /// </summary>
    public class FbxExportService
    {
        public event EventHandler<ExportProgressEventArgs>? ProgressChanged;
        public event EventHandler<string>? ErrorOccurred;

        public FbxExportService()
        {
        }

        /// <summary>
        /// Export model data to FBX file asynchronously
        /// </summary>
        public async System.Threading.Tasks.Task ExportFbxAsync(string outputPath, ModelData model)
        {
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            await System.Threading.Tasks.Task.Run(() => ExportFbx(outputPath, model));
        }

        /// <summary>
        /// Export model data to FBX file synchronously
        /// </summary>
        public void ExportFbx(string outputPath, ModelData model)
        {
            try
            {
                OnProgressChanged("Preparing export...", 0);

                // Convert managed model to native format
                var nativeModel = MarshalToNativeModel(model);
                OnProgressChanged("Marshaling data...", 50);

                // Export using interop
                FbxInteropService.SaveFbxFile(outputPath, nativeModel);
                OnProgressChanged("Export complete", 100);

                Debug.WriteLine($"Exported model: {model.Name} to {outputPath}");
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Failed to export FBX: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Convert managed ModelData to native FbxInteropService.Model
        /// </summary>
        private FbxInteropService.Model MarshalToNativeModel(ModelData managedModel)
        {
            var nativeModel = new FbxInteropService.Model
            {
                Name = managedModel.Name,
                MeshCount = (uint)managedModel.Meshes.Count,
                MaterialCount = (uint)managedModel.Materials.Count,
                BoundingBoxMinX = managedModel.BoundingBoxMin,
                BoundingBoxMinY = managedModel.BoundingBoxMin,
                BoundingBoxMinZ = managedModel.BoundingBoxMin,
                BoundingBoxMaxX = managedModel.BoundingBoxMax,
                BoundingBoxMaxY = managedModel.BoundingBoxMax,
                BoundingBoxMaxZ = managedModel.BoundingBoxMax
            };

            // Marshal meshes
            if (managedModel.Meshes.Count > 0)
            {
                int meshSize = Marshal.SizeOf<FbxInteropService.Mesh>();
                IntPtr meshArray = Marshal.AllocHGlobal(meshSize * managedModel.Meshes.Count);

                for (int i = 0; i < managedModel.Meshes.Count; i++)
                {
                    var nativeMesh = MarshalToNativeMesh(managedModel.Meshes[i]);
                    IntPtr meshPtr = new IntPtr(meshArray.ToInt64() + (i * meshSize));
                    Marshal.StructureToPtr(nativeMesh, meshPtr, false);
                }

                nativeModel.MeshArray = meshArray;
            }

            // Marshal materials
            if (managedModel.Materials.Count > 0)
            {
                int materialSize = Marshal.SizeOf<FbxInteropService.Material>();
                IntPtr materialArray = Marshal.AllocHGlobal(materialSize * managedModel.Materials.Count);

                for (int i = 0; i < managedModel.Materials.Count; i++)
                {
                    var nativeMaterial = MarshalToNativeMaterial(managedModel.Materials[i]);
                    IntPtr materialPtr = new IntPtr(materialArray.ToInt64() + (i * materialSize));
                    Marshal.StructureToPtr(nativeMaterial, materialPtr, false);
                }

                nativeModel.MaterialArray = materialArray;
            }

            return nativeModel;
        }

        private FbxInteropService.Mesh MarshalToNativeMesh(MeshData managedMesh)
        {
            var nativeMesh = new FbxInteropService.Mesh
            {
                Name = managedMesh.Name,
                VertexCount = managedMesh.VertexCount,
                IndexCount = managedMesh.IndexCount,
                VertexStride = 48, // sizeof(Vertex)
                MaterialIndex = managedMesh.MaterialIndex,
                BoundingBoxMinX = managedMesh.BoundingBoxMin,
                BoundingBoxMinY = managedMesh.BoundingBoxMin,
                BoundingBoxMinZ = managedMesh.BoundingBoxMin,
                BoundingBoxMaxX = managedMesh.BoundingBoxMax,
                BoundingBoxMaxY = managedMesh.BoundingBoxMax,
                BoundingBoxMaxZ = managedMesh.BoundingBoxMax
            };

            // Marshal vertices
            if (managedMesh.Vertices.Count > 0)
            {
                int vertexSize = Marshal.SizeOf<FbxInteropService.Vertex>();
                IntPtr vertexArray = Marshal.AllocHGlobal(vertexSize * managedMesh.Vertices.Count);

                for (int i = 0; i < managedMesh.Vertices.Count; i++)
                {
                    var nativeVertex = new FbxInteropService.Vertex
                    {
                        PositionX = managedMesh.Vertices[i].Position,
                        PositionY = managedMesh.Vertices[i].Position,
                        PositionZ = managedMesh.Vertices[i].Position,
                        NormalX = managedMesh.Vertices[i].Normal,
                        NormalY = managedMesh.Vertices[i].Normal,
                        NormalZ = managedMesh.Vertices[i].Normal,
                        TexCoordU = managedMesh.Vertices[i].TexCoord,
                        TexCoordV = managedMesh.Vertices[i].TexCoord,
                        ColorR = managedMesh.Vertices[i].Color,
                        ColorG = managedMesh.Vertices[i].Color,
                        ColorB = managedMesh.Vertices[i].Color,
                        ColorA = managedMesh.Vertices[i].Color
                    };

                    IntPtr vertexPtr = new IntPtr(vertexArray.ToInt64() + (i * vertexSize));
                    Marshal.StructureToPtr(nativeVertex, vertexPtr, false);
                }

                nativeMesh.VertexData = vertexArray;
            }

            // Marshal indices
            if (managedMesh.Indices.Count > 0)
            {
                IntPtr indexArray = Marshal.AllocHGlobal(sizeof(uint) * managedMesh.Indices.Count);
                Marshal.Copy(managedMesh.Indices.ToArray(), 0, indexArray, managedMesh.Indices.Count);
                nativeMesh.IndexData = indexArray;
            }

            return nativeMesh;
        }

        private FbxInteropService.Material MarshalToNativeMaterial(MaterialData managedMaterial)
        {
            return new FbxInteropService.Material
            {
                Name = managedMaterial.Name,
                DiffuseR = managedMaterial.DiffuseColor,
                DiffuseG = managedMaterial.DiffuseColor,
                DiffuseB = managedMaterial.DiffuseColor,
                DiffuseA = managedMaterial.DiffuseColor,
                SpecularR = managedMaterial.SpecularColor,
                SpecularG = managedMaterial.SpecularColor,
                SpecularB = managedMaterial.SpecularColor,
                SpecularA = managedMaterial.SpecularColor,
                Shininess = managedMaterial.Shininess,
                Transparency = managedMaterial.Transparency
            };
        }

        protected virtual void OnProgressChanged(string message, int percentage)
        {
            ProgressChanged?.Invoke(this, new ExportProgressEventArgs(message, percentage));
        }

        protected virtual void OnErrorOccurred(string error)
        {
            ErrorOccurred?.Invoke(this, error);
        }
    }

    public class ExportProgressEventArgs : EventArgs
    {
        public string Message { get; }
        public int Percentage { get; }

        public ExportProgressEventArgs(string message, int percentage)
        {
            Message = message;
            Percentage = Math.Clamp(percentage, 0, 100);
        }
    }
}
