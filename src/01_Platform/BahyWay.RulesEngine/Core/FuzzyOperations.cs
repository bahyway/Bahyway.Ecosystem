namespace BahyWay.RulesEngine.Core;

/// <summary>
/// Common fuzzy logic operations (T-norms, S-norms, hedges)
/// </summary>
public static class FuzzyOperations
{
    #region T-norms (AND operations)

    /// <summary>
    /// Minimum T-norm (most common AND operation)
    /// </summary>
    public static double And(double a, double b) => Math.Min(a, b);

    /// <summary>
    /// Minimum T-norm for multiple values
    /// </summary>
    public static double And(params double[] values) => values.Min();

    /// <summary>
    /// Algebraic Product T-norm
    /// </summary>
    public static double AlgebraicProduct(double a, double b) => a * b;

    /// <summary>
    /// Bounded Difference T-norm
    /// </summary>
    public static double BoundedDifference(double a, double b) => Math.Max(0, a + b - 1);

    #endregion

    #region S-norms (OR operations)

    /// <summary>
    /// Maximum S-norm (most common OR operation)
    /// </summary>
    public static double Or(double a, double b) => Math.Max(a, b);

    /// <summary>
    /// Maximum S-norm for multiple values
    /// </summary>
    public static double Or(params double[] values) => values.Max();

    /// <summary>
    /// Algebraic Sum S-norm
    /// </summary>
    public static double AlgebraicSum(double a, double b) => a + b - (a * b);

    /// <summary>
    /// Bounded Sum S-norm
    /// </summary>
    public static double BoundedSum(double a, double b) => Math.Min(1, a + b);

    #endregion

    #region Complement (NOT operation)

    /// <summary>
    /// Standard fuzzy complement (NOT)
    /// </summary>
    public static double Not(double a) => 1.0 - a;

    #endregion

    #region Hedges (Linguistic modifiers)

    /// <summary>
    /// Concentration hedge (very)
    /// Intensifies membership: μ²(x)
    /// </summary>
    public static double Very(double a) => a * a;

    /// <summary>
    /// Dilation hedge (somewhat)
    /// Weakens membership: √μ(x)
    /// </summary>
    public static double Somewhat(double a) => Math.Sqrt(a);

    /// <summary>
    /// Intensification hedge (extremely)
    /// μ³(x)
    /// </summary>
    public static double Extremely(double a) => a * a * a;

    /// <summary>
    /// Custom hedge with power
    /// </summary>
    public static double Hedge(double a, double power) => Math.Pow(a, power);

    #endregion

    #region Implication

    /// <summary>
    /// Mamdani implication (minimum)
    /// Used to clip consequent membership function
    /// </summary>
    public static double MamdaniImplication(double antecedent, double consequent)
        => Math.Min(antecedent, consequent);

    /// <summary>
    /// Larsen implication (product)
    /// </summary>
    public static double LarsenImplication(double antecedent, double consequent)
        => antecedent * consequent;

    #endregion

    #region Aggregation

    /// <summary>
    /// Maximum aggregation (standard Mamdani)
    /// </summary>
    public static double MaxAggregation(double a, double b) => Math.Max(a, b);

    /// <summary>
    /// Sum aggregation (bounded)
    /// </summary>
    public static double SumAggregation(double a, double b) => Math.Min(1.0, a + b);

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if value is in valid fuzzy range [0, 1]
    /// </summary>
    public static bool IsValidMembership(double value) => value >= 0 && value <= 1;

    /// <summary>
    /// Clamps value to valid fuzzy range [0, 1]
    /// </summary>
    public static double ClampMembership(double value) => Math.Max(0, Math.Min(1, value));

    /// <summary>
    /// Alpha-cut: Returns 1 if membership >= alpha, otherwise 0
    /// </summary>
    public static double AlphaCut(double membership, double alpha) => membership >= alpha ? 1.0 : 0.0;

    #endregion
}
