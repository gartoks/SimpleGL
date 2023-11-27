using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SimpleGL.Graphics.GLHandling;
public static partial class GLHandler {
    public static Box2i Viewport {
        get {
            if (!_Viewport.HasValue) {
                int[] viewport = new int[4];
                GL.GetInteger(GetPName.Viewport, viewport);

                int x = viewport[0];
                int y = viewport[1];
                int width = viewport[2];
                int height = viewport[3];
                _Viewport = new Box2i(x, y, width, height);
            }

            return _Viewport.Value;
        }
        internal set {
            GL.Viewport(value.Min.X, value.Min.Y, value.Size.X, value.Size.Y);
            _Viewport = value;
        }
    }

    public static eBlendMode BlendMode {
        set => BlendFunctions = GraphicUtils.ModeToFunctions(value);
    }

    public static (eBlendFunction source, eBlendFunction destination)? BlendFunctions {
        get {
            if (_BlendFunctions == null)
                return null;

            eBlendFunction s = _BlendFunctions.Value.source;
            eBlendFunction d = _BlendFunctions.Value.destination;

            return (s, d);
        }
        set {
            _BlendFunctions = value;

            EnableBlending = value != null;

            if (BlendFunctions != null) {
                (BlendingFactor source, BlendingFactor destination) fs = GraphicUtils.ToBlendFunctions(BlendFunctions.Value);
                GL.BlendFunc(fs.source, fs.destination);
            }
        }
    }

    public static eDepthFunction? DepthFunction {
        get => _DepthFunction;
        set {
            _DepthFunction = value;

            EnableDepthTest = value != null;

            if (DepthFunction != null)
                GL.DepthFunc(GraphicUtils.ToDepthFunctions(DepthFunction.Value));
        }
    }

    public static eAntiAliasMode AntiAliasMode {
        get => _AntiAliasMode;
        set {
            _AntiAliasMode = value;

            GL.Hint(HintTarget.PointSmoothHint, GraphicUtils.ToHint(value));
            GL.Hint(HintTarget.LineSmoothHint, GraphicUtils.ToHint(value));
            GL.Hint(HintTarget.PolygonSmoothHint, GraphicUtils.ToHint(value));
        }
    }

    public static Color4 ClearColor {
        get => _ClearColor;
        set {
            _ClearColor = value;
            GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
        }
    }

    public static void SetClearModes(bool color, bool depth, bool stencil) {
        ClearBufferMask cc = color ? ClearBufferMask.ColorBufferBit : 0;
        ClearBufferMask cd = depth ? ClearBufferMask.DepthBufferBit : 0;
        ClearBufferMask cs = stencil ? ClearBufferMask.StencilBufferBit : 0;

        _ClearBufferMask = cc | cd | cs;
    }

    public static bool EnableScissorTest {
        set {
            if (value)
                GL.Enable(EnableCap.ScissorTest);
            else
                GL.Disable(EnableCap.ScissorTest);
        }
    }

    public static Box2i ScissorBox {
        set => GL.Scissor(value.Min.X, value.Min.Y, value.Size.X, value.Size.Y);
    }

    public static bool EnableCulling {
        set {
            if (value)
                GL.Enable(EnableCap.CullFace);
            else
                GL.Disable(EnableCap.CullFace);
        }
    }

    public static bool ClockwiseCulling {
        set => GL.FrontFace(value ? FrontFaceDirection.Cw : FrontFaceDirection.Ccw);
    }

    public static void CullFaces(bool front, bool back) {
        if (front && back)
            GL.CullFace(CullFaceMode.FrontAndBack);
        else if (front)
            GL.CullFace(CullFaceMode.Front);
        else if (back)
            GL.CullFace(CullFaceMode.Back);
    }

    public static bool EnableBlending {
        set {
            if (value)
                GL.Enable(EnableCap.Blend);
            else
                GL.Disable(EnableCap.Blend);
        }
    }

    public static bool EnableDepthTest {
        set {
            if (value)
                GL.Enable(EnableCap.DepthTest);
            else
                GL.Disable(EnableCap.DepthTest);
        }
    }

    public static bool EnableEdgeAntialiasing {
        set {
            if (value) {
                GL.Enable(EnableCap.PointSprite);   // TOOD test if right one
                GL.Enable(EnableCap.LineSmooth);
                GL.Enable(EnableCap.PolygonSmooth);
            } else {
                GL.Disable(EnableCap.PointSprite);   // TOOD test if right one
                GL.Disable(EnableCap.LineSmooth);
                GL.Disable(EnableCap.PolygonSmooth);
            }
        }
    }

    internal static void EnableColors(bool red, bool green, bool blue, bool alpha) {
        GL.ColorMask(red, green, blue, alpha);
    }

    //internal static void EnableLighting(bool enable) {
    //    if (enable)
    //        GL.Enable(GL_LIGHTING);
    //    else
    //        GL.Disable(GL_LIGHTING);
    //}
}
