using OpenTK.Mathematics;
using SimpleGL.Graphics.GLHandling;
using SimpleGL.Graphics.Textures;
using SimpleGL.Util;

namespace SimpleGL.Graphics;
public sealed class Shader : IEquatable<Shader?>, IDisposable {
    public string Key { get; }
    private string VertexShaderSource { get; }
    private string FragmentShaderSource { get; }
    //internal string geometryShaderSource;

    public int ProgramHandle { get; }

    public IReadOnlyDictionary<string, ShaderUniform> Uniforms { get; }
    public IReadOnlyDictionary<string, ShaderVertexAttribute> Attributes { get; }

    private int Stride { get; }

    public bool IsCompiled => ProgramHandle > 0;
    public bool IsBound => IsCompiled && GLHandler.IsShaderBound(this);

    private bool disposedValue;

    internal Shader(string key, string vertexShaderSource, string fragmentShaderSource, int programHandle, Dictionary<string, ShaderUniform> uniforms, Dictionary<string, ShaderVertexAttribute> attributes, int stride) {
        Key = key;
        VertexShaderSource = vertexShaderSource;
        FragmentShaderSource = fragmentShaderSource;
        ProgramHandle = programHandle;
        Uniforms = uniforms;
        Attributes = attributes;
        Stride = stride;

        foreach (ShaderUniform uniform in Uniforms.Values) {
            uniform.AssignToShader(this);
        }
    }

    //public Shader(string vertexShaderSourceCode, string fragmentShaderSourceCode/*, string geometryShaderSourceCode = null*/) {
    //    vertexShaderSource = vertexShaderSourceCode;
    //    fragmentShaderSource = fragmentShaderSourceCode;
    //    //this.geometryShaderSource = geometryShaderSourceCode;

    //    GLHandler.InitializeShader(this);
    //}

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~Shader() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    internal void Bind() {
        if (!IsCompiled) {
            Log.WriteLine("Cannot bind shader, shader is not compiled.", eLogType.Warning);
            return;
        }

        GLHandler.BindShader(this);
    }

    internal void Release() {
        if (!IsCompiled)
            return;

        GLHandler.ReleaseShader(this);
    }

    internal void EnableVertexAttributes() {
        if (!IsBound) {
            Log.WriteLine("Cannot bind shader vertex attributes, shader is not bound.", eLogType.Warning);
            return;
        }

        foreach (ShaderVertexAttribute sva in Attributes.Values) {
            sva.Enable();
        }
    }

    internal void DisableVertexAttributes() {
        if (!IsBound) {
            Log.WriteLine("Cannot release shader vertex attributes, shader is not bound.", eLogType.Warning);
            return;
        }

        foreach (ShaderVertexAttribute sva in Attributes.Values) {
            sva.Disable();
        }
    }

    internal void AssignVertexAttributePointers() {
        //if (!IsBound) {
        //    Log.WriteLine("Cannot assign shader vertex attributes pointers, shader is not bound.", eLogType.Warning);
        //    return;
        //}

        foreach (ShaderVertexAttribute sva in Attributes.Values) {
            GLHandler.SetVertexAttributePointer(sva, Stride);
        }
    }

    //public bool HasGeometryShader {
    //    get => this.geometryShaderSource != null && this.geometryShaderHandle > 0;
    //}

    public bool HasUniform(string uniformName) {
        return Uniforms.ContainsKey(uniformName);
    }

    public bool HasAttribute(string attributeName) {
        return Attributes.ContainsKey(attributeName);
    }

    internal int GetUniformLocation(string uniformName) {
        if (Uniforms.TryGetValue(uniformName, out ShaderUniform? u))
            return u.UniformLocation;

        return -1;
    }

    internal int GetAttributeLocation(string attributeName) {
        if (Attributes.TryGetValue(attributeName, out ShaderVertexAttribute? u))
            return u.AttributeIndex;

        return -1;
    }

