using OpenTK.Mathematics;
using SimpleGL.Util.Math;

namespace SimpleGL.Graphics.Rendering;
public static class Primitives {
    private static Shader UntexturedShader { get; set; }
    private static RectanglePrimitive RectanglePrimitive { get; set; }


    public static void DrawRectangle(Renderer renderer, Vector2 position, Vector2 size, Vector2 pivot, float rotation, int zIndex, Color4 color) {
        if (!renderer.IsActive)
            throw new InvalidOperationException("Renderer is not active.");

        ValidatePrimitives();

        Action preRenderCallback = () => {
            RectanglePrimitive.Position = position;
            RectanglePrimitive.Scale = size;
            RectanglePrimitive.Pivot = pivot;
            RectanglePrimitive.Rotation = rotation;
            RectanglePrimitive.Tint = color;
        };

        RectanglePrimitive.Render(renderer, zIndex, preRenderCallback);
    }

    public static void DrawLine(Renderer renderer, Vector2 start, Vector2 end, float thickness, int zIndex, Color4 color) {
        if (!renderer.IsActive)
            throw new InvalidOperationException("Renderer is not active.");

        ValidatePrimitives();

        Action preRenderCallback = () => {
            RectanglePrimitive.Position = start;
            RectanglePrimitive.Scale = new Vector2((end - start).Length, thickness);
            RectanglePrimitive.Pivot = new Vector2(0, 0.5f);
            RectanglePrimitive.Rotation = MathUtils.CalculateSlope(start, end) + MathF.PI;
            RectanglePrimitive.Tint = color;
        };

        RectanglePrimitive.Render(renderer, zIndex, preRenderCallback);
    }

    private static void ValidatePrimitives() {
        if (UntexturedShader == null) {
            GraphicsHelper.CreateUntexturedPassthroughShader(true, out string vertexShader, out string fragmentShader);
            UntexturedShader = GraphicsHelper.CreateShader(vertexShader, fragmentShader);
        }

        if (RectanglePrimitive == null) {
            RectanglePrimitive = new RectanglePrimitive(UntexturedShader);
        }
    }
}
