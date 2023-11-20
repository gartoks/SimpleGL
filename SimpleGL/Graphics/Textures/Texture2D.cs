using StbImageSharp;

namespace SimpleGL.Graphics.Textures;
public class Texture2D : Texture {
    private static (float x, float y)[] TEXTURE_COORDS { get; } = new (float x, float y)[] { (0, 0), (1, 0), (0, 1), (1, 1) }; // TODO: test order

    public override IReadOnlyList<(float x, float y)> TextureCoordinates => TEXTURE_COORDS;

    protected ImageResult Image { get; }

    internal Texture2D(ImageResult image, int textureId)
        : base(image.Width, image.Height, textureId) {
    }
}