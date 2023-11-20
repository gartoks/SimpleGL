using System.Numerics;

namespace SimpleGL.Util;
internal struct Rectangle {

    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public Rectangle(float x, float y, float width, float height) {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Contains(Vector2 v) => Contains(v.X, v.Y);
    public bool Contains(float x, float y) => x >= X && x <= X + Width && y >= Y && y <= Y + Height;
}