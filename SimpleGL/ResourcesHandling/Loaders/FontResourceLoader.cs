using SimpleGL.Util;
using System.Collections.Concurrent;

namespace SimpleGL.ResourceHandling.Loaders;
internal sealed class FontResourceLoader : ResourceLoader<FontFamilyData> {
    public FontResourceLoader(BlockingCollection<(string key, Type type)> resourceLoadingQueue)
        : base(resourceLoadingQueue) {
    }

    protected override FontFamilyData LoadResourceInternal(string key) {
        FontFamilyData? res = ResourceManager.ActiveResourceFile.LoadFont(key);
        return res ?? throw new ArgumentException($"Font resource '{key}' does not exist in theme.");
    }

    protected override void UnloadResourceInternal(string key, FontFamilyData resource) {
    }
}