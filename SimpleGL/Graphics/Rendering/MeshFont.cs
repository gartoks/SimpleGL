using OpenTK.Mathematics;
using SimpleGL.Util;
using SimpleGL.Util.Extensions;
using SixLabors.Fonts;
using System.Diagnostics;

namespace SimpleGL.Graphics.Rendering;
public class MeshFont : IDisposable {
    public FontData Font { get; }
    public float FontSize => Font.Size;

    private Shader _Shader { get; set; }
    public Shader Shader {
        get => _Shader;
        set {
            _Shader = value;
            foreach (KeyValuePair<char, VertexArrayObject> kvp in GlyphVaos)
                kvp.Value.Shader = value;
        }
    }

    public Color4 Tint { get; set; }

    public Transform Transform { get; }

    private ShaderUniformAssignmentHandler _ShaderUniformAssignmentHandler { get; set; }
    public ShaderUniformAssignmentHandler ShaderUniformAssignmentHandler {
        get => _ShaderUniformAssignmentHandler;
        set {
            _ShaderUniformAssignmentHandler = value;

            foreach (VertexArrayObject? vao in GlyphVaos.Values) {
                if (vao != null)
                    vao.ShaderUniformAssignmentHandler = value;
            }
        }
    }

    private TextOptions TextOptions { get; }

    private Dictionary<char, VertexArrayObject?> GlyphVaos { get; }

    private bool disposedValue;

    internal MeshFont(FontData fontdata, Shader shader) {
        Font = fontdata;
        _Shader = shader;

        Tint = Color4.White;
        Transform = new Transform();

        _ShaderUniformAssignmentHandler = AssignShaderUniform;

        TextOptions = new TextOptions(Font.Font);

        GlyphVaos = new();
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~MeshFont() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public Vector2 MeasureText(string text) {
        FontRectangle fR = TextMeasurer.MeasureBounds(text, TextOptions);
        return new Vector2(fR.Width, fR.Height);
    }

    public void Render(string text) {
        ValidateGlyphVaos(text);

        Vector2 textSize = MeasureText(text);

        TextMeasurer.TryMeasureCharacterBounds(text, TextOptions, out ReadOnlySpan<GlyphBounds> tmp);
        TextMeasurer.TryMeasureCharacterAdvances(text, TextOptions, out ReadOnlySpan<GlyphBounds> tmp2);
        GlyphBounds[] glyphBounds = tmp.ToArray();
        GlyphBounds[] advances = tmp2.ToArray();

        Matrix4 transformMatrix = Matrix4.CreateTranslation((Transform.Pivot.X - 0.5f) * textSize.X, (Transform.Pivot.Y - 0.5f) * textSize.Y, 0) *
                                  Matrix4.CreateScale(Transform.Scale.X, Transform.Scale.Y, 1) *
                                  Matrix4.CreateRotationZ(Transform.Rotation) *
                                  Matrix4.CreateTranslation(Transform.Position.X, Transform.Position.Y, 0);

        float yOffset = 0;
        for (int i = 0, j = 0; i < text.Length; i++, j++) {
            char c = text[i];

            if (c == '\n')
                yOffset += FontSize;

            if (GlyphVaos[c] == null)
                continue;

            if (advances[j].Codepoint.Value != c) {
                j--;
            }

            GlyphBounds bounds = glyphBounds[j];
            GlyphBounds advance = advances[j];

            Matrix4 glyphMatrix = Matrix4.CreateTranslation(bounds.Bounds.X, yOffset, 0) * transformMatrix;

            Action preRenderCallback = () => {
                GlyphVaos[c]!.ShaderUniformAssignmentHandler = (shader, uniform) => {
                    ShaderUniformAssignmentHandler(shader, uniform);

                    string name = uniform.Name;
                    if (name == "u_modelMatrix" && uniform.Type == UniformType.Matrix4x4)
                        uniform.Set(glyphMatrix);
                };
            };

            GlyphVaos[c]!.Render(Transform.ZIndex, preRenderCallback);
        }
    }

    internal void Render(string text, int zIndex, Action preRenderCallback) {
        text = text.Replace('\0', '\r').Replace("\r", "");

        ValidateGlyphVaos(text);

        foreach (char c in text) {
            if (c == ' ')
                continue;
            GlyphVaos[c]?.Render(zIndex, preRenderCallback);
        }
    }

    private void ValidateGlyphVaos(string text) {
        foreach (char c in text) {
            if (GlyphVaos.ContainsKey(c))
                continue;

            Mesh? mesh = CreateMesh(c);

            if (mesh == null) {
                GlyphVaos.Add(c, null);
            } else {
                VertexArrayObject va = GraphicsHelper.CreateVertexArrayObject(ResolveShaderVertexAttribute, AssignShaderUniform, Shader, mesh);
                GlyphVaos.Add(c, va);
            }

        }
    }

    private VertexAttribute ResolveShaderVertexAttribute(VertexAttribute shaderAttribute, IEnumerable<VertexAttribute> meshAttributes) {
        return meshAttributes.Single(ma => shaderAttribute.Name.Split("_")[1] == ma.Name);
    }

    private void AssignShaderUniform(Shader shader, ShaderUniform uniform) {
        string name = uniform.Name;

        if (name == "u_color" && uniform.Type == UniformType.FloatVector4)
            uniform.Set(Tint);
        else if (name == "u_viewProjectionMatrix" && uniform.Type == UniformType.Matrix4x4)
            uniform.Set(Renderer.ActiveRenderer!.ViewProjectionMatrix!.Value);
    }

    private Mesh? CreateMesh(char c) {
        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        VertexAttribute[] vertexAtributes = { va_position, va_color, };

        (Vector2[] vertices, (uint i0, uint i1, uint i2)[] triangles) = TextMeshGenerator.ConvertToMesh(Font, $"{c}");

        if (vertices.Length == 0)
            return null;

        Debug.WriteLine($"{Font.Font.Name} '{c}' {vertices.Length} {triangles.Length}");
        Mesh mesh = GraphicsHelper.CreateMesh(vertices.Length, vertexAtributes, triangles);
        for (int i = 0; i < vertices.Length; i++) {
            VertexData va = mesh.GetVertexData(i);
            va.SetAttributeData(va_position, vertices[i].X, vertices[i].Y, 0);
            va.SetAttributeData(va_color, Color4.White.ToArray(true));
        }

        return mesh;
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

}
