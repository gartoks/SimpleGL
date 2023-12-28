using SimpleGL.Game.Gui.Constraints;

namespace SimpleGL.Game.Gui.Components;

internal class GuiScreen : GuiComponent {
    protected override bool IsInteractable => false;

    public GuiScreen(int width, int height)
        : base(ScreenConstraints(width, height)) {
        Pivot = (0, 0);

        Bounds = Constraints.CalculateBounds(Utility.Bounds.FromTLAndSize(0, 0, width, height), (0, 0));
    }

    internal void UpdateGuiConstraints(int width, int height) {
        SetConstraints(Constraints, width, height);
    }

    private static GuiConstraints ScreenConstraints(int width, int height) {
        GuiConstraints constraints = new GuiConstraints();
        SetConstraints(constraints, width, height);

        return constraints;
    }

    private static void SetConstraints(GuiConstraints constraints, int width, int height) {
        constraints.SetX("x:pixel:0:left");
        constraints.SetY("y:pixel:0:top");
        constraints.SetWidth($"w:pixel:{width}");
        constraints.SetHeight($"h:pixel:{height}");
    }

}