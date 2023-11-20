using SimpleGL.Util;

namespace SimpleGL.Graphics.Textures;
internal sealed class SubTexture : Texture {
    private TextureAtlas TextureAtlas { get; }

    public override IReadOnlyList<(float x, float y)> TextureCoordinates { get; }

    public SubTexture(TextureAtlas textureAtlas, RectangleI bounds)
        : base(bounds.Width, bounds.Height, textureAtlas.TextureId) {
        TextureAtlas = textureAtlas;
        TextureCoordinates = new (float x, float y)[] { // TODO: test order
            (bounds.X / (float)textureAtlas.Width, bounds.Y / (float)textureAtlas.Height),
            ((bounds.X + bounds.Width) / (float)textureAtlas.Width, bounds.Y / (float)textureAtlas.Height),
            (bounds.X / (float)textureAtlas.Width, (bounds.Y + bounds.Height) / (float)textureAtlas.Height),
            ((bounds.X + bounds.Width) / (float)textureAtlas.Width, (bounds.Y + bounds.Height) / (float)textureAtlas.Height),
        };
    }

    protected override void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // dispose managed state (managed objects)
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            disposedValue = true;
        }
    }
}