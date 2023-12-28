using OpenTK.Mathematics;

namespace SimpleGL.Graphics.Textures;
internal sealed class SubTexture : Texture {
    public override string Key => $"{TextureAtlas.Key}$${base.Key}";

    private TextureAtlas TextureAtlas { get; }

    public override Box2 TextureCoordinates { get; }

    public SubTexture(string subKey, TextureAtlas textureAtlas, Box2i bounds)
        : base(subKey, bounds.Size.X, bounds.Size.Y, textureAtlas.TextureId) {
        TextureAtlas = textureAtlas;
        TextureCoordinates = new Box2(
            bounds.Min.X / (float)textureAtlas.Width, bounds.Min.Y / (float)textureAtlas.Height,
            bounds.Max.X / (float)textureAtlas.Width, bounds.Max.Y / (float)textureAtlas.Height);
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