using System;

namespace DZModForger.Services
{
    public class RenderErrorEventArgs : EventArgs
    {
        public Exception? Exception { get; set; }
        public string? Message { get; set; }

        public RenderErrorEventArgs(Exception? ex = null, string? message = null)
        {
            Exception = ex;
            Message = message ?? ex?.Message;
        }
    }

    public class ModelLoadedEventArgs : EventArgs
    {
        public string? FilePath { get; set; }
        public bool IsCached { get; set; }
        public uint VertexCount { get; set; }
        public uint FaceCount { get; set; }

        public ModelLoadedEventArgs(
            string? filePath = null,
            bool isCached = false,
            uint vertexCount = 0,
            uint faceCount = 0)
        {
            FilePath = filePath;
            IsCached = isCached;
            VertexCount = vertexCount;
            FaceCount = faceCount;
        }
    }

    public class ModelLoadErrorEventArgs : EventArgs
    {
        public Exception? Exception { get; set; }
        public string? FilePath { get; set; }

        public ModelLoadErrorEventArgs(Exception? ex = null, string? filePath = null)
        {
            Exception = ex;
            FilePath = filePath;
        }
    }
}
