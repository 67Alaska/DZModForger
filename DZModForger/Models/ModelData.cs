using System;
using System.Collections.Generic;

namespace DZModForger.Models
{
    /// <summary>
    /// Complete model data with meshes, materials, and transform
    /// </summary>
    public class ModelData
    {
        /// <summary>
        /// Unique identifier for this model
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name of the model
        /// </summary>
        public string Name { get; set; } = "Model";

        /// <summary>
        /// File path where model was loaded from
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// List of meshes in this model
        /// </summary>
        public List<MeshData> Meshes { get; set; } = new();

        /// <summary>
        /// List of materials used by meshes
        /// </summary>
        public List<MaterialData> Materials { get; set; } = new();

        /// <summary>
        /// Model transform (position, rotation, scale)
        /// </summary>
        public TransformData Transform { get; set; } = new();

        /// <summary>
        /// Minimum point of bounding box
        /// </summary>
        public float[] BoundingBoxMin { get; set; } = new[] { 0f, 0f, 0f };

        /// <summary>
        /// Maximum point of bounding box
        /// </summary>
        public float[] BoundingBoxMax { get; set; } = new[] { 0f, 0f, 0f };

        /// <summary>
        /// Whether the model is visible
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Whether to render in wireframe mode
        /// </summary>
        public bool ShowWireframe { get; set; } = false;

        /// <summary>
        /// Creation date/time
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last modified date/time
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        public ModelData()
        {
        }

        public ModelData(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Calculate bounding box from all meshes
        /// </summary>
        public void CalculateBoundingBox()
        {
            if (Meshes.Count == 0)
            {
                BoundingBoxMin = new[] { 0f, 0f, 0f };
                BoundingBoxMax = new[] { 0f, 0f, 0f };
                return;
            }

            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            foreach (var mesh in Meshes)
            {
                minX = Math.Min(minX, mesh.BoundingBoxMin);
                minY = Math.Min(minY, mesh.BoundingBoxMin);
                minZ = Math.Min(minZ, mesh.BoundingBoxMin);

                maxX = Math.Max(maxX, mesh.BoundingBoxMax);
                maxY = Math.Max(maxY, mesh.BoundingBoxMax);
                maxZ = Math.Max(maxZ, mesh.BoundingBoxMax);
            }

            BoundingBoxMin = new[] { minX, minY, minZ };
            BoundingBoxMax = new[] { maxX, maxY, maxZ };
        }

        /// <summary>
        /// Get total vertex count
        /// </summary>
        public int GetVertexCount()
        {
            int total = 0;
            foreach (var mesh in Meshes)
                total += (int)mesh.VertexCount;
            return total;
        }

        /// <summary>
        /// Get total triangle count
        /// </summary>
        public int GetTriangleCount()
        {
            int total = 0;
            foreach (var mesh in Meshes)
                total += (int)mesh.GetTriangleCount();
            return total;
        }

        /// <summary>
        /// Get bounding box size
        /// </summary>
        public float GetBoundingBoxSize()
        {
            float sizeX = BoundingBoxMax - BoundingBoxMin;
            float sizeY = BoundingBoxMax - BoundingBoxMin;
            float sizeZ = BoundingBoxMax - BoundingBoxMin;

            return Math.Max(Math.Max(sizeX, sizeY), sizeZ);
        }

        /// <summary>
        /// Get bounding box center
        /// </summary>
        public float[] GetBoundingBoxCenter()
        {
            return new[]
            {
                (BoundingBoxMin + BoundingBoxMax) / 2f,
                (BoundingBoxMin + BoundingBoxMax) / 2f,
                (BoundingBoxMin + BoundingBoxMax) / 2f
            };
        }

        /// <summary>
        /// Add a mesh to the model
        /// </summary>
        public void AddMesh(MeshData mesh)
        {
            if (mesh != null)
            {
                Meshes.Add(mesh);
                ModifiedAt = DateTime.Now;
            }
        }

        /// <summary>
        /// Remove a mesh by index
        /// </summary>
        public bool RemoveMesh(int index)
        {
            if (index >= 0 && index < Meshes.Count)
            {
                Meshes.RemoveAt(index);
                ModifiedAt = DateTime.Now;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add a material to the model
        /// </summary>
        public void AddMaterial(MaterialData material)
        {
            if (material != null)
            {
                Materials.Add(material);
                ModifiedAt = DateTime.Now;
            }
        }

        /// <summary>
        /// Get or create default material
        /// </summary>
        public MaterialData GetOrCreateDefaultMaterial()
        {
            if (Materials.Count == 0)
            {
                var defaultMaterial = new MaterialData("Default");
                Materials.Add(defaultMaterial);
                return defaultMaterial;
            }
            return Materials;
        }

        /// <summary>
        /// Clone this model
        /// </summary>
        public ModelData Clone()
        {
            var cloned = new ModelData(Name + "_Clone")
            {
                FilePath = FilePath,
                Transform = Transform.Clone(),
                IsVisible = IsVisible,
                ShowWireframe = ShowWireframe,
                BoundingBoxMin = (float[])BoundingBoxMin.Clone(),
                BoundingBoxMax = (float[])BoundingBoxMax.Clone()
            };

            foreach (var mesh in Meshes)
                cloned.Meshes.Add(mesh.Clone());

            foreach (var material in Materials)
                cloned.Materials.Add(material.Clone());

            return cloned;
        }

        public override string ToString()
        {
            return $"Model: {Name} " +
                   $"({Meshes.Count} meshes, {Materials.Count} materials) " +
                   $"Vertices: {GetVertexCount()} " +
                   $"Triangles: {GetTriangleCount()}";
        }
    }
}
