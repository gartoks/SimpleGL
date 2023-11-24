﻿using OpenTK.Mathematics;
using SimpleGL.Graphics.GLHandling;
using SimpleGL.Graphics.Textures;
using StbImageSharp;
using System.Text;

namespace SimpleGL.Graphics;
public delegate VertexAttribute ShaderVertexAttributeResolver(VertexAttribute shaderAttribute, IEnumerable<VertexAttribute> meshAttributes);

public delegate void ShaderUniformAssignmentHandler(Shader shader, ShaderUniform uniform);

public static class GraphicsHelper {
    /*private static GraphicsHandler instance;
    internal static static GraphicsHandler Instance {
        get => GraphicsHandler.instance;
        private set { if (GraphicsHandler.instance != null) throw new InvalidOperationException("Only one instance per manager type permitted."); else instance = value; }
    }

    public static GraphicsHandler() {
        Instance = this;
    }*/

    public static bool IsGLThread() => GLHandler.RenderThread == Thread.CurrentThread;

    public static Matrix4 CurrentTransformationMatrix => GLHandler.CurrentTransformationMatrix;

    public static Mesh CreateMesh(int vertexCount, VertexAttribute[] vertexAttributes, (uint idx0, uint idx1, uint idx2)[] clockwiseTriangles) {
        return new Mesh(vertexCount, vertexAttributes, clockwiseTriangles);
    }

    /*internal static Mesh CreateDefaultMesh() {  // TODO maybe cache default mesh, but then one could modify its values
        VertexAttribute va_position = new VertexAttribute("position", 3);
        VertexAttribute va_color = new VertexAttribute("color", 3);
        VertexAttribute va_texCoords = new VertexAttribute("mainTexture_coords", 2);
        VertexAttribute[] vertexAtributes = { va_position, va_color, va_texCoords };

        (uint idx0, uint idx1, uint idx2)[] indices = {
                (0, 1, 2),
                (2, 1, 3)
            };

        float[][] textureCoordinates = Texture2D.GetDefaultTextureCoordinates();

        Mesh mesh = CreateMesh(4, vertexAtributes, indices);
        int i = 0;
        for (int y = 0; y < 2; y++) {
            for (int x = 0; x < 2; x++) {
                VertexData va = mesh.GetVertexData(i);
                va.SetAttributeData(va_position, -0.5f + x, -0.5f + y, 0);
                va.SetAttributeData(va_color, Color4.White.ToArray(false));
                va.SetAttributeData(va_texCoords, textureCoordinates[x + y * 2]);
                i++;
            }
        }

        return mesh;
    }*/

    public static TextureAtlas CreateTextureAtlas(ImageResult image, IReadOnlyDictionary<string, Box2i> subTextureBounds) {
        int textureId = ExecuteGLFunction(() => {
            GLHandler.InitializeTexture(image, out int texId);
            return texId;
        });

        return new TextureAtlas(image, textureId, subTextureBounds);
    }

    public static TextureAtlas CreateDefaultTextureAtlas() {  // TODO maybe cache default texture, but then one could modify its render settings
        ImageResult image = new ImageResult() {
            Comp = ColorComponents.RedGreenBlueAlpha,
            SourceComp = ColorComponents.RedGreenBlueAlpha,
            Data = new byte[] { 255, 255, 255, 255 },
        };

        return CreateTextureAtlas(image, new Dictionary<string, Box2i>());
    }

    public static Texture2D CreateTexture(ImageResult image) {
        int textureId = ExecuteGLFunction(() => {
            GLHandler.InitializeTexture(image, out int texId);
            return texId;
        });

        return new Texture2D(image, textureId);
    }

    public static Texture2D CreateDefaultTexture() {  // TODO maybe cache default texture, but then one could modify its render settings
        ImageResult image = new ImageResult() {
            Comp = ColorComponents.RedGreenBlueAlpha,
            SourceComp = ColorComponents.RedGreenBlueAlpha,
            Data = new byte[] { 255, 255, 255, 255 },
        };

        return CreateTexture(image);
    }

    internal static void DeleteTexture(Texture texture) {
        ExecuteGLFunction(() => {
            GLHandler.DeleteTexture(texture);
        });
    }

    public static Shader CreateShader(string vertexShaderSource, string fragmentShaderSource) {
        return ExecuteGLFunction(() => {
            return GLHandler.CreateShader(vertexShaderSource, fragmentShaderSource);
        });
    }

