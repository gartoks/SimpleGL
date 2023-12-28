using OpenTK.Mathematics;
using SimpleGL.Files;
using SimpleGL.Game.Nodes;
using SimpleGL.Game.Util;
using SimpleGL.Graphics;
using SimpleGL.Graphics.GLHandling;
using System.Reflection;
using System.Text.Json;

namespace SimpleGL.Game;
public static class GameScene {
    public static string Name { get; private set; } = string.Empty;

    public static Camera? MainCamera { get; set; }

    private static Dictionary<string, GameSceneBuilder> SceneBuilders { get; } = new Dictionary<string, GameSceneBuilder>();

    internal static GameNode GameRoot { get; } = GameNode.CreateRoot();
    internal static GameNode GuiRoot { get; } = GameNode.CreateRoot();

    private static Matrix4 GuiProjectionMatrix { get; set; }
    private static Renderer Renderer { get; } = new Renderer();

    private static ManualResetEvent NodeLockEvent { get; } = new ManualResetEvent(true);
    private static ManualResetEvent RenderLockEvent { get; } = new ManualResetEvent(true);
    private static ManualResetEvent UpdateLockEvent { get; } = new ManualResetEvent(true);

    public static event Action<string> OnSceneUnloaded = delegate { };
    public static event Action<string> OnSceneLoaded = delegate { };

