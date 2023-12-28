using OpenTK.Mathematics;

namespace SimpleGL.Util.Extensions;
public static class BoxExtensions {

    public static Box2 FromMinAndSize(Vector2 min, Vector2 size) => new(min, min + size);
    public static Box2 FromMinAndSize(float minX, float minY, float width, float height) => new(minX, minY, minX + width, minY + height);

    internal static Box2 FromPoints(float l, float t, float r, float b) {
        return new Box2(new Vector2(l, t), new Vector2(r, b));
    }

    public static Vector2 MinXMaxY(this Box2 box) => new(box.Min.X, box.Max.Y);
    public static Vector2 MaxXMinY(this Box2 box) => new(box.Max.X, box.Min.Y);
}
