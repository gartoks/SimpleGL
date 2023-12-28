using OpenTK.Mathematics;
using SimpleGL.Util.Extensions;

namespace SimpleGL.Game.Gui.Constraints;

public sealed class GuiConstraints {
    internal IPositionConstraint XConstraint { get; private set; }
    internal IPositionConstraint YConstraint { get; private set; }
    internal ISizeConstraint WidthConstraint { get; private set; }
    internal ISizeConstraint HeightConstraint { get; private set; }

    private bool hasChanged;

    public GuiConstraints() {
        hasChanged = true;
    }

    public GuiConstraints(string constraintString) {
        hasChanged = true;
        SetFromConstraintString(constraintString);
    }

    public Box2 CalculateBounds(Box2 parentBounds, (float x, float y) pivot) {
        float x = 0, y = 0, w = 0, h = 0;

        // Calculate Size
        if (!WidthConstraint.IsDependentOnOther)
            w = WidthConstraint.CalculateSizeValue(parentBounds.Size.X, 0);
        h = HeightConstraint.CalculateSizeValue(parentBounds.Size.Y, w);
        if (WidthConstraint.IsDependentOnOther)
            w = WidthConstraint.CalculateSizeValue(parentBounds.Size.X, h);

        // Calculate Position
        x = XConstraint.CalculatePositionValue(parentBounds.Size.X, w);
        y = YConstraint.CalculatePositionValue(parentBounds.Size.Y, h);

        hasChanged = false;

        return BoxExtensions.FromMinAndSize(parentBounds.Min.X + x - w * pivot.x, parentBounds.Min.Y + y - h * pivot.y, w, h);
    }

    public void SetX(IPositionConstraint constraint) {
        XConstraint = constraint;
        hasChanged = true;
    }

    public void SetX(string xConstraintString) {
        xConstraintString = xConstraintString.Trim().ToLower();
        string[] args = xConstraintString.Split(':');

        if (args.Length < 2 && args[0] == "x" || args.Length < 1)
            return;

        if (args[0] != "x")
            args = new[] { "x" }.Extend(args);

        SetXFromConstraintString(args);
    }

    public void SetY(IPositionConstraint constraint) {
        YConstraint = constraint;
        hasChanged = true;
    }

    public void SetY(string yConstraintString) {
        yConstraintString = yConstraintString.Trim().ToLower();
        string[] args = yConstraintString.Split(':');

        if (args.Length < 2 && args[0] == "y" || args.Length < 1)
            return;

        if (args[0] != "y")
            args = new[] { "y" }.Extend(args);

        SetYFromConstraintString(args);
    }

    public void SetWidth(ISizeConstraint constraint) {
        if (HeightConstraint != null && constraint.IsDependentOnOther && HeightConstraint.IsDependentOnOther)
            throw new ArgumentException("Unique gui constraint violation.");

        WidthConstraint = constraint;
        hasChanged = true;
    }

    public void SetWidth(string widthConstraintString) {
        widthConstraintString = widthConstraintString.Trim().ToLower();
        string[] args = widthConstraintString.Split(':');

        if (args.Length < 2 && (args[0] == "w" || args[0] == "width") || args.Length < 1)
            return;

        if (args[0] != "w" && args[0] != "width")
            args = new[] { "w" }.Extend(args);

        SetWidthFromConstraintString(args);
    }

    public void SetHeight(ISizeConstraint constraint) {
        if (WidthConstraint != null && constraint.IsDependentOnOther && WidthConstraint.IsDependentOnOther)
            throw new ArgumentException("Unique gui constraint violation.");

        HeightConstraint = constraint;
        hasChanged = true;
    }

    public void SetHeight(string heightConstraintString) {
        heightConstraintString = heightConstraintString.Trim().ToLower();
        string[] args = heightConstraintString.Split(':');

        if (args.Length < 2 && (args[0] == "h" || args[0] == "height") || args.Length < 1)
            return;

        if (args[0] != "h" && args[0] != "height")
            args = new[] { "h" }.Extend(args);

        SetHeightFromConstraintString(args);
    }

    public bool HasChanged() => hasChanged;

    public bool IsMissingConstraint => XConstraint == null || YConstraint == null || WidthConstraint == null || HeightConstraint == null;

