using SimpleGL.Graphics.Textures;
using SimpleGL.ResourceHandling;
using System.Collections.Concurrent;

namespace SimpleGL.ResourcesHandling.Loaders;
internal sealed class TextureAtlasResourceLoader : ResourceLoader<TextureAtlas> {
    public TextureAtlasResourceLoader(BlockingCollection<(string key, Type type)> resourceLoadingQueue)
        : base(resourceLoadingQueue) {
    }

    protected override TextureAtlas LoadResourceInternal(string key) {
        TextureAtlas? res = ResourceManager.ActiveResourceFile.LoadTextureAtlas(key);
        return res ?? throw new ArgumentException($"Texture atlas resource '{key}' does not exist in theme.");
    }

    protected override void UnloadResourceInternal(string key, TextureAtlas resource) {
        resource.Dispose();
    }
}