using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UltimateDZForge.Services
{
    /// <summary>
    /// Service for loading 3D models (FBX, OBJ, DAE)
    /// Uses Autodesk FBX SDK 2020.3.7 for professional-grade parsing
    /// </summary>
    public class ModelLoaderService
    {
        private FbxManager _fbxManager;
        private readonly Dictionary<string, ModelData> _cachedModels;
        private readonly string _fbxSdkVersion = "2020.3.7";
        private readonly string _fbxSdkPath = @"C:\Program Files\Autodesk\FBX\FBX SDK\2020.3.7";

        public ModelLoaderService()
        {
            Debug.WriteLine("[MODELLOADER] Initializing with FBX SDK 2020.3.7");
            Debug.WriteLine($"[MODELLOADER] FBX SDK Path: {_fbxSdkPath}");

            try
            {
                // Initialize FBX Manager
                _fbxManager = FbxManager.Create();
                if (_fbxManager == null)
                {
                    throw new Exception("Failed to create FBX Manager - ensure Autodesk.FBX NuGet package 2020.3.7 is installed");
                }

                // Create IO settings
                var ioSettings = FbxIOSettings.Create(_fbxManager, Globals.IOSN_BASE);
                _fbxManager.SetIOSettings(ioSettings);

                _cachedModels = new Dictionary<string, ModelData>();

                Debug.WriteLine($"[MODELLOADER] FBX SDK initialized successfully (v{_fbxSdkVersion})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] FBX SDK initialization failed: {ex.Message}");
                Debug.WriteLine($"[MODELLOADER] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // ==================== MODEL LOADING ====================

        /// <summary>
        /// Loads a model from file (FBX, OBJ, or DAE)
        /// </summary>
        /// <param name="filePath">Full path to model file</param>
        /// <returns>ModelData containing vertex/face information</returns>
        public async Task<ModelData> LoadModelAsync(string filePath)
        {
            return await Task.Run(() => LoadModel(filePath));
        }

        private ModelData LoadModel(string filePath)
        {
            try
            {
                Debug.WriteLine($"[MODELLOADER] Loading model: {filePath}");

                // Check if model is cached
                if (_cachedModels.ContainsKey(filePath))
                {
                    Debug.WriteLine($"[MODELLOADER] Model found in cache");
                    return _cachedModels[filePath];
                }

                // Verify file exists
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Model file not found: {filePath}");
                }

                // Get file extension
                var extension = Path.GetExtension(filePath).ToLower();

                ModelData modelData = null;

                // Route to appropriate loader
                switch (extension)
                {
                    case ".fbx":
                        modelData = LoadFBXModel(filePath);
                        break;
                    case ".obj":
                        modelData = LoadOBJModel(filePath);
                        break;
                    case ".dae":
                        modelData = LoadDAEModel(filePath);
                        break;
                    default:
                        throw new NotSupportedException($"File format not supported: {extension}");
                }

                // Cache the model
                if (modelData != null)
                {
                    _cachedModels[filePath] = modelData;
                    Debug.WriteLine($"[MODELLOADER] Model cached: {filePath}");
                }

                return modelData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in LoadModel: {ex.Message}");
                Debug.WriteLine($"[MODELLOADER] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // ==================== FBX LOADING ====================

        private ModelData LoadFBXModel(string filePath)
        {
            try
            {
                Debug.WriteLine($"[MODELLOADER] Loading FBX model: {filePath}");
                Debug.WriteLine($"[MODELLOADER] Using FBX SDK {_fbxSdkVersion}");

                // Create scene
                var scene = FbxScene.Create(_fbxManager, "ImportScene");
                if (scene == null)
                {
                    throw new Exception("Failed to create FBX scene");
                }

                // Create importer
                var importer = FbxImporter.Create(_fbxManager, "");
                if (!importer.Initialize(filePath, -1, _fbxManager.GetIOSettings()))
                {
                    var statusCode = importer.GetStatus().GetErrorString();
                    throw new Exception($"Failed to initialize FBX importer: {statusCode}");
                }

                // Import scene
                if (!importer.Import(scene))
                {
                    var statusCode = importer.GetStatus().GetErrorString();
                    throw new Exception($"Failed to import FBX scene: {statusCode}");
                }

                importer.Destroy();

                // Extract geometry data
                var modelData = ExtractGeometryFromScene(scene, Path.GetFileName(filePath));

                // Cleanup
                scene.Destroy();

                Debug.WriteLine($"[MODELLOADER] FBX model loaded successfully: {modelData.VertexCount} vertices, {modelData.FaceCount} faces");
                return modelData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in LoadFBXModel: {ex.Message}");
                throw;
            }
        }

        private ModelData ExtractGeometryFromScene(FbxScene scene, string fileName)
        {
            try
            {
                Debug.WriteLine("[MODELLOADER] Extracting geometry from FBX scene");

                var vertices = new List<Vector3>();
                var faces = new List<int>();
                var normals = new List<Vector3>();
                var bounds = new Vector3(0, 0, 0);

                // Get root node
                var rootNode = scene.GetRootNode();
                if (rootNode == null)
                {
                    throw new Exception("Failed to get root node from scene");
                }

                // Traverse scene hierarchy
                int vertexCount = 0;
                int faceCount = 0;

                TraverseNodeForGeometry(rootNode, vertices, faces, normals, ref vertexCount, ref faceCount, ref bounds);

                // Calculate bounds
                if (vertices.Count > 0)
                {
                    var minX = vertices.Min(v => v.X);
                    var maxX = vertices.Max(v => v.X);
                    var minY = vertices.Min(v => v.Y);
                    var maxY = vertices.Max(v => v.Y);
                    var minZ = vertices.Min(v => v.Z);
                    var maxZ = vertices.Max(v => v.Z);

                    bounds = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
                }

                var modelData = new ModelData
                {
                    FileName = fileName,
                    FilePath = "",
                    VertexCount = vertices.Count,
                    FaceCount = faces.Count / 3,
                    Vertices = vertices.ToArray(),
                    Normals = normals.ToArray(),
                    Bounds = bounds,
                    LoadedDate = DateTime.UtcNow
                };

                Debug.WriteLine($"[MODELLOADER] Geometry extracted: {vertices.Count} vertices, {faces.Count / 3} faces");
                return modelData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in ExtractGeometryFromScene: {ex.Message}");
                throw;
            }
        }

        private void TraverseNodeForGeometry(
            FbxNode node,
            List<Vector3> vertices,
            List<int> faces,
            List<Vector3> normals,
            ref int vertexCount,
            ref int faceCount,
            ref Vector3 bounds)
        {
            if (node == null) return;

            // Check if this node has mesh data
            var mesh = node.GetMesh();
            if (mesh != null)
            {
                Debug.WriteLine($"[MODELLOADER] Processing mesh: {node.GetName()}");

                // Get vertex positions
                var controlPoints = mesh.GetControlPoints();
                foreach (var point in controlPoints)
                {
                    vertices.Add(new Vector3(
                        (float)point.mData[0],
                        (float)point.mData[1],
                        (float)point.mData[2]
                    ));
                }

                vertexCount += controlPoints.Length;

                // Get polygon data (faces)
                int polygonCount = mesh.GetPolygonCount();
                for (int i = 0; i < polygonCount; i++)
                {
                    int polygonSize = mesh.GetPolygonSize(i);
                    for (int j = 0; j < polygonSize; j++)
                    {
                        int index = mesh.GetPolygonVertex(i, j);
                        faces.Add(index);
                    }
                }

                faceCount += polygonCount;

                // Get normals
                var normalLayer = mesh.GetLayer(0);
                if (normalLayer != null)
                {
                    var normalElement = normalLayer.GetNormals();
                    if (normalElement != null)
                    {
                        int normalCount = normalElement.GetDirectArray().GetCount();
                        for (int i = 0; i < normalCount; i++)
                        {
                            var normal = normalElement.GetDirectArray().GetAt(i);
                            normals.Add(new Vector3(
                                (float)normal.mData[0],
                                (float)normal.mData[1],
                                (float)normal.mData[2]
                            ));
                        }
                    }
                }

                Debug.WriteLine($"[MODELLOADER] Mesh processed: {controlPoints.Length} vertices, {polygonCount} faces");
            }

            // Traverse child nodes
            for (int i = 0; i < node.GetChildCount(); i++)
            {
                TraverseNodeForGeometry(node.GetChild(i), vertices, faces, normals, ref vertexCount, ref faceCount, ref bounds);
            }
        }

        // ==================== OBJ LOADING ====================

        private ModelData LoadOBJModel(string filePath)
        {
            try
            {
                Debug.WriteLine($"[MODELLOADER] Loading OBJ model: {filePath}");

                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var faces = new List<int>();

                // Read OBJ file
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed)) continue;

                    var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) continue;

                    switch (parts[0])
                    {
                        case "v": // Vertex
                            if (parts.Length >= 4)
                            {
                                vertices.Add(new Vector3(
                                    float.Parse(parts[1]),
                                    float.Parse(parts[2]),
                                    float.Parse(parts[3])
                                ));
                            }
                            break;

                        case "vn": // Normal
                            if (parts.Length >= 4)
                            {
                                normals.Add(new Vector3(
                                    float.Parse(parts[1]),
                                    float.Parse(parts[2]),
                                    float.Parse(parts[3])
                                ));
                            }
                            break;

                        case "f": // Face
                            for (int i = 1; i < parts.Length; i++)
                            {
                                var indices = parts[i].Split('/');
                                if (int.TryParse(indices[0], out int vertexIndex))
                                {
                                    faces.Add(vertexIndex - 1); // OBJ is 1-indexed
                                }
                            }
                            break;
                    }
                }

                // Calculate bounds
                Vector3 bounds = new Vector3(0, 0, 0);
                if (vertices.Count > 0)
                {
                    var minX = vertices.Min(v => v.X);
                    var maxX = vertices.Max(v => v.X);
                    var minY = vertices.Min(v => v.Y);
                    var maxY = vertices.Max(v => v.Y);
                    var minZ = vertices.Min(v => v.Z);
                    var maxZ = vertices.Max(v => v.Z);

                    bounds = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
                }

                var modelData = new ModelData
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    VertexCount = vertices.Count,
                    FaceCount = faces.Count / 3,
                    Vertices = vertices.ToArray(),
                    Normals = normals.ToArray(),
                    Bounds = bounds,
                    LoadedDate = DateTime.UtcNow
                };

                Debug.WriteLine($"[MODELLOADER] OBJ model loaded: {vertices.Count} vertices, {faces.Count / 3} faces");
                return modelData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in LoadOBJModel: {ex.Message}");
                throw;
            }
        }

        // ==================== DAE LOADING ====================

        private ModelData LoadDAEModel(string filePath)
        {
            try
            {
                Debug.WriteLine($"[MODELLOADER] Loading DAE (Collada) model: {filePath}");

                // TODO: Implement DAE/Collada loading
                // Could use FbxImporter with .dae format
                Debug.WriteLine("[MODELLOADER] DAE loading to be fully implemented");

                // For now, return empty model data
                return new ModelData
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    VertexCount = 0,
                    FaceCount = 0,
                    Vertices = Array.Empty<Vector3>(),
                    Normals = Array.Empty<Vector3>(),
                    Bounds = new Vector3(0, 0, 0),
                    LoadedDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in LoadDAEModel: {ex.Message}");
                throw;
            }
        }

        // ==================== CACHE MANAGEMENT ====================

        /// <summary>
        /// Clears model cache to free memory
        /// </summary>
        public void ClearCache()
        {
            try
            {
                Debug.WriteLine($"[MODELLOADER] Clearing cache ({_cachedModels.Count} models)");
                _cachedModels.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in ClearCache: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes specific model from cache
        /// </summary>
        public void RemoveFromCache(string filePath)
        {
            try
            {
                if (_cachedModels.Remove(filePath))
                {
                    Debug.WriteLine($"[MODELLOADER] Model removed from cache: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in RemoveFromCache: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets FBX SDK version info
        /// </summary>
        public string GetFBXSDKInfo()
        {
            return $"Autodesk FBX SDK {_fbxSdkVersion} at {_fbxSdkPath}";
        }

        // ==================== CLEANUP ====================

        public void Dispose()
        {
            try
            {
                Debug.WriteLine("[MODELLOADER] Disposing");
                ClearCache();
                _fbxManager?.Destroy();
                Debug.WriteLine("[MODELLOADER] Disposed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in Dispose: {ex.Message}");
            }
        }

        ~ModelLoaderService()
        {
            Dispose();
        }
    }

    // ==================== DATA STRUCTURES ====================

    /// <summary>
    /// Represents loaded 3D model data
    /// </summary>
    public class ModelData
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }
        public Vector3[] Vertices { get; set; }
        public Vector3[] Normals { get; set; }
        public Vector3 Bounds { get; set; }
        public DateTime LoadedDate { get; set; }
        public string[] MaterialNames { get; set; }
    }

    /// <summary>
    /// 3D Vector structure
    /// </summary>
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2}, {Z:F2})";
        }
    }
}