    private void SetFromConstraintString(string constraintString) {
        if (string.IsNullOrWhiteSpace(constraintString))
            return;

        string[] components = constraintString.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string component in components) {
            string[] args = component.Split(':');

            if (args.Length < 2)
                continue;

            switch (args[0].ToLower()) {
                case "x": {
                        SetXFromConstraintString(args);
                        break;
                    }

                case "y": {
                        SetYFromConstraintString(args);
                        break;
                    }

                case "w":
                case "width": {
                        SetWidthFromConstraintString(args);
                        break;
                    }

                case "h":
                case "height": {
                        SetHeightFromConstraintString(args);
                        break;
                    }
            }
        }
    }

    private void SetXFromConstraintString(string[] args) {
        IPositionConstraint constraint = ParsePositionConstraint(args);

        if (constraint != null)
            SetX(constraint);
    }

    private void SetYFromConstraintString(string[] args) {
        IPositionConstraint constraint = ParsePositionConstraint(args);

        if (constraint != null)
            SetY(constraint);
    }

    private void SetWidthFromConstraintString(string[] args) {
        ISizeConstraint constraint = ParseSizeConstraint(args);

        if (constraint != null)
            SetWidth(constraint);
    }

    private void SetHeightFromConstraintString(string[] args) {
        ISizeConstraint constraint = ParseSizeConstraint(args);

        if (constraint != null)
            SetHeight(constraint);
    }

    private IPositionConstraint ParsePositionConstraint(string[] args) {
        string type = args[1].ToLower();
        switch (type) {
            case "center": {
                    if (args.Length == 2)
                        return CenterConstraint.Position();

                    if (!int.TryParse(args[2], out int offset))
                        throw new Exception($"Invalid position constraint format '{string.Join(" ", args)}'.");

                    return CenterConstraint.Position(offset);
                }
            case "pixel": {
                    if (args.Length != 4)
                        throw new Exception($"Invalid position constraint format '{string.Join(" ", args)}'.");

                    if (!int.TryParse(args[2], out int pixel))
                        throw new Exception($"Invalid position constraint format '{string.Join(" ", args)}'.");

                    return args[3] switch {
                        "top" => PixelConstraint.PositionFromTop(pixel),
                        "bottom" => PixelConstraint.PositionFromBottom(pixel),
                        "left" => PixelConstraint.PositionFromLeft(pixel),
                        "right" => PixelConstraint.PositionFromRight(pixel),
                        _ => throw new Exception($"Invalid position constraint format '{string.Join(" ", args)}'.") // TODO custom exception
                    };
                }
            case "relative": {
                    if (args.Length != 4)
                        return null;

                    if (!float.TryParse(args[2], out float relative))
                        return null;

                    return args[3] switch {
                        "top" => RelativeConstraint.PositionFromTop(relative),
                        "bottom" => RelativeConstraint.PositionFromBottom(relative),
                        "left" => RelativeConstraint.PositionFromLeft(relative),
                        "right" => RelativeConstraint.PositionFromRight(relative),
                        _ => throw new Exception($"Invalid position constraint format '{string.Join(" ", args)}'.")
                    };
                }
            default:
                throw new Exception($"Invalid position constraint format '{string.Join(" ", args)}'.");
        }
    }

    private ISizeConstraint ParseSizeConstraint(string[] args) {
        string type = args[1].ToLower();
        switch (type) {
            case "fill": {
                    int change = 0;
                    if (args.Length == 3 && int.TryParse(args[2], out int c))
                        change = c;

                    return FillConstraint.Size(change);
                }
            case "pixel": {
                    if (args.Length != 3)
                        throw new Exception($"Invalid size constraint format '{string.Join(" ", args)}'."); // TODO custom exception

                    if (!int.TryParse(args[2], out int pixel))
                        throw new Exception($"Invalid size constraint format '{string.Join(" ", args)}'.");

                    return PixelConstraint.Size(pixel);
                }
            case "relative": {
                    if (args.Length != 3)
                        throw new Exception($"Invalid size constraint format '{string.Join(" ", args)}'.");

                    if (!float.TryParse(args[2], out float relative))
                        throw new Exception($"Invalid size constraint format '{string.Join(" ", args)}'.");

                    return RelativeConstraint.Size(relative);
                }
            case "ratio": {
                    if (args.Length != 3)
                        throw new Exception($"Invalid size constraint format '{string.Join(" ", args)}'.");

                    if (!float.TryParse(args[2], out float relative))
                        throw new Exception($"Invalid size constraint format '{string.Join(" ", args)}'.");

                    return RatioConstraint.Size(relative);
                }
            default:
                throw new Exception($"Invalid size constraint format '{string.Join(" ", args)}'.");
        }
    }
}