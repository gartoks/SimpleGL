using OpenTK.Mathematics;

namespace SimpleGL.Util;
public static class MeshTriangulation {

    public static void Triangulate(Vector2[] verticesInClockwiseOrder, Vector2[][] holesInCounterClockwiseOrder, out Vector2[] newVerticesInClockwiseOrder, out (uint i0, uint i1, uint i2)[] triangles) {
        List<Vector2> vertices = new List<Vector2>(verticesInClockwiseOrder);

        foreach (Vector2[] holeVertices in holesInCounterClockwiseOrder)
            InsertHole(vertices, holeVertices);

        newVerticesInClockwiseOrder = vertices.ToArray();

        if (!TryFindTriangles(vertices, out triangles))
            throw new Exception("Could not find triangles.");
    }

    private static bool TryFindTriangles(List<Vector2> vertices, out (uint i0, uint i1, uint i2)[] triangles) {
        triangles = new (uint i0, uint i1, uint i2)[0];

        List<int> vertexIndices = Enumerable.Range(0, vertices.Count).ToList();

        List<(uint i0, uint i1, uint i2)> trianglesList = new();
        while (vertices.Count > 2) {
            bool foundTriangle = false;

            for (int i = 0; i < vertices.Count; i++) {
                Vector2 v0 = vertices[i];
                Vector2 v1 = vertices[(i + 1) % vertices.Count];
                Vector2 v2 = vertices[(i + 2) % vertices.Count];

                if (!IsLocallyClockwiseBend(v0, v1, v2))
                    continue;

                if (TriangleContainsVertex(v0, v1, v2, vertices))
                    continue;

                trianglesList.Add(((uint)vertexIndices[i], (uint)vertexIndices[((i + 1) % vertices.Count)], (uint)vertexIndices[((i + 2) % vertices.Count)]));
                vertices.RemoveAt((i + 1) % vertices.Count);
                vertexIndices.RemoveAt((i + 1) % vertices.Count);

                foundTriangle = true;
                break;
            }

            if (!foundTriangle)
                return false;
        }

        triangles = trianglesList.ToArray();
        return true;
    }

    private static bool TriangleContainsVertex(Vector2 v0, Vector2 v1, Vector2 v2, List<Vector2> vertices) {
        foreach (Vector2 vertex in vertices)
            if (IsPointInTriangle(vertex, v0, v1, v2))
                return true;

        return false;
    }

    private static bool IsLocallyClockwiseBend(Vector2 v0, Vector2 v1, Vector2 v2) {
        Vector2 a = v1 - v0;
        Vector2 b = v2 - v1;

        // The "z-component" of the cross product in 2D
        float crossProductZ = a.X * b.Y - a.Y * b.X;

        // If crossProductZ is negative, the bend is clockwise
        return crossProductZ < 0;
    }

    private static bool IsPointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3) {
        bool b1, b2, b3;

        b1 = AreaSign(pt, v1, v2) < 0.0f;
        b2 = AreaSign(pt, v2, v3) < 0.0f;
        b3 = AreaSign(pt, v3, v1) < 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    private static float AreaSign(Vector2 p1, Vector2 p2, Vector2 p3) {
        return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
    }

    private static void InsertHole(List<Vector2> vertices, Vector2[] holeInCounterClockwiseOrder) {
        if (!TryFindClosestHoleIndex(vertices, holeInCounterClockwiseOrder, out int verticesIndex, out int holeIndex))
            throw new Exception("Could not find closest hole index");

        IEnumerable<Vector2> holeVerticesInOrder = holeInCounterClockwiseOrder.Skip(holeIndex).Concat(holeInCounterClockwiseOrder.Take(holeIndex));
        holeVerticesInOrder = holeVerticesInOrder.Concat(new Vector2[] { holeVerticesInOrder.First(), vertices[verticesIndex] });
        vertices.InsertRange(verticesIndex + 1, holeVerticesInOrder);
    }

    private static bool TryFindClosestHoleIndex(List<Vector2> vertices, Vector2[] holeInCounterClockwiseOrder, out int verticesIndex, out int holeIndex) {
        float distance = float.MaxValue;
        verticesIndex = -1;
        holeIndex = -1;
        for (int vI = 0; vI < vertices.Count; vI++) {
            Vector2 vert = vertices[vI];

            for (int hI = 0; hI < holeInCounterClockwiseOrder.Length; hI++) {
                Vector2 holeVert = holeInCounterClockwiseOrder[hI];

                float dist = Vector2.DistanceSquared(vert, holeVert);

                if (dist > distance)
                    continue;

                if (IntersectsLine(vert, holeVert, vertices, holeInCounterClockwiseOrder))
                    continue;

                distance = dist;
                verticesIndex = vI;
                holeIndex = hI;
            }
        }

        if (verticesIndex == -1 || holeIndex == -1)
            return false;

        return true;
    }

    private static bool IntersectsLine(Vector2 p0, Vector2 p1, List<Vector2> vertices, Vector2[] holeInCounterClockwiseOrder) {

        for (int vI = 1; vI <= vertices.Count; vI++) {
            Vector2 v0 = vertices[vI - 1];
            Vector2 v1 = vertices[vI % vertices.Count];

            if (p0 == v0 || p1 == v0 || p0 == v1 || p1 == v1)
                continue;


            if (DoLinesIntersect(p0, p1, v0, v1))
                return true;
        }

        for (int hI = 1; hI <= holeInCounterClockwiseOrder.Length; hI++) {
            Vector2 v0 = holeInCounterClockwiseOrder[hI - 1];
            Vector2 v1 = holeInCounterClockwiseOrder[hI % holeInCounterClockwiseOrder.Length];

            if (p0 == v0 || p1 == v0 || p0 == v1 || p1 == v1)
                continue;

            if (DoLinesIntersect(p0, p1, v0, v1))
                return true;
        }

        return false;
    }

    private static bool DoLinesIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
        Vector2 s1 = p1 - p0;
        Vector2 s2 = p3 - p2;

        float s, t;
        s = (-s1.Y * (p0.X - p2.X) + s1.X * (p0.Y - p2.Y)) / (-s2.X * s1.Y + s1.X * s2.Y);
        t = (s2.X * (p0.Y - p2.Y) - s2.Y * (p0.X - p2.X)) / (-s2.X * s1.Y + s1.X * s2.Y);

        return s >= 0 && s <= 1 && t >= 0 && t <= 1;
    }
}
