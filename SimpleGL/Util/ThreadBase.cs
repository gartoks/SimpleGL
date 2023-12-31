﻿using System.Diagnostics;

namespace SimpleGL.Util;
public abstract class ThreadBase {
    public string Name { get; }
    public int TargetTimePerTick { get; }
    private Stopwatch Stopwatch { get; }
    private Thread Thread { get; }

    public virtual int Tps { get; private set; }
    private float TpsTime { get; set; }
    private int TpsCounter { get; set; }

    public ThreadBase(string name, int ticksPerSecond) {
        Name = name;
        TargetTimePerTick = (int)(1000f / ticksPerSecond);
        Stopwatch = new Stopwatch();

        Thread = new Thread(Run);
        Thread.Name = name;
        Tps = ticksPerSecond;
    }

    internal void Start() {
        Thread.Start();
    }

    internal void Join() {
        Thread.Join();
    }

    private void Run() {
        OnStart();
        Application.ThreadManager.SignalSyncEvent();
        Application.ThreadManager.WaitForSyncEvent();

        float deltaTime = TargetTimePerTick;
        while (Application.State == eApplicationState.Running) {
            Stopwatch.Restart();
            Run(deltaTime);
            Stopwatch.Stop();


            deltaTime = Stopwatch.ElapsedMilliseconds / 1000f;
            TpsTime += deltaTime;
            TpsCounter++;

            if (TpsTime >= 1f) {
                Tps = TpsCounter;
                TpsTime = 0f;
                TpsCounter = 0;
            }

            int sleepTime = Stopwatch.ElapsedMilliseconds > TargetTimePerTick ? 0 : TargetTimePerTick - (int)Stopwatch.ElapsedMilliseconds;
            Thread.Sleep(sleepTime);
        }

        Application.ThreadManager.SignalSyncEvent();
        Application.ThreadManager.WaitForSyncEvent();

        OnStop();
    }

    protected abstract void OnStart();

    protected abstract void Run(float deltaTime);

    protected abstract void OnStop();

}
