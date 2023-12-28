using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SimpleGL.Util;
using System.Reflection;

namespace SimpleGL.Graphics.GLHandling;
public static partial class GLHandler {
    internal static Shader? CreateShader(string key, string vertexShaderSource, string fragmentShaderSource) {

        int programHandle = 0;
        int vertexShaderHandle = CompileShader(vertexShaderSource, ShaderType.VertexShader);
        int fragmentShaderHandle = CompileShader(fragmentShaderSource, ShaderType.FragmentShader);
        Dictionary<string, ShaderUniform> uniforms = new Dictionary<string, ShaderUniform>();
        Dictionary<string, ShaderVertexAttribute> attributes = new Dictionary<string, ShaderVertexAttribute>();
        int stride = 0;

        if (vertexShaderHandle == 0 || fragmentShaderHandle == 0) {
            Log.WriteLine("Could not create vertex- or fragment shader handle.", eLogType.Error);
            return null;
        }

        programHandle = CreateShaderProgram(vertexShaderHandle, fragmentShaderHandle);

        GL.DeleteShader(vertexShaderHandle);    // TODO test if works
        GL.DeleteShader(fragmentShaderHandle);

        if (programHandle == 0) {
            Log.WriteLine("Could not link shader and create program handle.", eLogType.Error);
            return null;
        }
        if (!TryRetrieveShaderUniforms(programHandle, out uniforms)) {
            Log.WriteLine("Could not retrieve shader uniforms.", eLogType.Error);
            GL.DeleteProgram(programHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
            return null;
        }

        if (!TryRetrieveShaderAttributes(programHandle, out attributes)) {
            Log.WriteLine("Could not retrieve shader attributes.", eLogType.Error);
            GL.DeleteProgram(programHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
            return null;
        }

        stride = attributes.Values.Sum(sva => sva.ComponentCount) * sizeof(float);

        ShaderUniform? uniform;
        if (!uniforms.TryGetValue(GraphicsHelper.DEFAULT_SHADER_VIEWPROJECTIONMATRIX_UNIFORM_NAME, out uniform) || uniform.Type != UniformType.Matrix4x4) {
            Log.WriteLine("Shader does not contain uniform 'u_viewProjectionMatrix'.", eLogType.Error);
            GL.DeleteProgram(programHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
            return null;
        }

        if (!uniforms.TryGetValue(GraphicsHelper.DEFAULT_SHADER_MODELMATRIX_UNIFORM_NAME, out uniform) || uniform.Type != UniformType.Matrix4x4) {
            Log.WriteLine("Shader does not contain uniform 'u_viewProjectionMatrix'.", eLogType.Error);
            GL.DeleteProgram(programHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
            return null;
        }

        return new Shader(key, vertexShaderSource, fragmentShaderSource, programHandle, uniforms, attributes, stride);
    }

    private static int CreateShaderProgram(int vertexShaderHandle, int fragmentShaderHandle/*, int geometryShaderHandle*/) {
        int shaderProgram = GL.CreateProgram();

        if (shaderProgram == 0)
            return 0;

        GL.AttachShader(shaderProgram, vertexShaderHandle);
        GL.AttachShader(shaderProgram, fragmentShaderHandle);
        //if (geometryShaderHandle > 0)
        //    GL.AttachShader(shaderProgram, geometryShaderHandle);

        GL.LinkProgram(shaderProgram);
        GL.ValidateProgram(shaderProgram);

        GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out int status);
        if (status == 0)
            return 0;

        return shaderProgram;
    }

    private static int CompileShader(string shaderSource, ShaderType shaderType) {
        if (shaderSource == null)
            return 0;

        int shader = GL.CreateShader(shaderType);

        if (shader == 0)
            return 0;

        GL.ShaderSource(shader, shaderSource);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0) {
            string infoLog = GL.GetShaderInfoLog(shader);
            Log.WriteLine($"{shaderType} compliation error: {infoLog}");
            return 0;
        }

        return shader;
    }

    internal static void DeleteShader(Shader shader) {
        if (!shader.IsCompiled)
            return;

        if (IsShaderBound(shader))
            ReleaseShader(shader);

        FieldInfo phFI = typeof(Shader).GetField("programHandle", BindingFlags.NonPublic | BindingFlags.Instance);
        int ph = (int)phFI.GetValue(shader);
        phFI.SetValue(shader, 0);

        FieldInfo vshFI = typeof(Shader).GetField("vertexShaderHandle", BindingFlags.NonPublic | BindingFlags.Instance);
        int vsh = (int)vshFI.GetValue(shader);
        vshFI.SetValue(shader, 0);

        FieldInfo fshFI = typeof(Shader).GetField("fragmentShaderHandle", BindingFlags.NonPublic | BindingFlags.Instance);
        int fsh = (int)fshFI.GetValue(shader);
        fshFI.SetValue(shader, 0);

        GL.DeleteProgram(ph);
        GL.DeleteShader(vsh);
        GL.DeleteShader(fsh);
    }

    internal static void BindShader(Shader shader) {
        if (shader.Equals(BoundShader))
            return;

        //(int program, int vertexShader, int fragmentShader) handles = GetShaderHandles(shader);

        GL.UseProgram(shader.ProgramHandle);
        BoundShader = shader;
    }

    internal static void ReleaseShader(Shader shader) {
        if (!BoundShader.Equals(shader))
            return;
        //asdasd // TODO set to default shader ?
        GL.UseProgram(0);
        BoundShader = null;
    }

    internal static void SetVertexAttributePointer(ShaderVertexAttribute vertexAttribute, /*Type type,bool normalized,  */int stride) {
        GL.VertexAttribPointer(vertexAttribute.AttributeIndex, vertexAttribute.ComponentCount, /*ToVertexAttribPointerType(type)*/ VertexAttribPointerType.Float, false, stride, vertexAttribute.ByteOffset);
    }

    //private static VertexAttribPointerType ToVertexAttribPointerType(Type type) {
    //    if (type.Equals(typeof(float)))
    //        return VertexAttribPointerType.Float;

    //    if (type.Equals(typeof(int)))
    //        return VertexAttribPointerType.Int;

    //    if (type.Equals(typeof(uint)))
    //        return VertexAttribPointerType.UnsignedInt;

    //    if (type.Equals(typeof(short)))
    //        return VertexAttribPointerType.Short;

    //    if (type.Equals(typeof(ushort)))
    //        return VertexAttribPointerType.UnsignedShort;

    //    if (type.Equals(typeof(byte)))
    //        return VertexAttribPointerType.UnsignedByte;

    //    if (type.Equals(typeof(double)))
    //        return VertexAttribPointerType.Double;

    //    throw new ArgumentException();
    //}

    internal static void EnableVertexAttributeArray(int attributeIndex) {
        GL.EnableVertexAttribArray(attributeIndex);
    }

    internal static void DisableVertexAttributeArray(int attributeIndex) {
        GL.DisableVertexAttribArray(attributeIndex);
    }

    /*private static (int ph, int vsh, int fsh) GetShaderHandles(Shader shader) {
        FieldInfo programHandleFI = shader.GetType().GetField("programHandle", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo vertexShaderHandleFI = shader.GetType().GetField("vertexShaderHandle", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo fragmentShaderHandleFI = shader.GetType().GetField("fragmentShaderHandle", BindingFlags.NonPublic | BindingFlags.Instance);

        int programHandle = (int)programHandleFI.GetValue(shader);
        int vertexShaderHandle = (int)vertexShaderHandleFI.GetValue(shader);
        int fragmentShaderHandle = (int)fragmentShaderHandleFI.GetValue(shader);

        return (programHandle, vertexShaderHandle, fragmentShaderHandle);
    }*/

    private static bool TryRetrieveShaderUniforms(int shaderProgramHandle, out Dictionary<string, ShaderUniform> uniforms) {
        uniforms = new Dictionary<string, ShaderUniform>();

        GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveUniforms, out int uniformCount);

        bool validUniforms = true;
        for (int i = 0; i < uniformCount; i++) {
            string uniformName = GL.GetActiveUniform(shaderProgramHandle, i, out int size, out ActiveUniformType type);
            int uniformLocation = GL.GetUniformLocation(shaderProgramHandle, uniformName);

            if (!IsValidShaderUniformType(type)) {
                Log.WriteLine($"Invalid shader uniform type {type} for uniform '{uniformName}'.", eLogType.Error);
                validUniforms = false;
                break;
            }

            UniformType uniformType = ConvertUniformType(type);

            ShaderUniform uniform = new ShaderUniform(uniformName, uniformType, uniformLocation, size * GraphicUtils.ActiveUniformTypeToSize(type));
            uniforms.Add(uniformName, uniform);
        }

        if (!validUniforms)
            uniforms.Clear();

        return validUniforms;
    }

    private static bool TryRetrieveShaderAttributes(int shaderProgramHandle, out Dictionary<string, ShaderVertexAttribute> attributes) {
        Dictionary<string, ShaderVertexAttribute> tmpAttributes = new Dictionary<string, ShaderVertexAttribute>();

        GL.GetProgram(shaderProgramHandle, GetProgramParameterName.ActiveAttributes, out int attributeCount);

        bool validAttributes = true;
        for (int i = 0; i < attributeCount; i++) {
            string attributeName = GL.GetActiveAttrib(shaderProgramHandle, i, out int size, out ActiveAttribType type);
            int attributeLocation = GL.GetAttribLocation(shaderProgramHandle, attributeName);

            if (!IsValidShaderAttributeType(type)) {
                Log.WriteLine($"Invalid shader attribute type {type} for attribute '{attributeName}'.", eLogType.Error);
                validAttributes = false;
                break;
            }

            ShaderVertexAttribute attribute = new ShaderVertexAttribute(attributeName, attributeLocation, size * GraphicUtils.ActiveAttribTypeToSize(type));
            tmpAttributes.Add(attributeName, attribute);
        }

        IOrderedEnumerable<KeyValuePair<string, ShaderVertexAttribute>> keyValuePairs = tmpAttributes.OrderBy(pair => pair.Value.AttributeIndex);
        attributes = new Dictionary<string, ShaderVertexAttribute>();
        foreach (KeyValuePair<string, ShaderVertexAttribute> pair in keyValuePairs) {
            attributes[pair.Key] = pair.Value;
        }

        if (!validAttributes) {
            attributes.Clear();
            return false;
        }

        int currentOffset = 0;
        List<ShaderVertexAttribute> nonProcessedVAs = new List<ShaderVertexAttribute>(attributes.Values);
        while (nonProcessedVAs.Count > 0) {

            // get smallest index sva
            ShaderVertexAttribute currentSVA = null;
            foreach (ShaderVertexAttribute sva2 in nonProcessedVAs) {
                if (currentSVA == null || sva2.AttributeIndex < currentSVA.AttributeIndex)
                    currentSVA = sva2;
            }

            // set offset
            currentSVA.ByteOffset = currentOffset;

            // update offset
            currentOffset += currentSVA.ComponentCount * sizeof(float);

            // remove from non processed
            nonProcessedVAs.Remove(currentSVA);
        }

        return true;
    }

    private static UniformType ConvertUniformType(ActiveUniformType uniformType) {
        return uniformType switch {
            //case ActiveUniformType.Int: return Shader.UniformType.Int;
            ActiveUniformType.Float => UniformType.Float,
            ActiveUniformType.FloatVec2 => UniformType.FloatVector2,
            ActiveUniformType.FloatVec3 => UniformType.FloatVector3,
            ActiveUniformType.FloatVec4 => UniformType.FloatVector4,
            ActiveUniformType.FloatMat2 => UniformType.Matrix2x2,
            ActiveUniformType.FloatMat3 => UniformType.Matrix3x3,
            ActiveUniformType.FloatMat4 => UniformType.Matrix4x4,
            ActiveUniformType.Sampler2D => UniformType.Texture2D,
            _ => throw new ArgumentException(),
        };
    }

    private static bool IsValidShaderUniformType(ActiveUniformType uniformType) {
        return ActiveUniformType.Sampler2D.Equals(uniformType) ||
                ActiveUniformType.Float.Equals(uniformType) ||
                ActiveUniformType.FloatVec2.Equals(uniformType) ||
                ActiveUniformType.FloatVec3.Equals(uniformType) ||
                ActiveUniformType.FloatVec4.Equals(uniformType) ||
                ActiveUniformType.FloatMat2.Equals(uniformType) ||
                ActiveUniformType.FloatMat3.Equals(uniformType) ||
                ActiveUniformType.FloatMat4.Equals(uniformType);
        //ActiveUniformType.Int.Equals(uniformType);
        //ActiveUniformType.IntVec2.Equals(uniformType) ||
        //ActiveUniformType.IntVec3.Equals(uniformType) ||
        //ActiveUniformType.IntVec4.Equals(uniformType);
    }

    private static bool IsValidShaderAttributeType(ActiveAttribType attributeType) {
        return ActiveAttribType.Float.Equals(attributeType) ||
                ActiveAttribType.FloatVec2.Equals(attributeType) ||
                ActiveAttribType.FloatVec3.Equals(attributeType) ||
                ActiveAttribType.FloatVec4.Equals(attributeType) ||
                ActiveAttribType.FloatMat2.Equals(attributeType) ||
                ActiveAttribType.FloatMat3.Equals(attributeType) ||
                ActiveAttribType.FloatMat4.Equals(attributeType);// ||
                                                                 //ActiveAttribType.Int.Equals(attributeType) ||
                                                                 //ActiveAttribType.IntVec2.Equals(attributeType) ||
                                                                 //ActiveAttribType.IntVec3.Equals(attributeType) ||
                                                                 //ActiveAttribType.IntVec4.Equals(attributeType);
    }

    internal static void SetShaderUniform(int uniformLocation, float v1) {
        GL.Uniform1(uniformLocation, v1);
    }

    internal static void SetShaderUniform(int uniformLocation, float v1, float v2) {
        GL.Uniform2(uniformLocation, v1, v2);
    }

    internal static void SetShaderUniform(int uniformLocation, Vector2 v) {
        GL.Uniform2(uniformLocation, v);
    }

    internal static void SetShaderUniform(int uniformLocation, float v1, float v2, float v3) {
        GL.Uniform3(uniformLocation, v1, v2, v3);
    }

    internal static void SetShaderUniform(int uniformLocation, Vector3 v) {
        GL.Uniform3(uniformLocation, v);
    }

    internal static void SetShaderUniform(int uniformLocation, float v1, float v2, float v3, float v4) {
        GL.Uniform4(uniformLocation, v1, v2, v3, v4);
    }

    internal static void SetShaderUniform(int uniformLocation, Vector4 v) {
        GL.Uniform4(uniformLocation, v);
    }

    internal static void SetShaderUniform(int uniformLocation, Color4 c) {
        GL.Uniform4(uniformLocation, c);
    }

    internal static void SetShaderUniform(int uniformLocation, int v1) {
        GL.Uniform1(uniformLocation, v1);
    }

    //internal static void SetShaderUniform(int uniformLocation, int v1, int v2) {
    //    GL.Uniform2(uniformLocation, v1, v2);
    //}

    //internal static void SetShaderUniform(int uniformLocation, int v1, int v2, int v3) {
    //    GL.Uniform3(uniformLocation, v1, v2, v3);
    //}

    //internal static void SetShaderUniform(int uniformLocation, int v1, int v2, int v3, int v4) {
    //    GL.Uniform4(uniformLocation, v1, v2, v3, v4);
    //}

    internal static void SetShaderUniform(int uniformLocation, Matrix2 v1) {
        GL.UniformMatrix2(uniformLocation, false, ref v1);
    }

    internal static void SetShaderUniform(int uniformLocation, Matrix3 v1) {
        GL.UniformMatrix3(uniformLocation, false, ref v1);
    }

    internal static void SetShaderUniform(int uniformLocation, Matrix4 v1) {
        GL.UniformMatrix4(uniformLocation, false, ref v1);
    }

    internal static bool IsShaderBound(Shader shader) {
        return shader.Equals(BoundShader);
    }


}
