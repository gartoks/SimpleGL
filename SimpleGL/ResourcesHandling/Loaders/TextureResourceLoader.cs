using SimpleGL.Graphics.Textures;
using System.Collections.Concurrent;

namespace SimpleGL.ResourceHandling.Loaders;
internal sealed class TextureResourceLoader : ResourceLoader<Texture2D> {
    public TextureResourceLoader(BlockingCollection<(string key, Type type)> resourceLoadingQueue)
        : base(resourceLoadingQueue) {
    }

    protected override Texture2D LoadResourceInternal(string key) {
        Texture2D? res = ResourceManager.ActiveResourceFile.LoadTexture(key);
        return res ?? throw new ArgumentException($"Texture resource '{key}' does not exist in theme.");
    }

    protected override void UnloadResourceInternal(string key, Texture2D texture) {
        texture.Dispose();
    }
}
