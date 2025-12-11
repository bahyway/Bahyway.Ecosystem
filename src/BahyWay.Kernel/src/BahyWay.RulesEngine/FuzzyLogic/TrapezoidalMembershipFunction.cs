using BahyWay.RulesEngine.Interfaces;

namespace BahyWay.RulesEngine.FuzzyLogic;

/// <summary>
/// Trapezoidal membership function defined by four points [a, b, c, d]
/// where a is left base, b is left shoulder, c is right shoulder, d is right base
/// </summary>
public class TrapezoidalMembershipFunction : IMembershipFunction
{
    public string Name { get; }
    public double A { get; }  // Left base
    public double B { get; }  // Left shoulder
    public double C { get; }  // Right shoulder
    public double D { get; }  // Right base

    public TrapezoidalMembershipFunction(string name, double a, double b, double c, double d)
    {
        if (a >= b || b >= c || c >= d)
            throw new ArgumentException("Parameters must satisfy: a < b < c < d");

        Name = name ?? throw new ArgumentNullException(nameof(name));
        A = a;
        B = b;
        C = c;
        D = d;
    }

    public double Evaluate(double x)
    {
        if (x <= A || x >= D)
            return 0.0;

        if (x <= B)
            return (x - A) / (B - A);

        if (x <= C)
            return 1.0;

        return (D - x) / (D - C);
    }

    public (double Min, double Max) GetSupport() => (A, D);

    public (double Min, double Max)? GetCore() => (B, C);

    public override string ToString() => $"Trapezoidal({A:F2}, {B:F2}, {C:F2}, {D:F2})";
}
