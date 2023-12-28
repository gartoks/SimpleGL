namespace SimpleGL.Game.Util;
public sealed class GameNodeData {
    private Dictionary<string, object> _Data { get; set; }
    internal IReadOnlyDictionary<string, object> Data => _Data;

    public GameNodeData() {
        _Data = new Dictionary<string, object>();
    }

    internal GameNodeData(Dictionary<string, object> data) {
        _Data = data;
    }

    public void Set(string key, string value) {
        _Data[key] = value;
    }

    public void Set(string key, GameNodeData value) {
        _Data[key] = value.Data;
    }

    public bool IsValue(string key) {
        return HasKey(key) && _Data[key] is string;
    }

    public bool IsData(string key) {
        return HasKey(key) && _Data[key] is Dictionary<string, object>;
    }

    public bool HasKey(string key) {
        return Data.ContainsKey(key);
    }

    public GameNodeData GetData(string key) {
        if (!IsData(key))
            throw new InvalidOperationException($"Cannot get data for key '{key}'.");

        GameNodeData data = new GameNodeData(Data[key] as Dictionary<string, object>);
        return data;
    }

    public string GetValue(string key) {
        if (!IsValue(key))
            throw new InvalidOperationException($"Cannot get value for key '{key}'.");

        return Data[key] as string;
    }
}
