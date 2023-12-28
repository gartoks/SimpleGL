using OpenTK.Mathematics;
using SimpleGL.Game.Gui.Constraints;
using SimpleGL.Graphics.Textures;

namespace SimpleGL.Game.Gui.Components;

public enum eProgressBarDirection { LeftToRight, RightToLeft, TopToBottom, BottomToTop }

public class GuiProgressBar : GuiPanel {
    private const int BAR_OFFSET = 2;

    public eProgressBarDirection Direction { get; set; }

    public Texture BarTexture {
        get => BarPanel.Texture;
        set => BarPanel.Texture = value;
    }

    private float _Value { get; set; }
    public float Value {
        get => _Value;
        set {
            if (value is < 0 or > 1)
                throw new ArgumentOutOfRangeException();

            Value = value;
            ValueChanged = true;
            ValidateBarSize();
        }
    }

    private GuiPanel BarPanel { get; }

    private bool ValueChanged { get; set; }

    public GuiProgressBar(string constraintString)
        : this(new GuiConstraints(constraintString)) { }

    public GuiProgressBar(GuiConstraints constraints)
        : base(constraints, false) {

        Direction = eProgressBarDirection.LeftToRight;

        BarPanel = new GuiPanel($"x:pixel:{BAR_OFFSET}:left y:pixel:{BAR_OFFSET}:top w:fill:{-2 * BAR_OFFSET} h:fill:{-2 * BAR_OFFSET}", false);
        BarPanel.Pivot = (0f, 0f);
        BarPanel.Parent = this;
        BarPanel.Color = Color4.Lime;

        Value = 0;
    }

    internal override void Update(float dT, out bool requiresRedraw) {
        base.Update(dT, out requiresRedraw);
        requiresRedraw |= ValueChanged;
        ValueChanged = false;
    }

    private void ValidateBarSize() {
        if (Direction == eProgressBarDirection.LeftToRight) {
            float barSize = (int)(Value * (Bounds.Width - 2 * BAR_OFFSET));
            BarPanel.Constraints.SetWidth($"pixel:{barSize}");
        } else if (Direction == eProgressBarDirection.RightToLeft) {
            float w = Bounds.Width - 2 * BAR_OFFSET;
            float barSize = Value * (Bounds.Width - 2 * BAR_OFFSET);
            float x = BAR_OFFSET + (w - barSize);
            BarPanel.Constraints.SetX($"pixel:{x}:top");
            BarPanel.Constraints.SetWidth($"pixel:{barSize}");
        } else if (Direction == eProgressBarDirection.TopToBottom) {
            float barSize = Value * (Bounds.Height - 2 * BAR_OFFSET);
            BarPanel.Constraints.SetHeight($"pixel:{barSize}");
        } else if (Direction == eProgressBarDirection.BottomToTop) {
            float h = Bounds.Height - 2 * BAR_OFFSET;
            float barSize = Value * (Bounds.Height - 2 * BAR_OFFSET);
            float y = BAR_OFFSET + (h - barSize);
            BarPanel.Constraints.SetY($"pixel:{y}:top");
            BarPanel.Constraints.SetHeight($"pixel:{barSize}");
        }
    }
}