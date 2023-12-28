using SimpleGL.Audio;
using System.Collections.Concurrent;

namespace SimpleGL.ResourceHandling.Loaders;
internal sealed class MusicResourceLoader : ResourceLoader<Music> {
    public MusicResourceLoader(BlockingCollection<(string key, Type type)> resourceLoadingQueue)
        : base(resourceLoadingQueue) {
    }

    protected override Music LoadResourceInternal(string key) {
        Music? res = ResourceManager.ActiveResourceFile.LoadMusic(key);
        return res ?? throw new ArgumentException($"Music resource '{key}' does not exist in theme.");
    }

    protected override void UnloadResourceInternal(string key, Music resource) {
        resource.Dispose();
    }
}
