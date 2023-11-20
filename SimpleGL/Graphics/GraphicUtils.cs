using OpenTK.Graphics.OpenGL4;

namespace SimpleGL.Graphics;
public enum eTextureFilterMode { Nearest, Linear }
public enum eTextureWrapMode { Repeat, Clamp }
public enum eAntiAliasMode { DontCare, Fastest, Nicest }
public enum eBlendFunction { Zero, One, SourceAlpha, OneMinusSourceAlpha, SourceColor, OneMinusSourceColor, DestinationAlpha, OneMinusDestinationAlpha, DestinationColor, OneMinusDestinationColor }
public enum eBlendMode { None, Default, Replace, Additive, Overlay, Premultiplied, }
public enum eDepthFunction { Always, Equal, Gequal, Greater, Lequal, Less, Never, Notequal }
public enum eBufferType { Static, Dynamic }

internal static class GraphicUtils {
    internal static TextureWrapMode ToWrapMode(eTextureWrapMode wrapMode) {
        return wrapMode switch {
            eTextureWrapMode.Repeat => TextureWrapMode.Repeat,
            eTextureWrapMode.Clamp => TextureWrapMode.ClampToBorder,
            _ => throw new ArgumentException(),
        };
    }

    internal static TextureMinFilter ToMinFilter(eTextureFilterMode filterMode) {
        return filterMode switch {
            eTextureFilterMode.Nearest => TextureMinFilter.Nearest,
            eTextureFilterMode.Linear => TextureMinFilter.Linear,
            _ => throw new ArgumentException(),
        };
    }

    internal static TextureMagFilter ToMagFilter(eTextureFilterMode filterMode) {
        return filterMode switch {
            eTextureFilterMode.Nearest => TextureMagFilter.Nearest,
            eTextureFilterMode.Linear => TextureMagFilter.Linear,
            _ => throw new ArgumentException(),
        };
    }

    internal static HintMode ToHint(eAntiAliasMode aaMode) {
        return aaMode switch {
            eAntiAliasMode.DontCare => HintMode.DontCare,
            eAntiAliasMode.Fastest => HintMode.Fastest,
            eAntiAliasMode.Nicest => HintMode.Nicest,
            _ => throw new ArgumentException(),
        };
    }

    internal static (eBlendFunction source, eBlendFunction destination)? ModeToFunctions(eBlendMode blendMode) {
        return blendMode switch {
            eBlendMode.Default => ((eBlendFunction source, eBlendFunction destination)?)(eBlendFunction.SourceAlpha, eBlendFunction.OneMinusSourceAlpha),
            eBlendMode.Replace => ((eBlendFunction source, eBlendFunction destination)?)(eBlendFunction.One, eBlendFunction.Zero),
            eBlendMode.Additive => ((eBlendFunction source, eBlendFunction destination)?)(eBlendFunction.SourceAlpha, eBlendFunction.One),
            eBlendMode.Overlay => ((eBlendFunction source, eBlendFunction destination)?)(eBlendFunction.DestinationColor, eBlendFunction.Zero),
            eBlendMode.Premultiplied => ((eBlendFunction source, eBlendFunction destination)?)(eBlendFunction.One, eBlendFunction.OneMinusSourceAlpha),
            _ => null,
        };
    }

    internal static (BlendingFactor source, BlendingFactor destination) ToBlendFunctions((eBlendFunction source, eBlendFunction destination) functions) {
        BlendingFactor source = functions.source switch {
            eBlendFunction.Zero => BlendingFactor.Zero,
            eBlendFunction.One => BlendingFactor.One,
            eBlendFunction.SourceAlpha => BlendingFactor.SrcAlpha,
            eBlendFunction.OneMinusSourceAlpha => BlendingFactor.OneMinusSrcAlpha,
            eBlendFunction.SourceColor => BlendingFactor.SrcColor,
            eBlendFunction.OneMinusSourceColor => BlendingFactor.OneMinusSrcColor,
            eBlendFunction.DestinationAlpha => BlendingFactor.DstAlpha,
            eBlendFunction.OneMinusDestinationAlpha => BlendingFactor.OneMinusDstAlpha,
            eBlendFunction.DestinationColor => BlendingFactor.DstColor,
            eBlendFunction.OneMinusDestinationColor => BlendingFactor.OneMinusDstColor,
            _ => throw new ArgumentException(),
        };
        BlendingFactor destination = functions.destination switch {
            eBlendFunction.Zero => BlendingFactor.Zero,
            eBlendFunction.One => BlendingFactor.One,
            eBlendFunction.SourceAlpha => BlendingFactor.SrcAlpha,
            eBlendFunction.OneMinusSourceAlpha => BlendingFactor.OneMinusSrcAlpha,
            eBlendFunction.SourceColor => BlendingFactor.SrcColor,
            eBlendFunction.OneMinusSourceColor => BlendingFactor.OneMinusSrcColor,
            eBlendFunction.DestinationAlpha => BlendingFactor.DstAlpha,
            eBlendFunction.OneMinusDestinationAlpha => BlendingFactor.OneMinusDstAlpha,
            eBlendFunction.DestinationColor => BlendingFactor.DstColor,
            eBlendFunction.OneMinusDestinationColor => BlendingFactor.OneMinusDstColor,
            _ => throw new ArgumentException(),
        };
        return (source, destination);
    }

    internal static DepthFunction ToDepthFunctions(eDepthFunction function) {
        return function switch {
            eDepthFunction.Always => DepthFunction.Always,
            eDepthFunction.Equal => DepthFunction.Equal,
            eDepthFunction.Gequal => DepthFunction.Gequal,
            eDepthFunction.Greater => DepthFunction.Greater,
            eDepthFunction.Lequal => DepthFunction.Lequal,
            eDepthFunction.Less => DepthFunction.Less,
            eDepthFunction.Never => DepthFunction.Never,
            eDepthFunction.Notequal => DepthFunction.Notequal,
            _ => throw new ArgumentException(),
        };
    }

    internal static BufferUsageHint ToBufferUsageHint(eBufferType bufferType) {
        return bufferType switch {
            eBufferType.Dynamic => BufferUsageHint.DynamicDraw,
            eBufferType.Static => BufferUsageHint.StaticDraw,
            _ => throw new ArgumentException(),
        };
    }

    internal static int ActiveAttribTypeToSize(ActiveAttribType type) {
        string typeString = type.ToString();

        if (typeString.EndsWith("Vec2"))
            return 2;

        if (typeString.EndsWith("Vec3"))
            return 3;

        if (typeString.EndsWith("Vec4"))
            return 4;

        return 1;
    }

    internal static int ActiveUniformTypeToSize(ActiveUniformType type) {
        string typeString = type.ToString();

        if (typeString.EndsWith("Vec2"))
            return 2;

        if (typeString.EndsWith("Vec3"))
            return 3;

        if (typeString.EndsWith("Vec4"))
            return 4;

        return 1;
    }
}