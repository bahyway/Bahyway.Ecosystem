using BahyWay.RulesEngine.Interfaces;

namespace BahyWay.RulesEngine.Models;

/// <summary>
/// Represents a fuzzy set with a name and membership function
/// </summary>
public class FuzzySet
{
    public string Name { get; }
    public IMembershipFunction MembershipFunction { get; }

    public FuzzySet(string name, IMembershipFunction membershipFunction)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MembershipFunction = membershipFunction ?? throw new ArgumentNullException(nameof(membershipFunction));
    }

    /// <summary>
    /// Gets the membership degree for a given value
    /// </summary>
    public double GetMembership(double value) => MembershipFunction.Evaluate(value);

    /// <summary>
    /// Gets memberships for multiple values
    /// </summary>
    public Dictionary<double, double> GetMemberships(IEnumerable<double> values)
    {
        return values.ToDictionary(v => v, v => GetMembership(v));
    }

    public override string ToString() => $"FuzzySet: {Name}";
}
