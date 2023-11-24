using SimpleGL.Graphics.GLHandling;
using SimpleGL.Util;

namespace SimpleGL.Graphics;
public sealed class VertexBufferObject : IDisposable {
    internal const int MAX_SIZE = ushort.MaxValue;

    internal eBufferType Type { get; }

    private float[] _Data { get; }
    public IReadOnlyList<float> Data => _Data;

    public int Size => _Data.Length;

    internal int VboId { get; }
    public bool IsBound => GLHandler.IsVboBound(this);

    public bool IsDisposed => VboId <= 0;
    private bool disposedValue;

    internal VertexBufferObject(int vboId, float[] data, eBufferType bufferType) {
        if (data.Length > MAX_SIZE)
            throw new ArgumentOutOfRangeException(nameof(data), $"There can be at maximum {MAX_SIZE} vertices.");

        VboId = vboId;

        Type = bufferType;

        _Data = new float[data.Length];
        Array.Copy(data, _Data, data.Length);
    }

    ~VertexBufferObject() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    internal float[] GetData() => _Data;

    internal void SetData(float[] data) {
        if (data == null || data.Length != _Data.Length) {
            Log.WriteLine("Cannot update vertex buffer object data. Data lengths do not match.", eLogType.Error);
            return;
        }

        Array.Copy(data, _Data, _Data.Length);
        GLHandler.UpdateVboData(this);
    }

    internal void Bind() {
        GLHandler.BindVbo(this);
    }

    internal void Release() {
        GLHandler.ReleaseVbo(this);
    }

    private void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // TODO: dispose managed state (managed objects)
            }

            GLHandler.DeleteVbo(this);
            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
