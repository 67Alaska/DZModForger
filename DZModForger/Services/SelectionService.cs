using System;
using System.Collections.Generic;
using System.Linq;

namespace DZModForger.Services
{
    /// <summary>
    /// Service for managing model selection
    /// </summary>
    public class SelectionService
    {
        private HashSet<string> _selectedModelIds = new();
        private string? _primarySelectedId;

        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        public SelectionService()
        {
        }

        /// <summary>
        /// Select a single model (deselects others)
        /// </summary>
        public void SelectModel(string modelId)
        {
            if (string.IsNullOrEmpty(modelId))
                return;

            bool changed = _selectedModelIds.Count != 1 ||
                          !_selectedModelIds.Contains(modelId);

            if (changed)
            {
                _selectedModelIds.Clear();
                _selectedModelIds.Add(modelId);
                _primarySelectedId = modelId;
                OnSelectionChanged();
            }
        }

        /// <summary>
        /// Toggle model selection (multi-select)
        /// </summary>
        public void ToggleModelSelection(string modelId)
        {
            if (string.IsNullOrEmpty(modelId))
                return;

            bool wasSelected = _selectedModelIds.Contains(modelId);

            if (wasSelected)
            {
                _selectedModelIds.Remove(modelId);
                if (_primarySelectedId == modelId)
                    _primarySelectedId = _selectedModelIds.Count > 0
                        ? _selectedModelIds.First()
                        : null;
            }
            else
            {
                _selectedModelIds.Add(modelId);
                _primarySelectedId = modelId;
            }

            OnSelectionChanged();
        }

        /// <summary>
        /// Add model to selection (multi-select)
        /// </summary>
        public void AddToSelection(string modelId)
        {
            if (string.IsNullOrEmpty(modelId))
                return;

            if (_selectedModelIds.Add(modelId))
            {
                if (_primarySelectedId == null)
                    _primarySelectedId = modelId;
                OnSelectionChanged();
            }
        }

        /// <summary>
        /// Remove model from selection
        /// </summary>
        public void RemoveFromSelection(string modelId)
        {
            if (_selectedModelIds.Remove(modelId))
            {
                if (_primarySelectedId == modelId)
                    _primarySelectedId = _selectedModelIds.Count > 0
                        ? _selectedModelIds.First()
                        : null;
                OnSelectionChanged();
            }
        }

        /// <summary>
        /// Clear all selections
        /// </summary>
        public void ClearSelection()
        {
            if (_selectedModelIds.Count > 0)
            {
                _selectedModelIds.Clear();
                _primarySelectedId = null;
                OnSelectionChanged();
            }
        }

        /// <summary>
        /// Get primary selected model ID
        /// </summary>
        public string? GetPrimarySelectedId()
        {
            return _primarySelectedId;
        }

        /// <summary>
        /// Get all selected model IDs
        /// </summary>
        public IEnumerable<string> GetSelectedIds()
        {
            return _selectedModelIds.ToList();
        }

        /// <summary>
        /// Check if model is selected
        /// </summary>
        public bool IsModelSelected(string modelId)
        {
            return _selectedModelIds.Contains(modelId);
        }

        /// <summary>
        /// Get number of selected models
        /// </summary>
        public int GetSelectionCount()
        {
            return _selectedModelIds.Count;
        }

        /// <summary>
        /// Select all models (requires model library)
        /// </summary>
        public void SelectAll(IEnumerable<string> availableModelIds)
        {
            _selectedModelIds.Clear();
            foreach (var id in availableModelIds)
                _selectedModelIds.Add(id);

            _primarySelectedId = _selectedModelIds.Count > 0
                ? _selectedModelIds.First()
                : null;

            OnSelectionChanged();
        }

        protected virtual void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(
                _primarySelectedId,
                _selectedModelIds.ToList()
            ));
        }
    }

    public class SelectionChangedEventArgs : EventArgs
    {
        public string? PrimarySelectedId { get; }
        public List<string> SelectedIds { get; }

        public SelectionChangedEventArgs(string? primaryId, List<string> selectedIds)
        {
            PrimarySelectedId = primaryId;
            SelectedIds = selectedIds;
        }
    }
}
