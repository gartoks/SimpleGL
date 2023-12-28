using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SimpleGL.Game.Gui.Constraints;
using SimpleGL.Input;
using SimpleGL.Util.Extensions;

namespace SimpleGL.Game.Gui.Components;

public abstract class GuiComponent : GameNode {
    public GuiConstraints Constraints { get; }

    private Box2 _Bounds { get; set; }
    public Box2 Bounds {
        get {
            if (Constraints.HasChanged())
                UpdateBounds();

            if (this is GuiTextButton btn && btn.Label != null && btn.Text.Split("_").Length >= 2) {    // TODO ??
                btn.Text = $"{btn.Text.Split("_")[0]}_{btn.Text.Split("_")[1]}_{_Bounds}";
            }

            return _Bounds;
        }
        protected set {
            _Bounds = value;
            OnBoundsChanged?.Invoke(this);
        }
    }

    private Box2 _ChildBounds { get; set; }
    protected internal Box2 ChildBounds {
        get {
            if (Constraints.HasChanged())
                UpdateBounds();

            return _ChildBounds;
        }

        private set {
            _ChildBounds = value;
            OnChildBoundsChanged?.Invoke(this);
        }
    }

    protected internal float ChildInputOffsetX { get; set; }
    protected internal float ChildInputOffsetY { get; set; }

