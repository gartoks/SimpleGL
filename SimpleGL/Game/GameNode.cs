using SimpleGL.Game.Util;

namespace SimpleGL.Game;
public class GameNode : IEquatable<GameNode?> {
    internal static GameNode CreateRoot(Guid? id = null) {
        GameNode root = new(id ?? Guid.NewGuid());
        root._Parent = null;
        return root;
    }

    public Guid Id { get; }

    public string Name { get; set; }

    public bool IsDestroyed { get; private set; }
    private bool _IsEnabled { get; set; }
    public bool IsEnabled => _IsEnabled && !IsDestroyed && (Parent == null || Parent.IsEnabled);

    private GameNode? _Parent { get; set; }
    public GameNode? Parent {
        get => _Parent;
        set {
            if (IsDestroyed)
                throw new InvalidOperationException("Cannot set parent of destroyed node.");

            if (value != null && value == _Parent)
                return;

            _Parent?._Children.Remove(this);

            if (value == null)
                value = GameScene.GameRoot;

            _Parent = value;
            _Parent?._Children.Add(this);

            //Transform.NotifyHierarchyChanged();
        }
    }

    private List<GameNode> _Children { get; }
    public IEnumerable<GameNode> Children => _Children;

    public Transform Transform { get; }

    public GameNode()
        : this(Guid.NewGuid()) {
    }

    public GameNode(Guid id) {
        Transform = new Transform(this);
        Id = id;

        _Children = new List<GameNode>();
        _IsEnabled = true;
        IsDestroyed = false;

        Parent = null;
    }

    internal void UpdateNode(float dT) {
        if (!IsEnabled)
            return;

        Update(dT);

        foreach (GameNode child in Children.ToList()) {
            if (child.IsDestroyed)
                _Children.Remove(child);
            else
                child.UpdateNode(dT);
        }
    }

    internal void RenderNode(float dT) {
        if (!IsEnabled)
            return;

        Render(dT);

        foreach (GameNode child in Children.ToList())
            child.RenderNode(dT);
    }

    public void Destroy() {
        IsDestroyed = true;
        _Parent = null;
        OnDestroy();

        foreach (GameNode child in Children.ToList())
            child.Destroy();
    }

    protected virtual void Update(float dT) { }

    protected virtual void Render(float dT) { }

    protected virtual void OnDestroy() { }

    internal GameNodeData SerializeNode() {
        GameNodeData serializedProperties = new();
        serializedProperties.Set(nameof(Id), Id.ToString());
        serializedProperties.Set("Type", GetType().AssemblyQualifiedName!);

        GameNodeData baseDict = new();
        baseDict.Set(nameof(Name), Name);
        baseDict.Set(nameof(IsEnabled), _IsEnabled.ToString());

        if (_Parent == null)
            throw new InvalidOperationException("Cannot serialize root node.");
        baseDict.Set(nameof(Parent), Parent == GameScene.GameRoot ? Guid.Empty.ToString() : Parent!.Id.ToString());

        serializedProperties.Set(nameof(GameNode), baseDict);

        serializedProperties.Set(nameof(Transform), Transform.Serialize());

        if (typeof(GameNode) != GetType()) {
            GameNodeData data = SerializeNode();
            serializedProperties.Set(GetType().Name, data);
        }
        return serializedProperties;
    }

    internal void DeserializeNode(IReadOnlyDictionary<Guid, GameNode> nodes, GameNodeData data) {
        Guid id = Guid.Parse(data.GetValue(nameof(Id)));
        if (id != Id)
            throw new InvalidOperationException("Cannot deserialize node with different id.");

        GameNodeData baseDict = data.GetData(nameof(GameNode));
        Name = baseDict.GetValue(nameof(Name));
        _IsEnabled = bool.Parse(baseDict.GetValue(nameof(IsEnabled)));

        Guid parentId = Guid.Parse(baseDict.GetValue(nameof(Parent)));

        if (parentId == Guid.Empty)
            Parent = GameScene.GameRoot;
        else
            Parent = nodes[parentId];

        Transform.Deserialize(data.GetData(nameof(Transform)));

        if (typeof(GameNode) != GetType()) {
            GameNodeData d = data.GetData(GetType().Name);
            Deserialize(d);
        }
    }

    protected virtual GameNodeData Serialize() => new();
    protected virtual void Deserialize(GameNodeData data) { }

    public override bool Equals(object? obj) {
        return Equals(obj as GameNode);
    }

    public bool Equals(GameNode? other) {
        return other is not null &&
               Id.Equals(other.Id);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Id);
    }

    public static bool operator ==(GameNode? left, GameNode? right) => EqualityComparer<GameNode>.Default.Equals(left, right);
    public static bool operator !=(GameNode? left, GameNode? right) => !(left == right);
}
