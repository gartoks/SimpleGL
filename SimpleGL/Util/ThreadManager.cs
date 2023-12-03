namespace SimpleGL.Util;
public sealed class ThreadManager {
    private Dictionary<string, ThreadBase> Threads { get; }
    private CountdownEvent ThreadSyncEvent { get; }

    internal ThreadManager() {
        Threads = new();
        ThreadSyncEvent = new(0);
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

        ResetSyncEvent();
        foreach (ThreadBase thread in Threads.Values)
            thread.Start();
    }

    internal void Join() {
        //if (Application.State != eApplicationState.Exiting)
        //    throw new InvalidOperationException("Cannot stop threads while application is not exiting");

        foreach (ThreadBase thread in Threads.Values)
            thread.Join();
    }

    internal void SignalSyncEvent() {
        ThreadSyncEvent.Signal();
    }

    internal void WaitForSyncEvent() {
        ThreadSyncEvent.Wait();
    }

    internal bool IsSyncEventSet() {
        return ThreadSyncEvent.IsSet;
    }

    internal void ResetSyncEvent() {
        ThreadSyncEvent.Reset(Threads.Count);
    }
}
