using OpenTK.Mathematics;
using SimpleGL.Game.Gui.Constraints;
using SimpleGL.Graphics;
using SimpleGL.Graphics.Textures;

namespace SimpleGL.Game.Gui.Components;

public abstract class GuiTextureComponent : GuiComponent {
    private int slice;
    public int Slice {
        get => slice;
        set {
            if (value < 0)
                throw new ArgumentOutOfRangeException();

            slice = value;
            RenderBuffers = null;
        }
    }

    private Texture texture;
    public Texture Texture {
        get => texture;
        set {
            texture = value ?? throw new ArgumentNullException();
            RenderBuffers = null;
        }
    }

    private Material _Material { get; set; }
    public Material Material {
        get => _Material;
        set {
            _Material = value;  // TODO VERY unclean
            RenderBuffers = null;
        }
    }

    private Color4 _Color { get; set; }
    public Color4 Color {
        get => _Color;
        set {
            _Color = value;
            RenderBuffers = null;
        }
    }

    private RenderBuffers? RenderBuffers { get; set; }

    protected GuiTextureComponent(string constraintString)
        : this(new GuiConstraints(constraintString)) { }

    protected GuiTextureComponent(GuiConstraints constraints)
        : base(constraints) {

        texture = App.Game.Resources.DefaultTexture;
        slice = 0;
        Color = Color4.White;

        OnBoundsChanged += c => {
            if (Slice != 0)
                RenderBuffers = null;
        };
    }

    public override void Render(IRenderer renderer, float xOffset, float yOffset) {
        if (!IsVisible)
            return;

        if (RenderBuffers == null)
            RenderBuffers = CreateRenderBuffers(renderer);

        RenderBaseTexture(renderer, xOffset, yOffset);

        base.Render(renderer, xOffset, yOffset);
    }

    public void RenderBaseTexture(IRenderer renderer, float xOffset, float yOffset) {
        if (RenderBuffers == null)
            RenderBuffers = CreateRenderBuffers(renderer);

        renderer.Render((Bounds.Min.X + xOffset, Bounds.Min.Y + yOffset), RenderBuffers, Texture);
    }

    protected virtual RenderBuffers CreateRenderBuffers(IRenderer renderer) {
        if (Slice == 0)
            return CreateNoSliceRenderBuffers(renderer);
        else
            return CreateSliceRenderBuffers(renderer);
    }

    private RenderBuffers CreateNoSliceRenderBuffers(IRenderer renderer) {
        TexturedVertex[] vertices = new TexturedVertex[4];
        short[] indices = new short[] { 0, 1, 2, 1, 3, 2 };

        for (int yi = 0; yi < 2; yi++) {
            for (int xi = 0; xi < 2; xi++) {
                float x = xi * Bounds.Width;
                float y = yi * Bounds.Height;

                float texX = xi;
                float texY = yi;

                vertices[xi + yi * 2] = ((x, y), Color, (texX, texY));
            }
        }

        return renderer.CreateBuffers(vertices, indices);
    }

    private RenderBuffers CreateSliceRenderBuffers(IRenderer renderer) {
        TexturedVertex[] vertices = new TexturedVertex[16];
        short[] indices = new short[] {
                0, 4, 1, 1, 4, 5,
                1, 5, 2, 2, 5, 6,
                2, 6, 3, 3, 6, 7,
                4, 8, 5, 5, 8, 9,
                5, 9, 6, 6, 9, 10,
                6, 10, 7, 7, 10, 11,
                8, 12, 9, 9, 12, 13,
                9, 13, 10, 10, 13, 14,
                10, 14, 11, 11, 14, 15
            };

        float sX = Slice / Texture.Width;
        float sY = Slice / Texture.Height;
        float[] slicesX = { 0, sX, 1 - sX, 1 };
        float[] slicesY = { 0, sY, 1 - sY, 1 };

        //int screenWidth = Hub.GuiManager.Screen.Bounds.Width;
        //int screenHeight = Hub.GuiManager.Screen.Bounds.Height;
        //TranslateBounds(Bounds, screenWidth, screenHeight, out float left, out float top, out float width, out float height);

        for (int yi = 0; yi < 4; yi++) {
            for (int xi = 0; xi < 4; xi++) {
                float x = slicesX[xi] * Bounds.Size.X;
                float y = slicesY[yi] * Bounds.Size.Y;

                float texX = slicesX[xi];
                float texY = slicesY[yi];

                vertices[xi + yi * 4] = ((x, y), Color, (texX, texY));
            }
        }

        return renderer.CreateBuffers(vertices, indices);
    }

