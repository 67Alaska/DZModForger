using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DZModForger.Models
{
    /// <summary>
    /// Represents loaded 3D model data
    /// Contains geometry, materials, and metadata
    /// </summary>
    public class ModelData
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("fileSize")]
        public long FileSize { get; set; }

        [JsonProperty("fileFormat")]
        public string FileFormat { get; set; }

        [JsonProperty("vertexCount")]
        public int VertexCount { get; set; }

        [JsonProperty("faceCount")]
        public int FaceCount { get; set; }

        [JsonProperty("edgeCount")]
        public int EdgeCount { get; set; }

        [JsonProperty("vertexData")]
        public float[] VertexData { get; set; }

        [JsonProperty("indexData")]
        public int[] IndexData { get; set; }

        [JsonProperty("normalData")]
        public float[] NormalData { get; set; }

        [JsonProperty("uvData")]
        public float[] UVData { get; set; }

        [JsonProperty("tangentData")]
        public float[] TangentData { get; set; }

        [JsonProperty("boundsMin")]
        public Vector3f BoundsMin { get; set; }

        [JsonProperty("boundsMax")]
        public Vector3f BoundsMax { get; set; }

        [JsonProperty("boundsCenter")]
        public Vector3f BoundsCenter { get; set; }

        [JsonProperty("boundsSize")]
        public Vector3f BoundsSize { get; set; }

        [JsonProperty("materials")]
        public List<MaterialData> Materials { get; set; }

        [JsonProperty("meshes")]
        public List<MeshData> Meshes { get; set; }

        [JsonProperty("bones")]
        public List<BoneData> Bones { get; set; }

        [JsonProperty("animations")]
        public List<AnimationData> Animations { get; set; }

        [JsonProperty("lodLevels")]
        public List<LODLevel> LODLevels { get; set; }

        [JsonProperty("loadedDate")]
        public DateTime LoadedDate { get; set; }

        [JsonProperty("isAnimated")]
        public bool IsAnimated { get; set; }

        [JsonProperty("isRigged")]
        public bool IsRigged { get; set; }

        [JsonProperty("hasNormals")]
        public bool HasNormals { get; set; }

        [JsonProperty("hasUVs")]
        public bool HasUVs { get; set; }

        [JsonProperty("hasTangents")]
        public bool HasTangents { get; set; }

        public ModelData()
        {
            Debug.WriteLine("[MODELDATA] Creating new ModelData");

            FileName = "";
            FilePath = "";
            FileSize = 0;
            FileFormat = "";
            VertexCount = 0;
            FaceCount = 0;
            EdgeCount = 0;

            VertexData = Array.Empty<float>();
            IndexData = Array.Empty<int>();
            NormalData = Array.Empty<float>();
            UVData = Array.Empty<float>();
            TangentData = Array.Empty<float>();

            BoundsMin = new Vector3f(0, 0, 0);
            BoundsMax = new Vector3f(0, 0, 0);
            BoundsCenter = new Vector3f(0, 0, 0);
            BoundsSize = new Vector3f(0, 0, 0);

            Materials = new List<MaterialData>();
            Meshes = new List<MeshData>();
            Bones = new List<BoneData>();
            Animations = new List<AnimationData>();
            LODLevels = new List<LODLevel>();

            LoadedDate = DateTime.UtcNow;
            IsAnimated = false;
            IsRigged = false;
            HasNormals = false;
            HasUVs = false;
            HasTangents = false;
        }

        // ==================== GEOMETRY OPERATIONS ====================

        /// <summary>
        /// Calculates bounds from vertex data
        /// </summary>
        public void CalculateBounds()
        {
            try
            {
                Debug.WriteLine("[MODELDATA] Calculating bounds");

                if (VertexData == null || VertexData.Length < 3)
                {
                    BoundsMin = new Vector3f(0, 0, 0);
                    BoundsMax = new Vector3f(0, 0, 0);
                    BoundsCenter = new Vector3f(0, 0, 0);
                    BoundsSize = new Vector3f(0, 0, 0);
                    return;
                }

                float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

                // Vertices are stored as X, Y, Z repeating
                for (int i = 0; i < VertexData.Length; i += 3)
                {
                    float x = VertexData[i];
                    float y = VertexData[i + 1];
                    float z = VertexData[i + 2];

                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    minZ = Math.Min(minZ, z);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                    maxZ = Math.Max(maxZ, z);
                }

                BoundsMin = new Vector3f(minX, minY, minZ);
                BoundsMax = new Vector3f(maxX, maxY, maxZ);
                BoundsCenter = new Vector3f(
                    (minX + maxX) / 2,
                    (minY + maxY) / 2,
                    (minZ + maxZ) / 2
                );
                BoundsSize = new Vector3f(
                    maxX - minX,
                    maxY - minY,
                    maxZ - minZ
                );

                Debug.WriteLine($"[MODELDATA] Bounds calculated: {BoundsMin} to {BoundsMax}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELDATA] Exception in CalculateBounds: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates normals from vertex and index data if not present
        /// </summary>
        public void CalculateNormals()
        {
            try
            {
                if (HasNormals || VertexData == null || IndexData == null)
                    return;

                Debug.WriteLine("[MODELDATA] Calculating normals");

                int vertexCount = VertexData.Length / 3;
                NormalData = new float[VertexData.Length];

                // Initialize normals to zero
                for (int i = 0; i < NormalData.Length; i++)
                    NormalData[i] = 0.0f;

                // Calculate face normals and accumulate to vertex normals
                for (int i = 0; i < IndexData.Length; i += 3)
                {
                    int i0 = IndexData[i] * 3;
                    int i1 = IndexData[i + 1] * 3;
                    int i2 = IndexData[i + 2] * 3;

                    // Get vertices
                    var v0 = new Vector3f(VertexData[i0], VertexData[i0 + 1], VertexData[i0 + 2]);
                    var v1 = new Vector3f(VertexData[i1], VertexData[i1 + 1], VertexData[i1 + 2]);
                    var v2 = new Vector3f(VertexData[i2], VertexData[i2 + 1], VertexData[i2 + 2]);

                    // Calculate face normal
                    var edge1 = v1 - v0;
                    var edge2 = v2 - v0;
                    var faceNormal = edge1.Cross(edge2).Normalized();

                    // Accumulate to vertex normals
                    NormalData[i0] += faceNormal.X;
                    NormalData[i0 + 1] += faceNormal.Y;
                    NormalData[i0 + 2] += faceNormal.Z;

                    NormalData[i1] += faceNormal.X;
                    NormalData[i1 + 1] += faceNormal.Y;
                    NormalData[i1 + 2] += faceNormal.Z;

                    NormalData[i2] += faceNormal.X;
                    NormalData[i2 + 1] += faceNormal.Y;
                    NormalData[i2 + 2] += faceNormal.Z;
                }

                // Normalize vertex normals
                for (int i = 0; i < NormalData.Length; i += 3)
                {
                    var normal = new Vector3f(NormalData[i], NormalData[i + 1], NormalData[i + 2]);
                    normal = normal.Normalized();
                    NormalData[i] = normal.X;
                    NormalData[i + 1] = normal.Y;
                    NormalData[i + 2] = normal.Z;
                }

                HasNormals = true;
                Debug.WriteLine("[MODELDATA] Normals calculated successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELDATA] Exception in CalculateNormals: {ex.Message}");
            }
        }

        // ==================== INFORMATION ====================

        /// <summary>
        /// Gets total memory usage in bytes
        /// </summary>
        public long GetMemoryUsage()
        {
            long total = 0;
            total += VertexData?.Length * sizeof(float) ?? 0;
            total += IndexData?.Length * sizeof(int) ?? 0;
            total += NormalData?.Length * sizeof(float) ?? 0;
            total += UVData?.Length * sizeof(float) ?? 0;
            total += TangentData?.Length * sizeof(float) ?? 0;
            return total;
        }

        /// <summary>
        /// Gets total memory usage in MB
        /// </summary>
        public double GetMemoryUsageMB()
        {
            return GetMemoryUsage() / (1024.0 * 1024.0);
        }

        /// <summary>
        /// Gets model statistics as string
        /// </summary>
        public override string ToString()
        {
            return $"ModelData [File: {FileName}, Vertices: {VertexCount}, Faces: {FaceCount}, " +
                   $"Memory: {GetMemoryUsageMB():F2}MB, Rigged: {IsRigged}, Animated: {IsAnimated}]";
        }

        /// <summary>
        /// Clones the model data
        /// </summary>
        public ModelData Clone()
        {
            var clone = new ModelData
            {
                FileName = FileName,
                FilePath = FilePath,
                FileSize = FileSize,
                FileFormat = FileFormat,
                VertexCount = VertexCount,
                FaceCount = FaceCount,
                EdgeCount = EdgeCount,
                VertexData = (float[])VertexData?.Clone(),
                IndexData = (int[])IndexData?.Clone(),
                NormalData = (float[])NormalData?.Clone(),
                UVData = (float[])UVData?.Clone(),
                TangentData = (float[])TangentData?.Clone(),
                BoundsMin = BoundsMin,
                BoundsMax = BoundsMax,
                BoundsCenter = BoundsCenter,
                BoundsSize = BoundsSize,
                IsAnimated = IsAnimated,
                IsRigged = IsRigged,
                HasNormals = HasNormals,
                HasUVs = HasUVs,
                HasTangents = HasTangents,
                LoadedDate = LoadedDate
            };

            // Clone lists
            clone.Materials.AddRange(Materials);
            clone.Meshes.AddRange(Meshes);
            clone.Bones.AddRange(Bones);
            clone.Animations.AddRange(Animations);
            clone.LODLevels.AddRange(LODLevels);

            return clone;
        }
    }

    // ==================== MATERIAL DATA ====================

    public class MaterialData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color")]
        public Vector3f Color { get; set; }

        [JsonProperty("metallic")]
        public float Metallic { get; set; }

        [JsonProperty("roughness")]
        public float Roughness { get; set; }

        [JsonProperty("normalMap")]
        public string NormalMap { get; set; }

        [JsonProperty("diffuseMap")]
        public string DiffuseMap { get; set; }

        public MaterialData()
        {
            Name = "Default Material";
            Color = new Vector3f(1, 1, 1);
            Metallic = 0.0f;
            Roughness = 0.5f;
            NormalMap = "";
            DiffuseMap = "";
        }
    }

    // ==================== MESH DATA ====================

    public class MeshData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("materialIndex")]
        public int MaterialIndex { get; set; }

        [JsonProperty("vertexStart")]
        public int VertexStart { get; set; }

        [JsonProperty("vertexCount")]
        public int VertexCount { get; set; }

        [JsonProperty("indexStart")]
        public int IndexStart { get; set; }

        [JsonProperty("indexCount")]
        public int IndexCount { get; set; }

        public MeshData()
        {
            Name = "Mesh";
            MaterialIndex = 0;
            VertexStart = 0;
            VertexCount = 0;
            IndexStart = 0;
            IndexCount = 0;
        }
    }

    // ==================== BONE DATA ====================

    public class BoneData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parentIndex")]
        public int ParentIndex { get; set; }

        [JsonProperty("position")]
        public Vector3f Position { get; set; }

        [JsonProperty("rotation")]
        public Vector3f Rotation { get; set; }

        public BoneData()
        {
            Name = "Bone";
            ParentIndex = -1;
            Position = new Vector3f(0, 0, 0);
            Rotation = new Vector3f(0, 0, 0);
        }
    }

    // ==================== ANIMATION DATA ====================

    public class AnimationData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("frameCount")]
        public int FrameCount { get; set; }

        [JsonProperty("frameRate")]
        public float FrameRate { get; set; }

        [JsonProperty("duration")]
        public float Duration { get; set; }

        [JsonProperty("keyframes")]
        public List<KeyframeData> Keyframes { get; set; }

        public AnimationData()
        {
            Name = "Animation";
            FrameCount = 0;
            FrameRate = 30.0f;
            Duration = 0.0f;
            Keyframes = new List<KeyframeData>();
        }
    }

    // ==================== KEYFRAME DATA ====================

    public class KeyframeData
    {
        [JsonProperty("boneIndex")]
        public int BoneIndex { get; set; }

        [JsonProperty("frame")]
        public int Frame { get; set; }

        [JsonProperty("position")]
        public Vector3f Position { get; set; }

        [JsonProperty("rotation")]
        public Vector3f Rotation { get; set; }

        [JsonProperty("scale")]
        public Vector3f Scale { get; set; }

        public KeyframeData()
        {
            BoneIndex = 0;
            Frame = 0;
            Position = new Vector3f(0, 0, 0);
            Rotation = new Vector3f(0, 0, 0);
            Scale = new Vector3f(1, 1, 1);
        }
    }

    // ==================== LOD DATA ====================

    public class LODLevel
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("screenSize")]
        public float ScreenSize { get; set; }

        [JsonProperty("vertexCount")]
        public int VertexCount { get; set; }

        [JsonProperty("indexCount")]
        public int IndexCount { get; set; }

        public LODLevel()
        {
            Level = 0;
            ScreenSize = 1.0f;
            VertexCount = 0;
            IndexCount = 0;
        }
    }

    // ==================== VECTOR STRUCTURE ====================

    [JsonConverter(typeof(Vector3fConverter))]
    public struct Vector3f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3f(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3f operator +(Vector3f a, Vector3f b)
            => new Vector3f(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3f operator -(Vector3f a, Vector3f b)
            => new Vector3f(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3f operator *(Vector3f a, float scalar)
            => new Vector3f(a.X * scalar, a.Y * scalar, a.Z * scalar);

        public float Dot(Vector3f other)
            => X * other.X + Y * other.Y + Z * other.Z;

        public Vector3f Cross(Vector3f other)
            => new Vector3f(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);

        public float Length()
            => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3f Normalized()
        {
            float len = Length();
            return len > 0.0001f ? new Vector3f(X / len, Y / len, Z / len) : this;
        }

        public override string ToString()
            => $"({X:F2}, {Y:F2}, {Z:F2})";
    }

    // ==================== JSON CONVERTER ====================

    public class Vector3fConverter : JsonConverter<Vector3f>
    {
        public override Vector3f ReadJson(JsonReader reader, Type objectType, Vector3f existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                float x = (float)reader.ReadAsDouble();
                float y = (float)reader.ReadAsDouble();
                float z = (float)reader.ReadAsDouble();
                reader.Read(); // End array
                return new Vector3f(x, y, z);
            }
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, Vector3f value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.X);
            writer.WriteValue(value.Y);
            writer.WriteValue(value.Z);
            writer.WriteEndArray();
        }
    }
}
