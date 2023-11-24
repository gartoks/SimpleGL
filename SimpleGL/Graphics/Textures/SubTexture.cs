using OpenTK.Mathematics;

namespace SimpleGL.Graphics.Textures;
internal sealed class SubTexture : Texture {
    private TextureAtlas TextureAtlas { get; }

    public override IReadOnlyList<(float x, float y)> TextureCoordinates { get; }

    public SubTexture(TextureAtlas textureAtlas, Box2i bounds)
        : base(bounds.Size.X, bounds.Size.Y, textureAtlas.TextureId) {
        TextureAtlas = textureAtlas;
        TextureCoordinates = new (float x, float y)[] { // TODO: test order
            (bounds.Min.X / (float)textureAtlas.Width, bounds.Min.Y / (float)textureAtlas.Height),
            ((bounds.Min.X + bounds.Size.X) / (float)textureAtlas.Width, bounds.Min.Y / (float)textureAtlas.Height),
            (bounds.Min.X / (float)textureAtlas.Width, (bounds.Min.Y + bounds.Size.Y) / (float)textureAtlas.Height),
            ((bounds.Min.X + bounds.Size.X) / (float)textureAtlas.Width, (bounds.Min.Y + bounds.Size.Y) / (float)textureAtlas.Height),
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