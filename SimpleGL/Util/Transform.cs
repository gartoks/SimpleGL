
using OpenTK.Mathematics;

namespace SimpleGL.Util;
public sealed class Transform {
    private Transform? _Parent { get; set; }
    public Transform? Parent {
        get => _Parent;
        set {
            _Parent = value;
            IsMatrixDirty = true;
        }
    }

    private Vector2 _Position { get; set; }
    public Vector2 Position {
        get => _Position;
        set {
            _Position = value;
            IsMatrixDirty = true;
        }
    }

    private float _Rotation { get; set; }
    public float Rotation {
        get => _Rotation;
        set {
            _Rotation = value;
            IsMatrixDirty = true;
        }
    }

    private Vector2 _Scale { get; set; }
    public Vector2 Scale {
        get => _Scale;
        set {
            _Scale = value;
            IsMatrixDirty = true;
        }
    }

    private Vector2 _Pivot { get; set; }
    public Vector2 Pivot {
        get => _Pivot;
        set {
            _Pivot = value;
            IsMatrixDirty = true;
        }
    }

    public int ZIndex { get; set; }

    private Matrix4 _LocalTransformationMatrix { get; set; }
    public Matrix4 TransformationMatrix {
        get {
            if (IsMatrixDirty) {
                _LocalTransformationMatrix = CalculateTransformationMatrix();
                IsMatrixDirty = false;
            }

            return _LocalTransformationMatrix * (Parent != null ? Parent.TransformationMatrix : Matrix4.Identity);
        }
    }

    private bool IsMatrixDirty { get; set; }

    public Transform() {
        Position = Vector2.Zero;
        Rotation = 0f;
        Scale = Vector2.One;
        Pivot = new Vector2(0.5f, 0.5f);
    }

    private Matrix4 CalculateTransformationMatrix() {
        Matrix4 localMatrix = Matrix4.CreateTranslation(Pivot.X - 0.5f, Pivot.Y - 0.5f, 0) *
                              Matrix4.CreateScale(Scale.X, Scale.Y, 1) *
                              Matrix4.CreateRotationZ(Rotation) *
                              Matrix4.CreateTranslation(Position.X, Position.Y, 0);
        return localMatrix;
    }
}
