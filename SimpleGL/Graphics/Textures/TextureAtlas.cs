using SimpleGL.Util;
using StbImageSharp;

namespace SimpleGL.Graphics.Textures;
public sealed class TextureAtlas : Texture2D {
    private IReadOnlyDictionary<string, RectangleI> SubTextureBounds { get; }

    public TextureAtlas(ImageResult image, int textureId, IReadOnlyDictionary<string, RectangleI> subTextureBounds)
        : base(image, textureId) {

        SubTextureBounds = subTextureBounds;
    }

    public bool TryGetSubTexture(string subTextureName, out Texture? texture) {
        texture = default;
        bool found = SubTextureBounds.TryGetValue(subTextureName, out RectangleI bounds);

        if (!found)
            return false;

        texture = new SubTexture(this, bounds);
        return true;
    }
}
