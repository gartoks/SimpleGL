using OpenTK.Mathematics;
using SimpleGL.Graphics.GLHandling;
using SimpleGL.Graphics.Textures;

namespace SimpleGL.Graphics;
public enum UniformType { Float, FloatVector2, FloatVector3, FloatVector4, /*Int,*/ Texture2D, Matrix2x2, Matrix3x3, Matrix4x4 }

public sealed class ShaderUniform : IEquatable<ShaderUniform?> {
    public string Name { get; }
    public UniformType Type { get; }
    public int UniformLocation { get; }
    public int ComponentCount { get; }
    public Shader Shader { get; private set; }

    internal ShaderUniform(string name, UniformType type, int uniformLocation, int componentCount) {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
        UniformLocation = uniformLocation;
        ComponentCount = componentCount;
    }

    internal void AssignToShader(Shader shader) {
        if (Shader != null)
            throw new InvalidOperationException("Cannot assign shader to uniform. Uniform is already assigned to a shader.");

        Shader = shader;
    }

    public void Set(float v1) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1);
    }

    public void Set(float v1, float v2) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1, v2);
    }

    public void Set(Vector2 v1) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1);
    }

    public void Set(float v1, float v2, float v3) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1, v2, v3);
    }

    public void Set(Vector3 v1) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1);
    }

    public void Set(float v1, float v2, float v3, float v4) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1, v2, v3, v4);
    }

    public void Set(Vector4 v1) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1);
    }

    public void Set(Color4 c) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, c);
    }

    public void Set(int v1) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1);
    }

    //public void Set(int v1, int v2) {
    //    GL.Uniform2(uniformLocation, v1, v2);
    //}

    //public void Set(int v1, int v2, int v3) {
    //    GL.Uniform3(uniformLocation, v1, v2, v3);
    //}

    //public void Set(int v1, int v2, int v3, int v4) {
    //    GL.Uniform4(uniformLocation, v1, v2, v3, v4);
    //}

    public void Set(Matrix2 v1) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1);
    }

    public void Set(Matrix3 v1) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1);
    }

    public void Set(Matrix4 v1) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, v1);
    }

    public void Set(Texture texture) {
        if (!GLHandler.IsRendering || !GLHandler.IsShaderBound(Shader))
            throw new InvalidOperationException("Cannot set shader uniform while not rendering or while shader is not bound.");

        int texUnit = GLHandler.AssignedTextureUnit(texture);
        if (texUnit < 0)
            throw new InvalidOperationException("Cannot set shader uniform to texture that is not bound.");

        GLHandler.SetShaderUniform(UniformLocation, texUnit);
    }



    public override string ToString() {
        return $"{Name} [{Type}]:{ComponentCount}";
    }

    public override bool Equals(object? obj) => Equals(obj as ShaderUniform);
    public bool Equals(ShaderUniform? other) => other is not null && Name == other.Name && ComponentCount == other.ComponentCount;
    public override int GetHashCode() => HashCode.Combine(Name, ComponentCount);

    public static bool operator ==(ShaderUniform? left, ShaderUniform? right) => EqualityComparer<ShaderUniform>.Default.Equals(left, right);
    public static bool operator !=(ShaderUniform? left, ShaderUniform? right) => !(left == right);
}