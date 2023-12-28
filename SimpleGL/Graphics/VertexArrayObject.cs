using SimpleGL.Graphics.GLHandling;

namespace SimpleGL.Graphics;
public sealed class VertexArrayObject : IDisposable {
    public VertexBufferObject VertexBufferObject { get; private set; }
    public ElementBufferObject ElementBufferObject { get; private set; }

    internal int VaoId { get; }
    public bool IsBound => GLHandler.IsVaoBound(this);

    public bool IsDisposed => VaoId <= 0;
    private bool disposedValue;

    internal VertexArrayObject(int vaoId) {
        VaoId = vaoId;
    }

    ~VertexArrayObject() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    internal void UpdateData(Shader shader, float[] vboData, int[] indices) {
        GLHandler.BindVao(this);
        GLHandler.DeleteVbo(VertexBufferObject);
        GLHandler.DeleteEbo(ElementBufferObject);

        VertexBufferObject = GLHandler.CreateVbo(vboData, eBufferType.Dynamic);
        VertexBufferObject.Bind();

        shader.EnableVertexAttributes();
        shader.AssignVertexAttributePointers();

        ElementBufferObject = GLHandler.CreateEBO(indices, eBufferType.Static);
        VertexBufferObject.Release();
        GLHandler.ReleaseVao(this);
    }

    internal void UpdateData(Shader shader, float[] vboData) {
        GLHandler.BindVao(this);
        VertexBufferObject.Bind();
        VertexBufferObject.SetData(vboData);
        shader.EnableVertexAttributes();
        shader.AssignVertexAttributePointers();
        VertexBufferObject.Release();
        GLHandler.ReleaseVao(this);
    }

    internal void UpdateData(float[] vboData) {
        GLHandler.BindVao(this);
        VertexBufferObject.Bind();
        VertexBufferObject.SetData(vboData);
        VertexBufferObject.Release();
        GLHandler.ReleaseVao(this);
    }

    private void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // TODO: dispose managed state (managed objects)
            }

            GLHandler.DeleteVao(this);
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
