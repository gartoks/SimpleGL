using OpenTK.Mathematics;
using SimpleGL.Util;
using SimpleGL.Util.Extensions;

namespace SimpleGL.Graphics.Rendering;
public class RectanglePrimitive : IDisposable {

    public Shader Shader {
        get => VertexArrayObject.Shader;
        set => VertexArrayObject.Shader = value;
    }

    private Color4 _Tint { get; set; }
    public Color4 Tint {
        get => _Tint;
        set => _Tint = value;
    }

    public Transform Transform { get; }

    public ShaderUniformAssignmentHandler ShaderUniformAssignmentHandler {
        get => VertexArrayObject.ShaderUniformAssignmentHandler;
        set => VertexArrayObject.ShaderUniformAssignmentHandler = value;
    }

    private VertexArrayObject VertexArrayObject { get; }

    private bool disposedValue;

    public RectanglePrimitive(Shader shader) {
        Mesh mesh = CreateMesh();
        VertexArrayObject = GraphicsHelper.CreateVertexArrayObject(ResolveShaderVertexAttribute, AssignShaderUniform, shader, mesh);

        Tint = Color4.White;

        Transform = new Transform();
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~RectanglePrimitive() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Render() {
        if (!Renderer.HasActiveRenderer)
            throw new InvalidOperationException("No renderer is active.");

        VertexArrayObject.Render(Transform.ZIndex);
    }

    internal void Render(int zIndex, Action preRenderCallback) {
        if (!Renderer.HasActiveRenderer)
            throw new InvalidOperationException("No renderer is active.");

        VertexArrayObject.Render(zIndex, preRenderCallback);
    }

    private VertexAttribute ResolveShaderVertexAttribute(VertexAttribute shaderAttribute, IEnumerable<VertexAttribute> meshAttributes) {
        return meshAttributes.Single(ma => shaderAttribute.Name.Split("_")[1] == ma.Name);
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // dispose managed state (managed objects)
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    public void AssignShaderUniform(Shader shader, ShaderUniform uniform) {
        string name = uniform.Name;

        if (name == "u_color" && uniform.Type == UniformType.FloatVector4)
            uniform.Set(Tint);
        else if (name == "u_viewProjectionMatrix" && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(Renderer.ActiveRenderer!.ViewProjectionMatrix!.Value);
        else if (name == "u_modelMatrix" && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(Transform.TransformationMatrix);
    }

    private static Mesh CreateMesh() {
        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        VertexAttribute[] vertexAtributes = { va_position, va_color };

        (uint idx0, uint idx1, uint idx2)[] indices = {
            (0, 1, 2),
            (2, 1, 3)
        };

        Mesh mesh = GraphicsHelper.CreateMesh(4, vertexAtributes, indices);
        for (int y = 0; y < 2; y++) {
            for (int x = 0; x < 2; x++) {
                int i = x + y * 2;
                VertexData va = mesh.GetVertexData(i);
                va.SetAttributeData(va_position, -0.5f + x, -0.5f + y, 0);
                va.SetAttributeData(va_color, Color4.White.ToArray(true));
            }
        }

        return mesh;
    }
}
