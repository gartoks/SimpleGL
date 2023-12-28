using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SimpleGL.Game.Gui.Constraints;

namespace SimpleGL.Game.Gui.Components;
public class GuiTextbox : GuiComponent {
    private const float CURSOR_BLINK_TIME = 0.5f;
    private const string ALLOWED_CHARS = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!\"§$%&/()=?*+~#'-_.:,;<>|^°@€ ";

    public ISpriteFont Font {
        get => Label.Font;
        set => Label.Font = value;
    }

    private string _Text { get; set; }
    public string Text {
        get => _Text;
        set {
            if (value == null)
                value = string.Empty;

            if (_Text?.Length >= MaxCharacters && MaxCharacters >= 0)
                return;

            _Text = value;
            Label.Text = $"{_Text}{(IsCursorVisible ? "|" : "")}";

            OnTextChanged?.Invoke(this);
        }
    }
    public int MaxCharacters { get; set; }
    public Predicate<char>? CharFilter { get; init; }

    protected override bool IsInteractable => true;

    private GuiPanel Background { get; }
    private GuiLabel Label { get; }
    private float BlinkTime { get; set; }

    private bool IsCursorVisible => HasFocus() && BlinkTime >= 0;

    public Action<GuiTextbox> OnTextChanged;

    public GuiTextbox(string constraintString)
            : base(constraintString) {

        Application.Window.TextInput += Window_TextInput;
        BlinkTime = -CURSOR_BLINK_TIME;

        Background = new GuiPanel($"x:pixel:0:left y:pixel:0:top w:fill h:fill");
        Background.Parent = this;
        Background.Pivot = (0, 0);
        Background.Color = Color4.DarkGray;

        Label = new GuiLabel("x:pixel:10:left y:relative:0.5:top w:relative:0.95 h:relative:0.75");
        Label.Pivot = (0, 0.5f);
        Label.Parent = this;
        Label.HorizontalAlignment = eHorizontalAlignment.Left;
        Label.VerticalAlignment = eVerticalAlignment.Center;
    }

    private void Window_TextInput(TextInputEventArgs e) {
        char c = e.AsString.FirstOrDefault();
        if (HasFocus() && ALLOWED_CHARS.Contains(c) && (CharFilter == null || CharFilter(c)))
            Text += c.ToString();
    }

    public GuiTextbox(GuiConstraints constraints)
        : base(constraints) {

        App.GameInternal.Window.TextInput += Window_TextInput;
        BlinkTime = -CURSOR_BLINK_TIME;

        Label = new GuiLabel("x:center y:center w:relative:0.8 h:relative:0.5");
        Label.Parent = this;
        Label.HorizontalAlignment = eHorizontalAlignment.Left;
        Label.VerticalAlignment = eVerticalAlignment.Center;
    }

    ~GuiTextbox() {
        App.GameInternal.Window.TextInput -= Window_TextInput;
    }

    internal override void Update(float dT, out bool requiresRedraw) {
        requiresRedraw = false;

        if (BlinkTime >= 0 && !HasFocus())
            Label.Text = Text;

        if (!HasFocus())
            return;

        if (App.GameInternal.InputInternal.IsKeyDown(Keys.Backspace) && Text.Length > 0)
            Text = Text[..^1];

        float prevBlinkTime = BlinkTime;

        BlinkTime += dT;
        if (BlinkTime >= CURSOR_BLINK_TIME)
            BlinkTime -= 2f * CURSOR_BLINK_TIME;

        if (prevBlinkTime < 0 && BlinkTime >= 0)
            Label.Text = Text + "|";
        else if (prevBlinkTime >= 0 && BlinkTime < 0)
            Label.Text = Text;

        base.Update(dT, out requiresRedraw);
    }


    public override string ToString() {
        return !string.IsNullOrWhiteSpace(Name) ? $"{GetType().Name}[{Name}][{Text}]" : $"{GetType().Name}[][{Text}]";
    }
}
