﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleGL.Graphics.Textures;

namespace SimpleGL.Graphics.GLHandling;


public static partial class GLHandler {   // TODO stencil
    internal static Thread RenderThread { get; private set; }

    private static Color4 _ClearColor { get; set; }
    private static ClearBufferMask _ClearBufferMask { get; set; }

    public static bool IsRendering { get; private set; }
    //private static Renderer renderer;

    private static Stack<Matrix4> TransformStack { get; }

    private static Texture?[] AssignedTextures { get; set; }
    private static Dictionary<Texture, int> AssignedTextureUnits { get; }

    private static Shader? BoundShader { get; set; }

    private static VertexArrayObject? BoundVertexArrayObject { get; set; }
    private static VertexBufferObject? BoundVertexBufferObject { get; set; }
    private static ElementBufferObject? BoundElementBufferObject { get; set; }

    private static int _SupportedTextureUnits { get; set; }

    private static (eBlendFunction source, eBlendFunction destination)? blendFunctions;
    private static eDepthFunction? depthFunction;
    private static eAntiAliasMode antiAliasMode;

    private static Queue<Task> GlTaskQueue { get; set; }
    private static Queue<Task> GlTaskQueue_swap { get; set; }

    internal static Matrix4 CurrentTransformationMatrix => TransformStack.Any() ? TransformStack.Peek() : Matrix4.Identity;

    static GLHandler() {
        AssignedTextureUnits = new Dictionary<Texture, int>();
        TransformStack = new Stack<Matrix4>();

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
        if (_SupportedTextureUnits == -1)
            _SupportedTextureUnits = GL.GetInteger(GetPName.MaxTextureUnits);

        lock (GlTaskQueue) {
            Queue<Task> tmp = GlTaskQueue;
            GlTaskQueue = GlTaskQueue_swap;
            GlTaskQueue_swap = tmp;
        }

        foreach (Task glTask in GlTaskQueue_swap)
            glTask.RunSynchronously();
        GlTaskQueue_swap.Clear();

        TransformStack.Clear();
        //GL.ClearDepth(1f);  // TODO maybe allow different values
        GL.Clear(_ClearBufferMask);

        //Viewport viewport = Game.Instance.Viewport;
        //GL.MatrixMode(MatrixMode.Projection);
        //GL.LoadMatrix(viewport.ViewProjectionMatrix.ColumnMajor);

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

        //GL.Flush();

        //Window.Window.Instance.SwapBuffers();   // un-nice // TODO

        TransformStack.Clear();
    }

    internal static void Render(ElementBufferObject ebo) {
        BindEbo(ebo);
        GL.DrawElements(PrimitiveType.Triangles, ebo.Size, DrawElementsType.UnsignedInt, 0);
        ReleaseEbo(ebo);
    }

    #region Transforms
    /*internal static void ApplyTransformation(Transform transform) {
        // TODO
        if (!IsRendering)
            return;

        Matrix4 m = transform.LocalTransformationMatrix;
        if (transformStack.Any())
            m.MultiplyRight(transformStack.Peek());

        transformStack.Push(m);
    }*/

    internal static void ApplyTranslation(float dx, float dy, float dz = 0) {
        if (!IsRendering)
            return;

        Matrix4 m = Matrix4.CreateTranslation(dx, dy, 0);
        if (TransformStack.Any())
            m *= TransformStack.Peek(); // TODO check mult order

        TransformStack.Push(m);
    }

    internal static void ApplyRotation(float angle) {
        if (!IsRendering)
            return;

        Matrix4 m = Matrix4.CreateRotationZ(angle);
        if (TransformStack.Any())
            m *= TransformStack.Peek(); // TODO check mult order

        TransformStack.Push(m);
    }

    internal static void ApplyScaling(float sx, float sy) {
        if (!IsRendering)
            return;

        Matrix4 m = Matrix4.CreateScale(sx, sy, 1);
        if (TransformStack.Any())
            m *= TransformStack.Peek(); // TODO check mult order

        TransformStack.Push(m);
    }

    internal static void RevertTransform() {
        if (!IsRendering)
            return;

        if (TransformStack.Count == 0)
            return;

        TransformStack.Pop();
    }

    #endregion
}