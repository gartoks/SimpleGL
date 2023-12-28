using System.Collections.Concurrent;

namespace SimpleGL.ResourceHandling.Loaders;
internal sealed class TextResourceLoader : ResourceLoader<IReadOnlyDictionary<string, string>> {
    public TextResourceLoader(BlockingCollection<(string key, Type type)> resourceLoadingQueue)
        : base(resourceLoadingQueue) {
    }

    protected override IReadOnlyDictionary<string, string> LoadResourceInternal(string key) {
        IReadOnlyDictionary<string, string>? res = ResourceManager.ActiveResourceFile.LoadText(key);
        return res ?? throw new ArgumentException($"Text resource '{key}' does not exist in theme.");
    }

    protected override void UnloadResourceInternal(string key, IReadOnlyDictionary<string, string> resource) {
    }
}