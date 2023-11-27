using OpenTK.Mathematics;

namespace SimpleGL.Util.Extensions;
public static class MathExtensions {
    public static System.Numerics.Vector2 Convert(this Vector2 value) => new System.Numerics.Vector2(value.X, value.Y);
    public static Vector2 Convert(this System.Numerics.Vector2 value) => new Vector2(value.X, value.Y);

    public static Vector2 Vec2(this Vector4 v) => new Vector2(v.X, v.Y);
    public static Vector4 Vec4(this Vector2 v) => new Vector4(v.X, v.Y, 0, 1);
}
