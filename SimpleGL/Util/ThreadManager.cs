namespace SimpleGL.Util;
public sealed class ThreadManager {
    private Dictionary<string, ThreadBase> Threads { get; }

    internal ThreadManager() {
        Threads = new();
    }

    public void RegisterGameThread(ThreadBase threadBase) {
        if (Application.State != eApplicationState.Initialized)
            throw new InvalidOperationException("Cannot register game thread while application is not initialized");

        Threads.Add(threadBase.Name, threadBase);
    }

    public ThreadBase GetThread(string name) {
        return Threads[name];
    }

    internal void Start() {
        if (Application.State != eApplicationState.Running)
            throw new InvalidOperationException("Cannot start threads while application is not running");

        foreach (ThreadBase thread in Threads.Values)
            thread.Start();
    }

    internal void Stop() {
        //if (Application.State != eApplicationState.Exiting)
        //    throw new InvalidOperationException("Cannot stop threads while application is not exiting");

        foreach (ThreadBase thread in Threads.Values)
            thread.Stop();
    }

    internal void Join() {
        //if (Application.State != eApplicationState.Exiting)
        //    throw new InvalidOperationException("Cannot stop threads while application is not exiting");

        foreach (ThreadBase thread in Threads.Values)
            thread.Join();
    }
}
