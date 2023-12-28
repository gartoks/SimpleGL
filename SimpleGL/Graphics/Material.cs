using OpenTK.Mathematics;
using System.Reflection;

namespace SimpleGL.Graphics;
public abstract class Material {
    public static Material Create(Type type, Shader shader) {
        if (!typeof(Material).IsAssignableFrom(type))
            throw new ArgumentException($"Cannot create Material. Type {type} is not a Material.");

        ConstructorInfo? ctor = type.GetConstructor(new Type[] { typeof(Shader) });
        if (ctor == null)
            throw new ArgumentException($"Cannot create Material. Type {type} does not have a constructor with a single Shader parameter.");

        Material material = (Material)ctor.Invoke(new object[] { shader });
        return material;
    }

    public static Material CreateDefaultMaterial(int textureCount) => new DefaultMaterial(textureCount);

    public Shader Shader { get; }
    public Color4 Color { get; set; }

    public Material(Shader shader) {
        if (!shader.IsCompiled)
            throw new ArgumentException($"Cannot assign shader to Material. Shader is not compiled.");

        Shader = shader;
        Color = Color4.White;
    }

    public abstract void AssignShaderUniform(Shader shader, ShaderUniform uniform);

    public virtual VertexAttribute ResolveShaderVertexAttribute(VertexAttribute shaderAttribute, IEnumerable<VertexAttribute> meshAttributes) {
        return meshAttributes.Single(ma => shaderAttribute.Name.Split("_")[1] == ma.Name);
    }
}

internal class DefaultMaterial : Material {
    public DefaultMaterial(int textureCount)
        : base(GraphicsHelper.CreateDefaultShader(true, textureCount)) {
    }

    public override void AssignShaderUniform(Shader shader, ShaderUniform uniform) {
        string name = uniform.Name;

        if (name == "u_color" && uniform.Type == UniformType.FloatVector4)
            uniform.Set(Color);
    }
}
