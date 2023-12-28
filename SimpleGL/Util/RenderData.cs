using OpenTK.Mathematics;
using SimpleGL.Graphics;
using SimpleGL.Graphics.Textures;

namespace SimpleGL.Util;
internal struct RenderData {
    public VertexArrayObject VertexArrayObject { get; }
    public Matrix4 ModelMatrix { get; }
    public int ZIndex { get; }
    public Material Material { get; }
    public IReadOnlyList<Texture> Textures { get; }
    public Action? PreRenderCallback { get; }

    public RenderData(VertexArrayObject vertexArrayObject, Matrix4 modelMatrix, int zIndex, Material material, IReadOnlyList<Texture> textures, Action? preRenderCallback) {
        VertexArrayObject = vertexArrayObject;
        ModelMatrix = modelMatrix;
        ZIndex = zIndex;
        Material = material;
        Textures = textures;
        PreRenderCallback = preRenderCallback;
    }
}
