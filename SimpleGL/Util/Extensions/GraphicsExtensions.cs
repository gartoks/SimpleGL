using OpenTK.Mathematics;

namespace SimpleGL.Util.Extensions;
public static class GraphicsExtensions {

    public static float[] ToArray(this Color4 color, bool includeAlpha) => includeAlpha ? new[] { color.R, color.G, color.B, color.A } : new[] { color.R, color.G, color.B };

}
