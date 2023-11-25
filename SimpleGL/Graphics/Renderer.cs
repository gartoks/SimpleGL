using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleGL.Graphics.GLHandling;
using SimpleGL.Graphics.Textures;

namespace SimpleGL.Graphics;

public enum eRenderingMode { Immediate, Batched }

public sealed class Renderer {
    public static Renderer? ActiveRenderer { get; private set; }

    public Box2i Viewport { get; set; }
    public Matrix4? ViewProjectionMatrix { get; private set; }

    public bool IsActive => GLHandler.IsRendering && ActiveRenderer == this;

    private eRenderingMode RenderingMode { get; set; }
    private List<VertexArrayObject> RenderingVaos { get; }

    public Renderer(Box2i viewport) {
        Viewport = viewport;
        RenderingVaos = new List<VertexArrayObject>();
    }

    public void BeginRendering(Matrix4 viewProjectionMatrix, eRenderingMode renderingMode) {
        if (GLHandler.IsRendering)
            throw new InvalidOperationException("Cannot begin rendering while already rendering.");

        GLHandler.BeginRendering();
        ActiveRenderer = this;
        ViewProjectionMatrix = viewProjectionMatrix;
        RenderingMode = renderingMode;
        RenderingVaos.Clear();

        GL.Viewport(Viewport);
    }

    public void EndRendering() {
        if (!GLHandler.IsRendering)
            throw new InvalidOperationException("Cannot end rendering while not rendering.");

        if (!IsActive)
            throw new InvalidOperationException("Cannot end rendering while not active.");

        if (RenderingMode == eRenderingMode.Batched) {
            IEnumerable<IGrouping<int, VertexArrayObject>> zGroups = RenderingVaos.GroupBy(vao => vao.Mesh.ZIndex);
            foreach (IGrouping<int, VertexArrayObject> group in zGroups) {
                IEnumerable<IGrouping<Shader, VertexArrayObject>> shaderGroups = group.GroupBy(vao => vao.Shader);
                foreach (IGrouping<Shader, VertexArrayObject> shaderGroup in shaderGroups) {
                    foreach (VertexArrayObject vao in shaderGroup)
                        PerformRenderOperation(vao);
                }
            }
            RenderingVaos.Clear();
        }

        GLHandler.EndRendering();
        ActiveRenderer = null;
        ViewProjectionMatrix = null;
    }

    public void Render(VertexArrayObject vao) {
        if (!GLHandler.IsRendering)
            throw new InvalidOperationException("Cannot render vertex array object while not rendering.");

        if (!IsActive)
            throw new InvalidOperationException("Cannot render vertex array object if the renderer is not active.");

        if (RenderingMode == eRenderingMode.Immediate) {
            PerformRenderOperation(vao);
        } else {
            RenderingVaos.Add(vao);
        }
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
