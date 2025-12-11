namespace BahyWay.RulesEngine.Models;

/// <summary>
/// Represents a linguistic variable with multiple fuzzy sets
/// Example: Temperature with fuzzy sets {Cold, Warm, Hot}
/// </summary>
public class LinguisticVariable
{
    public string Name { get; }
    public double MinValue { get; }
    public double MaxValue { get; }
    private readonly Dictionary<string, FuzzySet> _fuzzySets;

    public LinguisticVariable(string name, double minValue, double maxValue)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MinValue = minValue;
        MaxValue = maxValue;
        _fuzzySets = new Dictionary<string, FuzzySet>(StringComparer.OrdinalIgnoreCase);

        if (minValue >= maxValue)
            throw new ArgumentException("MinValue must be less than MaxValue");
    }

    /// <summary>
    /// Adds a fuzzy set to this linguistic variable
    /// </summary>
    public void AddFuzzySet(FuzzySet fuzzySet)
    {
        if (fuzzySet == null)
            throw new ArgumentNullException(nameof(fuzzySet));

        _fuzzySets[fuzzySet.Name] = fuzzySet;
    }

    /// <summary>
    /// Gets a fuzzy set by name
    /// </summary>
    public FuzzySet? GetFuzzySet(string name)
    {
        _fuzzySets.TryGetValue(name, out var fuzzySet);
        return fuzzySet;
    }

    /// <summary>
    /// Gets all fuzzy set names
    /// </summary>
    public IEnumerable<string> GetFuzzySetNames() => _fuzzySets.Keys;

    /// <summary>
    /// Fuzzifies a crisp value - returns membership degrees for all fuzzy sets
    /// </summary>
    public Dictionary<string, double> Fuzzify(double value)
    {
        return _fuzzySets.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.GetMembership(value)
        );
    }

    /// <summary>
    /// Classifies a value to the fuzzy set with highest membership
    /// </summary>
    public string? Classify(double value)
    {
        var memberships = Fuzzify(value);
        if (!memberships.Any())
            return null;

        var max = memberships.MaxBy(kvp => kvp.Value);
        return max.Value > 0 ? max.Key : null;
    }

    /// <summary>
    /// Gets the number of fuzzy sets
    /// </summary>
    public int Count => _fuzzySets.Count;

    public override string ToString() => $"LinguisticVariable: {Name} [{MinValue}, {MaxValue}] with {Count} fuzzy sets";
}
