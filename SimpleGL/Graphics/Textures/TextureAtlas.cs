using OpenTK.Mathematics;
using StbImageSharp;

namespace SimpleGL.Graphics.Textures;
public sealed class TextureAtlas : Texture2D {
    private IReadOnlyDictionary<string, Box2i> SubTextureBounds { get; }

    public TextureAtlas(ImageResult image, int textureId, IReadOnlyDictionary<string, Box2i> subTextureBounds)
        : base(image, textureId) {

        SubTextureBounds = subTextureBounds;
    }

    public bool TryGetSubTexture(string subTextureName, out Texture? texture) {
        texture = default;
        bool found = SubTextureBounds.TryGetValue(subTextureName, out Box2i bounds);

        if (!found)
            return false;

        texture = new SubTexture(this, bounds);
        return true;
    }
}
