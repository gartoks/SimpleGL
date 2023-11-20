using SimpleGL.Util;

namespace SimpleGL.Graphics;
public sealed class VertexData {
    public delegate void VertexDataChangedEventHandler(VertexData vertexData);

    public int InterleavedVertexDataLength { get; }
    public IReadOnlyDictionary<VertexAttribute, float[]> VertexAttributeData { get; }

    public event VertexDataChangedEventHandler? OnDataChanged;

    internal VertexData(VertexAttribute[] vertexattributes) {
        VertexAttributeData = new Dictionary<VertexAttribute, float[]>();

        InterleavedVertexDataLength = vertexattributes.Sum(va => va.ComponentCount);
        VertexAttributeData = vertexattributes.ToDictionary(va => va, va => new float[va.ComponentCount]);
    }

    public void SetAttributeData(string vertexAttributeName, params float[] data) {
        IEnumerable<VertexAttribute> vas = VertexAttributeData.Keys.Where(va => va.Name.Equals(vertexAttributeName));

        int matchingVAs = vas.Count();

        if (matchingVAs == 0) {
            Log.WriteLine($"Vertex attribute of name {vertexAttributeName} does not exist for this vertex.", eLogType.Error);
            return;
        }

        if (matchingVAs != 1) {
            Log.WriteLine($"Ambiguous attribute of name {vertexAttributeName}.", eLogType.Error);
            return;
        }

        SetAttributeData(vas.Single(), data);
    }


    public void SetAttributeData(VertexAttribute vertexAttribute, params float[] data) {
        if (data == null || vertexAttribute == null) {
            Log.WriteLine($"Arguments null.", eLogType.Error);
            return;
        }

        if (!VertexAttributeData.ContainsKey(vertexAttribute)) {
            Log.WriteLine($"VertexAttribute ({vertexAttribute.Name}:{vertexAttribute.ComponentCount}) does not exist for this vertex.", eLogType.Error);
            return;
        }

        if (vertexAttribute.ComponentCount != data.Length) {
            Log.WriteLine($"Vertex data component count does not match required vertex attribute ({vertexAttribute.Name}:{vertexAttribute.ComponentCount}) component count.", eLogType.Error);
            return;
        }

        Array.Copy(data, VertexAttributeData[vertexAttribute], data.Length);

        OnDataChanged?.Invoke(this);
    }

    internal float[] InterleavedVertexData(IEnumerable<VertexAttribute> attributesInOrder) {
        float[] interleavedVertexData = new float[InterleavedVertexDataLength];
        int interleavedIndex = 0;
        foreach (VertexAttribute vertexAttribute in attributesInOrder) {
            if (!VertexAttributeData.TryGetValue(vertexAttribute, out float[]? attributeData)) {
                Log.WriteLine($"Cannot interleave vertex data, vertex attribute {vertexAttribute.Name}:{vertexAttribute.ComponentCount} is not present in this vertex data object.", eLogType.Error);
                throw new InvalidOperationException($"Vertex attribute {vertexAttribute.Name}:{vertexAttribute.ComponentCount} is not present in this vertex data object.");
                //return null;
            }

            Array.Copy(attributeData, 0, interleavedVertexData, interleavedIndex, vertexAttribute.ComponentCount);
            interleavedIndex += vertexAttribute.ComponentCount;
        }

        return interleavedVertexData;
    }
}