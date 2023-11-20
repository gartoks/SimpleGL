namespace SimpleGL.Graphics;
public class VertexAttribute : IEquatable<VertexAttribute?> {
    public static VertexAttribute Create(string name, int componentCount) => new VertexAttribute(name, componentCount);

    public string Name { get; }
    public int ComponentCount { get; }

    internal VertexAttribute(string name, int componentCount) {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("VertexAttribute name must not be null or empty.", nameof(name));

        if (componentCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(componentCount), componentCount, "VertexAttribute component count must be bigger than zero.");

        Name = name;
        ComponentCount = componentCount;
    }

    public override string ToString() => $"{Name}:{ComponentCount}";

    public override bool Equals(object? obj) => Equals(obj as VertexAttribute);
    public bool Equals(VertexAttribute? other) => other is not null && Name == other.Name && ComponentCount == other.ComponentCount;
    public override int GetHashCode() => HashCode.Combine(Name, ComponentCount);

    public static bool operator ==(VertexAttribute? left, VertexAttribute? right) => EqualityComparer<VertexAttribute>.Default.Equals(left, right);
    public static bool operator !=(VertexAttribute? left, VertexAttribute? right) => !(left == right);
}
