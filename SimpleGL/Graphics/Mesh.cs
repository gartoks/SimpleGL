﻿using SimpleGL.Util;

namespace SimpleGL.Graphics;
public sealed class Mesh {
    public delegate void MeshVertexDataChangedEventHandler(Mesh mesh);
    public delegate void MeshIndicesChangedEventHandler(Mesh mesh);

    public IReadOnlyDictionary<string, VertexAttribute> VertexAttributes { get; }
    public IReadOnlyList<int> Indices { get; }

    private VertexData[] VertexData { get; }
    private int interleavedVertexDataLength;

    internal event MeshVertexDataChangedEventHandler OnMeshVertexDataChanged;

    internal Mesh(int vertexCount, VertexAttribute[] vertexAttributes, (uint idx0, uint idx1, uint idx2)[] clockwiseIndices) {
        if (vertexCount <= 0)
            throw new ArgumentException($"Cannot create mesh. Invalid vertex count ({vertexCount}). Must be at least one.");

        if (clockwiseIndices.Length == 0)
            throw new ArgumentException($"Cannot create mesh. Invalid triangle count ({clockwiseIndices.Length}). Must be at least one.");

        VertexAttributes = vertexAttributes.ToDictionary(va => va.Name, va => va);

        VertexData[] vertexData = new VertexData[vertexCount];
        for (int i = 0; i < vertexCount; i++) {
            vertexData[i] = new VertexData(vertexAttributes);
            vertexData[i].OnDataChanged += MeshVertexDataChanged;
        }

        VertexData = vertexData;

        this.interleavedVertexDataLength = vertexCount * VertexData[0].InterleavedVertexDataLength;

        int[] indices = new int[clockwiseIndices.Length * 3];
        for (int i = 0; i < clockwiseIndices.Length; i++) {
            (uint idx0, uint idx1, uint idx2) triangle = clockwiseIndices[i];
            if (triangle.idx0 >= VertexData.Length)
                throw new ArgumentException($"Invalid index {triangle.idx0}. Must be in range of [0, {VertexData.Length - 1}]");

            if (triangle.idx1 >= VertexData.Length) {
                Log.WriteLine($"Invalid index {triangle.idx1}. Must be in range of [0, {VertexData.Length - 1}]", eLogType.Error);
                return;
            }

            if (triangle.idx2 >= VertexData.Length)
                throw new ArgumentException($"Invalid index {triangle.idx2}. Must be in range of [0, {VertexData.Length - 1}]");

            indices[i * 3 + 0] = (int)triangle.idx0;
            indices[i * 3 + 1] = (int)triangle.idx1;
            indices[i * 3 + 2] = (int)triangle.idx2;
        }
        Indices = indices;
    }

    ~Mesh() {
        foreach (VertexData vertexData in VertexData) {
            vertexData.OnDataChanged -= MeshVertexDataChanged;
        }
    }

    public VertexData GetVertexData(int vertexIndex) {
        if (vertexIndex < 0 || vertexIndex >= VertexData.Length) {
            Log.WriteLine($"Invalid vertex index {vertexIndex}. Must be in range of [0, {VertexData.Length - 1}]", eLogType.Error);
            return null;
        }

        return VertexData[vertexIndex];
    }

    internal float[] GetInterleavedVertexData(IEnumerable<VertexAttribute> attributesInOrder) {
        if (attributesInOrder.Count() != VertexAttributes.Count) {
            Log.WriteLine($"Invalid attributes to interleave. Count does not match.", eLogType.Error);
            return null;
        }

        foreach (VertexAttribute att in attributesInOrder) {
            if (!VertexAttributes.Values.Contains(att)) {
                Log.WriteLine($"Invalid attribute '{att.Name}' to interleave. Attribute not found in mesh.", eLogType.Error);
                return null;
            }
        }

        float[] interleavedVertexData = new float[this.interleavedVertexDataLength];

        int interleavedIndex = 0;
        for (int v = 0; v < VertexData.Length; v++) {
            int interleavedLength = VertexData[v].InterleavedVertexDataLength;
            Array.Copy(VertexData[v].InterleavedVertexData(attributesInOrder), 0, interleavedVertexData, interleavedIndex, interleavedLength);
            interleavedIndex += interleavedLength;
        }

        return interleavedVertexData;
    }

    private void MeshVertexDataChanged(VertexData vertexData) {
        OnMeshVertexDataChanged?.Invoke(this);
    }
}
