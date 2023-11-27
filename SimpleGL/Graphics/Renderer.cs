using OpenTK.Mathematics;
using SimpleGL.Graphics.GLHandling;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util;

namespace SimpleGL.Graphics;
public sealed class Renderer {
    public static Renderer? ActiveRenderer { get; private set; }
    public static bool HasActiveRenderer => GLHandler.IsRendering && ActiveRenderer != null;

    public Matrix4? ViewProjectionMatrix { get; private set; }

    public bool IsActive => GLHandler.IsRendering && ActiveRenderer == this;

    private List<RenderData> RenderingObjects { get; }

    public Renderer() {
        RenderingObjects = new();
    }

    public void BeginRendering(Matrix4 viewProjectionMatrix) {
        if (GLHandler.IsRendering)
            throw new InvalidOperationException("Cannot begin rendering while already rendering.");

        GLHandler.BeginRendering();
        ActiveRenderer = this;
        ViewProjectionMatrix = viewProjectionMatrix;
        RenderingObjects.Clear();
    }

    public void EndRendering() {
        if (!GLHandler.IsRendering)
            throw new InvalidOperationException("Cannot end rendering while not rendering.");

        if (!IsActive)
            throw new InvalidOperationException("Cannot end rendering while not active.");

        IOrderedEnumerable<IGrouping<int, RenderData>> zGroups = RenderingObjects.GroupBy(vao => vao.ZIndex).OrderByDescending(vao => vao.Key);
        foreach (IGrouping<int, RenderData> group in zGroups) {
            IEnumerable<IGrouping<Shader, RenderData>> shaderGroups = group.GroupBy(vao => vao.VertexArrayObject.Shader);
            foreach (IGrouping<Shader, RenderData> shaderGroup in shaderGroups) {
                foreach (RenderData rD in shaderGroup) {
                    rD.PreRenderCallback?.Invoke();
                    PerformRenderOperation(rD.VertexArrayObject);
                }
            }
        }
        RenderingObjects.Clear();

        GLHandler.EndRendering();
        ActiveRenderer = null;
        ViewProjectionMatrix = null;
    }

    internal void Render(VertexArrayObject vao, int zIndex, Action? preRenderCallback) {
        if (!GLHandler.IsRendering)
            throw new InvalidOperationException("Cannot render vertex array object while not rendering.");

        if (!IsActive)
            throw new InvalidOperationException("Cannot render vertex array object if the renderer is not active.");

        RenderingObjects.Add(new RenderData(vao, zIndex, preRenderCallback));
    }

    public void PushTransform() {
        GLHandler.PushTransform();
    }

    public void ApplyTransformation(Matrix4 transformationMatrix) {
        GLHandler.ApplyTransformation(transformationMatrix);
    }

    public void ApplyTranslation(float dx, float dy, float dz = 0) {
        GLHandler.ApplyTranslation(dx, dy, dz);
    }

    public void ApplyRotation(float angle) {
        GLHandler.ApplyRotation(angle);
    }

    public void ApplyScaling(float sx, float sy) {
        GLHandler.ApplyScaling(sx, sy);
    }

    public void PopTransform() {
        GLHandler.PopTransform();
    }

    private void PerformRenderOperation(VertexArrayObject vao) {
        if (!GLHandler.IsShaderBound(vao.Shader))
            vao.Shader.Bind();

        for (int i = 0; i < vao.Textures.Length; i++) {
            Texture texture = vao.Textures[i];
            texture.Bind(i);
        }

        GLHandler.BindVao(vao);

        vao.AssignShaderUniforms();

        GLHandler.Render(vao.ElementBufferObject);

        GLHandler.ReleaseVao(vao);

        //foreach (Texture2D texture in Textures)
        //    texture.Release();

        //Shader.Release();




        // TODO keep in mind to maybe add array rendering
        /*if (vao.Mesh.IndexCount == 0)
            GL.DrawArrays(vao.Mesh.PrimitiveType, 0, vao.Mesh.VertexCount);
        else
            GL.DrawElements(vao.Mesh.PrimitiveType, vao.Mesh.IndexCount, vao.Mesh.IndexType, 0);*/
    }
}
