using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SimpleGL.Game.Gui.Constraints;
using SimpleGL.Graphics.Textures;
using SimpleGL.Input;
using SimpleGL.Util.Extensions;
using SimpleGL.Util.Math;

namespace SimpleGL.Game.Gui.Components;

public class GuiScrollView : GuiPanel {
    private const int SCROLLBAR_SIZE = 15;
    private const int WHEEL_SCROLL_SPEED = 15;

    protected override bool IsInteractable => false;

    public GuiPanel Content => AreaPanel;

    private GuiPanel VScrollPanel { get; }
    private GuiPanel VScrollKnobPanel { get; }
    private GuiPanel HScrollPanel { get; }
    private GuiPanel HScrollKnobPanel { get; }
    private GuiPanel AreaPanel { get; }

    //public bool ShowVerticalScrollbar { get; set; }
    //public bool ShowHorizontalScrollbar { get; set; }

    private float _VScroll { get; set; }
    public float VScroll {
        get => _VScroll;
        private set {
            _VScroll = MathUtils.Clamp(value, 0, AreaPanel.ChildBounds.Height - AreaPanel.Bounds.Height);
            OnScrollParameterChanged();
        }
    }

    private float _HScroll { get; set; }
    public float HScroll {
        get => _HScroll;
        private set {
            _HScroll = MathUtils.Clamp(value, 0, AreaPanel.ChildBounds.Width - AreaPanel.Bounds.Width);
            OnScrollParameterChanged();
        }
    }

    private char Dragging { get; set; }
    private Vector2 DragStartRelativeMousePosition { get; set; }
    private float DragStartVScroll { get; set; }
    private float DragStartHScroll { get; set; }

    private Texture? ContentTexture { get; set; }
    private RenderBuffers? ContentRenderBuffers { get; set; }

    public GuiScrollView(string constraintString)
        : this(new GuiConstraints(constraintString)) { }

    public GuiScrollView(GuiConstraints constraints)
        : base(constraints) {

        Dragging = '\0';

        VScrollPanel = new GuiPanel($"x:pixel:0:right y:pixel:0:top w:pixel:{SCROLLBAR_SIZE} h:fill:{-SCROLLBAR_SIZE}");
        VScrollPanel.Pivot = (0, 0);
        VScrollPanel.Parent = this;
        VScrollPanel.Color = Color4.LightGray;

        VScrollKnobPanel = new GuiPanel($"x:pixel:0:left y:pixel:0:top w:fill h:pixel:{SCROLLBAR_SIZE}", true);
        VScrollKnobPanel.Pivot = (0, 0);
        VScrollKnobPanel.Parent = VScrollPanel;
        VScrollKnobPanel.Color = Color4.Gray;

        HScrollPanel = new GuiPanel($"x:pixel:0:left y:pixel:0:bottom w:fill:{-SCROLLBAR_SIZE} h:pixel:{SCROLLBAR_SIZE}");
        HScrollPanel.Pivot = (0, 0);
        HScrollPanel.Parent = this;
        HScrollPanel.Color = Color4.LightGray;

        HScrollKnobPanel = new GuiPanel($"x:pixel:0:left y:pixel:0:bottom w:pixel:{SCROLLBAR_SIZE} h:fill", true);
        HScrollKnobPanel.Pivot = (0, 0);
        HScrollKnobPanel.Parent = HScrollPanel;
        HScrollKnobPanel.Color = Color4.Gray;

        AreaPanel = new GuiPanel($"x:pixel:0:top y:pixel:0:top w:fill:{-SCROLLBAR_SIZE} h:fill:{-SCROLLBAR_SIZE}");
        AreaPanel.Name = "ScrollViewContent";
        AreaPanel.Pivot = (0, 0);
        AreaPanel.Parent = this;
        AreaPanel.Color = Color4.Transparent;
        AreaPanel.OnBoundsChanged += c => {
            OnScrollParameterChanged();
            //UpdateScrollKnobSizes();
        };
        AreaPanel.OnChildBoundsChanged += c => {
            OnScrollParameterChanged();
            //UpdateScrollKnobSizes();
        };

        OnBoundsChanged += c => {
            OnScrollParameterChanged();
            //UpdateScrollKnobSizes();
        };

        OnScrollParameterChanged();
        //UpdateScrollKnobSizes();
    }

