using OpenTK.Mathematics;
using SimpleGL;
using SimpleGL.Graphics;
using SimpleGL.Graphics.Rendering;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util;
using SimpleGL.Util.Extensions;
using SixLabors.Fonts;
using StbImageSharp;
using System.Diagnostics;

namespace SimpleGLTest;
internal sealed class TestApplication : Application {
    private const int FPS = 60;
    private const int UPS = 60;

    private Renderer Renderer { get; set; }

    private Shader Shader { get; set; }
    private Shader TextureShader { get; set; }
    private Texture Texture { get; set; }

    private MeshFont Font { get; set; }
    private Sprite Sprite { get; set; }

    private Matrix4 ProjectionMatrix { get; }

    public TestApplication()
        : base(FPS, UPS) {

        Log.OnLog += (message, type) => Console.WriteLine($"[{type}] {message}");
        Log.OnLog += (message, type) => Debug.WriteLine($"[{type}] {message}");

        const int SIZE = 20;
        ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(-SIZE * 16f / 9f, SIZE * 16f / 9f, SIZE, -SIZE, -1, 1);
    }

    public override void OnRenderStart() {
        GraphicsHelper.CreateUntexturedPassthroughShader(true, out string vertexShader, out string fragmentShader);
        Shader = GraphicsHelper.CreateShader(vertexShader, fragmentShader);
        GraphicsHelper.CreateTexturedPassthroughShader(true, 1, out string textureVertexShader, out string textureFragmentShader);
        TextureShader = GraphicsHelper.CreateShader(textureVertexShader, textureFragmentShader);

        FontCollection fontCollection = new FontCollection();
        FontFamily fontFamily = fontCollection.Add("Resources/New Bread.ttf");
        Font font = fontFamily.CreateFont(4, FontStyle.Regular);
        Font = new MeshFont(font, Shader);
        Font.Transform.ZIndex = 4;
        Font.Transform.Position = new Vector2(0, 0f);
        Font.Transform.Pivot = new Vector2(0f, 0f);

        using FileStream fs = new FileStream(Path.Combine("Resources", "TestTex01.png"), FileMode.Open);
        ImageResult image = ImageResult.FromStream(fs);
        Texture = GraphicsHelper.CreateTexture(image);
        Sprite = new Sprite(Texture, TextureShader);
        Sprite.Transform.ZIndex = 3;
        Sprite.Transform.Scale = new Vector2(4f, 4f);

        Window.ClientSize = new(1920, 1080);
        Renderer = new Renderer();
    }

    public override void OnRenderStop() {
    }

    private float Time = 0;
    public override void OnRender(float deltaTime) {
        Renderer.BeginRendering(ProjectionMatrix);

        Time += deltaTime;

        Font.Transform.Rotation = Time;
        Font.Render("Hellop,\nabp\nAB");
        Sprite.Render();
        Primitives.DrawRectangleLines(Vector2.Zero, Vector2.One, 0.1f, new Vector2(0f, 0f), Time, 5, Color4.Red);
        Primitives.DrawRectangle(Vector2.Zero, Vector2.One / 2f, new Vector2(0.5f, 0.5f), -Time, 6, Color4.Lime);
        Primitives.DrawLine(Vector2.Zero, Vector2.One, 0.1f, 7, Color4.Blue);

        Renderer.EndRendering();
    }

    public override void OnUpdateStart() {
    }

    public override void OnUpdateStop() {
    }

    public override void OnUpdate(float deltaTime) {
    }

    private static Mesh CreateTextMesh() {
        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        VertexAttribute[] vertexAtributes = { va_position, va_color };

        FontCollection fontCollection = new FontCollection();
        FontFamily fontFamily = fontCollection.Add("Resources/New Bread.ttf");
        Font font = fontFamily.CreateFont(40, FontStyle.Regular);
        (Vector2[] vertices, (uint i0, uint i1, uint i2)[] triangles) = TextMeshGenerator.ConvertToMesh(font, "Hello World!");

        Random random = new();
        Func<Color4> RandomColor = () => new Color4(random.NextSingle(), random.NextSingle(), random.NextSingle(), 1);
        //Color4 c = RandomColor();
        Color4 c = Color4.White;

        Debug.WriteLine($"{vertices.Length} {triangles.Length}");
        Mesh mesh = GraphicsHelper.CreateMesh(vertices.Length, vertexAtributes, triangles);
        for (int i = 0; i < vertices.Length; i++) {
            VertexData va = mesh.GetVertexData(i);
            va.SetAttributeData(va_position, vertices[i].X, vertices[i].Y, 0);
            va.SetAttributeData(va_color, c.ToArray(true));
        }

        return mesh;
    }

    private VertexAttribute ResolveShaderVertexAttribute(VertexAttribute shaderAttribute, IEnumerable<VertexAttribute> meshAttributes) {
        return meshAttributes.Single(ma => shaderAttribute.Name.Split("_")[1].Contains(ma.Name));
    }

    private void AssignShaderUniform(Shader shader, ShaderUniform uniform) {
        if (uniform.Name.ToLower().Contains("tex"))
            uniform.Set(Texture);
        else if (uniform.Name.ToLowerInvariant().Contains("projection"))
            uniform.Set(ProjectionMatrix);
        else
            uniform.Set(Matrix4.Identity);
    }
}
