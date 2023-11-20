using System.Numerics;

namespace SimpleGL.Util;
public struct RectangleI {
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public RectangleI(int x, int y, int width, int height) {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Contains(Vector2 v) => Contains(v.X, v.Y);
    public bool Contains(float x, float y) => x >= X && x <= X + Width && y >= Y && y <= Y + Height;
}