using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
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
    private Mesh Mesh { get; set; }
    private VertexArrayObject Vao { get; set; }
    private Texture Texture { get; set; }

    private Sprite Sprite { get; set; }

    private Matrix4 ProjectionMatrix { get; }

    public TestApplication()
        : base(FPS, UPS) {

        Log.OnLog += (message, type) => Console.WriteLine($"[{type}] {message}");
        Log.OnLog += (message, type) => Debug.WriteLine($"[{type}] {message}");

        const int SIZE = 50;
        ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(-SIZE * 0 * 16f / 9f, SIZE * 16f / 9f, SIZE, -SIZE, -1, 1);
    }

    public override void OnRenderStart() {
        GraphicsHelper.CreateUntexturedPassthroughShader(true, out string vertexShader, out string fragmentShader);
        Shader = GraphicsHelper.CreateShader(vertexShader, fragmentShader);

        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        VertexAttribute[] vertexAtributes = { va_position, va_color };

        //const int VERTICES = 32;
        //Vector2[] outerVertices = Enumerable.Range(0, VERTICES).Select(i => new Vector2(1f * -MathF.Cos(i / (float)VERTICES * MathF.Tau), 1f * MathF.Sin(i / (float)VERTICES * MathF.Tau))).ToArray();
        //Vector2[] innerVertices = Enumerable.Range(0, VERTICES).Select(i => new Vector2(0.5f * MathF.Cos(i / (float)VERTICES * MathF.Tau), 0.5f * MathF.Sin(i / (float)VERTICES * MathF.Tau))).ToArray();
        //MeshTriangulation.Triangulate(outerVertices, new Vector2[][] { innerVertices }, out Vector2[] vertices, out (uint i0, uint i1, uint i2)[] indices);

        using FileStream fs = new FileStream(Path.Combine("Resources", "TestTex01.png"), FileMode.Open);
        ImageResult image = ImageResult.FromStream(fs);
        Texture = GraphicsHelper.CreateTexture(image);

        FontCollection fontCollection = new FontCollection();
        FontFamily fontFamily = fontCollection.Add("Resources/New Bread.ttf");
        Font font = fontFamily.CreateFont(12, FontStyle.Regular);
        (Vector2[] vertices, (uint i0, uint i1, uint i2)[] triangles) = TextMeshGenerator.ConvertToMesh(font, "Hello World!");

        Random random = new Random();
        Mesh = GraphicsHelper.CreateMesh(vertices.Length, vertexAtributes, triangles);
        for (int i = 0; i < vertices.Length; i++) {
            VertexData va = Mesh.GetVertexData(i);
            va.SetAttributeData(va_position, vertices[i].X, vertices[i].Y, 0);
            va.SetAttributeData(va_color, /*RndColor(random)*/Color4.White.ToArray(true));
        }
        Vao = GraphicsHelper.CreateVertexArrayObject(ResolveShaderVertexAttribute, AssignShaderUniform, Shader, Mesh, Texture);

        Sprite = new Sprite(Texture, Shader);

        Window.ClientSize = new(1920, 1080);
        Renderer = new(new Box2i(0, 0, Window.ClientSize.X, Window.ClientSize.Y));
        Window.Resize += Window_Resize;
    }

    private Color4 RndColor(Random random) {
        return new Color4(random.NextSingle(), random.NextSingle(), random.NextSingle(), 1);
    }

    private void Window_Resize(ResizeEventArgs obj) {
        Renderer.Viewport = new(0, 0, obj.Width, obj.Height);
    }

    public override void OnRenderStop() {
    }

    private float Time = 0;
    public override void OnRender(float deltaTime) {
        Renderer.BeginRendering(ProjectionMatrix, eRenderingMode.Batched);

        Time += deltaTime;

        Vao.Render(Renderer);
        //Sprite.Render(Renderer);

        Renderer.EndRendering();
    }

    public override void OnUpdateStart() {
    }

    public override void OnUpdateStop() {
    }

    public override void OnUpdate(float deltaTime) {
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
