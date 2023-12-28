namespace SimpleGL.Game.Gui.Constraints;

public class FillConstraint : ISizeConstraint
{
    public static ISizeConstraint Size() => new FillConstraint(0);
    public static ISizeConstraint Size(int change) => new FillConstraint(change);

    public bool IsDependentOnOther => false;

    private int Change { get; }

    public FillConstraint(int change)
    {
        Change = change;
    }

    public float CalculateSizeValue(float parentSize, float referenceValue) => parentSize + Change;
}