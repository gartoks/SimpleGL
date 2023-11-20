using SimpleGL.Graphics.GLHandling;

namespace SimpleGL.Graphics;
public sealed class ElementBufferObject : IDisposable {
    public const int MAX_SIZE = 60000;

    public eBufferType Type { get; }

    internal int[] _Data { get; }
    public IReadOnlyList<int> Data => _Data;

    public int Size => _Data.Length;

    internal int EboId { get; }
    public bool IsBound => GLHandler.IsEboBound(this);

    public bool IsDisposed => EboId <= 0;
    private bool disposedValue;

    internal ElementBufferObject(int eboId, int[] data, eBufferType bufferType) {
        if (data.Length > MAX_SIZE)
            throw new ArgumentOutOfRangeException(nameof(data), $"There can be at maximum {MAX_SIZE} indices.");

        EboId = eboId;
        Type = bufferType;
        _Data = data;
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~ElementBufferObject() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    internal void Bind() {
        GLHandler.BindEbo(this);
    }

    internal void Release() {
        GLHandler.ReleaseEbo(this);
    }

    private void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // dispose managed state (managed objects)
            }

            GLHandler.DeleteEbo(this);
            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
