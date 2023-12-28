using SimpleGL.ResourceHandling;

namespace SimpleGL.Util.ThreadBases;
internal class ResourceThreadBase : ThreadBase {
    public const string NAME = "ResourceThread";

    public ResourceThreadBase(int ticksPerSecond)
        : base(NAME, ticksPerSecond) {
    }

    protected override void OnStart() {
        ResourceManager.Initialize();
        ResourceManager.Load();
    }

    protected override void OnStop() {
        ResourceManager.Unload();
    }

    protected override void Run(float deltaTime) {
        ResourceManager.Update();
    }
}
