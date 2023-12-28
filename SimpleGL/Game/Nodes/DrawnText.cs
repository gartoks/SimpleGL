using SimpleGL.Game.Util;
using SimpleGL.Graphics;
using SimpleGL.Graphics.Rendering;
using SimpleGL.ResourceHandling;
using System.Globalization;

namespace SimpleGL.Game.Nodes;
internal class DrawnText : GameNode {

    public MeshFont? Font { get; set; }
    public string Text { get; set; }

    public Material Material { get; set; }

    public DrawnText() {
        Font = null;
        Text = string.Empty;

        Material = Material.CreateDefaultMaterial(0);
    }

    public DrawnText(Guid id)
        : base(id) {

        Text = string.Empty;
        Font = null;
    }

    protected override void Render(float dT) {
        if (!string.IsNullOrEmpty(Text))
            Font?.Render(Transform, Text, Material);
    }

    protected override GameNodeData Serialize() {

    }

    protected override void Deserialize(GameNodeData data) {
        if (!data.IsData(nameof(DrawnText)))
            return;

        GameNodeData textData = data.GetData(nameof(DrawnText));

        Text = textData.GetValue(nameof(Text));

        string fontKeyStr = textData.GetValue(nameof(Font));
        string fontKey = fontKeyStr.Split("_", 2)[0];
        float fontSize = float.Parse(fontKeyStr.Split("_", 2)[1], CultureInfo.InvariantCulture);
        if (!ResourceManager.MeshFontLoader.TryGetResource(MeshFontLoader.GetKey(fontKey, fontSize), out MeshFont? meshFont))
            throw new Exception($"MeshFont '{fontKeyStr}' not found");
        Font = meshFont;

        string shaderKey = textData.GetValue(nameof(Material.Shader));
        if (!ResourceManager.ShaderLoader.TryGetResource(shaderKey, out Shader? shader))
            throw new Exception($"Shader '{shaderKey}' not found");

        string materialTypeName = textData.GetValue(nameof(Material));
        Type? materialType = Type.GetType(materialTypeName);
        if (materialType == null)
            throw new Exception($"Material type '{materialTypeName}' not found");

        Material = Material.Create(materialType, shader!);
    }
}
