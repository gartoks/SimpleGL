using OpenTK.Graphics.OpenGL4;
using SimpleGL.Util;

namespace SimpleGL.Graphics.GLHandling;
public static partial class GLHandler {
    internal static int CreateVao() {
        return GL.GenVertexArray();
    }

    internal static void BindVao(VertexArrayObject vao) {
        if (IsVaoBound(vao))
            return;

        if (vao.IsDisposed) {
            Log.WriteLine("Cannot bind vertex array object. It is disposed.", eLogType.Error);
            return;
        }

        int vaoId = vao.VaoId;

        GL.BindVertexArray(vaoId);
        BoundVertexArrayObject = vao;
    }

    internal static void ReleaseVao(VertexArrayObject vao) {
        if (!IsVaoBound(vao))
            return;

        if (vao.IsDisposed) {
            Log.WriteLine("Cannot release vertex array object. It is disposed.", eLogType.Error);
            return;
        }

        GL.BindVertexArray(0);
        BoundVertexArrayObject = null;
    }

    internal static void DeleteVao(VertexArrayObject vao) {
        if (vao == null || vao.IsDisposed)
            return;

        if (vao.IsBound)
            ReleaseVao(vao);

        int vaoId = vao.VaoId;
        ReflectionHelper.SetProperty(vao, "VaoId", -1);

        GL.DeleteVertexArray(vaoId);
    }

    public static bool IsVaoBound(VertexArrayObject vao) => vao != null && vao.Equals(BoundVertexArrayObject);
}
