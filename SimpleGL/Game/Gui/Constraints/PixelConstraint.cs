namespace SimpleGL.Game.Gui.Constraints;

public sealed class PixelConstraint : IPositionConstraint, ISizeConstraint {
    public static IPositionConstraint PositionFromLeft(int pixels) => new PixelConstraint(pixels, true);
    public static IPositionConstraint PositionFromRight(int pixels) => new PixelConstraint(pixels, false);
    public static IPositionConstraint PositionFromTop(int pixels) => new PixelConstraint(pixels, true);
    public static IPositionConstraint PositionFromBottom(int pixels) => new PixelConstraint(pixels, false);

    public static ISizeConstraint Size(int pixels) => new PixelConstraint(pixels, false);

    public bool IsDependentOnOther => false;

    public int Pixels { get; set; }
    private bool LeftSide { get; set; }

    private PixelConstraint(int pixels, bool leftSide) {
        Pixels = pixels;
        LeftSide = leftSide;
    }

    public float CalculateSizeValue(float parentSize, float referenceValue) => Pixels;

    public float CalculatePositionValue(float parentSize, float referenceValue) {
        if (LeftSide)
            return Pixels;
        else
            return parentSize - Pixels - referenceValue;
    }
}