    private static void TranslateBounds(Box2 bounds, int screenWidth, int screenHeight, out float left, out float top, out float width, out float height) {
        left = bounds.Min.X / screenWidth;
        left = 2f * left - 1f;
        //right = bounds.Right / (float)screenWidth;
        //right = 2f * right - 1f;
        top = (screenHeight - bounds.Min.Y) / screenHeight;
        top = 2f * top - 1f;
        //bottom = (screenHeight - bounds.Bottom) / (float)screenHeight;
        //bottom = 2f * bottom - 1f;

        width = bounds.Size.X / screenWidth;
        width *= 2;

        height = bounds.Size.Y / screenHeight;
        height *= 2;
    }

    /*
    private void RecalculateBoundsSplit() {
        if (NineTilingSlice <= 0)
            return;

        int x0 = Bounds.Left + 0;
        int x1 = Bounds.Left + NineTilingSlice;
        int x2 = Bounds.Right - NineTilingSlice;
        int x3 = Bounds.Right;
        int y0 = Bounds.Top + 0;
        int y1 = Bounds.Top + NineTilingSlice;
        int y2 = Bounds.Bottom - NineTilingSlice;
        int y3 = Bounds.Bottom;
        
        BoundsSplit = new Bounds[9];
        BoundsSplit[0] = Bounds.FromPoints(x0, y0, x1, y1);
        BoundsSplit[1] = Bounds.FromPoints(x1, y0, x2, y1);
        BoundsSplit[2] = Bounds.FromPoints(x2, y0, x3, y1);

        BoundsSplit[3] = Bounds.FromPoints(x0, y1, x1, y2);
        BoundsSplit[4] = Bounds.FromPoints(x1, y1, x2, y2);
        BoundsSplit[5] = Bounds.FromPoints(x2, y1, x3, y2);

        BoundsSplit[6] = Bounds.FromPoints(x0, y2, x1, y3);
        BoundsSplit[7] = Bounds.FromPoints(x1, y2, x2, y3);
        BoundsSplit[8] = Bounds.FromPoints(x2, y2, x3, y3);
    }

    private void RecalculateSliceBounds() {
        if (NineTilingSlice <= 0 || Texture == null)
            return;

        SliceBounds = new Bounds[9];
        SliceBounds[0] = Bounds.FromPoints(0, 0, NineTilingSlice, NineTilingSlice);
        SliceBounds[1] = Bounds.FromPoints(NineTilingSlice, 0, Texture.Width - NineTilingSlice, NineTilingSlice);
        SliceBounds[2] = Bounds.FromPoints(Texture.Width - NineTilingSlice, 0, Texture.Width, NineTilingSlice);

        SliceBounds[3] = Bounds.FromPoints(0, NineTilingSlice, NineTilingSlice, Texture.Height - NineTilingSlice);
        SliceBounds[4] = Bounds.FromPoints(NineTilingSlice, NineTilingSlice, Texture.Width - NineTilingSlice, Texture.Height - NineTilingSlice);
        SliceBounds[5] = Bounds.FromPoints(Texture.Width - NineTilingSlice, NineTilingSlice, Texture.Width, Texture.Height - NineTilingSlice);
        
        SliceBounds[6] = Bounds.FromPoints(0, Texture.Height - NineTilingSlice, NineTilingSlice, Texture.Height);
        SliceBounds[7] = Bounds.FromPoints(NineTilingSlice, Texture.Height - NineTilingSlice, Texture.Width - NineTilingSlice, Texture.Height);
        SliceBounds[8] = Bounds.FromPoints(Texture.Width - NineTilingSlice, Texture.Height - NineTilingSlice, Texture.Width, Texture.Height);
    }

*/
}