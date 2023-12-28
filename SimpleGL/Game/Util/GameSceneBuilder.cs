namespace SimpleGL.Game.Util;
public abstract class GameSceneBuilder {

    public abstract IReadOnlyList<GameNode> CreateScene();
    public abstract IReadOnlyList<GameNode> CreateGui();
}
