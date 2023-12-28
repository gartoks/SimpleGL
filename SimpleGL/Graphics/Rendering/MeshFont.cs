using OpenTK.Mathematics;
using SimpleGL.Game.Util;
using SimpleGL.Util;
using SimpleGL.Util.Extensions;
using SixLabors.Fonts;
using System.Diagnostics;

namespace SimpleGL.Graphics.Rendering;
public class MeshFont : IDisposable {
    public string Key => MeshFontLoader.GetKey(Font.Family.Name, FontSize);
    public FontData Font { get; }
    public float FontSize => Font.Size;

    private TextOptions TextOptions { get; }
    private Material DefaultMaterial { get; }
    private Dictionary<char, RenderObject?> GlyphObjects { get; }


    private bool disposedValue;

    internal MeshFont(FontData fontdata) {
        Font = fontdata;

        DefaultMaterial = Material.CreateDefaultMaterial(0);
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

    public void Render(Transform transform, string text, Material material) {
        ValidateGlyphVaos(text);

        Vector2 textSize = MeasureText(text);

        TextMeasurer.TryMeasureCharacterBounds(text, TextOptions, out ReadOnlySpan<GlyphBounds> tmp);
        TextMeasurer.TryMeasureCharacterAdvances(text, TextOptions, out ReadOnlySpan<GlyphBounds> tmp2);
        GlyphBounds[] glyphBounds = tmp.ToArray();
        GlyphBounds[] advances = tmp2.ToArray();

        Matrix4 transformMatrix = Matrix4.CreateTranslation((transform.Pivot.X - 0.5f) * textSize.X, (transform.Pivot.Y - 0.5f) * textSize.Y, 0) *
                                  Matrix4.CreateScale(transform.Scale.X, transform.Scale.Y, 1) *
                                  Matrix4.CreateRotationZ(transform.Rotation) *
                                  Matrix4.CreateTranslation(transform.Position.X, transform.Position.Y, 0);

        float yOffset = 0;
        for (int i = 0, j = 0; i < text.Length; i++, j++) {
            char c = text[i];

            if (c == '\n')
                yOffset += FontSize;

            if (GlyphObjects[c] == null)
                continue;

            if (advances[j].Codepoint.Value != c) {
                j--;
            }

            GlyphBounds bounds = glyphBounds[j];
            GlyphBounds advance = advances[j];

            Matrix4 glyphMatrix = Matrix4.CreateTranslation(bounds.Bounds.X, yOffset, 0) * transformMatrix;
            GlyphObjects[c]!.Material = material;
            GlyphObjects[c]!.Render(glyphMatrix, transform.ZIndex);
        }
    }

    private void ValidateGlyphVaos(string text) {
        foreach (char c in text) {
            if (GlyphObjects.ContainsKey(c))
                continue;

            Mesh? mesh = CreateMesh(c);

            if (mesh == null) {
                GlyphObjects.Add(c, null);
            } else {
                RenderObject renderObject = new RenderObject(mesh, DefaultMaterial);
                GlyphObjects.Add(c, renderObject);
            }

        }
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
                foreach (KeyValuePair<char, RenderObject?> kvp in GlyphObjects)
                    kvp.Value?.Dispose();
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
