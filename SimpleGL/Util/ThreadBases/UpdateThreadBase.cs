namespace SimpleGL.Util.ThreadBases;
internal sealed class UpdateThreadBase : ThreadBase {
    public UpdateThreadBase(int ups)
        : base("UpdateThread", ups) {
    }

    protected override void OnStart() {
        Application.Instance.OnUpdateStart();
    }

    protected override void Run(float deltaTime) {
        Application.Instance.OnUpdate(deltaTime);
    }

    protected override void OnStop() {
        Application.Instance.OnUpdateStop();
    }
}
