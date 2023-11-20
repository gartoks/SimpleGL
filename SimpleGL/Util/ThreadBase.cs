using System.Diagnostics;

namespace SimpleGL.Util;
public abstract class ThreadBase {
    private string Name { get; }
    private int TimePerTick { get; }
    private Stopwatch Stopwatch { get; }
    private Thread Thread { get; }

    public ThreadBase(string name, int ticksPerSecond) {
        Name = name;
        TimePerTick = (int)(1000f / ticksPerSecond);
        Stopwatch = new Stopwatch();

        Thread = new Thread(Run);
    }

    internal void Start() {
        Thread.Start();
    }

    internal void Join() {
        Thread.Join();
    }

    private void Run() {
        OnStart();

        float deltaTime = TimePerTick;
        while (Application.State == eApplicationState.Running) {
            Stopwatch.Restart();
            Run(deltaTime);
            Stopwatch.Stop();

            deltaTime = Stopwatch.ElapsedMilliseconds / 1000f;
            int sleepTime = Stopwatch.ElapsedMilliseconds > TimePerTick ? 0 : TimePerTick - (int)Stopwatch.ElapsedMilliseconds;
            Thread.Sleep(sleepTime);
        }

        OnStop();
    }

    protected abstract void OnStart();

    protected abstract void Run(float deltaTime);

    protected abstract void OnStop();

}
