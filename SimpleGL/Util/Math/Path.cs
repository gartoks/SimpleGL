using OpenTK.Mathematics;
using System.Collections;

namespace SimpleGL.Util.Math;
public sealed class Path : IReadOnlyList<Vector2> {
    public IReadOnlyList<Vector2> Points { get; }
    public int Count => Points.Count;

    private bool? _isClockwise { get; set; }
    public bool IsClockwise {
        get {
            if (!_isClockwise.HasValue) {
                float sum = 0;
                for (int i = 0; i < Points.Count; i++) {
                    Vector2 p0 = Points[i];
                    Vector2 p1 = Points[(i + 1) % Points.Count];

                    sum += (p1.X - p0.X) * (p1.Y + p0.Y);
                }

                _isClockwise = sum > 0;
            }

            return _isClockwise.Value;
        }
    }

    public Path(params Vector2[] points)
        : this((IEnumerable<Vector2>)points) {
    }

    public Path(IEnumerable<Vector2> points) {
        Points = new List<Vector2>(points);
    }

    public Vector2 this[int index] => Points[index];

    public Path Reverse() => new Path(Points.Reverse());

    public IEnumerator<Vector2> GetEnumerator() => Points.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
