using SimpleGL.Graphics;

namespace SimpleGL.Util;
internal struct RenderData {
    public VertexArrayObject VertexArrayObject { get; }
    public int ZIndex { get; }
    public Action? PreRenderCallback { get; }

    public RenderData(VertexArrayObject vertexArrayObject, int zIndex, Action? preRenderCallback) {
        VertexArrayObject = vertexArrayObject;
        ZIndex = zIndex;
        PreRenderCallback = preRenderCallback;
    }
}
