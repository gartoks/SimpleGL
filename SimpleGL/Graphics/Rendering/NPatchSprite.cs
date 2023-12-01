using OpenTK.Mathematics;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util;
using SimpleGL.Util.Extensions;

namespace SimpleGL.Graphics.Rendering;
public class NPatchSprite : IDisposable {

    private Shader _Shader { get; set; }
    public Shader Shader {
        get => _Shader;
        set {
            _Shader = value;

            foreach (VertexArrayObject vao in VertexArrayObjects)
                vao.Shader = value;
        }
    }

    private NPatchTexture _Texture;
    public NPatchTexture Texture {
        get => _Texture;
        set {
            _Texture = value;

            float leftSize = _Texture.left / (float)_Texture.Texture.Width;
            float topSize = _Texture.top / (float)_Texture.Texture.Height;
            float horizontalSize = (_Texture.right - _Texture.left) / (float)_Texture.Texture.Width;
            float verticalSize = (_Texture.bottom - _Texture.top) / (float)_Texture.Texture.Height;
            float rightSize = (_Texture.Texture.Width - _Texture.right) / (float)_Texture.Texture.Width;
            float bottomSize = (_Texture.Texture.Height - _Texture.bottom) / (float)_Texture.Texture.Height;

            float[] xOffsets = new float[] { 0, leftSize, leftSize + horizontalSize };
            float[] yOffsets = new float[] { 0, topSize, topSize + verticalSize };
            float[] widths = new float[] { leftSize, horizontalSize, rightSize };
            float[] heights = new float[] { topSize, verticalSize, bottomSize };

            for (int yPi = 0; yPi < 3; yPi++) {
                for (int xPi = 0; xPi < 3; xPi++) {
                    Mesh mesh = Meshes[xPi + yPi * 3];

                    for (int yi = 0; yi < 2; yi++) {
                        for (int xi = 0; xi < 2; xi++) {
                            int i = xi + yi * 2;
                            VertexArrayObjects[i].Textures[0] = _Texture.Texture;

                            float x = -0.5f + xOffsets[xPi] + xi * widths[xPi];
                            float y = -0.5f + yOffsets[yPi] + yi * heights[yPi];
                            float tx = _Texture.Texture.TextureCoordinates.Min.X + (xOffsets[xPi] + xi * widths[xPi]) * _Texture.Texture.TextureCoordinates.Size.X;
                            float ty = _Texture.Texture.TextureCoordinates.Min.Y + (yOffsets[yPi] + xi * heights[yPi]) * _Texture.Texture.TextureCoordinates.Size.Y;

                            VertexData va = mesh.GetVertexData(i);
                            va.SetAttributeData(mesh.VertexAttributes["position"], x, y, 0);
                            va.SetAttributeData(mesh.VertexAttributes["texCoords0"], tx, ty);
                        }
                    }
                }
            }
        }
    }

    private IReadOnlyList<Mesh> Meshes { get; }

    public Color4 Tint { get; set; }

    public Transform Transform { get; }

    private ShaderUniformAssignmentHandler _ShaderUniformAssignmentHandler { get; set; }
    public ShaderUniformAssignmentHandler ShaderUniformAssignmentHandler {
        get => _ShaderUniformAssignmentHandler;
        set {
            _ShaderUniformAssignmentHandler = value;

            foreach (VertexArrayObject vap in VertexArrayObjects)
                vap.ShaderUniformAssignmentHandler = value;
        }
    }

    private IReadOnlyList<VertexArrayObject> VertexArrayObjects { get; }

    private bool disposedValue;

    public NPatchSprite(NPatchTexture texture, Shader shader) {
        Tint = Color4.White;
        Transform = new Transform();

        _Texture = texture;
        _ShaderUniformAssignmentHandler = AssignShaderUniform;

        Meshes = CreateMesh(texture);
        VertexArrayObjects = Meshes.Select(mesh => GraphicsHelper.CreateVertexArrayObject(ResolveShaderVertexAttribute, AssignShaderUniform, shader, mesh, texture.Texture)).ToArray();
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~NPatchSprite() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }


