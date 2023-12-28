using OpenTK.Windowing.Desktop;
using SimpleGL.Graphics.GLHandling;
using SimpleGL.Util;
using SimpleGL.Util.ThreadBases;
using System.Globalization;

namespace SimpleGL;

public enum eApplicationState {
    NotInitialized,
    Initialized,
    Running,
    Exiting,
}

public abstract class Application {
    internal static Application Instance { get; set; }
    public static eApplicationState State { get; private set; }
    public static ThreadManager ThreadManager { get; }

    public static Window Window { get; }

    static Application() {
        Window = new Window(GameWindowSettings.Default, NativeWindowSettings.Default);

        ThreadManager = new ThreadManager();

        State = eApplicationState.NotInitialized;
    }

    public static void Initialize(Application app) {
        if (State != eApplicationState.NotInitialized)
            throw new InvalidOperationException("Cannot initialize application while it is running");

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        Instance = app;
        State = eApplicationState.Initialized;

        Window.UpdateFrequency = app.TargetFramesPerSecond;
        GLHandler.Initialize();

        ThreadManager.RegisterGameThread(new UpdateThreadBase(Instance.TargetUpdatesPerSecond));
        ThreadManager.RegisterGameThread(new ResourceThreadBase(Instance.TargetResourceOperationsPerSecond));

        Instance.OnInitialize();
    }

    public static void Start() {
        if (State != eApplicationState.Initialized)
            throw new InvalidOperationException("Cannot start application while it is not initialized");

        State = eApplicationState.Running;
        ThreadManager.Start();

        Window.Run();

        ThreadManager.WaitForSyncEvent();

        ThreadManager.Join();
        Window.Dispose();
    }

    public static void Exit() {
        if (State != eApplicationState.Running)
            throw new InvalidOperationException("Cannot exit application while it is not running");

        ThreadManager.ResetSyncEvent();
        State = eApplicationState.Exiting;
        Window.Close();
    }

    private int TargetFramesPerSecond { get; }
    private int TargetUpdatesPerSecond { get; }
    private int TargetResourceOperationsPerSecond { get; }

    public Application(int targetFramesPerSecond, int targetUpdatesPerSecond, int targetResourceOperationsPerSecond) {
        TargetFramesPerSecond = targetFramesPerSecond;
        TargetUpdatesPerSecond = targetUpdatesPerSecond;
        TargetResourceOperationsPerSecond = targetResourceOperationsPerSecond;
    }

    protected virtual void OnInitialize() { }
}