    internal static void DisposeShader(Shader shader) {
        ExecuteGLFunction(() => {
            GLHandler.DeleteShader(shader);
        });
    }

    public static VertexArrayObject CreateVertexArrayObject(ShaderVertexAttributeResolver attributeResolver, ShaderUniformAssignmentHandler shaderUniformAssignmentHandler, Shader shader, Mesh mesh, params Texture[] textures) {
        int vaoId = ExecuteGLFunction(GLHandler.CreateVao);

        return new VertexArrayObject(vaoId, attributeResolver, shaderUniformAssignmentHandler, shader, mesh, textures);
    }

    /*internal static VertexArrayObject CreateRenderable(ShaderVertexAttributeResolver attributeResolver, ShaderUniformAssignmentHandler shaderUniformAssignmentHandler) {
        Shader shader = CreateDefaultShader(true, 1);
        Mesh mesh = CreateDefaultMesh();
        Texture2D[] texture = { CreateDefaultTexture() };

        return CreateRenderable(attributeResolver, shaderUniformAssignmentHandler, shader, mesh, texture);
    }*/

    internal static void DisposeVertexArrayObject(VertexArrayObject renderable) {
        ExecuteGLFunction(() => {
            GLHandler.DeleteEbo(renderable.ElementBufferObject);
            GLHandler.DeleteVbo(renderable.VertexBufferObject);
            GLHandler.DeleteVao(renderable);
        });
    }

    internal static Shader CreateDefaultShader(bool createMatrixUniforms, int textureCount) {
        (string vS, string fS) = ExecuteGLFunction(() => {
            string vS, fS;
            if (textureCount == 0)
                CreateUntexturedPassthroughShader(createMatrixUniforms, out vS, out fS);
            else
                CreateTexturedPassthroughShader(createMatrixUniforms, textureCount, out vS, out fS);

            return (vS, fS);
        });

        return CreateShader(vS, fS);
    }

    public const string DEFAULT_SHADER_VIEWPROJECTIONMATRIX_UNIFORM_NAME = "u_viewProjectionMatrix";
    public const string DEFAULT_SHADER_MODELMATRIX_UNIFORM_NAME = "u_modelMatrix";

    public static void CreateUntexturedPassthroughShader(bool createMatrixUniforms, out string vertexShaderSource, out string fragmentShaderSource) {
        StringBuilder sb_vert = new StringBuilder();
        sb_vert.Append("#version 330 core\n\n");
        if (createMatrixUniforms) {
            sb_vert.Append("uniform mat4 " + DEFAULT_SHADER_VIEWPROJECTIONMATRIX_UNIFORM_NAME + " = mat4(1.0);\n");
            sb_vert.Append("uniform mat4 " + DEFAULT_SHADER_MODELMATRIX_UNIFORM_NAME + " = mat4(1.0);\n\n");
        }
        sb_vert.Append("layout(location = 0) in vec3 in_position;\n");
        sb_vert.Append("layout(location = 1) in vec4 in_color;\n\n");
        sb_vert.Append("out vec4 v_worldPos;\n");
        sb_vert.Append("out vec4 v_color;\n\n");
        sb_vert.Append("void main() {\n");
        sb_vert.Append("\tgl_Position = ");
        if (createMatrixUniforms) {
            sb_vert.Append(DEFAULT_SHADER_VIEWPROJECTIONMATRIX_UNIFORM_NAME + " * " + DEFAULT_SHADER_MODELMATRIX_UNIFORM_NAME + " * ");
        }
        sb_vert.Append("vec4(in_position, 1.0);\n");
        sb_vert.Append("\tv_color = in_color;\n");
        sb_vert.Append("\tv_worldPos = ");
        if (createMatrixUniforms) {
            sb_vert.Append(DEFAULT_SHADER_MODELMATRIX_UNIFORM_NAME + " * ");
        }
        sb_vert.Append("vec4(in_position, 1.0);\n");
        sb_vert.Append("}\n");

        StringBuilder sb_frag = new StringBuilder();
        sb_frag.Append("#version 330 core\n\n");
        sb_frag.Append("in vec4 v_worldPos;\n");
        sb_frag.Append("in vec4 v_color;\n\n");
        sb_frag.Append("layout(location = 0) out vec4 out_color;\n\n");
        sb_frag.Append("void main() {\n");
        sb_frag.Append("\tout_color = v_color;\n");
        sb_frag.Append("}\n");

        vertexShaderSource = sb_vert.ToString();
        fragmentShaderSource = sb_frag.ToString();
    }

