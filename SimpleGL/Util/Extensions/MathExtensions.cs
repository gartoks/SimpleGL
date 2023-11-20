using OpenTK.Mathematics;

namespace SimpleGL.Util.Extensions;
public static class MathExtensions {
    internal static float[] ToArray(this Color4 c, bool includeAlpha) {
        return includeAlpha ? (new[] { c.R, c.G, c.B, c.A }) : (new[] { c.R, c.G, c.B });
    }
}
