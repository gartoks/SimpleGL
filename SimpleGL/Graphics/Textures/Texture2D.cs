using OpenTK.Mathematics;
using StbImageSharp;

namespace SimpleGL.Graphics.Textures;
public class Texture2D : Texture {
    private static (float x, float y)[] TEXTURE_COORDS { get; } = new (float x, float y)[] { (0, 0), (1, 0), (0, 1), (1, 1) };

    public override Box2 TextureCoordinates => new Box2(0, 0, 1, 1);

    protected ImageResult Image { get; }

    internal Texture2D(ImageResult image, int textureId)
        : base(image.Width, image.Height, textureId) {
    }
}