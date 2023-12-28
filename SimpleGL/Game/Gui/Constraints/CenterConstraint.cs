using SimpleGL.Util.Math;

namespace SimpleGL.Game.Gui.Constraints;

public sealed class CenterConstraint : IPositionConstraint {
    public static IPositionConstraint Position() => new CenterConstraint(0);

    public static IPositionConstraint Position(int pixelOffset) => new CenterConstraint(pixelOffset);

    private int PixelOffset { get; }

    public CenterConstraint(int pixelOffset) {
        PixelOffset = pixelOffset;
    }

    public float CalculatePositionValue(float parentSize, float referenceValue) => (0.5f * parentSize).RoundToInt() + PixelOffset;
}