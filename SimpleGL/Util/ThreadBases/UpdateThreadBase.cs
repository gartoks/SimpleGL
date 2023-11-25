namespace SimpleGL.Util.ThreadBases;
internal sealed class UpdateThreadBase : ThreadBase {
    public const string NAME = "UpdateThread";

    public UpdateThreadBase(int ups)
        : base(NAME, ups) {
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
