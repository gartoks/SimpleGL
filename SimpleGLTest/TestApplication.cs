using OpenTK.Mathematics;
using SimpleGL;
using SimpleGL.Graphics;
using SimpleGL.Graphics.GLHandling;
using SimpleGL.Util;
using SimpleGL.Util.Extensions;
using System.Diagnostics;

namespace SimpleGLTest;
internal sealed class TestApplication : Application {
    private const int FPS = 60;
    private const int UPS = 60;

    private Shader Shader { get; set; }
    private Mesh Mesh { get; set; }
    private VertexArrayObject Vao { get; set; }

    private Matrix4 ProjectionMatrix { get; }

    public TestApplication()
        : base(FPS, UPS) {

        Log.OnLog += (message, type) => Console.WriteLine($"[{type}] {message}");
        Log.OnLog += (message, type) => Debug.WriteLine($"[{type}] {message}");

        ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(-5, 5, -5, 5, -1, 1);
    }

    public override void OnRenderStart() {
        GraphicsHelper.CreateUntexturedPassthroughShader(true, out string vertexShader, out string fragmentShader);
        Shader = GraphicsHelper.CreateShader(vertexShader, fragmentShader);

        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        //VertexAttribute va_texCoords = VertexAttribute.Create("mainTexture_coords", 2);
        VertexAttribute[] vertexAtributes = { va_position, va_color/*, va_texCoords*/ };

        (uint idx0, uint idx1, uint idx2)[] indices = {
                (0, 1, 2),
                (2, 1, 3)
            };

        //float[][] textureCoordinates = new float[][] { new float[] { 0, 0 }, new float[] { 1, 0 }, new float[] { 0, 1 }, new float[] { 1, 1 } };

        Mesh = GraphicsHelper.CreateMesh(4, vertexAtributes, indices);
        /*VertexData va = Mesh.GetVertexData(0);
        va.SetAttributeData(va_position, -0.5f, -0.5f, 0);
        va.SetAttributeData(va_color, Color4.White.ToArray(true));
        va = Mesh.GetVertexData(1);
        va.SetAttributeData(va_position, 0.5f, -0.5f, 0);
        va.SetAttributeData(va_color, Color4.White.ToArray(true));
        va = Mesh.GetVertexData(2);
        va.SetAttributeData(va_position, -0.5f, 0.5f, 0);
        va.SetAttributeData(va_color, Color4.White.ToArray(true));*/

        int i = 0;
        for (int y = 0; y < 2; y++) {
            for (int x = 0; x < 2; x++) {
                VertexData va = Mesh.GetVertexData(i);
                va.SetAttributeData(va_position, -0.5f + x, -0.5f + y, 0);
                va.SetAttributeData(va_color, Color4.White.ToArray(true));
                //va.SetAttributeData(va_texCoords, textureCoordinates[x + y * 2]);
                i++;
            }
        }

        Vao = GraphicsHelper.CreateVertexArrayObject(ResolveShaderVertexAttribute, AssignShaderUniform, Shader, Mesh);


        //GLHandler.ClockwiseCulling = true;
        Window.ClientSize = new(1920, 1080);
    }

    public override void OnRenderStop() {
    }

    public override void OnRender(float deltaTime) {
        GLHandler.BeginRendering();

        Vao.Render();

        GLHandler.EndRendering();
    }

    public override void OnUpdateStart() {
    }

    public override void OnUpdateStop() {
    }

    public override void OnUpdate(float deltaTime) {
    }

    private VertexAttribute ResolveShaderVertexAttribute(VertexAttribute shaderAttribute, IEnumerable<VertexAttribute> meshAttributes) {
        return meshAttributes.Single(ma => shaderAttribute.Name.Split("_")[1] == ma.Name);
    }

    private void AssignShaderUniform(Shader shader, ShaderUniform uniform) {
        if (uniform.Name.ToLowerInvariant().Contains("projection"))
            uniform.Set(ProjectionMatrix);
        else
            uniform.Set(Matrix4.Identity);
    }
}
