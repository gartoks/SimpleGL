namespace SimpleGL.Util.Extensions;
public static class MathExtensions {
    public static System.Numerics.Vector2 Convert(this OpenTK.Mathematics.Vector2 value) => new System.Numerics.Vector2(value.X, value.Y);
    public static OpenTK.Mathematics.Vector2 Convert(this System.Numerics.Vector2 value) => new OpenTK.Mathematics.Vector2(value.X, value.Y);
}