    private GuiComponent _Parent { get; set; }
    public new GuiComponent Parent {
        get => _Parent;
        set {
            if (value == null)
                value = App.Game.Gui.Screen;

            bool changed = Parent != value;
            if (!changed)
                return;

            //if (!value.IsContainer)
            //    throw new InvalidOperationException($"Gui component of type '{GetType().Name}' does not allow children.");

            GuiComponent oldParent = Parent;
            oldParent?._Children.Remove(this);
            _Parent = value;
            Parent?._Children.Add(this);

            oldParent?.UpdateBounds();
            UpdateBounds();
            Parent?.UpdateBounds();
        }
    }

    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }
    protected abstract bool IsInteractable { get; }

    public Vector2 RelativeMousePosition { get; private set; }
    public eMouseUiComponentState MouseState { get; private set; }
    public eInteractionState MouseClickState { get; private set; }

    protected internal event Action<GuiComponent> OnBoundsChanged;
    protected internal event Action<GuiComponent> OnChildBoundsChanged;

    protected GuiComponent(string constraintString)
        : this(new GuiConstraints(constraintString)) { }

    protected GuiComponent(GuiConstraints constraints) {
        if (constraints.IsMissingConstraint)
            throw new ArgumentException();

        Transform.OnTransformChanged += Transform_OnTransformChanged;

        Constraints = constraints;
        Parent = null;
        MouseState = eMouseUiComponentState.Off;
        MouseClickState = eInteractionState.Up;
        IsVisible = true;
        IsEnabled = true;
    }

    private void Transform_OnTransformChanged() {
        UpdateBounds();
    }

    public virtual void PreRender(IRenderer renderer, float xOffset, float yOffset) {
        if (!IsVisible)
            return;

        foreach (GuiComponent child in Children) {
            child.PreRender(renderer, xOffset, yOffset);
        }
    }

    public virtual void Render(IRenderer renderer, float xOffset, float yOffset) {
        if (!IsVisible)
            return;

        foreach (GuiComponent child in Children) {
            child.Render(renderer, xOffset, yOffset);
        }
    }

    public void RemoveFromGui() {
        Parent?._Children.Remove(this);
        _Parent?.UpdateBounds();
        _Parent = null;
    }

    internal void UpdateInput(ref bool clickHandled, bool parentIsInvisible, bool parentContainsMouse, float inputOffsetX, float inputOffsetY) {
        bool containsMouse = false;
        bool containedMouse = false;
        if (IsVisible && !parentIsInvisible) {
            UpdateMouseMoveState(inputOffsetX, inputOffsetY, out containsMouse, out containedMouse);
        } else {
            ResetInput();
        }

        foreach (GuiComponent child in Children) {
            child.UpdateInput(ref clickHandled, !IsVisible || parentIsInvisible, containsMouse, inputOffsetX + ChildInputOffsetX, inputOffsetY + ChildInputOffsetY);
        }

        if (IsVisible && IsInteractable && IsEnabled && parentContainsMouse && !parentIsInvisible) {
            UpdateMouseClickState(containsMouse, containedMouse, ref clickHandled);
        }
    }

    internal virtual void Update(float dT, out bool requiresRedraw) {
        requiresRedraw = false;
        foreach (GuiComponent child in Children.ToList()) {
            child.Update(dT, out bool rR);
            requiresRedraw |= rR;
        }
    }

    public bool HasFocus() => App.GameInternal.GuiInternal.FocussedObject == this;

    internal virtual void FocusGained() { }

    internal virtual void FocusLost() { }

    //private void ValidateBounds() {
    //    if (!Changed && !Constraints.HasChanged())
    //        return;

    //    UpdateBounds();

    //    Changed = false;
    //}

    private void UpdateMouseMoveState(float inputOffsetX, float inputOffsetY, out bool containsMouse, out bool containedMouse) {
        Vector2 mpos = App.Game.Input.MousePosition + new Vector2(inputOffsetX, inputOffsetY);
        RelativeMousePosition = new Vector2(mpos.X - Bounds.Min.X, mpos.Y - Bounds.Min.Y);
        containsMouse = Bounds.ContainsInclusive(new Vector2(mpos.X, mpos.Y));
        containedMouse = MouseState is eMouseUiComponentState.Entered or eMouseUiComponentState.Hovering;

        if (containedMouse && !containsMouse) {
            MouseState = eMouseUiComponentState.Exited;
        } else if (!containedMouse && containsMouse) {
            MouseState = eMouseUiComponentState.Entered;
        } else if (containedMouse && containsMouse) {
            MouseState = eMouseUiComponentState.Hovering;
        } else if (!containedMouse && !containsMouse) {
            MouseState = eMouseUiComponentState.Off;
        }
    }

    private void UpdateMouseClickState(bool containsMouse, bool containedMouse, ref bool clickHandled) {
        if (clickHandled && (MouseClickState == eInteractionState.Down || MouseClickState == eInteractionState.Pressed)) {
            MouseClickState = eInteractionState.Released;
        } else if (!clickHandled) {
            eInteractionState mS = App.Game.Input.GetMouseButtonState(MouseButton.Left);

            if (containsMouse && mS == eInteractionState.Pressed) {
                clickHandled = true;
                MouseClickState = eInteractionState.Pressed;
                App.GameInternal.GuiInternal.FocussedObject = this;
            } else if (containsMouse && !containedMouse && mS == eInteractionState.Down) {
                MouseClickState = eInteractionState.Down;
                App.GameInternal.GuiInternal.FocussedObject = this;
            } else if (containsMouse && MouseClickState == eInteractionState.Pressed && mS == eInteractionState.Down) {
                MouseClickState = eInteractionState.Down;
                App.GameInternal.GuiInternal.FocussedObject = this;
            } else if ((MouseClickState == eInteractionState.Pressed || MouseClickState == eInteractionState.Down) && (containedMouse && !containsMouse || mS == eKeyState.Released)) {
                MouseClickState = eInteractionState.Released;
            } else if (MouseClickState == eInteractionState.Released)
                MouseClickState = eInteractionState.Up;
        }
    }

    private void ResetInput() {
        MouseState = eMouseUiComponentState.Off;
        MouseClickState = eInteractionState.Up;
    }

    private void UpdateBounds() {
        float vw = App.Game.MainCamera.ViewportWidth;
        float vh = App.Game.MainCamera.ViewportHeight;

        //Bounds = Constraints.CalculateBounds(Parent?.Bounds ?? Bounds.FromTLAndSize(-vw / 2f, -vh / 2f, vw, vh), Pivot);   // TODO -1 -> 1 instead of viewport size ?
        Bounds = Constraints.CalculateBounds(Parent?.Bounds ?? BoxExtensions.FromMinAndSize(0, 0, vw, vh), Pivot);   // TODO -1 -> 1 instead of viewport size ?

        foreach (GuiComponent child in Children) {
            child.UpdateBounds();
        }

        UpdateChildBounds();

        Parent?.UpdateChildBounds();
    }

    private void UpdateChildBounds() {
        float l = Bounds.Min.X;
        float t = Bounds.Min.Y;
        float r = Bounds.Max.X;
        float b = Bounds.Max.Y;
        foreach (GuiComponent child in Children) {
            //child.UpdateBounds();

            l = MathF.Min(l, child.Bounds.Min.X);
            t = MathF.Min(t, child.Bounds.Min.Y);
            r = MathF.Max(r, child.Bounds.Max.X);
            b = MathF.Max(b, child.Bounds.Max.Y);
        }
        ChildBounds = BoxExtensions.FromPoints(l, t, r, b);
    }

    public override string ToString() {
        return !string.IsNullOrWhiteSpace(Name) ? $"{GetType().Name}[{Name}]" : $"{GetType().Name}";
    }
}