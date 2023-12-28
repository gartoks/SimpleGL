using SimpleGL.Util.Math;

namespace SimpleGL.Game.Gui.Constraints;

public sealed class RatioConstraint : ISizeConstraint {
    public static ISizeConstraint Size(float Value) {
        return new RatioConstraint(Value);
    }

    public bool IsDependentOnOther => true;

    private float ratio;
    public float Ratio {
        get => ratio;
        set {
            if (value <= 0)
                throw new ArgumentOutOfRangeException();

            ratio = value;
        }
    }

    private RatioConstraint(float ratio) {
        Ratio = ratio;
    }

    public float CalculateSizeValue(float parentSize, float referenceValue) {
        return (Ratio * referenceValue).RoundToInt();
    }
}