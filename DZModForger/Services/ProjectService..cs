// Models/DZProject.cs - COMPLETE FILE

using System;
using System.Collections.Generic;

namespace DZModForger.Models
{
    public class DZProject
    {
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ProjectVersion { get; set; } = "1.0.0";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public List<ModelData> Models { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();

        public DZProject()
        {
            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
        }

        public DZProject(string name, string filePath) : this()
        {
            Name = name;
            FilePath = filePath;
        }

        public void Save()
        {
            LastModified = DateTime.Now;
        }

        public void AddModel(ModelData model)
        {
            if (model != null)
            {
                Models.Add(model);
                Save();
            }
        }

        public void RemoveModel(ModelData model)
        {
            if (model != null)
            {
                Models.Remove(model);
                Save();
            }
        }

        public override string ToString() => $"{Name} (v{ProjectVersion})";
    }
}
