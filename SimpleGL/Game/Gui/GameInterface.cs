using SimpleGL.Game.Gui.Components;

namespace SimpleGL.Game.Gui;
internal class UserInterface : IUserInterface {

    public GuiComponent Screen => RootComponent;

    internal bool IsDisposed { get; private set; }
    private GuiScreen RootComponent { get; set; }

    private GuiComponent _FocussedObject { get; set; }
    public GuiComponent FocussedObject {
        get => _FocussedObject;
        internal set {
            if (value == _FocussedObject)
                return;

            _FocussedObject?.FocusLost();
            _FocussedObject = value;
            _FocussedObject?.FocusGained();
        }
    }

    internal UserInterface() {
    }

    internal void MatchScreenToViewport(Camera camera) {
        if (RootComponent == null)
            RootComponent = new GuiScreen(camera.ViewportWidth, camera.ViewportHeight);
        else
            RootComponent.UpdateGuiConstraints(camera.ViewportWidth, camera.ViewportHeight);

    }

    public void Render(IRenderer renderer) {
        if (IsDisposed)
            throw new InvalidOperationException();

        if (RootComponent == null)
            return;

        //Stack<Bounds> boundsStack = new Stack<Bounds>();
        //boundsStack.Push(Bounds.FromTLAndSize(0, 0, App.Game.MainCamera.ViewportWidth, App.Game.MainCamera.ViewportHeight));
        RootComponent.PreRender(renderer, 0, 0);
        RootComponent.Render(renderer, 0, 0);
    }

    public void Update(float dT) {
        if (IsDisposed)
            throw new InvalidOperationException();

        if (RootComponent == null)
            return;

        bool clickHandled = false;
        RootComponent.UpdateInput(ref clickHandled, false, true, 0, 0);
        RootComponent.Update(dT, out bool requiresRedraw);
    }

    internal void ClearComponents() {
        foreach (GuiComponent child in Screen.Children.ToList()) {
            child.RemoveFromGui();
        }
    }

    internal void Dispose() {
        if (IsDisposed)
            throw new InvalidOperationException();

        IsDisposed = true;
    }

}