namespace BahyWay.RulesEngine.Interfaces;

/// <summary>
/// Represents a fuzzy membership function that maps a crisp input value to a membership degree [0,1]
/// </summary>
public interface IMembershipFunction
{
    /// <summary>
    /// Gets the name of the membership function
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Evaluates the membership degree for a given input value
    /// </summary>
    /// <param name="x">The input value</param>
    /// <returns>Membership degree in range [0, 1]</returns>
    double Evaluate(double x);

    /// <summary>
    /// Gets the range where membership is non-zero (support)
    /// </summary>
    (double Min, double Max) GetSupport();

    /// <summary>
    /// Gets the range where membership equals 1 (core)
    /// </summary>
    (double Min, double Max)? GetCore();
}
