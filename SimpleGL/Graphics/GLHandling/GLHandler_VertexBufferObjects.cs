using OpenTK.Graphics.OpenGL4;
using SimpleGL.Util;
using SimpleGL.Util.Extensions;
using System.Runtime.InteropServices;

namespace SimpleGL.Graphics.GLHandling;
public static partial class GLHandler {

    internal static VertexBufferObject CreateVbo(float[] data, eBufferType type) {
        int vboId = GL.GenBuffer();

        VertexBufferObject vbo = new VertexBufferObject(vboId, data, type);

        VertexBufferObject previouslyBoundVbo = BoundVertexBufferObject;

        BindVbo(vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vbo.Size * sizeof(float), (IntPtr)null, GraphicUtils.ToBufferUsageHint(vbo.Type));

        IntPtr mapBufferPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
        Marshal.Copy(vbo.GetData(), 0, mapBufferPtr, vbo.Size);
        GL.UnmapBuffer(BufferTarget.ArrayBuffer);

        if (previouslyBoundVbo != null)
            BindVbo(previouslyBoundVbo);
        else
            ReleaseVbo(vbo);

        return vbo;
    }

    internal static void UpdateVboData(VertexBufferObject vbo) {
        VertexBufferObject previouslyBoundVbo = BoundVertexBufferObject;

        BindVbo(vbo);

        //GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(vbo.Size * sizeof(float)), vbo.Data);

        IntPtr mapBufferPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
        Marshal.Copy(vbo.GetData(), 0, mapBufferPtr, vbo.Size);
        GL.UnmapBuffer(BufferTarget.ArrayBuffer);

        if (previouslyBoundVbo != null)
            BindVbo(previouslyBoundVbo);
        else
            ReleaseVbo(vbo);
    }

    internal static void BindVbo(VertexBufferObject vbo) {
        if (IsVboBound(vbo))
            return;

        if (vbo.IsDisposed) {
            Log.WriteLine("Cannot bind vertex buffer object. It is disposed.", eLogType.Error);
            return;
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VboId);
        BoundVertexBufferObject = vbo;
    }

    internal static void ReleaseVbo(VertexBufferObject vbo) {
        if (!IsVboBound(vbo))
            return;

        if (vbo.IsDisposed) {
            Log.WriteLine("Cannot release vertex buffer object. It is disposed.", eLogType.Error);
            return;
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        BoundVertexBufferObject = null;
    }

    //internal static void MapVboData(VertexBufferObject vbo) {
    //    if (vbo.IsDisposed) {
    //        Log.WriteLine("Cannot release vertex buffer object. It is disposed.", eLogType.Error);
    //        return;
    //    }

    //    VertexBufferObject previouslyBoundVbo = boundVertexBufferObject;

    //    BindVbo(vbo);

    //    IntPtr mapBufferPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
    //    Marshal.Copy(vbo.Data, 0, mapBufferPtr, vbo.Data.Length);
    //    GL.UnmapBuffer(BufferTarget.ArrayBuffer);

    //    if (previouslyBoundVbo != null)
    //        BindVbo(previouslyBoundVbo);
    //    else
    //        ReleaseVbo(vbo);
    //}

    internal static void DeleteVbo(VertexBufferObject vbo) {
        if (vbo == null || vbo.IsDisposed)
            return;

        if (vbo.IsBound)
            ReleaseVbo(vbo);

        int vboId = vbo.VboId;
        ReflectionExtensions.SetProperty(vbo, "VboId", -1);

        GL.DeleteBuffer(vboId);
    }

    internal static bool IsVboBound(VertexBufferObject vbo) => vbo != null && vbo.Equals(BoundVertexBufferObject);
}
