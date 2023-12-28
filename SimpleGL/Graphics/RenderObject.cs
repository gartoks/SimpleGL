using OpenTK.Mathematics;
using SimpleGL.Game.Util;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util;

namespace SimpleGL.Graphics;
public class RenderObject : IDisposable {
    public VertexArrayObject VertexArrayObject { get; }

    public Texture[] Textures { get; set; }

    private Material _Material { get; set; }
    public Material Material {
        get => _Material;
        set {
            if (value == null) {
                Log.WriteLine($"Cannot assign material to RenderObject. Material is null.");
                return;
            }

            _Material = value;
            IsShaderDirty = true;
        }
    }

    private Mesh _Mesh { get; set; }
    public Mesh Mesh {
        get => _Mesh;
        set {
            if (value == null) {
                Log.WriteLine($"Cannot assign mesh to RenderObject. Mesh is null.");
                return;
            }

            if (Mesh != null) {
                Mesh.OnMeshVertexDataChanged -= OnMeshVertexDataChanged;
            }

            _Mesh = value;
            Mesh.OnMeshVertexDataChanged += OnMeshVertexDataChanged;
            IsMeshDirty = true;
        }
    }

    private Dictionary<ShaderVertexAttribute, VertexAttribute> ResolvedMeshAttributes { get; }

    private bool IsMeshDirty { get; set; }
    private bool IsShaderDirty { get; set; }
    private bool IsDataDirty { get; set; }

    private bool disposedValue;

    public RenderObject(Mesh mesh, Material material) {
        if (material == null) {
            Log.WriteLine($"Cannot create RenderObject. Material is null.");
            return;
        }

        if (mesh == null) {
            Log.WriteLine($"Cannot create RenderObject. Mesh is null.");
            return;
        }

        VertexArrayObject = GraphicsHelper.CreateVertexArrayObject();
        Mesh = mesh;
        Material = material;

        ResolvedMeshAttributes = new Dictionary<ShaderVertexAttribute, VertexAttribute>();

        IsMeshDirty = true;
    }

    public void Render(Transform transform) {
        Render(transform.TransformationMatrix, transform.ZIndex);
    }

    public void Render(Matrix4 modelMatrix, int zIndex) {
        Renderer.ActiveRenderer?.Render(VertexArrayObject, modelMatrix, zIndex, Material, Textures, PreRenderAction);
    }

    protected virtual void PreRenderAction() {
        Clean();
    }

    private void OnMeshVertexDataChanged(Mesh mesh) {
        IsDataDirty = true;
    }

    private VertexAttribute ResolveShaderVertexAttribute(VertexAttribute shaderAttribute, IEnumerable<VertexAttribute> meshAttributes) {
        return meshAttributes.Single(ma => shaderAttribute.Name.Split("_")[1] == ma.Name);
    }

    internal void Clean() {
        if (IsMeshDirty) {
            ResolvedMeshAttributes.Clear();
            foreach (ShaderVertexAttribute shaderAttribute in Material.Shader.Attributes.Values) {
                VertexAttribute resolvedMeshAttribute = ResolveShaderVertexAttribute(shaderAttribute, Mesh.VertexAttributes.Values);
                ResolvedMeshAttributes.Add(shaderAttribute, resolvedMeshAttribute);
            }

            float[] vboData = Mesh.GetInterleavedVertexData(ResolvedMeshAttributes.Values);
            int[] indices = Mesh.Indices.ToArray();

            Material.Shader.Bind();
            VertexArrayObject.UpdateData(Material.Shader, vboData, indices);
            Material.Shader.Release();

            IsMeshDirty = false;
            IsShaderDirty = false;
            IsDataDirty = false;
        } else if (IsShaderDirty) {
            ResolvedMeshAttributes.Clear();
            foreach (ShaderVertexAttribute shaderAttribute in Material.Shader.Attributes.Values) {
                VertexAttribute resolvedMeshAttribute = ResolveShaderVertexAttribute(shaderAttribute, Mesh.VertexAttributes.Values);
                ResolvedMeshAttributes.Add(shaderAttribute, resolvedMeshAttribute);
            }

            float[] vboData = Mesh.GetInterleavedVertexData(ResolvedMeshAttributes.Values);

            Material.Shader.Bind();
            VertexArrayObject.UpdateData(Material.Shader, vboData);
            Material.Shader.Release();

            IsShaderDirty = false;
            IsDataDirty = false;
        } else if (IsDataDirty) {
            float[] vboData = Mesh.GetInterleavedVertexData(ResolvedMeshAttributes.Values);
            VertexArrayObject.UpdateData(vboData);

            IsDataDirty = false;
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                VertexArrayObject.Dispose();
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~RenderObject()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
