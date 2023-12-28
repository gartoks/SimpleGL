using SimpleGL.Audio;
using SimpleGL.Files;
using SimpleGL.Game;
using System.Globalization;

namespace SimpleGL.Util.ThreadBases;
internal sealed class UpdateThreadBase : ThreadBase {
    public const string NAME = "UpdateThread";

    public UpdateThreadBase(int ups)
        : base(NAME, ups) {
    }

    protected override void OnStart() {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        FileManager.Initialize();

        AudioManager.Initialize();
        AudioManager.Load();

        GameScene.Initialize();

        //Application.Instance.OnUpdateStart();
    }

    protected override void Run(float deltaTime) {
        GameScene.Update(deltaTime);
    }

    protected override void OnStop() {

        AudioManager.Unload();

        FileManager.Unload();
        //Application.Instance.OnUpdateStop();
    }
}
