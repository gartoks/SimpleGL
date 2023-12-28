using OpenTK.Mathematics;
using SimpleGL.Graphics;
using SimpleGL.Util.Extensions;

namespace SimpleGL.Game.Nodes;
public class RectanglePrimitive : GameNode, IDisposable {
    private RenderObject RenderObject { get; }

    private bool disposedValue;

    public RectanglePrimitive() {
        Mesh mesh = CreateMesh();
        RenderObject = new RenderObject(mesh, Material.CreateDefaultMaterial(0));
    }

    public RectanglePrimitive(Guid id)
        : base(id) {

        Mesh mesh = CreateMesh();
        RenderObject = new RenderObject(mesh, Material.CreateDefaultMaterial(0));
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~RectanglePrimitive() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    protected override void Render(float dT) {
        if (!Renderer.HasActiveRenderer)
            throw new InvalidOperationException("No renderer is active.");

        RenderObject.Render(Transform);
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // dispose managed state (managed objects)
                RenderObject.Dispose();
            }

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

    private static Mesh CreateMesh() {
        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        VertexAttribute[] vertexAtributes = { va_position, va_color };

        (uint idx0, uint idx1, uint idx2)[] indices = {
            (0, 1, 2),
            (2, 1, 3)
        };

        Mesh mesh = GraphicsHelper.CreateMesh(4, vertexAtributes, indices);
        for (int y = 0; y < 2; y++) {
            for (int x = 0; x < 2; x++) {
                int i = x + y * 2;
                VertexData va = mesh.GetVertexData(i);
                va.SetAttributeData(va_position, -0.5f + x, -0.5f + y, 0);
                va.SetAttributeData(va_color, Color4.White.ToArray(true));
            }
        }

        return mesh;
    }
}
