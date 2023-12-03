/*using SimpleGL.Graphics.GLHandling;
using System.Diagnostics;

namespace SimpleGL.Util.ThreadBases;
internal sealed class GLThreadBase : ThreadBase {
    private Window Window { get; }

    public GLThreadBase(Window window, int fps)
        : base("GlThread", fps) {
        Window = window;
    }

    protected override void OnStart() {
        //Window.MakeCurrent();
        //Thread.Sleep(1000);
        //Window.Context.MakeCurrent();
        //GL.LoadBindings(new GLFWBindingsContext());
        Window.IsVisible = true;
    }

    protected override void Run(float deltaTime) {
        GLHandler.BeginRendering();

        Debug.WriteLine("Render");
        // TODO

        GLHandler.EndRendering();
        Window.SwapBuffers();
        Window.ProcessEvents(0);
    }

    protected override void OnStop() {
        Window.Close();
        Window.Dispose();
    }
}
*/