using SimpleGL.Graphics;
using System.Collections.Concurrent;

namespace SimpleGL.ResourceHandling.Loaders;
internal sealed class ShaderResourceLoader : ResourceLoader<Shader> {
    public ShaderResourceLoader(BlockingCollection<(string key, Type type)> resourceLoadingQueue)
        : base(resourceLoadingQueue) {
    }

    protected override Shader LoadResourceInternal(string key) {
        Shader? res = ResourceManager.ActiveResourceFile.LoadShader(key);
        return res ?? throw new ArgumentException($"Shader resource '{key}' does not exist in theme.");
    }

    protected override void UnloadResourceInternal(string key, Shader resource) {
        resource.Dispose();
    }
}