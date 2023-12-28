using OpenTK.Mathematics;
using SimpleGL.Game.Gui.Constraints;

namespace SimpleGL.Game.Gui.Components;

public enum eHorizontalAlignment { Left, Center, Right }
public enum eVerticalAlignment { Top, Center, Bottom }

public class GuiLabel : GuiComponent {
    protected override bool IsInteractable => false;

    private SpriteFont _Font { get; set; }
    public SpriteFont Font {
        get => _Font;
        set {
            if (value == null)
                value = App.Game.Resources.DefaultFont;

            _Font = value;
            InvalidateFontSize();
        }
    }

    private string _Text { get; set; }
    public string Text {
        get => _Text;
        set {
            if (_Text == value)
                return;

            _Text = value;
            InvalidateFontSize();
        }
    }


    private Color4 _TextColor { get; set; }
    public Color4 TextColor {
        get => _TextColor;
        set {
            if (_TextColor == value)
                return;

            _TextColor = value;
            InvalidateFontSize();
        }
    }

    private eHorizontalAlignment _HorizontalAlignment { get; set; }
    public eHorizontalAlignment HorizontalAlignment {
        get => _HorizontalAlignment;
        set {
            _HorizontalAlignment = value;
            InvalidateFontSize();
        }
    }

    private eVerticalAlignment _VerticalAlignment { get; set; }
    public eVerticalAlignment VerticalAlignment {
        get => _VerticalAlignment;
        set {
            _VerticalAlignment = value;
            InvalidateFontSize();
        }
    }

    private float RenderOffsetX { get; set; }
    private float RenderOffsetY { get; set; }
    private float FontScale { get; set; }

    private TextRenderData TextRenderData { get; set; }

    public GuiLabel(string constraintString, string text = "")
        : this(new GuiConstraints(constraintString), text) { }

    public GuiLabel(GuiConstraints constraints, string text = "")
        : base(constraints) {

        Font = App.Game.Resources.DefaultFont;
        _Text = text ?? string.Empty;
        FontScale = -1;
        HorizontalAlignment = eHorizontalAlignment.Center;
        VerticalAlignment = eVerticalAlignment.Center;
        TextColor = Color4.White;
    }

    internal override void Update(float dT, out bool requiresRedraw) {
        base.Update(dT, out requiresRedraw);

        requiresRedraw |= HasInvalidFontSize();
    }

    public override void Render(IRenderer renderer, float xOffset, float yOffset) {
        if (!IsVisible)
            return;

        if (string.IsNullOrWhiteSpace(Text)) {
            FontScale = 1;
            return;
        }

        if (HasInvalidFontSize()) {
            CalculateFontScale();
            //RecalculateBounds();
            TextRenderData?.Dispose();
            TextRenderData = _Font.GenerateTextRenderData(renderer, Text, FontScale, TextColor);
        }

        //Material?.SetParameter("tint", TextColor);    // TODO font color

        renderer.Render((Bounds.Min.X + RenderOffsetX + xOffset, Bounds.Min.Y + RenderOffsetY + yOffset), TextRenderData.RenderBuffers, TextRenderData.Texture);

        base.Render(renderer, xOffset + RenderOffsetX, yOffset + RenderOffsetY);
    }

    /*private void RecalculateBounds() {
        (int width, int height) size = Font.MeasureString(Text, FontScale);
        Constraints.SetWidth(PixelConstraint.Size(size.width));
        Constraints.SetHeight(PixelConstraint.Size(size.height));
    }*/

    private void CalculateFontScale() {
        float fontScale = 20;
        (int width, int height)? size = null;
        do {    // TODO test
            if (size.HasValue) {
                float xRatio = size.Value.width / Bounds.Size.X;
                float yRatio = size.Value.height / Bounds.Size.Y;

                fontScale = fontScale / MathF.Max(xRatio, yRatio);
            }

            size = _Font.MeasureString(Text, fontScale);
        } while (size.Value.width > Bounds.Size.X || size.Value.height > Bounds.Size.Y || (size.Value.width < Bounds.Size.X - 5 && size.Value.height < Bounds.Size.Y - 5));

        FontScale = fontScale;

        if (HorizontalAlignment == eHorizontalAlignment.Left) {
            RenderOffsetX = 0;
        } else if (HorizontalAlignment == eHorizontalAlignment.Center) {
            RenderOffsetX = (Bounds.Size.X - size.Value.width) / 2f;
        } else if (HorizontalAlignment == eHorizontalAlignment.Right) {
            RenderOffsetX = Bounds.Size.X - size.Value.width;
        }

        if (VerticalAlignment == eVerticalAlignment.Top) {
            RenderOffsetY = 0;
        } else if (VerticalAlignment == eVerticalAlignment.Center) {
            RenderOffsetY = (Bounds.Size.Y - size.Value.height) / 2f;
        } else if (VerticalAlignment == eVerticalAlignment.Bottom) {
            RenderOffsetY = Bounds.Size.Y - size.Value.height;
        }
    }

    private void InvalidateFontSize() {
        FontScale = -1;
    }

    private bool HasInvalidFontSize() => FontScale <= 0;

    public override string ToString() {
        return !string.IsNullOrWhiteSpace(Name) ? $"{GetType().Name}[{Name}][{Text}]" : $"{GetType().Name}[][{Text}]";
    }
}