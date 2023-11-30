using OpenTK.Mathematics;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util;
using SimpleGL.Util.Extensions;
using System.Drawing;

namespace SimpleGL.Graphics.Rendering;
public class NPatchSprite : IDisposable {

    public Shader Shader {
        get => VertexArrayObject.Shader;
        set => VertexArrayObject.Shader = value;
    }

    public Mesh Mesh {
        get => VertexArrayObject.Mesh;
        set => VertexArrayObject.Mesh = value;
    }

    public NPatchTexture Texture {
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

    public Color4 Tint { get; set; }

    public Transform Transform { get; }

    public ShaderUniformAssignmentHandler ShaderUniformAssignmentHandler {
        get => VertexArrayObject.ShaderUniformAssignmentHandler;
        set => VertexArrayObject.ShaderUniformAssignmentHandler = value;
    }

    private IReadOnlyList<VertexArrayObject> VertexArrayObject { get; }

    private bool disposedValue;

    public NPatchSprite(Texture texture, Shader shader) {
        Tint = Color4.White;
        Transform = new Transform();

        Mesh mesh = CreateMesh(texture);
        VertexArrayObject = GraphicsHelper.CreateVertexArrayObject(ResolveShaderVertexAttribute, AssignShaderUniform, shader, mesh, texture);
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~NPatchSprite() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }


    public void Render() {
        float tw = Texture.Texture.Width;
        float th = Texture.Texture.Height;
        float bw = Transform.Scale.X;
        float bh = Transform.Scale.Y;
        float r = Texture.Texture.Width - Texture.right;
        float b = Texture.Texture.Height - Texture.bottom;

        float centerW = Math.Max(0, Transform.Scale.X - r - Texture.left);
        float centerH = Math.Max(0, Transform.Scale.Y - b - Texture.top);
        float wScale = Math.Min(1, bw / (Texture.left + r));
        float hScale = Math.Min(1, bh / (Texture.top + b));

        void Draw(float xT, float yT, float wT, float hT, float xB, float yB, float wB, float hB) {
            Raylib.DrawTexturePro(
                    Resource.Texture,
                    new Rectangle(xT, yT, wT, hT),
                    new Rectangle(bounds.x + xB, bounds.y + yB, wB, hB),
                    Vector2.Zero,   // TODO
                    0,  // TODO
                    tint != null ? tint.Value : Color4.White);
        }















        float leftOffset = Texture.left / (float)Texture.Texture.Width;
        float topOffset = Texture.top / (float)Texture.Texture.Height;

        VertexArrayObject.Render(Transform.ZIndex);
    }

    internal void Render(int zIndex, Action preRenderCallback) {
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

        if (name == "u_texture0" && uniform.Type == UniformType.Texture2D)
            uniform.Set(Texture);
        else if (name == "u_color" && uniform.Type == UniformType.FloatVector4)
            uniform.Set(Tint);
        else if (name == "u_viewProjectionMatrix" && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(Renderer.ActiveRenderer!.ViewProjectionMatrix!.Value);
        else if (name == "u_modelMatrix" && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(Transform.TransformationMatrix);
    }

    private static Mesh CreateMesh(Texture texture) {
        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        VertexAttribute va_texCoords = VertexAttribute.Create("texCoords0", 2);
        VertexAttribute[] vertexAtributes = { va_position, va_color, va_texCoords };

        (uint idx0, uint idx1, uint idx2)[] indices = {
            // TL
            (0, 1, 2),
            (2, 1, 3),
            // TM
            (1, 4, 3),
            (3, 4, 5),
            // TR
            (4, 5, 6),
            (6, 5, 7),
            // ML
            (2, 3, 8),
            (8, 3, 9),
            // MM
            (3, 6, 9),
            (9, 6, 12),
            // MR
            (6, 7, 12),
            (12, 7, 13),
            // BL
            (8, 9, 10),
            (10, 9, 11),
            // BM
            (9, 12, 11),
            (11, 12, 14),
            // BR
            (12, 13, 14),
            (14, 13, 15),
        };

        float[][] textureCoordinates = texture.TextureCoordinates.Select(t => new float[] { t.x, t.y }).ToArray();

        Mesh mesh = GraphicsHelper.CreateMesh(4, vertexAtributes, indices);
        for (int oy = 0; oy < 2; oy++) {
            for (int ox = 0; ox < 2; ox++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        int i = x + y * 2;
                        VertexData va = mesh.GetVertexData(i);
                        va.SetAttributeData(va_position, -0.5f + x + , -0.5f + y, 0);
                        va.SetAttributeData(va_color, Color4.White.ToArray(true));
                        va.SetAttributeData(va_texCoords, textureCoordinates[i]);
                    }
                }
            }
        }

        return mesh;
    }
}
