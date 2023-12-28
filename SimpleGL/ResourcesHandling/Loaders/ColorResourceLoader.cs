using OpenTK.Mathematics;
using System.Collections.Concurrent;

namespace SimpleGL.ResourceHandling.Loaders;
internal sealed class ColorResourceLoader : ResourceLoader<Color4> {
    public ColorResourceLoader(BlockingCollection<(string key, Type type)> resourceLoadingQueue)
   : base(resourceLoadingQueue) {
    }

    protected override Color4 LoadResourceInternal(string key) {
        Color4? res = ResourceManager.ActiveResourceFile.GetColor(key);
        return res ?? throw new ArgumentException($"Color resource '{key}' does not exist in theme.");
    }

    protected override void UnloadResourceInternal(string key, Color4 resource) {
        throw new NotImplementedException();
    }
}