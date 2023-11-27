using OpenTK.Mathematics;
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

    private Vector2 _Position { get; set; }
    public Vector2 Position {
        get => _Position;
        set {
            _Position = value;
            IsModelMatrixDirty = true;
        }
    }

    private Vector2 _Scale { get; set; }
    public Vector2 Scale {
        get => _Scale;
        set {
            _Scale = value;
            IsModelMatrixDirty = true;
        }
    }

    private float _Rotation { get; set; }
    public float Rotation {
        get => _Rotation;
        set {
            _Rotation = value;
            IsModelMatrixDirty = true;
        }
    }

    private Vector2 _Pivot { get; set; }
    public Vector2 Pivot {
        get => _Pivot;
        set {
            _Pivot = value;
            IsModelMatrixDirty = true;
        }
    }

    private int _ZIndex { get; set; }
    public int ZIndex {
        get => _ZIndex;
        set => _ZIndex = value;
    }


    public ShaderUniformAssignmentHandler ShaderUniformAssignmentHandler {
        get => VertexArrayObject.ShaderUniformAssignmentHandler;
        set => VertexArrayObject.ShaderUniformAssignmentHandler = value;
    }

    private VertexArrayObject VertexArrayObject { get; }

    //public Color4 Tint { get; }
    private Matrix4 _ModelMatrix { get; set; }
    private Matrix4 ModelMatrix {
        get {
            if (IsModelMatrixDirty) {
                _ModelMatrix = Matrix4.CreateTranslation(Pivot.X - 0.5f, Pivot.Y - 0.5f, 0) *
                               Matrix4.CreateScale(Scale.X, Scale.Y, 1) *
                               Matrix4.CreateRotationZ(Rotation) *
                               Matrix4.CreateTranslation(Position.X, Position.Y, 0);
                IsModelMatrixDirty = false;
            }

            return _ModelMatrix;
        }
    }
    private bool IsModelMatrixDirty { get; set; }

    private bool disposedValue;

    public RectanglePrimitive(Shader shader) {
        Mesh mesh = CreateMesh();
        VertexArrayObject = GraphicsHelper.CreateVertexArrayObject(ResolveShaderVertexAttribute, AssignShaderUniform, shader, mesh);

        Tint = Color4.White;

        Position = Vector2.Zero;
        Scale = Vector2.One;
        Rotation = 0;
        Pivot = Vector2.Zero;
        IsModelMatrixDirty = true;
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~RectanglePrimitive() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Render(Renderer renderer) {
        if (!renderer.IsActive)
            throw new InvalidOperationException("Cannot render with an inactive renderer.");

        VertexArrayObject.Render(renderer, ZIndex);
    }

    internal void Render(Renderer renderer, int zIndex, Action preRenderCallback) {
        if (!renderer.IsActive)
            throw new InvalidOperationException("Cannot render with an inactive renderer.");

        VertexArrayObject.Render(renderer, zIndex, preRenderCallback);
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
        string name = uniform.Name/*.ToLowerInvariant()*/;

        if (name == "u_color" && uniform.Type == UniformType.FloatVector4)
            uniform.Set(Tint);
        else if (name == "u_viewProjectionMatrix" && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(Renderer.ActiveRenderer!.ViewProjectionMatrix!.Value);
        else if (name == "u_modelMatrix" && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(ModelMatrix);
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
