namespace SimpleGL.Util.ThreadBases;
internal sealed class WindowThreadBase : ThreadBase {
    private Window Window { get; }

    public WindowThreadBase(Window window)
        : base("WindowThread", 1) {

        Window = window;
    }

    protected override void OnStart() {
        Window.Run();
    }

    protected override void OnStop() {
        Window.Close();
    }

    protected override void Run(float deltaTime) {
    }
}