    public void Render() {
        float leftSize = Texture.left / (float)Texture.Texture.Width;
        float topSize = Texture.top / (float)Texture.Texture.Height;
        float horizontalSize = (Texture.right - Texture.left) / (float)Texture.Texture.Width;
        float verticalSize = (Texture.bottom - Texture.top) / (float)Texture.Texture.Height;
        float rightSize = (Texture.Texture.Width - Texture.right) / (float)Texture.Texture.Width;
        float bottomSize = (Texture.Texture.Height - Texture.bottom) / (float)Texture.Texture.Height;
        float[] widths = new float[] { leftSize, horizontalSize, rightSize };
        float[] heights = new float[] { topSize, verticalSize, bottomSize };

        for (int yi = 0; yi < 3; yi++) {
            for (int xi = 0; xi < 3; xi++) {
                float xScale = xi == 1 ? Transform.Scale.X : 1;
                float yScale = yi == 1 ? Transform.Scale.Y : 1;

                Matrix4 transformMatrix =
                    Matrix4.CreateTranslation(Transform.Pivot.X - 0.5f, Transform.Pivot.Y - 0.5f, 0) *
                    Matrix4.CreateScale(xScale, yScale, 1) *
                    Matrix4.CreateRotationZ(Transform.Rotation) *
                    Matrix4.CreateTranslation(Transform.Position.X, Transform.Position.Y, 0);

                Matrix4 patchMatrix = Matrix4.CreateTranslation(
                    ((Transform.Scale.X * horizontalSize / 2f) - widths[xi]) * (xi - 1),
                    ((Transform.Scale.Y * verticalSize / 2f) - heights[yi]) * (yi - 1),
                    0) * transformMatrix;

                int i = xi + yi * 3;
                Action preRenderCallback = () => {
                    VertexArrayObjects[i].ShaderUniformAssignmentHandler = (shader, uniform) => {
                        ShaderUniformAssignmentHandler(shader, uniform);

                        string name = uniform.Name;
                        if (name == "u_modelMatrix" && uniform.Type == UniformType.Matrix4x4)
                            uniform.Set(patchMatrix);
                    };
                };

                VertexArrayObjects[i].Render(Transform.ZIndex, preRenderCallback);
            }
        }
    }

    internal void Render(int zIndex, Action preRenderCallback) {
        foreach (VertexArrayObject vao in VertexArrayObjects)
            vao.Render(zIndex, preRenderCallback);
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
            uniform.Set(Texture.Texture);
        else if (name == "u_color" && uniform.Type == UniformType.FloatVector4)
            uniform.Set(Tint);
        else if (name == "u_viewProjectionMatrix" && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(Renderer.ActiveRenderer!.ViewProjectionMatrix!.Value);
        ;
    }

    private static IReadOnlyList<Mesh> CreateMesh(NPatchTexture texture) {
        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        VertexAttribute va_texCoords = VertexAttribute.Create("texCoords0", 2);
        VertexAttribute[] vertexAtributes = { va_position, va_color, va_texCoords };

        void SetVertexAttributeData(Mesh mesh, int i, float x, float y, float tx, float ty) {
            VertexData va = mesh.GetVertexData(i);
            va.SetAttributeData(va_position, x, y, 0);
            va.SetAttributeData(va_color, Color4.White.ToArray(true));
            va.SetAttributeData(va_texCoords, tx, ty);
        }

        (uint idx0, uint idx1, uint idx2)[] indices = {
            (0, 1, 2),
            (2, 1, 3)
        };

        float leftSize = texture.left / (float)texture.Texture.Width;
        float topSize = texture.top / (float)texture.Texture.Height;
        float horizontalSize = (texture.right - texture.left) / (float)texture.Texture.Width;
        float verticalSize = (texture.bottom - texture.top) / (float)texture.Texture.Height;
        float rightSize = (texture.Texture.Width - texture.right) / (float)texture.Texture.Width;
        float bottomSize = (texture.Texture.Height - texture.bottom) / (float)texture.Texture.Height;

        float[] xOffsets = new float[] { 0, leftSize, leftSize + horizontalSize };
        float[] yOffsets = new float[] { 0, topSize, topSize + verticalSize };
        float[] widths = new float[] { leftSize, horizontalSize, rightSize };
        float[] heights = new float[] { topSize, verticalSize, bottomSize };

        Mesh[] meshes = new Mesh[9];
        for (int yPi = 0; yPi < 3; yPi++) {
            for (int xPi = 0; xPi < 3; xPi++) {
                Mesh mesh = GraphicsHelper.CreateMesh(4, vertexAtributes, indices);
                meshes[xPi + yPi * 3] = mesh;

                for (int yi = 0; yi < 2; yi++) {
                    for (int xi = 0; xi < 2; xi++) {
                        int i = xi + yi * 2;
                        float x = -0.5f + xOffsets[xPi] + xi * widths[xPi];
                        float y = -0.5f + yOffsets[yPi] + yi * heights[yPi];
                        float tx = texture.Texture.TextureCoordinates.Min.X + (xOffsets[xPi] + xi * widths[xPi]) * texture.Texture.TextureCoordinates.Size.X;
                        float ty = texture.Texture.TextureCoordinates.Min.Y + (yOffsets[yPi] + yi * heights[yPi]) * texture.Texture.TextureCoordinates.Size.Y;

                        SetVertexAttributeData(mesh, i, x, y, tx, ty);
                    }
                }
            }
        }

        return meshes;
    }
}
