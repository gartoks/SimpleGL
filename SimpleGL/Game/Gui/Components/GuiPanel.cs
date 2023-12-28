using SimpleGL.Game.Gui.Constraints;

namespace SimpleGL.Game.Gui.Components;

public class GuiPanel : GuiTextureComponent {
    protected override bool IsInteractable { get; }

    public GuiPanel(string constraintString, bool isInteractable = false)
        : base(constraintString) {
        IsInteractable = isInteractable;
    }

    public GuiPanel(GuiConstraints constraints, bool isInteractable = false)
        : base(constraints) {
        IsInteractable = isInteractable;
    }

}