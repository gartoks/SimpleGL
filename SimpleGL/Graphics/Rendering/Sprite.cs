using OpenTK.Mathematics;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util.Extensions;

namespace SimpleGL.Graphics.Rendering;
public class Sprite : IDisposable {

    public Shader Shader {
        get => VertexArrayObject.Shader;
        set => VertexArrayObject.Shader = value;
    }

    public Mesh Mesh {
        get => VertexArrayObject.Mesh;
        set => VertexArrayObject.Mesh = value;
    }

    public Texture Texture {
        get => VertexArrayObject.Textures[0];
        set {
            VertexArrayObject.Textures[0] = value;

            float[][] textureCoordinates = value.TextureCoordinates.Select(t => new float[] { t.x, t.y }).ToArray();
            for (int y = 0; y < 2; y++) {
                for (int x = 0; x < 2; x++) {
                    int i = x + y * 2;
                    VertexData va = Mesh.GetVertexData(i);
                    va.SetAttributeData(Mesh.VertexAttributes["texCoords0"], textureCoordinates[/*x + y * 2*/i]);
                }
            }
        }
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
                _ModelMatrix = Matrix4.CreateScale(Scale.X, Scale.Y, 1) * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(Position.X, Position.Y, 0);
                IsModelMatrixDirty = false;
            }

            return _ModelMatrix;
        }
    }

    private bool IsModelMatrixDirty { get; set; }

    private bool disposedValue;

    public Sprite(Texture texture, Shader shader) {
        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        VertexAttribute va_texCoords = VertexAttribute.Create("texCoords0", 2);
        VertexAttribute[] vertexAtributes = { va_position, va_color, va_texCoords };

        (uint idx0, uint idx1, uint idx2)[] indices = {
            (0, 1, 2),
            (2, 1, 3)
        };

        float[][] textureCoordinates = texture.TextureCoordinates.Select(t => new float[] { t.x, t.y }).ToArray();

        Mesh mesh = GraphicsHelper.CreateMesh(4, vertexAtributes, indices);
        for (int y = 0; y < 2; y++) {
            for (int x = 0; x < 2; x++) {
                int i = x + y * 2;
                VertexData va = mesh.GetVertexData(i);
                va.SetAttributeData(va_position, -0.5f + x, -0.5f + y, 0);
                va.SetAttributeData(va_color, Color4.White.ToArray(true));
                va.SetAttributeData(va_texCoords, textureCoordinates[/*x + y * 2*/i]);
            }
        }

        VertexArrayObject = GraphicsHelper.CreateVertexArrayObject(ResolveShaderVertexAttribute, AssignShaderUniform, shader, mesh, texture);

        Position = Vector2.Zero;
        Scale = Vector2.One;
        Rotation = 0;
        IsModelMatrixDirty = true;
    }

    public void Render(Renderer renderer) {
        if (!renderer.IsActive)
            throw new InvalidOperationException("Cannot render with an inactive renderer.");

        VertexArrayObject.Render();
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

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~Sprite() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    public void AssignShaderUniform(Shader shader, ShaderUniform uniform) {
        string name = uniform.Name.ToLowerInvariant();

        if (name.Contains("tex") && uniform.Type == UniformType.Texture2D)
            uniform.Set(Texture);
        else if (name.Contains("projection") && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(Renderer.ActiveRenderer!.ViewProjectionMatrix!.Value);
        else if (name.Contains("model") && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(ModelMatrix);
    }

}
