using SimpleGL.Graphics.GLHandling;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util;

namespace SimpleGL.Graphics;
public sealed class VertexArrayObject : IDisposable {
    private ShaderVertexAttributeResolver AttributeResolver { get; }
    public ShaderUniformAssignmentHandler ShaderUniformAssignmentHandler { get; set; }

    private Shader _Shader { get; set; }
    public Shader Shader {
        get => _Shader;
        set {
            if (value == null) {
                Log.WriteLine($"Cannot assign shader to renderable. Shader is null.");
                return;
            }

            if (!value.IsCompiled) {
                Log.WriteLine($"Cannot assign shader to renderable. Shader is not compiled.");
                return;
            }

            _Shader = value;
            IsShaderDirty = true;
        }
    }

    private Mesh _Mesh { get; set; }
    public Mesh Mesh {
        get => _Mesh;
        set {
            if (value == null) {
                Log.WriteLine($"Cannot assign mesh to renderable. Mesh is null.");
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

    private Texture[] _Textures { get; set; }
    public Texture[] Textures {
        get => _Textures;
        set {
            if (value == null)
                value = new Texture[0];

            _Textures = value;
        }
    }


    public VertexBufferObject VertexBufferObject { get; private set; }
    public ElementBufferObject ElementBufferObject { get; private set; }

    private Dictionary<ShaderVertexAttribute, VertexAttribute> ResolvedMeshAttributes { get; }

    internal int VaoId { get; }
    public bool IsBound => GLHandler.IsVaoBound(this);

    public bool IsDisposed => VaoId <= 0;
    private bool disposedValue;

    private bool IsMeshDirty { get; set; }
    private bool IsShaderDirty { get; set; }
    private bool IsDataDirty { get; set; }

    internal VertexArrayObject(
        int vaoId,
        ShaderVertexAttributeResolver attributeResolver,
        ShaderUniformAssignmentHandler shaderUniformAssignmentHandler,
        Shader shader,
        Mesh mesh,
        Texture[] textures) {

        if (attributeResolver == null) {
            Log.WriteLine($"Cannot create renderable, attributeResolver is null.");
            return;
        }

        if (shaderUniformAssignmentHandler == null) {
            Log.WriteLine($"Cannot create renderable, shaderUniformAssignmentHandler is null.");
            return;
        }

        if (shader == null) {
            Log.WriteLine($"Cannot create renderable, shader is null.");
            return;
        }

        if (mesh == null) {
            Log.WriteLine($"Cannot create renderable, mesh is null.");
            return;
        }

        ResolvedMeshAttributes = new Dictionary<ShaderVertexAttribute, VertexAttribute>();

        VaoId = vaoId;

        AttributeResolver = attributeResolver;
        ShaderUniformAssignmentHandler = shaderUniformAssignmentHandler;

        Textures = textures;
        Shader = shader;
        Mesh = mesh;
    }

    ~VertexArrayObject() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Render() {
        Clean();

        if (!GLHandler.IsShaderBound(Shader))
            Shader.Bind();

        foreach (Texture2D texture in Textures)
            texture.Bind();

        GLHandler.BindVao(this);

        foreach (ShaderUniform uniform in Shader.Uniforms.Values)
            ShaderUniformAssignmentHandler(Shader, uniform);

        GLHandler.Render(ElementBufferObject);

        GLHandler.ReleaseVao(this);

        foreach (Texture2D texture in Textures)
            texture.Release();

        //Shader.Release();
    }

    private void Clean() {
        if (IsMeshDirty) {
            Shader.Bind();
            GLHandler.BindVao(this);
            GLHandler.DeleteVbo(VertexBufferObject);
            GLHandler.DeleteEbo(ElementBufferObject);
            ResolvedMeshAttributes.Clear();
            foreach (ShaderVertexAttribute shaderAttribute in Shader.Attributes.Values) {
                VertexAttribute resolvedMeshAttribute = AttributeResolver(shaderAttribute, Mesh.VertexAttributes.Values);
                ResolvedMeshAttributes.Add(shaderAttribute, resolvedMeshAttribute);
            }
            VertexBufferObject = GLHandler.CreateVbo(Mesh.GetInterleavedVertexData(ResolvedMeshAttributes.Values), eBufferType.Dynamic);
            VertexBufferObject.Bind();
            Shader.EnableVertexAttributes();
            Shader.AssignVertexAttributePointers();
            ElementBufferObject = GLHandler.CreateEBO(Mesh.Indices.ToArray(), eBufferType.Static);
            GLHandler.ReleaseVao(this);
            Shader.Release();

            IsMeshDirty = false;
            IsShaderDirty = false;
            IsDataDirty = false;
        } else if (IsShaderDirty) {

            Shader.Bind();
            GLHandler.BindVao(this);
            ResolvedMeshAttributes.Clear();
            foreach (ShaderVertexAttribute shaderAttribute in Shader.Attributes.Values) {
                VertexAttribute resolvedMeshAttribute = AttributeResolver(shaderAttribute, Mesh.VertexAttributes.Values);
                ResolvedMeshAttributes.Add(shaderAttribute, resolvedMeshAttribute);
            }
            VertexBufferObject.Bind();
            VertexBufferObject.SetData(Mesh.GetInterleavedVertexData(ResolvedMeshAttributes.Values));
            Shader.EnableVertexAttributes();
            Shader.AssignVertexAttributePointers();
            VertexBufferObject.Release();
            GLHandler.ReleaseVao(this);
            Shader.Release();

            IsShaderDirty = false;
            IsDataDirty = false;
        } else if (IsDataDirty) {

            GLHandler.BindVao(this);
            VertexBufferObject.Bind();
            VertexBufferObject.SetData(Mesh.GetInterleavedVertexData(ResolvedMeshAttributes.Values));
            VertexBufferObject.Release();
            GLHandler.ReleaseVao(this);

            IsDataDirty = false;
        }
    }

    private void OnMeshVertexDataChanged(Mesh mesh) {
        IsDataDirty = true;
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
