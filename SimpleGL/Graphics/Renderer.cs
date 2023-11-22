using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleGL.Graphics.GLHandling;

namespace SimpleGL.Graphics;
public sealed class Renderer {
    public static Renderer? ActiveRenderer { get; private set; }

    public Box2i Viewport { get; set; }

    public Matrix4? ViewProjectionMatrix { get; private set; }

    public bool IsActive => GLHandler.IsRendering && ActiveRenderer == this;
    public Renderer(Box2i viewport) {
        Viewport = viewport;
    }

    public void BeginRendering(Matrix4 viewProjectionMatrix) {
        if (GLHandler.IsRendering)
            throw new InvalidOperationException("Cannot begin rendering while already rendering.");

        GLHandler.BeginRendering();
        ActiveRenderer = this;
        ViewProjectionMatrix = viewProjectionMatrix;

        GL.Viewport(Viewport);
    }

    public void EndRendering() {
        if (!GLHandler.IsRendering)
            throw new InvalidOperationException("Cannot end rendering while not rendering.");

        if (!IsActive)
            throw new InvalidOperationException("Cannot end rendering while not active.");

        GLHandler.EndRendering();
        ActiveRenderer = null;
        ViewProjectionMatrix = null;
    }
}
