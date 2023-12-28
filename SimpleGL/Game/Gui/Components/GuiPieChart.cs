using OpenTK.Mathematics;
using SimpleGL.Game.Gui.Constraints;
using SimpleGL.Input;
using SimpleGL.Util.Extensions;
using SimpleGL.Util.Math;

namespace SimpleGL.Game.Gui.Components;

public class PieChartData {
    public string Name { get; }
    public Color4 Color { get; }

    private float value;
    public float Value {
        get => value;
        set {
            if (float.IsNaN(value) || value <= 0)
                throw new ArgumentOutOfRangeException();

            float oldValue = value;

            this.value = value;
            OnDataValueChanged?.Invoke(this, oldValue);
        }
    }

    internal event Action<PieChartData, float> OnDataValueChanged;

    public PieChartData(string name, Color4 color, float value) {
        Name = name;
        Color = color;
        Value = value;
    }
}

public class GuiPieChart : GuiComponent {
    protected override bool IsInteractable => false;

    private Dictionary<PieChartData, RenderBuffers> _Data { get; }
    public IEnumerable<PieChartData> Data => _Data.Select(d => d.Key);

    private float Total { get; set; }

    private GuiLabel TooltipLabel { get; }

    private bool IsDirty { get; set; }

    public GuiPieChart(string constraintString)
        : this(new GuiConstraints(constraintString)) { }

    public GuiPieChart(GuiConstraints constraints)
        : base(constraints) {

        _Data = new Dictionary<PieChartData, RenderBuffers>();

        OnBoundsChanged += c => IsDirty = true;

        TooltipLabel = new GuiLabel($"x:pixel:{App.Game.Input.MousePosition.X}:left y:pixel:{App.Game.Input.MousePosition.Y}:top w:pixel:100 h:pixel:30", string.Empty);
        TooltipLabel.Pivot = (0, 0);
        //TooltipLabel.Parent = this;
        TooltipLabel.HorizontalAlignment = eHorizontalAlignment.Left;
        TooltipLabel.TextColor = Color4.White;

        Total = 0;
        IsDirty = true;
    }

    public void AddData(PieChartData data) {
        if (this._Data.ContainsKey(data))
            return;

        this._Data[data] = null;
        data.OnDataValueChanged += OnOnDataValueChanged;

        Total += data.Value;
        IsDirty = true;
    }

    public void RemoveData(PieChartData data) {
        if (!this._Data.ContainsKey(data))
            return;

        this._Data.Remove(data);
        data.OnDataValueChanged -= OnOnDataValueChanged;

        Total -= data.Value;
        IsDirty = true;
    }

    private void OnOnDataValueChanged(PieChartData data, float oldValue) {
        Total -= oldValue;
        Total += data.Value;
        IsDirty = true;
    }

    internal override void Update(float dT, out bool requiresRedraw) {
        PieChartData? hoveredData = GetDataAtPosition(App.Game.Input.MousePosition);

        if ((MouseState == eMouseUiComponentState.Entered || MouseState == eMouseUiComponentState.Hovering) && hoveredData != null) {
            TooltipLabel.IsVisible = true;
            TooltipLabel.Constraints.SetX($"x:pixel:{App.Game.Input.MousePosition.X + 20}:left");
            TooltipLabel.Constraints.SetY($"y:pixel:{App.Game.Input.MousePosition.Y}:top");
            TooltipLabel.Text = hoveredData.Name;
        } else {
            TooltipLabel.IsVisible = false;
        }

        base.Update(dT, out requiresRedraw);
    }

    public override void Render(IRenderer renderer, float xOffset, float yOffset) {
        base.Render(renderer, xOffset, yOffset);

        if (_Data.Count == 0)
            return;

        if (IsDirty)
            UpdateRenderBuffers(renderer);

        foreach (KeyValuePair<PieChartData, RenderBuffers> tuple in _Data) {
            renderer.Render((Bounds.Center.X, Bounds.Center.Y), tuple.Value);
        }
    }

    private void UpdateRenderBuffers(IRenderer renderer) {
        float angle = 0;
        foreach (PieChartData data in _Data.Keys.ToList()) {
            float sweep = 2f * MathF.PI * data.Value / Total;

            ShapeGenerator.CreateCircleSegment(/*Bounds.Center*/Vector2.Zero, GetRadius(), angle, sweep, 0.05f, out (float x, float y)[] verts, out short[] idcs);
            RenderBuffers renderBuffers = renderer.CreateBuffers(verts.Select(v => (v, data.Color)).ToArray(), idcs);
            _Data[data] = renderBuffers;

            angle += sweep;
        }

        IsDirty = false;
    }

    private PieChartData? GetDataAtPosition(Vector2 p) {
        if (_Data.Count == 0)
            return null;

        Vector2 pPolar = (p - Bounds.Center).ToPolar();
        App.GameInternal.Window.Title = $"{pPolar.X} {pPolar.Y.ToDeg()}";

        if (pPolar.X > GetRadius())
            return null;

        float angle = 0;
        foreach (PieChartData data in _Data.Keys.ToList()) {
            float sweep = 2f * MathF.PI * data.Value / Total;

            if (pPolar.Y >= angle && pPolar.Y < angle + sweep)
                return data;

            angle += sweep;
        }

        throw new Exception();
    }

    private float GetRadius() => MathF.Min(Bounds.Size.X, Bounds.Size.Y) / 2f;

}