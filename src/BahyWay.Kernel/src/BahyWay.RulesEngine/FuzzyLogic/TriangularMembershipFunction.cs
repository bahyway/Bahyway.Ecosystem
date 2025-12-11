using BahyWay.RulesEngine.Interfaces;

namespace BahyWay.RulesEngine.FuzzyLogic;

/// <summary>
/// Triangular membership function defined by three points [a, b, c]
/// where a is left base, b is peak, c is right base
/// </summary>
public class TriangularMembershipFunction : IMembershipFunction
{
    public string Name { get; }
    public double A { get; }  // Left base
    public double B { get; }  // Peak
    public double C { get; }  // Right base

    public TriangularMembershipFunction(string name, double a, double b, double c)
    {
        if (a >= b || b >= c)
            throw new ArgumentException("Parameters must satisfy: a < b < c");

        Name = name ?? throw new ArgumentNullException(nameof(name));
        A = a;
        B = b;
        C = c;
    }

    public double Evaluate(double x)
    {
        if (x <= A || x >= C)
            return 0.0;

        if (x <= B)
            return (x - A) / (B - A);

        return (C - x) / (C - B);
    }

    public (double Min, double Max) GetSupport() => (A, C);

    public (double Min, double Max)? GetCore() => (B, B);

    public override string ToString() => $"Triangular({A:F2}, {B:F2}, {C:F2})";
}
