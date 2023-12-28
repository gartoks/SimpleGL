using SimpleGL.Util.Math;

namespace SimpleGL.Game.Gui.Constraints;

public sealed class RelativeConstraint : IPositionConstraint, ISizeConstraint {
    public static IPositionConstraint PositionFromLeft(float Value) => new RelativeConstraint(Value, true);
    public static IPositionConstraint PositionFromRight(float Value) => new RelativeConstraint(Value, false);
    public static IPositionConstraint PositionFromTop(float Value) => new RelativeConstraint(Value, true);
    public static IPositionConstraint PositionFromBottom(float Value) => new RelativeConstraint(Value, false);

    public static ISizeConstraint Size(float Value) => new RelativeConstraint(Value, false);

    public bool IsDependentOnOther => false;

    private float Value { get; }
    private bool LeftSide { get; }

    private RelativeConstraint(float value, bool leftSide) {
        Value = value;
        LeftSide = leftSide;
    }

    public float CalculateSizeValue(float parentSize, float referenceValue) => (parentSize * Value).RoundToInt();

    public float CalculatePositionValue(float parentSize, float referenceValue) {
        if (LeftSide)
            return (Value * parentSize).RoundToInt();
        else
            return parentSize - referenceValue - (Value * parentSize).RoundToInt();
    }
}