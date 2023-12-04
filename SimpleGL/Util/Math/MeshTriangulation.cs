using OpenTK.Mathematics;
using System.Diagnostics;

namespace SimpleGL.Util.Math;
public static class MeshTriangulation {
    public static void Triangulate(Path shape, IReadOnlyList<Path> holes, out Vector2[] newVertices, out (uint i0, uint i1, uint i2)[] triangles) {
        //List<Path> shapes = paths.Where(p => p.IsClockwise).ToList();
        //List<Path> holes = paths.Where(p => !p.IsClockwise).ToList();

        if (shape.Count == 0)
            throw new Exception("No shapes found.");

        if (!shape.IsClockwise)
            throw new Exception("Shape is not clockwise.");

        if (holes.Any(h => h.IsClockwise))
            throw new Exception("Hole is clockwise.");

        List<Vector2> vertices = new List<Vector2>(shape);
        //foreach (Path shape in shapes.Skip(1))
        //    MergeShapes(vertices, shape);

        foreach (Path holeVertices in holes)
            InsertHole(vertices, holeVertices);

        newVertices = vertices.ToArray();

        if (!TryFindTriangles(vertices, out triangles))
            throw new InvalidOperationException("Could not find triangles.");
    }

    private static bool TryFindTriangles(IReadOnlyList<Vector2> vertices, out (uint i0, uint i1, uint i2)[] triangles) {
        //Debug.WriteLine(string.Join("\n", vertices.Select(v => $"{v.X} {v.Y}")));
        //verts.Reverse();

        triangles = new (uint i0, uint i1, uint i2)[0];

        List<(uint i0, uint i1, uint i2)> trianglesList;
        int startIndex = 0;
        while (!TryTriangulate(vertices, startIndex, out trianglesList)) {
            Debug.WriteLine("Failed to triangulate, retrying...");
            startIndex = new Random().Next(vertices.Count);
        }

        triangles = trianglesList.ToArray();
        return true;
    }

    private static bool TryTriangulate(IReadOnlyList<Vector2> vertices, int startIndex, out List<(uint i0, uint i1, uint i2)> trianglesList) {
        List<Vector2> verts = new List<Vector2>(vertices);
        List<int> vertexIndices = Enumerable.Range(0, vertices.Count).ToList();
        trianglesList = new();

        while (verts.Count > 2) {
            bool foundTriangle = false;

            for (int j = 0; j < verts.Count; j++) {
                int i = (startIndex + j) % verts.Count;

                Vector2 v0 = verts[i];
                Vector2 v1 = verts[(i + 1) % verts.Count];
                Vector2 v2 = verts[(i + 2) % verts.Count];

                if (!IsLocallyClockwiseBend(v0, v1, v2))
                    continue;

                if (TriangleContainsVertex(v0, v1, v2, verts))
                    continue;

                trianglesList.Add(((uint)vertexIndices[i], (uint)vertexIndices[(i + 1) % verts.Count], (uint)vertexIndices[(i + 2) % verts.Count]));
                verts.RemoveAt((i + 1) % verts.Count);
                vertexIndices.RemoveAt((i + 1) % verts.Count);

                foundTriangle = true;
                break;
            }

            if (!foundTriangle) {
                //Debug.WriteLine(string.Join("\n", verts.Select(v => $"{v.X} {v.Y}")));
                return false;
            }
        }

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

        b1 = MathUtils.AreaSign(pt, v1, v2) < 0.0f;
        b2 = MathUtils.AreaSign(pt, v2, v3) < 0.0f;
        b3 = MathUtils.AreaSign(pt, v3, v1) < 0.0f;

        return b1 == b2 && b2 == b3;
    }

    private static void MergeShapes(List<Vector2> vertices, Path shape) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector2 v0 = vertices[i];
            Vector2 v1 = vertices[(i + 1) % vertices.Count];

            // TODO: maybe
        }

    }

    private static void InsertHole(List<Vector2> vertices, Path hole) {
        if (!TryFindClosestHoleIndex(vertices, hole, out int verticesIndex, out int holeIndex))
            throw new Exception("Could not find closest hole index");

        IEnumerable<Vector2> holeVerticesInOrder = hole.Skip(holeIndex).Concat(hole.Take(holeIndex));
        holeVerticesInOrder = holeVerticesInOrder.Concat(new Vector2[] { holeVerticesInOrder.First(), vertices[verticesIndex] });
        vertices.InsertRange(verticesIndex + 1, holeVerticesInOrder);
    }

    private static bool TryFindClosestHoleIndex(List<Vector2> vertices, Path hole, out int verticesIndex, out int holeIndex) {
        float distance = float.MaxValue;
        verticesIndex = -1;
        holeIndex = -1;
        for (int vI = 0; vI < vertices.Count; vI++) {
            Vector2 vert = vertices[vI];

            for (int hI = 0; hI < hole.Count; hI++) {
                Vector2 holeVert = hole[hI];

                float dist = Vector2.DistanceSquared(vert, holeVert);

                if (dist > distance)
                    continue;

                if (IntersectsLine(vert, holeVert, vertices, hole))
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

    private static bool IntersectsLine(Vector2 p0, Vector2 p1, List<Vector2> vertices, Path hole) {

        for (int vI = 1; vI <= vertices.Count; vI++) {
            Vector2 v0 = vertices[vI - 1];
            Vector2 v1 = vertices[vI % vertices.Count];

            if (p0 == v0 || p1 == v0 || p0 == v1 || p1 == v1)
                continue;


            if (MathUtils.DoLinesIntersect(p0, p1, v0, v1, out _))
                return true;
        }

        for (int hI = 1; hI <= hole.Count; hI++) {
            Vector2 v0 = hole[hI - 1];
            Vector2 v1 = hole[hI % hole.Count];

            if (p0 == v0 || p1 == v0 || p0 == v1 || p1 == v1)
                continue;

            if (MathUtils.DoLinesIntersect(p0, p1, v0, v1, out _))
                return true;
        }

        return false;
    }
}
