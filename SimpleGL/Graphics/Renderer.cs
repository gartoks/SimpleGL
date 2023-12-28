using OpenTK.Mathematics;
using SimpleGL.Game.Nodes;
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

    public void BeginRendering(Camera camera) {
        BeginRendering(camera.ViewProjectionMatrix);
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
            IEnumerable<IGrouping<Shader, RenderData>> shaderGroups = group.GroupBy(rD => rD.Material.Shader);
            foreach (IGrouping<Shader, RenderData> shaderGroup in shaderGroups) {
                foreach (RenderData rD in shaderGroup) {
                    PerformRenderOperation(rD);
                }
            }
        }
        RenderingObjects.Clear();

        GLHandler.EndRendering();
        ActiveRenderer = null;
        ViewProjectionMatrix = null;
    }

    internal void Render(VertexArrayObject vao, Matrix4 modelMatrix, int zIndex, Material material, IReadOnlyList<Texture> textures, Action? preRenderCallback) {
        if (!GLHandler.IsRendering)
            throw new InvalidOperationException("Cannot render vertex array object while not rendering.");

        if (!IsActive)
            throw new InvalidOperationException("Cannot render vertex array object if the renderer is not active.");

        RenderingObjects.Add(new RenderData(vao, modelMatrix, zIndex, material, textures, preRenderCallback));
    }

    /*public void PushTransform() {
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
    }*/

    private void PerformRenderOperation(RenderData rD) {
        rD.PreRenderCallback?.Invoke();

        if (!GLHandler.IsShaderBound(rD.Material.Shader))
            rD.Material.Shader.Bind();

        for (int i = 0; i < rD.Textures.Count; i++) {
            Texture texture = rD.Textures[i];
            texture.Bind(i);
        }

        GLHandler.BindVao(rD.VertexArrayObject);

        foreach (ShaderUniform uniform in rD.Material.Shader.Uniforms.Values) {
            string name = uniform.Name;

            if (name.StartsWith("u_texture") && uniform.Type == UniformType.Texture2D) {
                int textureIndex = int.Parse(name["u_texture".Length..]);
                uniform.Set(rD.Textures[textureIndex]);
            } else if (name == "u_viewProjectionMatrix" && uniform.Type == UniformType.Matrix4x4)
                uniform.Set(ActiveRenderer!.ViewProjectionMatrix!.Value);
            else if (name == "u_modelMatrix" && uniform.Type == UniformType.Matrix4x4)
                uniform.Set(rD.ModelMatrix);

            rD.Material.AssignShaderUniform(rD.Material.Shader, uniform);
        }

        GLHandler.Render(rD.VertexArrayObject.ElementBufferObject);

        GLHandler.ReleaseVao(rD.VertexArrayObject);

        foreach (Texture2D texture in rD.Textures)
            texture.Release();

        //Shader.Release();




        // TODO keep in mind to maybe add array rendering
        /*if (vao.Mesh.IndexCount == 0)
            GL.DrawArrays(vao.Mesh.PrimitiveType, 0, vao.Mesh.VertexCount);
        else
            GL.DrawElements(vao.Mesh.PrimitiveType, vao.Mesh.IndexCount, vao.Mesh.IndexType, 0);*/
    }
}
