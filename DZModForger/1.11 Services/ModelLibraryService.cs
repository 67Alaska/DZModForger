using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DZModForger.Services
{
    /// <summary>
    /// Service for managing loaded models in the scene
    /// </summary>
    public class ModelLibraryService
    {
        private Dictionary<string, ModelData> _models = new();
        private string? _selectedModelId;

        public event EventHandler<ModelEventArgs>? ModelAdded;
        public event EventHandler<ModelEventArgs>? ModelRemoved;
        public event EventHandler<ModelEventArgs>? ModelSelected;
        public event EventHandler? SceneCleared;

        public ModelLibraryService()
        {
        }

        /// <summary>
        /// Add a model to the library
        /// </summary>
        public void AddModel(ModelData model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrEmpty(model.Id))
                model.Id = Guid.NewGuid().ToString();

            _models[model.Id] = model;
            OnModelAdded(model);
        }

        /// <summary>
        /// Remove a model from the library by ID
        /// </summary>
        public bool RemoveModel(string modelId)
        {
            if (string.IsNullOrEmpty(modelId))
                return false;

            if (_models.TryGetValue(modelId, out var model))
            {
                _models.Remove(modelId);

                if (_selectedModelId == modelId)
                    _selectedModelId = null;

                OnModelRemoved(model);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get a model by ID
        /// </summary>
        public ModelData? GetModel(string modelId)
        {
            if (string.IsNullOrEmpty(modelId))
                return null;

            _models.TryGetValue(modelId, out var model);
            return model;
        }

        /// <summary>
        /// Get all models
        /// </summary>
        public IEnumerable<ModelData> GetAllModels()
        {
            return _models.Values.ToList();
        }

        /// <summary>
        /// Select a model by ID
        /// </summary>
        public bool SelectModel(string modelId)
        {
            if (!_models.ContainsKey(modelId))
                return false;

            _selectedModelId = modelId;
            OnModelSelected(_models[modelId]);
            return true;
        }

        /// <summary>
        /// Get the currently selected model
        /// </summary>
        public ModelData? GetSelectedModel()
        {
            if (string.IsNullOrEmpty(_selectedModelId))
                return null;

            return GetModel(_selectedModelId);
        }

        /// <summary>
        /// Get the ID of the selected model
        /// </summary>
        public string? GetSelectedModelId()
        {
            return _selectedModelId;
        }

        /// <summary>
        /// Clear all models
        /// </summary>
        public void ClearAll()
        {
            _models.Clear();
            _selectedModelId = null;
            OnSceneCleared();
        }

        /// <summary>
        /// Get model count
        /// </summary>
        public int GetModelCount()
        {
            return _models.Count;
        }

        /// <summary>
        /// Get total vertex count across all models
        /// </summary>
        public int GetTotalVertexCount()
        {
            int total = 0;
            foreach (var model in _models.Values)
            {
                foreach (var mesh in model.Meshes)
                {
                    total += (int)mesh.VertexCount;
                }
            }
            return total;
        }

        /// <summary>
        /// Get total triangle count across all models
        /// </summary>
        public int GetTotalTriangleCount()
        {
            int total = 0;
            foreach (var model in _models.Values)
            {
                foreach (var mesh in model.Meshes)
                {
                    total += (int)(mesh.IndexCount / 3);
                }
            }
            return total;
        }

        /// <summary>
        /// Update model transform
        /// </summary>
        public void UpdateModelTransform(string modelId, TransformData transform)
        {
            if (_models.TryGetValue(modelId, out var model))
            {
                model.Transform = transform;
            }
        }

        /// <summary>
        /// Update model material
        /// </summary>
        public void UpdateModelMaterial(string modelId, int meshIndex, MaterialData material)
        {
            if (_models.TryGetValue(modelId, out var model))
            {
                if (meshIndex >= 0 && meshIndex < model.Materials.Count)
                {
                    model.Materials[meshIndex] = material;
                }
            }
        }

        protected virtual void OnModelAdded(ModelData model)
        {
            ModelAdded?.Invoke(this, new ModelEventArgs(model));
        }

        protected virtual void OnModelRemoved(ModelData model)
        {
            ModelRemoved?.Invoke(this, new ModelEventArgs(model));
        }

        protected virtual void OnModelSelected(ModelData model)
        {
            ModelSelected?.Invoke(this, new ModelEventArgs(model));
        }

        protected virtual void OnSceneCleared()
        {
            SceneCleared?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ModelEventArgs : EventArgs
    {
        public ModelData Model { get; }

        public ModelEventArgs(ModelData model)
        {
            Model = model;
        }
    }
}