    internal static void Initialize() {
        GuiProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, GLHandler.Viewport.Size.X, GLHandler.Viewport.Size.Y, 0, -1, 1);
        GLHandler.ViewportChanged += OnViewportChanged;
    }

    internal static void Render(float dT) {
        if (MainCamera == null)
            throw new InvalidOperationException("Cannot render scene without a camera.");

        NodeLockEvent.WaitOne();
        RenderLockEvent.Reset();

        Renderer.BeginRendering(MainCamera);
        GameRoot.RenderNode(dT);
        Renderer.EndRendering();

        Renderer.BeginRendering(GuiProjectionMatrix);
        GuiRoot.RenderNode(dT);
        Renderer.EndRendering();

        RenderLockEvent.Set();
    }

    internal static void Update(float dT) {
        NodeLockEvent.WaitOne();
        UpdateLockEvent.Reset();

        GameRoot.UpdateNode(dT);
        GuiRoot.UpdateNode(dT);

        UpdateLockEvent.Set();
    }

    public static void RegisterSceneBuilder(string name, GameSceneBuilder builder, bool overwrite = false) {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cannot register scene builder. Name cannot be null or whitespace.", nameof(name));

        if (!overwrite && SceneBuilders.ContainsKey(name))
            throw new InvalidOperationException($"Cannot register scene builder. A scene builder with the name '{name}' already exists.");

        SceneBuilders.Add(name, builder);
    }

    public static void Create(string name) {
        if (!SceneBuilders.TryGetValue(name, out GameSceneBuilder? builder))
            throw new InvalidOperationException($"Cannot create scene. No scene builder with the name '{name}' could be found.");

        if (!string.IsNullOrEmpty(Name))
            OnSceneUnloaded(Name);

        NodeLockEvent.WaitOne();
        NodeLockEvent.Reset();
        RenderLockEvent.WaitOne();
        UpdateLockEvent.WaitOne();

        foreach (GameNode node in GameRoot.Children.ToList())
            node.Destroy();
        foreach (GameNode node in GuiRoot.Children.ToList())
            node.Destroy();

        Name = name;

        IReadOnlyList<GameNode> nodes = builder.CreateScene();
        IReadOnlyList<GameNode> gui = builder.CreateGui();

        GameNode gameNode = new GameNode();
        foreach (GameNode node in nodes)
            node.Parent = gameNode;

        GameNode guiNode = new GameNode();
        foreach (GameNode node in gui)
            node.Parent = guiNode;

        if (!string.IsNullOrEmpty(Name))
            OnSceneLoaded(Name);

        NodeLockEvent.Set();
    }

    public static void Load(string name) {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cannot load scene. Name cannot be null or whitespace.", nameof(name));

        if (!FileManager.IsSaveFileLoaded || !FileManager.LoadedSaveFile!.FileExists($"{name}.json")) {
            NodeLockEvent.Set();
            Create(name);
            return;
        }

        if (!string.IsNullOrEmpty(Name))
            OnSceneUnloaded(Name);

        NodeLockEvent.WaitOne();
        NodeLockEvent.Reset();
        RenderLockEvent.WaitOne();
        UpdateLockEvent.WaitOne();

        foreach (GameNode node in GameRoot.Children.ToList())
            node.Destroy();
        foreach (GameNode node in GuiRoot.Children.ToList())
            node.Destroy();

        Name = name;

        string jsonString = string.Empty;
        FileManager.LoadedSaveFile!.ReadFileStream($"{name}.json", s => {
            using StreamReader sr = new StreamReader(s);
            jsonString = sr.ReadToEnd();
        });

        if (string.IsNullOrEmpty(jsonString))
            throw new InvalidOperationException($"Cannot load scene. Scene file '{name}.json' is empty.");

        Dictionary<string, object> mainDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString)!;

        GameNodeData[] serializedGameNodes = (mainDict["GameNodes"] as Dictionary<string, object>[])!.Select(d => new GameNodeData(d)).ToArray();
        Dictionary<Guid, GameNode> gameNodes = new();
        Dictionary<Guid, GameNodeData> gameNodeData = new();
        foreach (GameNodeData data in serializedGameNodes) {
            GameNode node = CreateNode(data);
            gameNodes.Add(node.Id, node);
            gameNodeData.Add(node.Id, data);
        }

        foreach (KeyValuePair<Guid, GameNode> node in gameNodes) {
            node.Value.DeserializeNode(gameNodes, gameNodeData[node.Key]);
        }

        GameNodeData[] serializedGuiNodes = (mainDict["GuiNodes"] as Dictionary<string, object>[])!.Select(d => new GameNodeData(d)).ToArray();
        Dictionary<Guid, GameNode> guiNodes = new();
        Dictionary<Guid, GameNodeData> guiNodeData = new();
        foreach (GameNodeData data in serializedGuiNodes) {
            GameNode node = CreateNode(data);
            guiNodes.Add(node.Id, node);
            guiNodeData.Add(node.Id, data);
        }

        foreach (KeyValuePair<Guid, GameNode> node in guiNodes) {
            node.Value.DeserializeNode(guiNodes, guiNodeData[node.Key]);
        }

        NodeLockEvent.Set();
    }

    public static void Save() {
        if (!FileManager.IsSaveFileLoaded)
            throw new InvalidOperationException("Cannot save scene. No save file is loaded.");

        NodeLockEvent.WaitOne();
        NodeLockEvent.Reset();
        RenderLockEvent.WaitOne();
        UpdateLockEvent.WaitOne();

        Queue<GameNode> gameNodes = new();
        gameNodes.Enqueue(GameRoot);

        List<GameNodeData> serializedGameNodes = new();
        while (gameNodes.Count > 0) {
            GameNode node = gameNodes.Dequeue();

            if (node == GameRoot)
                continue;

            GameNodeData data = node.SerializeNode();
            serializedGameNodes.Add(data);

            foreach (GameNode child in node.Children)
                gameNodes.Enqueue(child);
        }

        Queue<GameNode> guiNodes = new();
        guiNodes.Enqueue(GuiRoot);

        List<GameNodeData> serializedGuiNodes = new();
        while (guiNodes.Count > 0) {
            GameNode node = guiNodes.Dequeue();

            if (node == GuiRoot)
                continue;

            GameNodeData data = node.SerializeNode();
            serializedGuiNodes.Add(data);

            foreach (GameNode child in node.Children)
                guiNodes.Enqueue(child);
        }

        Dictionary<string, object> mainDict = new() {
            { "GameNodes", serializedGameNodes.Select(n => n.Data).ToArray()},
            { "GuiNodes", serializedGuiNodes.Select(n => n.Data).ToArray()},
        };

        string serializedScene = JsonSerializer.Serialize(mainDict);

        FileManager.LoadedSaveFile!.WriteToFileStream($"{Name}.json", s => {
            using StreamWriter sw = new StreamWriter(s);
            sw.Write(serializedScene);
        });

        NodeLockEvent.Set();
    }

    private static GameNode CreateNode(GameNodeData data) {
        if (data.HasKey("Id"))
            throw new InvalidOperationException("Cannot create node from data without Id.");

        if (data.HasKey("Type"))
            throw new InvalidOperationException("Cannot create node from data without Type.");


        Guid id = Guid.Parse(data.GetValue("Id"));

        string typeName = data.GetValue("Type");
        Type? type = Type.GetType(typeName);
        if (type == null)
            throw new InvalidOperationException($"Cannot create node from data. Type '{typeName}' could not be found.");

        ConstructorInfo? ctor = type.GetConstructor(new Type[] { typeof(Guid) });
        if (ctor == null)
            throw new InvalidOperationException($"Cannot create node from data. Type '{typeName}' does not have a constructor with a single Guid parameter.");

        GameNode node = (GameNode)ctor.Invoke(new object[] { id });
        return node;
    }

    private static void OnViewportChanged() {
        GuiProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, GLHandler.Viewport.Size.X, GLHandler.Viewport.Size.Y, 0, -1, 1);
    }
}
