using OpenTK.Mathematics;
using SimpleGL.Game.Gui.Constraints;
using System.Diagnostics;

namespace SimpleGL.Game.Gui.Components;

public interface IGuiComboBoxItem {
    string DisplayName { get; }
}

public class GuiComboBox<T> : GuiComponent {
    private const int SELECTED_LABEL_OFFSET_X = 5;
    private const int SELECTED_LABEL_OFFSET_Y = 2;

    protected override bool IsInteractable => false;

    private bool ItemSourceChanged { get; set; }
    private List<T> _ItemSource { get; set; }
    public IEnumerable<T> ItemSource {
        get => _ItemSource;
        set {
            value = value == null ? new T[0] : value.Where(v => v != null);

            _ItemSource = new List<T>(value);
            ItemSourceChanged = true;
        }
    }

    private int _SelectedIndex { get; set; }
    public int SelectedIndex {
        get => _SelectedIndex;
        private set {
            _SelectedIndex = value;

            OnSelectedIndexChanged();
        }
    }

    public T SelectedItem => SelectedIndex < 0 ? default : _ItemSource[SelectedIndex];

    private GuiPanel SelectedPanel { get; }
    private GuiLabel SelectedLabel { get; }
    private GuiButton ExpandButton { get; }

    private GuiPanel ExtendedPanel { get; }
    private List<GuiTextButton> SelectionButtons { get; }

    private bool _IsExtended { get; set; }
    private bool IsExtended {
        get => _IsExtended;
        set {
            if (value == _IsExtended)
                return;

            _IsExtended = value;

            float y = Bounds.Top;

            if (IsExtended) {
                float height = Bounds.Height;
                Debug.WriteLine(IsExtended + " " + SelectedPanel.Bounds + " " + ExtendedPanel.Bounds);
                Constraints.SetY($"pixel:{y}:top");
                Constraints.SetHeight($"pixel:{(1 + _ItemSource.Count) * height}");
                SelectedPanel.Constraints.SetHeight($"pixel:{height}");
                SelectedLabel.Constraints.SetHeight($"pixel:{height - 2 * SELECTED_LABEL_OFFSET_Y}");
                ExpandButton.Constraints.SetHeight($"pixel:{height}");
                ExtendedPanel.IsVisible = true;
                ExtendedPanel.Constraints.SetY($"pixel:{height}:top");
                ExtendedPanel.Constraints.SetHeight($"pixel:{_ItemSource.Count * height}");

            } else {
                float height = SelectedPanel.Bounds.Height;
                Debug.WriteLine(IsExtended + " " + SelectedPanel.Bounds + " " + ExtendedPanel.Bounds);
                Constraints.SetY($"pixel:{y}:top");
                Constraints.SetHeight($"pixel:{height}");
                SelectedPanel.Constraints.SetHeight($"pixel:{height}");
                SelectedLabel.Constraints.SetHeight($"pixel:{height - 2 * SELECTED_LABEL_OFFSET_Y}");
                ExpandButton.Constraints.SetHeight($"pixel:{height}");
                ExtendedPanel.IsVisible = false;
            }

            RequiresRedraw = true;
        }
    }

    private bool RequiresRedraw { get; set; }

    public GuiComboBox(string constraintString)
        : this(new GuiConstraints(constraintString)) { }

    public GuiComboBox(GuiConstraints constraints)
        : base(constraints) {

        SelectionButtons = new List<GuiTextButton>();

        SelectedPanel = new GuiPanel($"x:pixel:0:left y:pixel:0:top w:relative:0.9 h:fill");
        SelectedPanel.Parent = this;
        SelectedPanel.Color = Color4.LightGray;

        SelectedLabel = new GuiLabel($"x:pixel:{SELECTED_LABEL_OFFSET_X}:left y:pixel:{SELECTED_LABEL_OFFSET_Y}:top w:fill:{-2 * SELECTED_LABEL_OFFSET_X} h:fill:{-2 * SELECTED_LABEL_OFFSET_Y}");
        SelectedLabel.Parent = SelectedPanel;
        SelectedLabel.HorizontalAlignment = eHorizontalAlignment.Left;
        SelectedLabel.TextColor = Color4.Black;
        SelectedLabel.Text = string.Empty;

        ExpandButton = new GuiButton($"x:pixel:0:right y:pixel:0:top w:relative:0.1 h:fill");
        ExpandButton.Parent = this;
        ExpandButton.DefaultColor = Color4.LightGray;
        ExpandButton.OnClick += btn => IsExtended = !IsExtended;
        ExpandButton.DefaultColor = Color4.DarkGray;

        ExtendedPanel = new GuiPanel("x:pixel:0:left y:pixel:0:top w:fill h:fill");
        ExtendedPanel.Parent = this;
        ExtendedPanel.IsVisible = false;

        ItemSource = new T[0];
    }

    internal override void Update(float dT, out bool requiresRedraw) {
        if (ItemSourceChanged) {
            UpdateItems();
            RequiresRedraw = true;
        }

        base.Update(dT, out requiresRedraw);

        requiresRedraw |= RequiresRedraw;
        RequiresRedraw = false;
    }

    private void UpdateItems() {
        float height = SelectedPanel.Bounds.Height;

        foreach (GuiTextButton button in SelectionButtons) {
            button.OnClick -= SelectionButtonClicked;
            button.Parent = null;
        }
        SelectionButtons.Clear();

        if (_ItemSource.Count == 0) {
            SelectedIndex = -1;
        } else {
            SelectedIndex = 0;

            for (int i = 0; i < _ItemSource.Count; i++) {
                T item = _ItemSource[i];
                GuiTextButton btn = new GuiTextButton($"x:pixel:0:left y:pixel:{i * height}:top w:fill h:pixel:{height}");
                btn.Name = $"{i}";
                btn.Label.Constraints.SetX($"pixel:{SELECTED_LABEL_OFFSET_X}:left");
                btn.Label.Constraints.SetY($"pixel:{SELECTED_LABEL_OFFSET_Y}:top");
                btn.Label.HorizontalAlignment = eHorizontalAlignment.Left;
                btn.Text = GetItemDisplayName(item);
                btn.TextColor = Color4.Black;
                btn.OnClick += SelectionButtonClicked;
                btn.Parent = ExtendedPanel;

                SelectionButtons.Add(btn);
            }
        }

        ItemSourceChanged = false;
    }

    private void SelectionButtonClicked(GuiButton btn) {
        int index = int.Parse(btn.Name);
        SelectedIndex = index;
        IsExtended = false;
    }

    private void OnSelectedIndexChanged() {
        if (SelectedIndex < 0) {
            SelectedLabel.Text = string.Empty;
        } else {
            SelectedLabel.Text = GetItemDisplayName(SelectedItem);
        }
    }

    private string GetItemDisplayName(T item) {
        if (item is IGuiComboBoxItem i) {
            return i.DisplayName;
        } else {
            return item.ToString();
        }
    }
}