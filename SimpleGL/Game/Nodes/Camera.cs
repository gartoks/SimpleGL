using OpenTK.Mathematics;
using SimpleGL.Game.Util;
using SimpleGL.Graphics.GLHandling;

namespace SimpleGL.Game.Nodes;
public class Camera : GameNode {
    private Matrix4 ProjectionMatrix { get; set; }
    private Matrix4 ViewMatrix => Transform.TransformationMatrix;
    public Matrix4 ViewProjectionMatrix => ViewMatrix * ProjectionMatrix;

    public Camera() {
        ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, GLHandler.Viewport.Size.X, GLHandler.Viewport.Size.Y, 0, -1, 1);
        //Transform.OnTransformChanged += OnTransformChanged;
    }

    public Camera(Guid id)
        : base(id) {
        ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, GLHandler.Viewport.Size.X, GLHandler.Viewport.Size.Y, 0, -1, 1);
        //Transform.OnTransformChanged += OnTransformChanged;
    }

    public bool AutomaticallyAdjustToViewort {
        set {
            if (value)
                GLHandler.ViewportChanged += OnViewportChanged;
            else
                GLHandler.ViewportChanged -= OnViewportChanged;
        }
    }

    //private void OnTransformChanged() {
    //    ViewMatrix = Transform.TransformationMatrix;
    //}

    private void OnViewportChanged() {
        ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, GLHandler.Viewport.Size.X, GLHandler.Viewport.Size.Y, 0, -1, 1);
    }

    public Vector2 NormalizedToViewport(Vector2 normalized) {
        return new Vector2(normalized.X * GLHandler.Viewport.Size.X, normalized.Y * GLHandler.Viewport.Size.Y);
    }

    /// <summary>
    /// Converts a view position to a world position.
    /// </summary>
    /// <param name="viewPos"></param>
    /// <returns></returns>
    public Vector2 ViewToWorld(Vector2 viewPos) {
        Matrix4 inv = Matrix4.Invert(ViewProjectionMatrix);
        Vector4 v = inv * new Vector4(viewPos.X, viewPos.Y, 0, 1);
        return new Vector2(v.X, v.Y);
    }

    /// <summary>
    /// Converts a world position to a view position.
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Vector2 WorldToView(Vector2 worldPos) {
        Vector4 v = ViewProjectionMatrix * new Vector4(worldPos.X, worldPos.Y, 0, 1);
        return new Vector2(v.X, v.Y);
    }
}