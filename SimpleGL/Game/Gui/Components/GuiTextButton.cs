using OpenTK.Mathematics;
using SimpleGL.Game.Gui.Constraints;

namespace SimpleGL.Game.Gui.Components;

public class GuiTextButton : GuiButton {

    public ISpriteFont Font {
        get => Label.Font;
        set => Label.Font = value;
    }

    public string Text {
        get => Label.Text;
        set => Label.Text = value;
    }

    public Color4 TextColor {
        get => Label.TextColor;
        set => Label.TextColor = value;
    }

    public GuiLabel Label { get; }

    public GuiTextButton(string constraintString)
        : this(new GuiConstraints(constraintString)) { }

    public GuiTextButton(GuiConstraints constraints)
        : base(constraints) {

        Label = new GuiLabel("x:center y:center w:relative:0.8 h:relative:0.5");
        Label.Parent = this;
        Label.HorizontalAlignment = eHorizontalAlignment.Center;
        Label.VerticalAlignment = eVerticalAlignment.Center;
    }

    public override string ToString() {
        return !string.IsNullOrWhiteSpace(Name) ? $"{GetType().Name}[{Name}][{Text}]" : $"{GetType().Name}[][{Text}]";
    }
}