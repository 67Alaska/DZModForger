using System;

namespace DZModForger.Models
{
    /// <summary>
    /// Represents the current state of the viewport
    /// </summary>
    public class ViewportState
    {
        /// <summary>
        /// Whether grid is visible
        /// </summary>
        public bool IsGridVisible { get; set; } = true;

        /// <summary>
        /// Whether axes are visible
        /// </summary>
        public bool IsAxisVisible { get; set; } = true;

        /// <summary>
        /// Current gizmo mode: "None", "Move", "Rotate", "Scale"
        /// </summary>
        public string GizmoMode { get; set; } = "None";

        /// <summary>
        /// Whether wireframe mode is enabled
        /// </summary>
        public bool IsWireframe { get; set; } = false;

        /// <summary>
        /// Whether to show normal vectors
        /// </summary>
        public bool ShowNormals { get; set; } = false;

        /// <summary>
        /// Whether to show tangent vectors
        /// </summary>
        public bool ShowTangents { get; set; } = false;

        /// <summary>
        /// Current shading mode: "Flat", "Smooth", "Material"
        /// </summary>
        public string ShadingMode { get; set; } = "Smooth";

        /// <summary>
        /// Grid size
        /// </summary>
        public float GridSize { get; set; } = 1.0f;

        /// <summary>
        /// Number of grid subdivisions
        /// </summary>
        public int GridSubdivisions { get; set; } = 10;

        /// <summary>
        /// Background color (R, G, B, A)
        /// </summary>
        public float[] BackgroundColor { get; set; } = new[] { 0.1f, 0.15f, 0.2f, 1.0f };

        /// <summary>
        /// Grid color (R, G, B, A)
        /// </summary>
        public float[] GridColor { get; set; } = new[] { 0.3f, 0.3f, 0.3f, 1.0f };

        /// <summary>
        /// Frame time in milliseconds
        /// </summary>
        public float FrameTime { get; set; } = 16.67f;

        /// <summary>
        /// Current FPS
        /// </summary>
        public int FramesPerSecond { get; set; } = 60;

        public ViewportState()
        {
        }

        /// <summary>
        /// Reset to default state
        /// </summary>
        public void Reset()
        {
            IsGridVisible = true;
            IsAxisVisible = true;
            GizmoMode = "None";
            IsWireframe = false;
            ShowNormals = false;
            ShowTangents = false;
            ShadingMode = "Smooth";
            GridSize = 1.0f;
            GridSubdivisions = 10;
            BackgroundColor = new[] { 0.1f, 0.15f, 0.2f, 1.0f };
            GridColor = new[] { 0.3f, 0.3f, 0.3f, 1.0f };
        }

        /// <summary>
        /// Clone this viewport state
        /// </summary>
        public ViewportState Clone()
        {
            return new ViewportState
            {
                IsGridVisible = IsGridVisible,
                IsAxisVisible = IsAxisVisible,
                GizmoMode = GizmoMode,
                IsWireframe = IsWireframe,
                ShowNormals = ShowNormals,
                ShowTangents = ShowTangents,
                ShadingMode = ShadingMode,
                GridSize = GridSize,
                GridSubdivisions = GridSubdivisions,
                BackgroundColor = (float[])BackgroundColor.Clone(),
                GridColor = (float[])GridColor.Clone(),
                FrameTime = FrameTime,
                FramesPerSecond = FramesPerSecond
            };
        }

        public override string ToString()
        {
            return $"ViewportState: " +
                   $"Grid={IsGridVisible} Axes={IsAxisVisible} " +
                   $"Gizmo={GizmoMode} Wireframe={IsWireframe} " +
                   $"Shading={ShadingMode} FPS={FramesPerSecond}";
        }
    }
}
