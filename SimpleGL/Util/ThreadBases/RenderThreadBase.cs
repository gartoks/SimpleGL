/*using OpenTK.Windowing.Desktop;
using SimpleGL.Graphics.GLHandling;

namespace SimpleGL.Util.ThreadBases;
internal sealed class RenderThreadBase : ThreadBase {
    public const string NAME = "RenderThread";

    private int TargetFps { get; }
    internal Window Window { get; private set; }

    public override int Tps => Window.Fps;

    public RenderThreadBase(int fps)
        : base(NAME, 1) {
        TargetFps = fps;
    }

    internal override void Stop() {
        base.Stop();
        Window.Close();
    }

    protected override void OnStart() {
        GLFWProvider.EnsureInitialized();
        bool iomt = GLFWProvider.IsOnMainThread;
        Window = new Window(GameWindowSettings.Default, NativeWindowSettings.Default);
        Window.UpdateFrequency = TargetFps;

        GLHandler.Initialize();
    }

    protected override void OnStop() {
        Window.Close();
        Window.Dispose();
    }

    protected override void Run(float deltaTime) {
        Window.Run();
    }
}
*/