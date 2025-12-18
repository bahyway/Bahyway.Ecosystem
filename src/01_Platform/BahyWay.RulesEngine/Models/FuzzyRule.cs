namespace BahyWay.RulesEngine.Models;

/// <summary>
/// Represents a fuzzy IF-THEN rule
/// Example: IF temperature is Cold AND humidity is High THEN power is High
/// </summary>
public class FuzzyRule
{
    public string Name { get; }
    public Func<Dictionary<string, double>, double> Antecedent { get; }
    public string ConsequentVariable { get; }
    public string ConsequentFuzzySet { get; }
    public double Weight { get; set; }

    public FuzzyRule(
        string name,
        Func<Dictionary<string, double>, double> antecedent,
        string consequentVariable,
        string consequentFuzzySet,
        double weight = 1.0)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Antecedent = antecedent ?? throw new ArgumentNullException(nameof(antecedent));
        ConsequentVariable = consequentVariable ?? throw new ArgumentNullException(nameof(consequentVariable));
        ConsequentFuzzySet = consequentFuzzySet ?? throw new ArgumentNullException(nameof(consequentFuzzySet));

        if (weight <= 0 || weight > 1)
            throw new ArgumentException("Weight must be in range (0, 1]", nameof(weight));

        Weight = weight;
    }

    /// <summary>
    /// Evaluates the rule antecedent with given fuzzy memberships
    /// </summary>
    public double Evaluate(Dictionary<string, double> fuzzifiedInputs)
    {
        return Antecedent(fuzzifiedInputs) * Weight;
    }

    public override string ToString() => $"Rule: {Name} -> {ConsequentVariable}={ConsequentFuzzySet} (Weight: {Weight:F2})";
}
