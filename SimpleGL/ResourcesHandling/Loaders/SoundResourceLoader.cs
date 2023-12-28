using SimpleGL.Audio;
using System.Collections.Concurrent;

namespace SimpleGL.ResourceHandling.Loaders;
internal sealed class SoundResourceLoader : ResourceLoader<Sound> {
    public SoundResourceLoader(BlockingCollection<(string key, Type type)> resourceLoadingQueue)
        : base(resourceLoadingQueue) {
    }

    protected override Sound LoadResourceInternal(string key) {
        Sound? res = ResourceManager.ActiveResourceFile.LoadSound(key);
        return res ?? throw new ArgumentException($"Sound resource '{key}' does not exist in theme.");
    }

    protected override void UnloadResourceInternal(string key, Sound resource) {
        resource.Dispose();
    }
}