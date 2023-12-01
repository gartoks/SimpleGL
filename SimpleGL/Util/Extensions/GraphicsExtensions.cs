using OpenTK.Mathematics;

namespace SimpleGL.Util.Extensions;
public static class GraphicsExtensions {

    public static float[] ToArray(this Color4 color, bool includeAlpha) => includeAlpha ? new[] { color.R, color.G, color.B, color.A } : new[] { color.R, color.G, color.B };

    public static float[][] ToArray(this Box2 box) => new float[][] {
        new float[] { box.Min.X, box.Min.Y },
        new float[] { box.Min.X, box.Max.Y },
        new float[] { box.Max.X, box.Min.Y },
        new float[] { box.Max.X, box.Max.Y } };
}
