using OpenTK.Mathematics;
using SimpleGL.Game.Util;
using SimpleGL.Graphics;
using SimpleGL.Graphics.Textures;
using SimpleGL.ResourceHandling;
using SimpleGL.Util.Extensions;

namespace SimpleGL.Game.Nodes;
public class Sprite : GameNode, IDisposable {

    public Material Material {
        get => RenderObject.Material;
        set => RenderObject.Material = value;
    }

    public Texture Texture {
        get => RenderObject.Textures[0];
        set {
            RenderObject.Textures[0] = value;

            float[][] textureCoordinates = value.TextureCoordinates.ToArray();
            for (int y = 0; y < 2; y++) {
                for (int x = 0; x < 2; x++) {
                    int i = x + y * 2;
                    VertexData va = Mesh.GetVertexData(i);
                    va.SetAttributeData(Mesh.VertexAttributes["texCoords0"], textureCoordinates[i]);
                }
            }
        }
    }

    private Mesh? Mesh => RenderObject?.Mesh;

    private RenderObject? RenderObject { get; set; }

    private bool disposedValue;

    public Sprite(Guid id)
        : base(id) {
    }

    public Sprite(Texture texture) {
        Mesh mesh = CreateMesh(texture);

        RenderObject = new RenderObject(mesh, Material.CreateDefaultMaterial(1));
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~Sprite() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }


    protected override void Render(float dT) {
        RenderObject?.Render(Transform);
    }

    protected override GameNodeData Serialize() {
        GameNodeData data = base.Serialize();

        GameNodeData spriteData = new GameNodeData();
        data.Set(nameof(Sprite), spriteData);

        spriteData.Set(nameof(Texture), Texture.Key);
        spriteData.Set(nameof(Material), Material.GetType().AssemblyQualifiedName!);
        spriteData.Set(nameof(Material.Shader), Material.Shader.Key);

        return data;
    }

    protected override void Deserialize(GameNodeData data) {
        if (!data.IsData(nameof(Sprite)))
            return;

        GameNodeData spriteData = data.GetData(nameof(Sprite));

        string textureKey = spriteData.GetValue(nameof(Texture));
        if (!ResourceManager.TextureLoader.TryGetResource(textureKey, out Texture2D? texture))
            throw new Exception($"Texture '{textureKey}' not found");

        string shaderKey = spriteData.GetValue(nameof(Material.Shader));
        if (!ResourceManager.ShaderLoader.TryGetResource(shaderKey, out Shader? shader))
            throw new Exception($"Shader '{shaderKey}' not found");

        string materialTypeName = spriteData.GetValue(nameof(Material));
        Type? materialType = Type.GetType(materialTypeName);
        if (materialType == null)
            throw new Exception($"Material type '{materialTypeName}' not found");

        Mesh mesh = CreateMesh(texture!);
        RenderObject = new RenderObject(mesh, Material.CreateDefaultMaterial(1));
        Material = Material.Create(materialType, shader!);
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                RenderObject?.Dispose();
                // dispose managed state (managed objects)
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

    private static Mesh CreateMesh(Texture texture) {
        VertexAttribute va_position = VertexAttribute.Create("position", 3);
        VertexAttribute va_color = VertexAttribute.Create("color", 4);
        VertexAttribute va_texCoords = VertexAttribute.Create("texCoords0", 2);
        VertexAttribute[] vertexAtributes = { va_position, va_color, va_texCoords };

        (uint idx0, uint idx1, uint idx2)[] indices = {
            (0, 1, 2),
            (2, 1, 3)
        };

        float[][] textureCoordinates = texture.TextureCoordinates.ToArray();

        Mesh mesh = GraphicsHelper.CreateMesh(4, vertexAtributes, indices);
        for (int y = 0; y < 2; y++) {
            for (int x = 0; x < 2; x++) {
                int i = x + y * 2;
                VertexData va = mesh.GetVertexData(i);
                va.SetAttributeData(va_position, -0.5f + x, -0.5f + y, 0);
                va.SetAttributeData(va_color, Color4.White.ToArray(true));
                va.SetAttributeData(va_texCoords, textureCoordinates[i]);
            }
        }

        return mesh;
    }
}
