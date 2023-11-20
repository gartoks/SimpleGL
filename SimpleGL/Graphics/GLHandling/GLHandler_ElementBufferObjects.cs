using OpenTK.Graphics.OpenGL4;
using SimpleGL.Util;
using System.Runtime.InteropServices;

namespace SimpleGL.Graphics.GLHandling;
public static partial class GLHandler {
    internal static ElementBufferObject CreateEBO(int[] data, eBufferType type) {
        int eboId = GL.GenBuffer();

        ElementBufferObject ebo = new ElementBufferObject(eboId, data, type);

        ElementBufferObject previouslyBoundEbo = BoundElementBufferObject;

        BindEbo(ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, ebo.Size * sizeof(int), (IntPtr)null, GraphicUtils.ToBufferUsageHint(ebo.Type));

        IntPtr mapBufferPtr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.WriteOnly);
        Marshal.Copy(ebo._Data, 0, mapBufferPtr, ebo.Size);
        GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);

        if (previouslyBoundEbo != null)
            BindEbo(previouslyBoundEbo);
        else
            ReleaseEbo(ebo);

        return ebo;
    }

    internal static void UpdateEBOData(ElementBufferObject ebo) {
        ElementBufferObject previouslyBoundEbo = BoundElementBufferObject;

        BindEbo(ebo);

        IntPtr mapBufferPtr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.WriteOnly);
        Marshal.Copy(ebo._Data, 0, mapBufferPtr, ebo.Size);
        GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);

        if (previouslyBoundEbo != null)
            BindEbo(previouslyBoundEbo);
        else
            ReleaseEbo(ebo);
    }

    internal static void BindEbo(ElementBufferObject ebo) {
        if (IsEboBound(ebo))
            return;

        if (ebo.IsDisposed) {
            Log.WriteLine("Cannot bind element buffer object. It is disposed.", eLogType.Error);
            return;
        }

        int eboId = ebo.EboId;

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboId);
        BoundElementBufferObject = ebo;
    }

    internal static void ReleaseEbo(ElementBufferObject ebo) {
        if (!IsEboBound(ebo))
            return;

        if (ebo.IsDisposed) {
            Log.WriteLine("Cannot release element buffer object. It is disposed.", eLogType.Error);
            return;
        }

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        BoundElementBufferObject = null;
    }

    //private static void MapEboData(ElementBufferObject ebo) {
    //    if (ebo.IsDisposed) {
    //        Log.WriteLine("Cannot release element buffer object. It is disposed.", eLogType.Error);
    //        return;
    //    }

    //    ElementBufferObject previouslyBoundEbo = boundElementBufferObject;

    //    BindEbo(ebo);

    //    IntPtr mapBufferPtr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.WriteOnly);
    //    Marshal.Copy(ebo.Data, 0, mapBufferPtr, ebo.Size);
    //    GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);

    //    if (previouslyBoundEbo != null)
    //        BindEbo(previouslyBoundEbo);
    //    else
    //        ReleaseEbo(ebo);
    //}

    internal static void DeleteEbo(ElementBufferObject ebo) {
        if (ebo == null || ebo.IsDisposed)
            return;

        if (ebo.IsBound)
            ReleaseEbo(ebo);

        int eboId = ebo.EboId;
        ReflectionHelper.SetProperty(ebo, "EboId", -1);

        GL.DeleteBuffer(eboId);
    }

    public static bool IsEboBound(ElementBufferObject ebo) => ebo != null && ebo.Equals(BoundElementBufferObject);
}