    public static void CreateTexturedPassthroughShader(bool createMatrixUniforms, int textureCount, out string vertexShaderSource, out string fragmentShaderSource) {
        int maxTU = GLHandler.SupportedTextureUnits;
        if (textureCount < 0 || textureCount > maxTU)
            throw new ArgumentException(textureCount + " Texture Units are not supported. " + maxTU + " are maximal supported.");

        if (textureCount == 0) {
            CreateUntexturedPassthroughShader(createMatrixUniforms, out vertexShaderSource, out fragmentShaderSource);
            return;
        }

        StringBuilder sb_vert = new StringBuilder();
        sb_vert.Append("#version 330 core\n\n");
        if (createMatrixUniforms) {
            sb_vert.Append($"uniform mat4 {DEFAULT_SHADER_VIEWPROJECTIONMATRIX_UNIFORM_NAME} = mat4(1.0);\n");
            sb_vert.Append($"uniform mat4 {DEFAULT_SHADER_MODELMATRIX_UNIFORM_NAME} = mat4(1.0);\n\n");
        }
        int layoutLocation = 0;
        sb_vert.Append($"layout(location = {layoutLocation++}) in vec3 in_position;\n");
        sb_vert.Append($"layout(location = {layoutLocation++}) in vec4 in_color;\n");
        for (int i = 0; i < textureCount; i++) {
            sb_vert.Append($"layout(location = {layoutLocation++}) in vec2 in_texCoords{i};\n");
        }
        sb_vert.Append("\n");
        sb_vert.Append("out vec4 v_worldPos;\n");
        sb_vert.Append("out vec4 v_color;\n");
        for (int i = 0; i < textureCount; i++) {
            sb_vert.Append($"out vec2 v_texCoords{i};\n");
        }
        sb_vert.Append("\n");
        sb_vert.Append("void main() {\n");
        sb_vert.Append("\tvec4 pos = vec4(in_position, 1.0);\n");
        sb_vert.Append("\tgl_Position = ");
        if (createMatrixUniforms) {
            sb_vert.Append($"{DEFAULT_SHADER_VIEWPROJECTIONMATRIX_UNIFORM_NAME} * {DEFAULT_SHADER_MODELMATRIX_UNIFORM_NAME} * ");
        }
        sb_vert.Append("pos;\n");
        sb_vert.Append("\tv_color = in_color;\n");
        sb_vert.Append("\tv_worldPos = ");
        if (createMatrixUniforms) {
            sb_vert.Append($"{DEFAULT_SHADER_MODELMATRIX_UNIFORM_NAME} * ");
        }
        sb_vert.Append("pos;\n");
        for (int i = 0; i < textureCount; i++) {
            sb_vert.Append($"\tv_texCoords{i} = in_texCoords{i};\n");
        }
        sb_vert.Append("}\n");

        layoutLocation = 0;
        StringBuilder sb_frag = new StringBuilder();
        sb_frag.Append("#version 330 core\n\n");
        for (int i = 0; i < textureCount; i++) {
            sb_frag.Append($"uniform sampler2D u_texture{i};\n");
        }
        sb_frag.Append("\n");
        sb_frag.Append("in vec4 v_worldPos;\n");
        sb_frag.Append("in vec4 v_color;\n");
        for (int i = 0; i < textureCount; i++) {
            sb_frag.Append($"in vec2 v_texCoords{i};\n");
        }
        sb_frag.Append("\n");
        sb_frag.Append($"layout(location = {layoutLocation++}) out vec4 out_color;\n\n");
        sb_frag.Append("void main() {\n");
        sb_frag.Append("\tout_color = v_color");
        for (int i = 0; i < textureCount; i++) {
            sb_frag.Append($" * texture(u_texture{i}, v_texCoords{i})");
        }
        sb_frag.Append(";\n");
        sb_frag.Append("}\n");

        vertexShaderSource = sb_vert.ToString();
        fragmentShaderSource = sb_frag.ToString();
    }

    private static T ExecuteGLFunction<T>(Func<T> func) {
        if (IsGLThread())
            return func();

        Task<T> task = new Task<T>(func);
        GLHandler.Queue(task);
        task.Wait();
        return task.Result;
    }

    private static void ExecuteGLFunction(Action action) {
        if (IsGLThread())
            action();

        Task task = new Task(action);
        GLHandler.Queue(task);
        task.Wait();
    }
}