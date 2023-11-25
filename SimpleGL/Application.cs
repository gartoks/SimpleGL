using SimpleGL.Util;
using SimpleGL.Util.ThreadBases;

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

    public static Window Window => RenderThread.Window;

    private static RenderThreadBase RenderThread { get; set; }
    private static UpdateThreadBase UpdateThread { get; set; }

    static Application() {
        ThreadManager = new ThreadManager();

        State = eApplicationState.NotInitialized;
    }

    public static void Initialize(Application app) {
        if (State != eApplicationState.NotInitialized)
            throw new InvalidOperationException("Cannot initialize application while it is running");

        Instance = app;
        State = eApplicationState.Initialized;

        UpdateThread = new UpdateThreadBase(Instance.TargetUpdatesPerSecond);
        RenderThread = new RenderThreadBase(Instance.TargetFramesPerSecond);

        ThreadManager.RegisterGameThread(UpdateThread);
        ThreadManager.RegisterGameThread(RenderThread);
    }

    public static void Start() {
        if (State != eApplicationState.Initialized)
            throw new InvalidOperationException("Cannot start application while it is not initialized");

        State = eApplicationState.Running;
        ThreadManager.Start();

        ThreadManager.Join();
    }

    public static void Exit() {
        if (State != eApplicationState.Running)
            throw new InvalidOperationException("Cannot exit application while it is not running");

        State = eApplicationState.Exiting;
        ThreadManager.Stop();
    }

    private int TargetFramesPerSecond { get; }
    private int TargetUpdatesPerSecond { get; }

    public Application(int targetFramesPerSecond, int targetUpdatesPerSecond) {
        TargetFramesPerSecond = targetFramesPerSecond;
        TargetUpdatesPerSecond = targetUpdatesPerSecond;
    }

    public abstract void OnUpdateStart();
    public abstract void OnUpdate(float deltaTime);
    public abstract void OnUpdateStop();

    public abstract void OnRenderStart();
    public abstract void OnRender(float deltaTime);
    public abstract void OnRenderStop();
}
