using OpenTK.Mathematics;
using SimpleGL.Game.Gui.Constraints;

namespace SimpleGL.Game.Gui.Components;

public class GuiButton : GuiTextureComponent {
    protected override bool IsInteractable => true;

    public Color4 DefaultColor { get; set; }
    public Color4 HoverColor { get; set; }
    public Color4 ClickColor { get; set; }

    public event Action<GuiButton> OnClick;

    public GuiButton(string constraintString)
        : base(constraintString) {

        DefaultColor = Color4.White;
        HoverColor = new Color4(212, 212, 212, 255);
        ClickColor = Color4.Gray;

        Color = DefaultColor;
    }

    public GuiButton(GuiConstraints constraints)
        : base(constraints) {

        DefaultColor = Color4.White;
        HoverColor = new Color4(212, 212, 212, 255);
        ClickColor = Color4.Gray;

        Color = DefaultColor;
    }

    internal override void Update(float dT, out bool requiresRedraw) {
        requiresRedraw = false;

        Color4 color = Color;

        if (IsEnabled) {
            if (MouseClickState == eInteractionState.Released && (MouseState == eMouseUIComponentState.Entered || MouseState == eMouseUIComponentState.Hovering))
                OnClick?.Invoke(this);

            if (MouseClickState is eInteractionState.Down or eInteractionState.Released) {
                color = ClickColor;
            } else if (MouseState is eMouseUIComponentState.Entered or eMouseUIComponentState.Hovering) {
                color = HoverColor;
            } else if (MouseState is eMouseUIComponentState.Exited or eMouseUIComponentState.Off) {
                color = DefaultColor;
            } else if ((MouseClickState == eInteractionState.Released || MouseClickState == eInteractionState.Up) && (MouseState == eMouseUIComponentState.Entered || MouseState == eMouseUIComponentState.Hovering)) {
                color = HoverColor;
            }
        } else {
            color = DefaultColor;
        }

        if (Color != color) {
            Color = color;
            requiresRedraw = true;
        }

        base.Update(dT, out bool rR);

        requiresRedraw |= rR;
    }
}