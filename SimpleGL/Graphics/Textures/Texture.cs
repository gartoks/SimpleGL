using OpenTK.Mathematics;
using SimpleGL.Graphics.GLHandling;

namespace SimpleGL.Graphics.Textures;
public abstract class Texture : IDisposable {
    public virtual string Key { get; }
    public int Width { get; }
    public int Height { get; }

    private eTextureWrapMode _WrapS { get; set; }
    public eTextureWrapMode WrapS {
        get => _WrapS;
        set {
            _WrapS = value;
            hasDirtyWrap = true;
        }
    }

    private eTextureWrapMode _WrapT { get; set; }
    public eTextureWrapMode WrapT {
        get => _WrapT;
        set {
            _WrapT = value;
            hasDirtyWrap = true;
        }
    }

    private eTextureFilterMode _MinFilter { get; set; }
    public eTextureFilterMode MinFilter {
        get => _MinFilter;
        set {
            _MinFilter = value;
            hasDirtyFilter = true;
        }
    }

    private eTextureFilterMode _MagFilter { get; set; }
    public eTextureFilterMode MagFilter {
        get => _MagFilter;
        set {
            _MagFilter = value;
            hasDirtyFilter = true;
        }
    }

    internal bool IsDisposed => TextureId <= 0;

    internal int TextureId { get; }
    internal bool IsBound => GLHandler.IsTextureAssigned(this);

    protected bool hasDirtyWrap;
    protected bool hasDirtyFilter;

    protected bool disposedValue;

    public abstract Box2 TextureCoordinates { get; }

    protected internal Texture(string key, int width, int height, int textureId) {
        Key = key;
        Width = width;
        Height = height;
        TextureId = textureId;

        _WrapS = eTextureWrapMode.Clamp;
        _WrapT = eTextureWrapMode.Clamp;
        _MinFilter = eTextureFilterMode.Nearest;
        _MagFilter = eTextureFilterMode.Nearest;
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~Texture() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    internal void Bind(int textureUnit = -1) {
        Clean();

        if (textureUnit < 0)
            GLHandler.AssignTexture(this);
        else
            GLHandler.AssignTexture(this, textureUnit);
    }

    internal void Release() {
        GLHandler.UnassignTexture(this);
    }

    internal void Clean() {
        if (!hasDirtyWrap && !hasDirtyFilter)
            return;

        if (hasDirtyWrap) {
            GLHandler.UpdateTextureWrapMode(this);
            hasDirtyWrap = false;
        }

        if (hasDirtyFilter) {
            GLHandler.UpdateTextureFilterMode(this);
            hasDirtyFilter = false;
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // dispose managed state (managed objects)
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            GraphicsHelper.DeleteTexture(this);
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}