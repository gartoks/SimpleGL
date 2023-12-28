using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SimpleGL.Game.Gui.Constraints;
using SimpleGL.Input;
using System.Diagnostics;

namespace SimpleGL.Game.Gui.Components;

public class GuiWindow : GuiComponent {
    private const int BAR_SIZE = 30;
    private const int CLOSE_BUTTON_SPACING = 5;
    private const int BORDER_DRAG_AREA = 10;
    private const int MIN_WINDOW_HEIGHT = 3 * BAR_SIZE;
    private const int MIN_WINDOW_WIDTH = 5 * BAR_SIZE;

    protected override bool IsInteractable => false;

    public GuiComponent Content => WindowPanel;

    private GuiPanel BarPanel { get; }
    private GuiLabel TitleLabel { get; }
    private GuiButton CloseButton { get; }
    private GuiPanel WindowPanel { get; }

    private bool draggingWindow;
    private bool draggingSides;
    private int dragSide;
    private int dragBottom;
    private Vector2 dragStartRelativeMousePosition;
    private Vector2 dragStartMousePosition;
    private Box2 dragStartBounds;

    public string Title {
        get => TitleLabel.Text;
        set => TitleLabel.Text = value;
    }

    public GuiWindow(string constraintString)
        : this(new GuiConstraints(constraintString)) {
    }

    public GuiWindow(GuiConstraints constraints)
        : base(constraints) {
        //App.Game.Resources.TryGet("tex_Test9", out ITexture tex);

        BarPanel = new GuiPanel($"x:pixel:0:left y:pixel:0:top w:fill h:pixel:{BAR_SIZE}", true);
        BarPanel.Pivot = (0f, 0f);
        BarPanel.Parent = this;
        BarPanel.Color = Color4.DarkGray;

        TitleLabel = new GuiLabel($"x:pixel:{2 * CLOSE_BUTTON_SPACING}:left y:pixel:{CLOSE_BUTTON_SPACING}:top w:fill:{-(BAR_SIZE + 2 * CLOSE_BUTTON_SPACING)} h:fill:{-2 * CLOSE_BUTTON_SPACING}");
        TitleLabel.Pivot = (0f, 0f);
        TitleLabel.Parent = BarPanel;
        TitleLabel.HorizontalAlignment = eHorizontalAlignment.Left;
        TitleLabel.VerticalAlignment = eVerticalAlignment.Center;
        TitleLabel.TextColor = Color4.Black;
        TitleLabel.Text = "Title";

        CloseButton = new GuiButton($"x:pixel:{CLOSE_BUTTON_SPACING}:right y:pixel:{CLOSE_BUTTON_SPACING}:top w:ratio:1 h:pixel:{BAR_SIZE - 2 * CLOSE_BUTTON_SPACING}");
        CloseButton.Pivot = (0f, 0f);
        CloseButton.Parent = BarPanel;
        CloseButton.Texture = App.Game.Resources.DefaultTexture;
        CloseButton.DefaultColor = Color4.Red;
        CloseButton.OnClick += btn => {
            if (draggingWindow || draggingSides)
                return;

            IsVisible = false;
        };

        WindowPanel = new GuiPanel($"x:pixel:0:left y:pixel:{BAR_SIZE}:top w:fill h:fill:{-BAR_SIZE}", true);
        WindowPanel.Pivot = (0f, 0f);
        WindowPanel.Parent = this;
    }

