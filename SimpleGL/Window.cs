using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SimpleGL.Game;
using SimpleGL.Graphics;
using SimpleGL.Graphics.GLHandling;
using System.ComponentModel;

namespace SimpleGL;
public sealed class Window : GameWindow {
    public int Fps { get; private set; }
    private float TpsTime { get; set; }
    private int TpsCounter { get; set; }

    private bool IsStarted { get; set; }
    private bool IsLoading { get; set; }

    internal Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings) {

        IsLoading = true;
        IsStarted = false;
    }

    protected override void OnLoad() {
        base.OnLoad();

        GLHandler.Viewport = new Box2i(0, 0, ClientSize.X, ClientSize.Y);
    }

    protected override void OnUnload() {
        base.OnUnload();

        //Application.Instance.OnRenderStop();
    }

    protected override void OnRenderFrame(FrameEventArgs args) {
        base.OnRenderFrame(args);

        GLHandler.ProcessQueue();

        if (!IsStarted) {
            //Application.Instance.OnRenderStart();
            IsStarted = true;
        }

        if (IsLoading) {
            if (!Application.ThreadManager.IsSyncEventSet())
                return;

            IsLoading = false;
            return;
        }

        float deltaTime = (float)args.Time;

        TpsTime += deltaTime;
        TpsCounter++;

        if (TpsTime >= 1f) {
            Fps = TpsCounter;
            TpsTime = 0f;
            TpsCounter = 0;
        }

        GameScene.Render(deltaTime);

        SwapBuffers();
    }

    /*protected override void OnFocusedChanged(FocusedChangedEventArgs e) {
        base.OnFocusedChanged(e);
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e) {
        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e) {
        base.OnKeyUp(e);
    }*/

    public MonitorInfo GetMonitor() {
        return GraphicsHelper.ExecuteGLFunction(() => Monitors.GetMonitorFromWindow(this));
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);

        GLHandler.Viewport = new Box2i(0, 0, e.Width, e.Height);
    }

    protected override void OnClosing(CancelEventArgs e) {
        if (Application.State == eApplicationState.Running) {
            Application.Exit();
        }

        base.OnClosing(e);
    }
}
