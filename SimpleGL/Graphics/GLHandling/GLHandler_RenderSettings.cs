using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SimpleGL.Graphics.GLHandling;
public static partial class GLHandler {

    public static eBlendMode BlendMode {
        set => BlendFunctions = GraphicUtils.ModeToFunctions(value);
    }

    public static (eBlendFunction source, eBlendFunction destination)? BlendFunctions {
        get {
            if (blendFunctions == null)
                return null;

            eBlendFunction s = blendFunctions.Value.source;
            eBlendFunction d = blendFunctions.Value.destination;

            return (s, d);
        }
        set {
            blendFunctions = value;

            EnableBlending = value != null;

            if (BlendFunctions != null) {
                (BlendingFactor source, BlendingFactor destination) fs = GraphicUtils.ToBlendFunctions(BlendFunctions.Value);
                GL.BlendFunc(fs.source, fs.destination);
            }
        }
    }

    public static eDepthFunction? DepthFunction {
        get => depthFunction;
        set {
            depthFunction = value;

            EnableDepthTest = value != null;

            if (DepthFunction != null)
                GL.DepthFunc(GraphicUtils.ToDepthFunctions(DepthFunction.Value));
        }
    }

    public static eAntiAliasMode AntiAliasMode {
        get => antiAliasMode;
        set {
            antiAliasMode = value;

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
