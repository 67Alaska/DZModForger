using System;
using System.Collections.Generic;

namespace DZModForger.Models
{
    /// <summary>
    /// Represents a mesh with vertices, indices, and material reference
    /// </summary>
    public class MeshData
    {
        /// <summary>
        /// Unique identifier for this mesh
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name of the mesh
        /// </summary>
        public string Name { get; set; } = "Mesh";

        /// <summary>
        /// List of vertices in the mesh
        /// </summary>
        public List<VertexData> Vertices { get; set; } = new();

        /// <summary>
        /// List of vertex indices (triangle faces)
        /// </summary>
        public List<uint> Indices { get; set; } = new();

        /// <summary>
        /// Number of vertices
        /// </summary>
        public uint VertexCount => (uint)Vertices.Count;

        /// <summary>
        /// Number of indices
        /// </summary>
        public uint IndexCount => (uint)Indices.Count;

        /// <summary>
        /// Index of the material to use for this mesh
        /// </summary>
        public int MaterialIndex { get; set; } = -1;

        /// <summary>
        /// Minimum point of bounding box
        /// </summary>
        public float[] BoundingBoxMin { get; set; } = new[] { 0f, 0f, 0f };

        /// <summary>
        /// Maximum point of bounding box
        /// </summary>
        public float[] BoundingBoxMax { get; set; } = new[] { 0f, 0f, 0f };

        /// <summary>
        /// Whether the mesh is visible
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Whether to render in wireframe mode
        /// </summary>
        public bool ShowWireframe { get; set; } = false;

        public MeshData()
        {
        }

        public MeshData(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Calculate bounding box from vertices
        /// </summary>
        public void CalculateBoundingBox()
        {
            if (Vertices.Count == 0)
            {
                BoundingBoxMin = new[] { 0f, 0f, 0f };
                BoundingBoxMax = new[] { 0f, 0f, 0f };
                return;
            }

            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            foreach (var vertex in Vertices)
            {
                minX = Math.Min(minX, vertex.Position);
                minY = Math.Min(minY, vertex.Position);
                minZ = Math.Min(minZ, vertex.Position);

                maxX = Math.Max(maxX, vertex.Position);
                maxY = Math.Max(maxY, vertex.Position);
                maxZ = Math.Max(maxZ, vertex.Position);
            }

            BoundingBoxMin = new[] { minX, minY, minZ };
            BoundingBoxMax = new[] { maxX, maxY, maxZ };
        }

        /// <summary>
        /// Get triangle count
        /// </summary>
        public uint GetTriangleCount()
        {
            return IndexCount / 3;
        }

        /// <summary>
        /// Get size in bytes for GPU
        /// </summary>
        public uint GetVertexBufferSize()
        {
            return VertexCount * 48; // sizeof(Vertex) = 48 bytes
        }

        public uint GetIndexBufferSize()
        {
            return IndexCount * sizeof(uint);
        }

        /// <summary>
        /// Clone this mesh
        /// </summary>
        public MeshData Clone()
        {
            var cloned = new MeshData(Name + "_Clone")
            {
                MaterialIndex = MaterialIndex,
                IsVisible = IsVisible,
                ShowWireframe = ShowWireframe,
                BoundingBoxMin = (float[])BoundingBoxMin.Clone(),
                BoundingBoxMax = (float[])BoundingBoxMax.Clone()
            };

            foreach (var vertex in Vertices)
                cloned.Vertices.Add(vertex.Clone());

            cloned.Indices.AddRange(Indices);

            return cloned;
        }
    }
}