    ~GuiScrollView() {
        ContentTexture?.Dispose();
        ContentTexture = null;
        ContentRenderBuffers?.Dispose();
        ContentRenderBuffers = null;
    }

    internal override void Update(float dT, out bool requiresRedraw) {
        requiresRedraw = false;

        if (AreaPanel.ChildBounds.Size.Y > AreaPanel.Bounds.Size.Y && VScrollKnobPanel.MouseClickState == eInteractionState.Pressed) {
            Dragging = 'v';
            DragStartRelativeMousePosition = VScrollPanel.RelativeMousePosition;
            DragStartVScroll = VScrollKnobPanel.Bounds.Min.Y - Bounds.Min.Y;
        } else if (AreaPanel.ChildBounds.Size.X > AreaPanel.Bounds.Size.X && HScrollKnobPanel.MouseClickState == eInteractionState.Pressed) {
            Dragging = 'h';
            DragStartRelativeMousePosition = HScrollPanel.RelativeMousePosition;
            DragStartHScroll = HScrollKnobPanel.Bounds.Min.X - Bounds.Min.X;
        } else if (Dragging != '\0' && App.Game.Input.GetMouseButtonState(MouseButton.Left) == eInteractionState.Released) {
            Dragging = '\0';
        }

        if (Dragging == 'v') {
            DragVertical();
            requiresRedraw = true;
        } else if (Dragging == 'h') {
            DragHorizontal();
            requiresRedraw = true;
        } else if (App.Game.Input.MouseWheelChange != 0) {
            Scroll();
            requiresRedraw = true;
        }

        base.Update(dT, out bool rR);
        requiresRedraw |= rR;

        if (rR) {
            ContentTexture?.Dispose();
            ContentTexture = null;
        }
    }

    public override void PreRender(IRenderer renderer, float xOffset, float yOffset) {
        if (ContentTexture == null) {
            ContentTexture = RenderContent(renderer);
        }

        if (ContentRenderBuffers == null) {
            float vScrollPct = VScroll / Math.Max(1, (Content.ChildBounds.Size.Y - Content.Bounds.Size.Y));
            float hScrollPct = HScroll / Math.Max(1, (Content.ChildBounds.Size.X - Content.Bounds.Size.X));
            float texW = Math.Min(1, Content.Bounds.Size.X / Content.ChildBounds.Size.X);
            float texH = Math.Min(1, Content.Bounds.Size.Y / Content.ChildBounds.Size.Y);
            Box2 texBounds = BoxExtensions.FromMinAndSize(hScrollPct, vScrollPct, texW, texH);

            TexturedVertex[] vertices = new TexturedVertex[4] {
                ((0, 0), Color4.White, texBounds.Min),
                ((Content.Bounds.Size.X, 0), Color4.White, texBounds.MaxXMinY),
                ((0, Content.Bounds.Size.Y), Color4.White, texBounds.MinXMaxY),
                ((Content.Bounds.Size.X, Content.Bounds.Size.Y), Color4.White, texBounds.Max)
            };
            ContentRenderBuffers = renderer.CreateBuffers(vertices, new short[] { 0, 1, 2, 1, 3, 2 });
        }
    }

    public override void Render(IRenderer renderer, float xOffset, float yOffset) {
        if (!IsVisible)
            return;

        RenderBaseTexture(renderer, xOffset, yOffset);

        VScrollPanel.Render(renderer, xOffset, yOffset);
        VScrollKnobPanel.Render(renderer, xOffset, yOffset);
        HScrollPanel.Render(renderer, xOffset, yOffset);
        HScrollKnobPanel.Render(renderer, xOffset, yOffset);

        AreaPanel.RenderBaseTexture(renderer, xOffset, yOffset);

        renderer.Render((Bounds.Min.X + xOffset, Bounds.Min.Y + yOffset), ContentRenderBuffers!, ContentTexture);
    }

    private Texture RenderContent(IRenderer renderer) {
        renderer.BeginRenderToTexture((int)Content.ChildBounds.Size.X, (int)Content.ChildBounds.Size.Y, Color4.Transparent);
        foreach (GuiComponent child in Content.Children) {
            child.Render(renderer, -Content.ChildBounds.Min.X, -Content.ChildBounds.Min.Y);
        }
        Texture texture = renderer.EndRenderToTexture();

        return texture;
    }

