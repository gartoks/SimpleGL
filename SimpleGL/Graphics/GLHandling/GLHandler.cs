using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleGL.Graphics.Textures;

namespace SimpleGL.Graphics.GLHandling;


public static partial class GLHandler {   // TODO stencil
    internal static Thread RenderThread { get; private set; }

    private static Color4 _ClearColor { get; set; }
    private static ClearBufferMask _ClearBufferMask { get; set; }

    public static bool IsRendering { get; private set; }
    //private static Renderer renderer;

    //private static Stack<Matrix4> TransformStack { get; }

    private static Texture?[] AssignedTextures { get; set; }
    private static Dictionary<Texture, int> AssignedTextureUnits { get; }

    private static Shader? BoundShader { get; set; }

    private static VertexArrayObject? BoundVertexArrayObject { get; set; }
    private static VertexBufferObject? BoundVertexBufferObject { get; set; }
    private static ElementBufferObject? BoundElementBufferObject { get; set; }

    private static int _SupportedTextureUnits { get; set; }

    private static (eBlendFunction source, eBlendFunction destination)? _BlendFunctions { get; set; }
    private static eDepthFunction? _DepthFunction { get; set; }
    private static eAntiAliasMode _AntiAliasMode { get; set; }
    private static Box2i? _Viewport { get; set; }

    private static Queue<Task> GlTaskQueue { get; set; }
    private static Queue<Task> GlTaskQueue_swap { get; set; }

    //internal static Matrix4 CurrentTransformationMatrix => TransformStack.Any() ? TransformStack.Peek() : Matrix4.Identity;

    static GLHandler() {
        AssignedTextureUnits = new Dictionary<Texture, int>();
        //TransformStack = new Stack<Matrix4>();

        GlTaskQueue = new Queue<Task>();
        GlTaskQueue_swap = new Queue<Task>();


        IsRendering = false;
        //renderer = null;

        //ActiveTextureUnit = -1;
        //boundTextures = null;
        //availableTextureUnits = -1;
        _SupportedTextureUnits = -1;
    }

    internal static void Initialize() {
        RenderThread = Thread.CurrentThread;

        EnableBlending = true;
        BlendMode = eBlendMode.Default;

        EnableDepthTest = false;
        DepthFunction = eDepthFunction.Less;

        //EnableColors(true, true, true, true);

        ////EnableEdgeAntialiasing = true;
        ////AntiAliasMode = AntiAliasMode.Fastest;

        //EnableCulling = true;
        //ClockwiseCulling = true;
        //CullFaces(false, true);

        ClearColor = Color4.DimGray;
        SetClearModes(true, true, false);

        AssignedTextures = new Texture[SupportedTextureUnits];
    }

    internal static void Queue(Task glTask) {
        lock (GlTaskQueue) {
            GlTaskQueue.Enqueue(glTask);
        }
    }

    internal static void BeginRendering() {
        //if (_SupportedTextureUnits == -1)
        //    _SupportedTextureUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);

        lock (GlTaskQueue) {
            Queue<Task> tmp = GlTaskQueue;
            GlTaskQueue = GlTaskQueue_swap;
            GlTaskQueue_swap = tmp;
        }

        foreach (Task glTask in GlTaskQueue_swap)
            glTask.RunSynchronously();
        GlTaskQueue_swap.Clear();

        //TransformStack.Clear();
        //GL.ClearDepth(1f);  // TODO maybe allow different values
        GL.Clear(_ClearBufferMask);

        IsRendering = true;
    }

    internal static void EndRendering() {
        IsRendering = false;

        if (BoundShader != null)
            ReleaseShader(BoundShader);

        for (int i = 0; i < AssignedTextures.Length; i++) {
            if (AssignedTextures[i] != null)
                UnassignTextureUnit(i);
        }

        //TransformStack.Clear();
    }

    internal static void Render(ElementBufferObject ebo) {
        BindEbo(ebo);
        GL.DrawElements(PrimitiveType.Triangles, ebo.Size, DrawElementsType.UnsignedInt, 0);
        ReleaseEbo(ebo);
    }

    #region Transforms
    /*internal static void PushTransform() {
        if (!IsRendering)
            return;

        if (TransformStack.Any())
            TransformStack.Push(TransformStack.Peek());
        else
            TransformStack.Push(Matrix4.Identity);
    }

    internal static void ApplyTransformation(Matrix4 transformationMatrix) {
        if (!IsRendering)
            return;

        if (!TransformStack.Any())
            throw new InvalidOperationException("Cannot apply transformation to empty transform stack.");

        Matrix4 m = transformationMatrix * TransformStack.Pop();
        TransformStack.Push(m);
    }

    internal static void ApplyTranslation(float dx, float dy, float dz = 0) {
        ApplyTransformation(Matrix4.CreateTranslation(dx, dy, dz));
    }

    internal static void ApplyRotation(float angle) {
        ApplyTransformation(Matrix4.CreateRotationZ(angle));
    }

    internal static void ApplyScaling(float sx, float sy) {
        ApplyTransformation(Matrix4.CreateScale(sx, sy, 1));
    }

    internal static void PopTransform() {
        if (!IsRendering)
            return;

        if (TransformStack.Count == 0)
            return;

        TransformStack.Pop();
    }*/

    #endregion
}
