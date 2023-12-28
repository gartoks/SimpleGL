using OpenTK.Mathematics;
using SimpleGL.Game.Gui.Constraints;

namespace SimpleGL.Game.Gui.Components;

public class GuiCheckBox : GuiPanel {
    protected override bool IsInteractable => false;

    private bool isChecked;
    public bool IsChecked {
        get => isChecked;
        set {
            isChecked = value;

            CheckButton.DefaultColor = IsChecked ? Color4.LightGray : Color4.White;
            CheckButton.HoverColor = IsChecked ? Color4.Gray : Color4.LightGray;
            CheckButton.ClickColor = IsChecked ? Color4.DarkGray : Color4.Gray;

            OnCheckChanged?.Invoke(this);
        }
    }

    public event Action<GuiCheckBox> OnCheckChanged;

    private GuiButton CheckButton { get; }

    public GuiCheckBox(string constraintString)
        : this(new GuiConstraints(constraintString)) { }

    public GuiCheckBox(GuiConstraints constraints)
        : base(constraints, false) {

        CheckButton = new GuiButton("x:relative:0.15:left y:relative:0.15:top w:relative:0.7 h:relative:0.7");
        CheckButton.Pivot = (0f, 0f);
        CheckButton.Parent = this;
        CheckButton.DefaultColor = Color4.White;
        CheckButton.HoverColor = Color4.LightGray;
        CheckButton.ClickColor = Color4.Gray;
        CheckButton.OnClick += btn => IsChecked = !IsChecked;
    }
}