    internal override void Update(float dT, out bool requiresRedraw) {
        base.Update(dT, out requiresRedraw);

        if (BarPanel.MouseClickState == eInteractionState.Pressed) {
            draggingWindow = true;
            dragStartRelativeMousePosition = App.Game.Input.MousePosition;
            Debug.WriteLine($"{BarPanel.MouseClickState} :: {App.Game.Input.MousePosition} :: {RelativeMousePosition}");
        } else if (draggingWindow && App.Game.Input.GetMouseButtonState(MouseButton.Left) == eInteractionState.Released) {
            draggingWindow = false;
        }

        if (draggingWindow) {
            Vector2 mpos = App.Game.Input.MousePosition;
            Vector2 offset = mpos - dragStartRelativeMousePosition;

            Constraints.SetX($"pixel:{offset.X}:left");
            Constraints.SetY($"pixel:{offset.Y}:top");
        }

        //var b = TryGetDraggedBorder(out int ds, out int db);
        //Debug.WriteLine($"{b} {RelativeMousePosition.X} {RelativeMousePosition.Y - (Bounds.Height - BORDER_DRAG_AREA)}");

        return; // TODO

        if (!draggingSides && WindowPanel.MouseClickState == eInteractionState.Pressed && TryGetDraggedBorder(out int dragSide, out int dragBottom)) {
            this.dragSide = dragSide;
            this.dragBottom = dragBottom;
            draggingSides = true;
            dragStartRelativeMousePosition = RelativeMousePosition;
            dragStartMousePosition = App.Game.Input.MousePosition;
            dragStartBounds = Bounds;
        } else if (draggingSides && App.Game.Input.GetMouseButtonState(MouseButton.Left) == eInteractionState.Released) {
            draggingSides = false;
        }

        if (draggingSides) {
            Vector2 mpos = App.Game.Input.MousePosition;
            Vector2 relativeOffset = RelativeMousePosition - dragStartRelativeMousePosition;
            Vector2 offset = mpos - dragStartMousePosition;

            if (this.dragBottom > 0) {
                Constraints.SetY($"pixel:{Bounds.Min.Y}:top");
                Constraints.SetHeight($"pixel:{Math.Max(MIN_WINDOW_HEIGHT, dragStartBounds.Size.Y + relativeOffset.Y)}");
            }

            if (this.dragSide > 0) {
                Constraints.SetX($"pixel:{Bounds.Min.X}:left");
                Constraints.SetWidth($"pixel:{Math.Max(MIN_WINDOW_WIDTH, dragStartBounds.Size.X + relativeOffset.X)}");
            } else if (this.dragSide < 0) {
                float newWidth = Math.Max(MIN_WINDOW_WIDTH, dragStartBounds.Size.X - offset.X);

                Constraints.SetWidth($"pixel:{newWidth}");

                if (newWidth > MIN_WINDOW_WIDTH) {
                    Constraints.SetX($"pixel:{dragStartBounds.Min.X + offset.X}:left");
                } else {
                    Constraints.SetX($"pixel:{dragStartBounds.Min.X + (dragStartBounds.Size.X - MIN_WINDOW_WIDTH)}:left");
                }
            }
        }
    }

    private bool TryGetDraggedBorder(out int dragSide, out int dragBottom) {
        dragSide = 0;
        dragBottom = 0;

        bool outsideX = RelativeMousePosition.X < 0 || RelativeMousePosition.X > Bounds.Size.X;
        bool outsideXMiddle = RelativeMousePosition.X > BORDER_DRAG_AREA && RelativeMousePosition.X < Bounds.Size.X - BORDER_DRAG_AREA;
        bool outsideY = RelativeMousePosition.Y > Bounds.Size.Y;
        bool outsideYMiddle = RelativeMousePosition.Y < Bounds.Size.Y - BORDER_DRAG_AREA;

        if ((outsideX || outsideXMiddle) && (outsideY || outsideYMiddle))
            return false;

        if (RelativeMousePosition.X is >= 0 and <= BORDER_DRAG_AREA)
            dragSide = -1;
        else if (RelativeMousePosition.X < Bounds.Size.X && RelativeMousePosition.X >= Bounds.Size.X - BORDER_DRAG_AREA)
            dragSide = 1;

        if (RelativeMousePosition.Y < Bounds.Size.Y && RelativeMousePosition.Y >= Bounds.Size.Y - BORDER_DRAG_AREA)
            dragBottom = 1;

        return true;
    }
}