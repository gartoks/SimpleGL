using SimpleGL.Game.Gui.Components;

namespace SimpleGL.Game.Gui;
public interface IUserInterface {
    GuiComponent Screen { get; }
    GuiComponent FocussedObject { get; }
}