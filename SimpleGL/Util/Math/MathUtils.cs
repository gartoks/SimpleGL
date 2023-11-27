using OpenTK.Mathematics;

namespace SimpleGL.Util.Math;
public static class MathUtils {

    /*public static bool IsPointInPolygon(IReadOnlyList<System.Numerics.Vector2> vertices, System.Numerics.Vector2 point) {
        bool result = false;
        int j = vertices.Count - 1;
        for (int i = 0; i < vertices.Count; i++) {
            if (vertices[i].Y < point.Y && vertices[j].Y >= point.Y || vertices[j].Y < point.Y && vertices[i].Y >= point.Y) {
                if (vertices[i].X + (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) * (vertices[j].X - vertices[i].X) < point.X) {
                    result = !result;
                }
            }
            j = i;
        }
        return result;
    }

    public static bool DoLinesIntersect(System.Numerics.Vector2 p0, System.Numerics.Vector2 p1, System.Numerics.Vector2 p2, System.Numerics.Vector2 p3, out System.Numerics.Vector2 intersectionPoint) {
        System.Numerics.Vector2 s1 = p1 - p0;
        System.Numerics.Vector2 s2 = p3 - p2;

        float s, t;
        s = (-s1.Y * (p0.X - p2.X) + s1.X * (p0.Y - p2.Y)) / (-s2.X * s1.Y + s1.X * s2.Y);
        t = (s2.X * (p0.Y - p2.Y) - s2.Y * (p0.X - p2.X)) / (-s2.X * s1.Y + s1.X * s2.Y);


        if (s >= 0 && s <= 1 && t >= 0 && t <= 1) {
            intersectionPoint = p0 + (t * s1);
            return true;
        } else {
            intersectionPoint = System.Numerics.Vector2.Zero;
            return false;
        }
    }

    public static float AreaSign(System.Numerics.Vector2 p1, System.Numerics.Vector2 p2, System.Numerics.Vector2 p3) {
        return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
    }*/

    public static float ToDeg(this float rad) {
        return rad * 180 / MathF.PI;
    }

    public static float ToRad(this float deg) {
        return deg * MathF.PI / 180;
    }

    public static Vector2 Remap(this Vector2 value, Vector2 from1, Vector2 to1, Vector2 from2, Vector2 to2) {
        return Lerp(to2, from2, (value - from1) / (to1 - from1));
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, Vector2 t) {
        return a + (b - a) * t;
    }

    public static float Remap(this float value, float from1, float to1, float from2, float to2) {
        return Lerp(to2, from2, (value - from1) / (to1 - from1));
    }

    public static float Lerp(float a, float b, float t) {
        return a + (b - a) * t;
    }

    public static bool IsPointInPolygon(IReadOnlyList<OpenTK.Mathematics.Vector2> vertices, OpenTK.Mathematics.Vector2 point) {
        bool result = false;
        int j = vertices.Count - 1;
        for (int i = 0; i < vertices.Count; i++) {
            if (vertices[i].Y < point.Y && vertices[j].Y >= point.Y || vertices[j].Y < point.Y && vertices[i].Y >= point.Y) {
                if (vertices[i].X + (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) * (vertices[j].X - vertices[i].X) < point.X) {
                    result = !result;
                }
            }
            j = i;
        }
        return result;
    }

    public static bool DoLinesIntersect(OpenTK.Mathematics.Vector2 p0, OpenTK.Mathematics.Vector2 p1, OpenTK.Mathematics.Vector2 p2, OpenTK.Mathematics.Vector2 p3, out OpenTK.Mathematics.Vector2 intersectionPoint) {
        OpenTK.Mathematics.Vector2 s1 = p1 - p0;
        OpenTK.Mathematics.Vector2 s2 = p3 - p2;

        float s, t;
        s = (-s1.Y * (p0.X - p2.X) + s1.X * (p0.Y - p2.Y)) / (-s2.X * s1.Y + s1.X * s2.Y);
        t = (s2.X * (p0.Y - p2.Y) - s2.Y * (p0.X - p2.X)) / (-s2.X * s1.Y + s1.X * s2.Y);


        if (s >= 0 && s <= 1 && t >= 0 && t <= 1) {
            intersectionPoint = p0 + (t * s1);
            return true;
        } else {
            intersectionPoint = OpenTK.Mathematics.Vector2.Zero;
            return false;
        }
    }

    public static float AreaSign(OpenTK.Mathematics.Vector2 p1, OpenTK.Mathematics.Vector2 p2, OpenTK.Mathematics.Vector2 p3) {
        return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
    }
}