    private void DragVertical() {
        Vector2 relMPos = VScrollPanel.RelativeMousePosition;
        Vector2 scrollAmount = relMPos - DragStartRelativeMousePosition;
        int vScrollKnobOffset = MathUtils.Clamp(DragStartVScroll + scrollAmount.Y, 0, VScrollPanel.Bounds.Size.Y - VScrollKnobPanel.Bounds.Size.Y).RoundToInt();

        float scrollPct = vScrollKnobOffset / (float)(VScrollPanel.Bounds.Size.Y - VScrollKnobPanel.Bounds.Size.Y);
        VScroll = (scrollPct * (Content.ChildBounds.Size.Y - Content.Bounds.Size.Y)).RoundToInt();
    }

    private void DragHorizontal() {
        Vector2 relMPos = HScrollPanel.RelativeMousePosition;
        Vector2 scrollAmount = relMPos - DragStartRelativeMousePosition;
        int hScrollKnobOffset = MathUtils.Clamp(DragStartHScroll + scrollAmount.X, 0, HScrollPanel.Bounds.Size.X - HScrollKnobPanel.Bounds.Size.X).RoundToInt();

        float scrollPct = hScrollKnobOffset / (float)(HScrollPanel.Bounds.Size.X - HScrollKnobPanel.Bounds.Size.X);
        HScroll = (scrollPct * (Content.ChildBounds.Size.X - Content.Bounds.Size.X)).RoundToInt();
    }

    private void Scroll() {
        int dWheel = App.Game.Input.MouseWheelChange;
        if (dWheel == 0)
            return;

        if (App.Game.Input.IsModifierDown(Keys.LeftShift))
            HScroll -= WHEEL_SCROLL_SPEED * dWheel;
        else
            VScroll -= WHEEL_SCROLL_SPEED * dWheel;

        ContentRenderBuffers?.Dispose();
        ContentRenderBuffers = null;
    }

    private void OnScrollParameterChanged() {
        UpdateScrollKnobSizes();

        Content.ChildInputOffsetY = VScroll / Math.Max(1, (Content.ChildBounds.Size.Y - Content.Bounds.Size.Y)) * Content.ChildBounds.Size.Y;
        float vContentOverflow = Content.ChildBounds.Size.Y - Content.Bounds.Size.Y;
        int vScrollKnobOffset = vContentOverflow != 0 ? (VScroll * (VScrollPanel.Bounds.Size.Y - VScrollKnobPanel.Bounds.Size.Y) / vContentOverflow).RoundToInt() : 0;
        vScrollKnobOffset = Math.Max(0, vScrollKnobOffset);
        VScrollKnobPanel.Constraints.SetY($"pixel:{vScrollKnobOffset}:top");

        Content.ChildInputOffsetX = HScroll / Math.Max(1, (Content.ChildBounds.Size.X - Content.Bounds.Size.X)) * Content.ChildBounds.Size.X;
        float hContentOverflow = Content.ChildBounds.Size.X - Content.Bounds.Size.X;
        int hScrollKnobOffset = hContentOverflow != 0 ? (HScroll * (HScrollPanel.Bounds.Size.X - HScrollKnobPanel.Bounds.Size.X) / hContentOverflow).RoundToInt() : 0;
        hScrollKnobOffset = Math.Max(0, hScrollKnobOffset);
        HScrollKnobPanel.Constraints.SetX($"pixel:{hScrollKnobOffset}:left");

        ContentRenderBuffers?.Dispose();
        ContentRenderBuffers = null;
    }

    private void UpdateScrollKnobSizes() {
        VScrollKnobPanel.Constraints.SetY($"pixel:0:top");
        float vr = Math.Min(1, AreaPanel.Bounds.Size.Y / Content.ChildBounds.Size.Y);
        int h = (vr * AreaPanel.Bounds.Size.Y).RoundToInt();
        VScrollKnobPanel.Constraints.SetHeight($"pixel:{h}");

        HScrollKnobPanel.Constraints.SetX($"pixel:0:left");
        float hr = Math.Min(1, AreaPanel.Bounds.Size.X / Content.ChildBounds.Size.X);
        int w = (hr * AreaPanel.Bounds.Size.X).RoundToInt();
        HScrollKnobPanel.Constraints.SetWidth($"pixel:{w}");
    }
}