    #region Uniforms
    internal void SetUniform(string uniformName, float value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value);
    }

    internal void SetUniform(int uniformLocation, float value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value);
    }

    internal void SetUniform(string uniformName, float value1, float value2) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value1, value2);
    }

    internal void SetUniform(int uniformLocation, float value1, float value2) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value1, value2);
    }

    internal void SetUniform(string uniformName, float value1, float value2, float value3) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value1, value2, value3);
    }

    internal void SetUniform(int uniformLocation, float value1, float value2, float value3) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value1, value2, value3);
    }

    internal void SetUniform(string uniformName, float value1, float value2, float value3, float value4) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value1, value2, value3, value4);
    }

    internal void SetUniform(int uniformLocation, float value1, float value2, float value3, float value4) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value1, value2, value3, value4);
    }

    internal void SetUniform(string uniformName, Texture2D value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value);
    }

    internal void SetUniform(int uniformLocation, Texture2D value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        if (!value.IsBound) {
            Log.WriteLine("Cannot assign texture shader uniform. Texture is not assigned to a texture unit.", eLogType.Error);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, GLHandler.AssignedTextureUnit(value));
    }

    internal void SetUniform(string uniformName, Vector2 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value);
    }

    internal void SetUniform(int uniformLocation, Vector2 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value.X, value.Y);
    }

    internal void SetUniform(string uniformName, Vector3 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value);
    }

    internal void SetUniform(int uniformLocation, Vector3 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value.X, value.Y, value.Z);
    }

    internal void SetUniform(string uniformName, Vector4 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value);
    }

    internal void SetUniform(int uniformLocation, Vector4 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value.X, value.Y, value.Z, value.W);
    }

    internal void SetUniform(string uniformName, Color4 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value);
    }

    internal void SetUniform(int uniformLocation, Color4 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value.R, value.G, value.B, value.A);
    }

    internal void SetUniform(string uniformName, Matrix2 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value);
    }

    internal void SetUniform(int uniformLocation, Matrix2 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value);
    }

    internal void SetUniform(string uniformName, Matrix3 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value);
    }

    internal void SetUniform(int uniformLocation, Matrix3 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value);
    }

    internal void SetUniform(string uniformName, Matrix4 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        int location;
        if ((location = GetUniformLocation(uniformName)) == -1) {
            Log.WriteLine($"Could not find uniform of name '{uniformName}' location.", eLogType.Warning);
            return;
        }

        SetUniform(location, value);
    }

    internal void SetUniform(int uniformLocation, Matrix4 value) {
        if (!IsBound) {
            Log.WriteLine("Cannot set shader uniform, shader is not bound.", eLogType.Warning);
            return;
        }

        if (uniformLocation == -1) {
            Log.WriteLine($"Could not find uniform location {uniformLocation}.", eLogType.Warning);
            return;
        }

        GLHandler.SetShaderUniform(uniformLocation, value);
    }
    #endregion

    //public void setVertexAttribute(String attributeName, VertexAttribute attribute, int vertexByteSize, int offset) {   // FIXME (re)move
    //    setVertexAttribute(getAttributeLocation(attributeName), attribute, vertexByteSize, offset);
    //}
    // TOOD
    //public void setVertexAttribute(int attributeLocation, VertexAttribute attribute, int vertexByteSize, int offset) {  // FIXME (re)move
    //    if (attributeLocation < 0 || attribute == null || !isBound())
    //        return;

    //    glVertexAttribPointer(attributeLocation, attribute.numComponents, GL_DATA_TYPE.FLOAT, false, vertexByteSize, offset);
    //}

    public override bool Equals(object? obj) => Equals(obj as Shader);
    public bool Equals(Shader? other) => other is not null && ProgramHandle == other.ProgramHandle && IsCompiled == other.IsCompiled;
    public override int GetHashCode() => HashCode.Combine(ProgramHandle, IsCompiled);

    public static bool operator ==(Shader? left, Shader? right) => EqualityComparer<Shader>.Default.Equals(left, right);
    public static bool operator !=(Shader? left, Shader? right) => !(left == right);

    private void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // dispose managed state (managed objects)
            }

            GraphicsHelper.DisposeShader(this);
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
