namespace SimpleGL.Game.Gui.Constraints;

public interface IGuiConstraint
{
}

public interface IPositionConstraint : IGuiConstraint
{
    float CalculatePositionValue(float parentSize, float referenceValue);
}

public interface ISizeConstraint : IGuiConstraint
{
    bool IsDependentOnOther { get; }
    float CalculateSizeValue(float parentSize, float referenceValue);
}