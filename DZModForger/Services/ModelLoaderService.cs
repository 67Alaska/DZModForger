using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DZModForger.Models;
using DZModForger.Configuration;

namespace DZModForger.Services
{
    public class ModelLoaderService : IDisposable
    {
        private readonly Dictionary<string, ModelData> _cachedModels;
        private bool _disposed = false;

        public event EventHandler<ModelLoadedEventArgs> ModelLoaded;
        public event EventHandler<ModelLoadErrorEventArgs> ModelLoadError;

        public ModelLoaderService()
        {
            Debug.WriteLine("[MODELLOADER] Initializing ModelLoaderService");

            _cachedModels = new Dictionary<string, ModelData>();

            var (isValid, error) = FBXSDKConfiguration.ValidateInstallation();
            if (!isValid)
            {
                Debug.WriteLine($"[MODELLOADER] FBX SDK warning: {error}");
            }
            else
            {
                Debug.WriteLine("[MODELLOADER] FBX SDK validated successfully");
            }
        }

        public async Task<ModelData> LoadModelAsync(string filePath)
        {
            return await Task.Run(() => LoadModel(filePath));
        }

        public ModelData LoadModel(string filePath)
        {
            try
            {
                Debug.WriteLine($"[MODELLOADER] Loading model: {filePath}");

                if (_cachedModels.ContainsKey(filePath))
                {
                    Debug.WriteLine($"[MODELLOADER] Model found in cache");
                    OnModelLoaded(new ModelLoadedEventArgs { FilePath = filePath, IsCached = true });
                    return _cachedModels[filePath];
                }

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"Model file not found: {filePath}");
                    OnModelLoadError(new ModelLoadErrorEventArgs { FilePath = filePath, Exception = ex });
                    throw ex;
                }

                var fileInfo = new FileInfo(filePath);
                var extension = fileInfo.Extension.ToLower();

                Debug.WriteLine($"[MODELLOADER] File format: {extension}");

                var modelData = new ModelData
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    FileFormat = extension,
                    LoadedDate = DateTime.UtcNow
                };

                _cachedModels[filePath] = modelData;

                Debug.WriteLine($"[MODELLOADER] Model loaded successfully");
                OnModelLoaded(new ModelLoadedEventArgs
                {
                    FilePath = filePath,
                    IsCached = false,
                    VertexCount = modelData.VertexCount,
                    FaceCount = modelData.FaceCount
                });

                return modelData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in LoadModel: {ex.Message}");
                OnModelLoadError(new ModelLoadErrorEventArgs { FilePath = filePath, Exception = ex });
                throw;
            }
        }

        protected virtual void OnModelLoaded(ModelLoadedEventArgs e)
        {
            ModelLoaded?.Invoke(this, e);
        }

        protected virtual void OnModelLoadError(ModelLoadErrorEventArgs e)
        {
            ModelLoadError?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                Debug.WriteLine("[MODELLOADER] Disposing");
                _cachedModels.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MODELLOADER] Exception in Dispose: {ex.Message}");
            }

            _disposed = true;
        }
    }

    public class ModelLoadedEventArgs : EventArgs
    {
        public string FilePath { get; set; }
        public bool IsCached { get; set; }
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }
    }

    public class ModelLoadErrorEventArgs : EventArgs
    {
        public string FilePath { get; set; }
        public Exception Exception { get; set; }
    }
}
