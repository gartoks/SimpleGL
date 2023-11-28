using OpenTK.Mathematics;
using SimpleGL.Util.Extensions;
using SimpleGL.Util.Math;
using SixLabors.Fonts;
using System.Diagnostics;
using Path = SimpleGL.Util.Math.Path;

namespace SimpleGL.Util;
public static class TextMeshGenerator {
    public static (Vector2[] vertices, (uint i0, uint i1, uint i2)[] triangles) ConvertToMesh(Font font, string text) {
        IReadOnlyList<(char character, IReadOnlyList<Path> paths)> glyphPaths = ConvertToPaths(font, text);

        List<Vector2> vertices = new();
        List<(uint i0, uint i1, uint i2)> triangles = new();
        foreach ((char character, IReadOnlyList<Path> paths) item in glyphPaths) {
            char character = item.character;
            IReadOnlyList<Path> paths = item.paths;

            IReadOnlyList<(Path shape, Path[] holes)> shapePaths = SeparatePaths(paths);

            foreach ((Path shape, Path[] holes) shapePath in shapePaths) {
                MeshTriangulation.Triangulate(shapePath.shape, shapePath.holes, out Vector2[]? shapeVertices, out (uint i0, uint i1, uint i2)[] shapeTrianges);

                uint nextVertexIndex = (uint)vertices.Count;
                vertices.AddRange(shapeVertices);
                triangles.AddRange(shapeTrianges.Select(t => (t.i0 + nextVertexIndex, t.i1 + nextVertexIndex, t.i2 + nextVertexIndex)));
            }
        }

        return (vertices.ToArray(), triangles.ToArray());
    }

    public static IReadOnlyList<(char character, IReadOnlyList<Path> paths)> ConvertToPaths(Font font, string text) {
        PathFindingGlyphRenderer renderer = new PathFindingGlyphRenderer(font.Size);
        return renderer.ConvertToPaths(font, text);
    }

    private static IReadOnlyList<(Path shape, Path[] holes)> SeparatePaths(IReadOnlyList<Path> paths) {
        List<Path> shapes = paths.Where(p => p.IsClockwise).ToList();
        List<Path> holes = paths.Where(p => !p.IsClockwise).ToList();

        if (shapes.Count == 0)
            throw new Exception("No shapes found.");

        return shapes.Select(s => (s, holes.Where(h => h.All(hp => MathUtils.IsPointInPolygon(s, hp))).ToArray())).ToList();
    }

    private class PathFindingGlyphRenderer : IGlyphRenderer {
        private float FontSize { get; }

        private List<(char character, IReadOnlyList<Path> paths)> GlyphPaths { get; }

        private char CurrentGlyph { get; set; }
        private List<Path> CurrentGlyphPaths { get; }

        private List<Vector2> CurrentPath { get; }
        private Vector2 CurrentPoint { get; set; }

        public PathFindingGlyphRenderer(float fontSize) {
            FontSize = fontSize;

            GlyphPaths = new();
            CurrentGlyph = '\0';
            CurrentGlyphPaths = new();
            CurrentPath = new();
            CurrentPoint = Vector2.Zero;
        }

        public IReadOnlyList<(char character, IReadOnlyList<Path>)> ConvertToPaths(Font font, string text) {
            this.Render(text, new TextOptions(font));

            return GlyphPaths.Select(ConvertGlyphPath).ToList();
        }

        private static (char character, IReadOnlyList<Path> paths) ConvertGlyphPath((char character, IReadOnlyList<Path> paths) gp) {
            return (gp.character, gp.paths.Select(p => p.Reverse()).ToList());
        }

        public void BeginText(in FontRectangle bounds) {
            GlyphPaths.Clear();
        }

        public void EndText() {
        }

        public bool BeginGlyph(in FontRectangle bounds, in GlyphRendererParameters parameters) {
            CurrentGlyph = (char)parameters.CodePoint.Value;
            CurrentGlyphPaths.Clear();
            CurrentPath.Clear();
            return true;
        }

        public void EndGlyph() {
            if (CurrentGlyphPaths.Count > 0)
                GlyphPaths.Add((CurrentGlyph, CurrentGlyphPaths.ToList()));

            CurrentGlyph = '\0';
            CurrentGlyphPaths.Clear();
            CurrentPath.Clear();
        }

        public void BeginFigure() {
            CurrentPath.Clear();
        }

        public void EndFigure() {
            if (CurrentPath.Count > 0) {
                if (CurrentPath.Count > 1 && CurrentPath[0].X == CurrentPath[^1].X && CurrentPath[0].Y == CurrentPath[^1].Y)
                    CurrentPath.RemoveAt(CurrentPath.Count - 1);

                CurrentGlyphPaths.Add(new Path(CurrentPath.Select(v => new Vector2(v.X, v.Y))));
            }
            CurrentPath.Clear();
        }

        public void MoveTo(System.Numerics.Vector2 point) {
            CurrentPoint = point.Convert();
        }

        public void LineTo(System.Numerics.Vector2 point) {
            if (CurrentPath.Count == 0)
                AddPointToCurrentPath(CurrentPoint);

            // Add vertex for line
            AddPointToCurrentPath(point.Convert());
        }

        public void QuadraticBezierTo(System.Numerics.Vector2 secondControlPoint, System.Numerics.Vector2 point) {
            if (CurrentPath.Count == 0)
                AddPointToCurrentPath(CurrentPoint);

            int bezierSteps = BezierCurveSteps();
            for (int i = 1; i < bezierSteps; i++)
                AddPointToCurrentPath(BezierCurves.Quadratic(CurrentPoint, secondControlPoint.Convert(), point.Convert(), i / (float)bezierSteps));

            AddPointToCurrentPath(point.Convert());
        }

        public void CubicBezierTo(System.Numerics.Vector2 secondControlPoint, System.Numerics.Vector2 thirdControlPoint, System.Numerics.Vector2 point) {
            if (CurrentPath.Count == 0)
                AddPointToCurrentPath(CurrentPoint);

            int bezierSteps = BezierCurveSteps();
            for (int i = 1; i < bezierSteps; i++)
                AddPointToCurrentPath(BezierCurves.Cubic(CurrentPoint, secondControlPoint.Convert(), thirdControlPoint.Convert(), point.Convert(), i / (float)bezierSteps));
            AddPointToCurrentPath(point.Convert());
        }

        private void AddPointToCurrentPath(Vector2 point) {
            if (CurrentPath.Count > 0 && CurrentPath[^1].X == point.X && CurrentPath[^1].Y == point.Y)
                return;

            if (CurrentPath.Count > 1 && (CurrentPoint - point).LengthSquared < VertexEliminationDistance())
                return;

            CurrentPath.Add(point);
            CurrentPoint = point;
        }

        public TextDecorations EnabledDecorations() {
            return TextDecorations.None;
        }

        public void SetDecoration(TextDecorations textDecorations, System.Numerics.Vector2 start, System.Numerics.Vector2 end, float thickness) {
            Debug.WriteLine($"Deco: {textDecorations} {thickness}");
        }

        private int BezierCurveSteps() {
            return (int)FontSize;
        }

        private float VertexEliminationDistance() {
            return 0.025f * MathF.Sqrt(FontSize);
        }
    }
}