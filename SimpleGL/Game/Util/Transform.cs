
using OpenTK.Mathematics;

namespace SimpleGL.Game.Util;
public sealed class Transform {
    public GameNode GameNode { get; }

    public Transform? Parent => GameNode.Parent?.Transform;
    public IEnumerable<Transform> Children => GameNode.Children.Select(c => c.Transform);

    private Vector2 _Position { get; set; }
    public Vector2 Position {
        get => _Position;
        set {
            _Position = value;
            //IsMatrixDirty = true;

            //OnTransformChanged?.Invoke();
        }
    }

    private float _Rotation { get; set; }
    public float Rotation {
        get => _Rotation;
        set {
            _Rotation = value;
            //IsMatrixDirty = true;

            //OnTransformChanged?.Invoke();
        }
    }

    private Vector2 _Scale { get; set; }
    public Vector2 Scale {
        get => _Scale;
        set {
            _Scale = value;
            //IsMatrixDirty = true;

            //OnTransformChanged?.Invoke();
        }
    }

    private Vector2 _Pivot { get; set; }
    public Vector2 Pivot {
        get => _Pivot;
        set {
            _Pivot = value;
            //IsMatrixDirty = true;

            //OnTransformChanged?.Invoke();
        }
    }

    public int ZIndex { get; set; }

    //private Matrix4 _LocalTransformationMatrix { get; set; }
    public Matrix4 TransformationMatrix {
        get {
            //if (IsMatrixDirty) {
            //    _LocalTransformationMatrix = CalculateTransformationMatrix();
            //    IsMatrixDirty = false;
            //}

            return CalculateTransformationMatrix() * (Parent != null ? Parent.TransformationMatrix : Matrix4.Identity);
        }
    }

    //private bool IsMatrixDirty { get; set; }

    //public event Action? OnTransformChanged;

    public Transform(GameNode node) {
        GameNode = node;
        Position = Vector2.Zero;
        Rotation = 0f;
        Scale = Vector2.One;
        Pivot = new Vector2(0.5f, 0.5f);
    }

    /*internal void NotifyHierarchyChanged() {
        IsMatrixDirty = true;
        OnTransformChanged?.Invoke();
    }*/

    internal GameNodeData Serialize() {
        GameNodeData serializedProperties = new();
        serializedProperties.Set(nameof(Position), $"{Position.X},{Position.Y}");
        serializedProperties.Set(nameof(Rotation), Rotation.ToString());
        serializedProperties.Set(nameof(Scale), $"{Scale.X},{Scale.Y}");
        serializedProperties.Set(nameof(Pivot), $"{Pivot.X},{Pivot.Y}");
        serializedProperties.Set(nameof(ZIndex), ZIndex.ToString());

        return serializedProperties;
    }

    internal void Deserialize(GameNodeData data) {
        string positionString = data.GetValue(nameof(Position));
        string rotationString = data.GetValue(nameof(Rotation));
        string scaleString = data.GetValue(nameof(Scale));
        string pivotString = data.GetValue(nameof(Pivot));
        string zIndexString = data.GetValue(nameof(ZIndex));

        string[] positionSplit = positionString.Split(',');
        string[] scaleSplit = scaleString.Split(',');
        string[] pivotSplit = pivotString.Split(',');

        Position = new Vector2(float.Parse(positionSplit[0]), float.Parse(positionSplit[1]));
        Rotation = float.Parse(rotationString);
        Scale = new Vector2(float.Parse(scaleSplit[0]), float.Parse(scaleSplit[1]));
        Pivot = new Vector2(float.Parse(pivotSplit[0]), float.Parse(pivotSplit[1]));
        ZIndex = int.Parse(zIndexString);
    }

    private Matrix4 CalculateTransformationMatrix() {
        Matrix4 localMatrix = Matrix4.CreateTranslation(Pivot.X - 0.5f, Pivot.Y - 0.5f, 0) *
                              Matrix4.CreateScale(Scale.X, Scale.Y, 1) *
                              Matrix4.CreateRotationZ(Rotation) *
                              Matrix4.CreateTranslation(Position.X, Position.Y, 0);
        return localMatrix;
    }
}
