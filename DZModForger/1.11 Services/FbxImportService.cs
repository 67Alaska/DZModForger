using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DZModForger.Services
{
    /// <summary>
    /// Service for importing FBX files
    /// </summary>
    public class FbxImportService
    {
        public event EventHandler<ImportProgressEventArgs>? ProgressChanged;
        public event EventHandler<string>? ErrorOccurred;

        public FbxImportService()
        {
        }

        /// <summary>
        /// Import an FBX file asynchronously
        /// </summary>
        public async System.Threading.Tasks.Task<ModelData> ImportFbxAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            return await System.Threading.Tasks.Task.Run(() => ImportFbx(filePath));
        }

        /// <summary>
        /// Import an FBX file synchronously
        /// </summary>
        public ModelData ImportFbx(string filePath)
        {
            try
            {
                OnProgressChanged("Loading FBX file...", 0);

                // Load native model
                var nativeModel = FbxInteropService.LoadFbxFile(filePath);
                OnProgressChanged($"Loaded: {nativeModel.Name}", 50);

                // Marshal to managed data
                var modelData = FbxInteropService.MarshalModel(nativeModel);
                modelData.FilePath = filePath;
                modelData.Id = Guid.NewGuid().ToString();

                OnProgressChanged("Import complete", 100);

                Debug.WriteLine($"Imported model: {modelData.Name} " +
                    $"({modelData.Meshes.Count} meshes, " +
                    $"{GetTotalVertexCount(modelData)} vertices)");

                return modelData;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Failed to import FBX: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validate FBX file before import
        /// </summary>
        public bool ValidateFbxFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                // Check file exists
                if (!System.IO.File.Exists(filePath))
                    return false;

                // Check file extension
                string ext = System.IO.Path.GetExtension(filePath).ToLower();
                if (ext != ".fbx")
                    return false;

                // Try to load without full import
                var nativeModel = FbxInteropService.LoadFbxFile(filePath);
                return nativeModel.MeshCount > 0;
            }
            catch
            {
                return false;
            }
        }

        private int GetTotalVertexCount(ModelData model)
        {
            int total = 0;
            foreach (var mesh in model.Meshes)
                total += (int)mesh.VertexCount;
            return total;
        }

        protected virtual void OnProgressChanged(string message, int percentage)
        {
            ProgressChanged?.Invoke(this, new ImportProgressEventArgs(message, percentage));
        }

        protected virtual void OnErrorOccurred(string error)
        {
            ErrorOccurred?.Invoke(this, error);
        }
    }

    public class ImportProgressEventArgs : EventArgs
    {
        public string Message { get; }
        public int Percentage { get; }

        public ImportProgressEventArgs(string message, int percentage)
        {
            Message = message;
            Percentage = Math.Clamp(percentage, 0, 100);
        }
    }
}
