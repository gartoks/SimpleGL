using SimpleGL.Graphics.GLHandling;

namespace SimpleGL.Graphics;
public sealed class ShaderVertexAttribute : VertexAttribute {
    public int AttributeIndex { get; }
    public int ByteOffset { get; set; }

    internal ShaderVertexAttribute(string name, int attributeIndex, int componentCount)
        : base(name, componentCount) {

        AttributeIndex = attributeIndex;
        ByteOffset = -1;
    }

    internal void Enable() {
        GLHandler.EnableVertexAttributeArray(AttributeIndex);
    }

    internal void Disable() {
        GLHandler.DisableVertexAttributeArray(AttributeIndex);
    }
}