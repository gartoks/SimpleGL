using OpenTK.Mathematics;
using SimpleGL.Util.Extensions;
using SimpleGL.Util.Math;

namespace SimpleGL.Graphics.Rendering;
public static class Primitives {
    private static Shader UntexturedShader { get; set; }
    private static RectanglePrimitive RectanglePrimitive { get; set; }

    public static void DrawRectangle(Vector2 position, Vector2 size, Vector2 pivot, float rotation, int zIndex, Color4 color) {
        if (!Renderer.HasActiveRenderer)
            throw new InvalidOperationException("No renderer is active.");

        ValidatePrimitives();

        Action preRenderCallback = () => {
            RectanglePrimitive.Transform.Position = position;
            RectanglePrimitive.Transform.Scale = size;
            RectanglePrimitive.Transform.Pivot = pivot;
            RectanglePrimitive.Transform.Rotation = rotation;
            RectanglePrimitive.Tint = color;
        };

        RectanglePrimitive.Render(zIndex, preRenderCallback);
    }

    public static void DrawRectangleLines(Vector2 position, Vector2 size, float thickness, Vector2 pivot, float rotation, int zIndex, Color4 color) {

        Matrix4 transformMatrix = Matrix4.CreateTranslation((pivot.X - 0.5f), (pivot.Y - 0.5f), 0) *
                                  Matrix4.CreateScale(size.X, size.Y, 1) *
                                  Matrix4.CreateRotationZ(rotation) *
                                  Matrix4.CreateTranslation(position.X, position.Y, 0);

        Vector2[] corners = new Vector2[] {
            (new Vector4(-0.5f, -0.5f, 0, 1) * transformMatrix).Vec2(),
            (new Vector4(0.5f, -0.5f, 0, 1) * transformMatrix).Vec2(),
            (new Vector4(0.5f, 0.5f, 0, 1) * transformMatrix).Vec2(),
            (new Vector4(-0.5f, 0.5f, 0, 1) * transformMatrix).Vec2()
        };

        for (int i = 0; i < corners.Length; i++) {
            Vector2 start = corners[i];
            Vector2 end = corners[(i + 1) % corners.Length];

            DrawLineWithPivot(start, end, thickness, zIndex, color, Vector2.Zero);
        }
    }

    public static void DrawLine(Vector2 start, Vector2 end, float thickness, int zIndex, Color4 color) {
        if (!Renderer.HasActiveRenderer)
            throw new InvalidOperationException("No renderer is active.");

        ValidatePrimitives();

        DrawLineWithPivot(start, end, thickness, zIndex, color, new Vector2(0, 0.5f));
    }

    private static void DrawLineWithPivot(Vector2 start, Vector2 end, float thickness, int zIndex, Color4 color, Vector2 pivot) {
        if (!Renderer.HasActiveRenderer)
            throw new InvalidOperationException("No renderer is active.");

        ValidatePrimitives();

        // TODO may need to transform points based on camera projection, this would replace CalculateSlope

        Action preRenderCallback = () => {
            RectanglePrimitive.Transform.Position = start;
            RectanglePrimitive.Transform.Scale = new Vector2((end - start).Length, thickness);
            RectanglePrimitive.Transform.Pivot = pivot;
            RectanglePrimitive.Transform.Rotation = MathUtils.CalculateSlope(start, end) + MathF.PI;
            RectanglePrimitive.Tint = color;
        };

        RectanglePrimitive.Render(zIndex, preRenderCallback);
    }


    private static void ValidatePrimitives() {
        // TODO implement custom shader that can draw circles too

        if (UntexturedShader == null) {
            GraphicsHelper.CreateUntexturedPassthroughShader(true, out string vertexShader, out string fragmentShader);
            UntexturedShader = GraphicsHelper.CreateShader(vertexShader, fragmentShader);
        }

        if (RectanglePrimitive == null) {
            RectanglePrimitive = new RectanglePrimitive(UntexturedShader);
        }
    }
}
