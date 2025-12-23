using BahyWay.RulesEngine.Models;

namespace BahyWay.RulesEngine.Core;

/// <summary>
/// Handles defuzzification of fuzzy outputs to crisp values
/// </summary>
public class DefuzzificationEngine
{
    private const int DefaultSamplePoints = 101;

    /// <summary>
    /// Defuzzifies a fuzzy output using the Centroid (Center of Gravity) method
    /// </summary>
    public double Defuzzify(
        LinguisticVariable outputVariable,
        Dictionary<string, double> aggregatedMemberships,
        DefuzzificationMethod method = DefuzzificationMethod.Centroid,
        int samplePoints = DefaultSamplePoints)
    {
        return method switch
        {
            DefuzzificationMethod.Centroid => DefuzzifyCentroid(outputVariable, aggregatedMemberships, samplePoints),
            DefuzzificationMethod.MeanOfMaximum => DefuzzifyMeanOfMaximum(outputVariable, aggregatedMemberships, samplePoints),
            DefuzzificationMethod.SmallestOfMaximum => DefuzzifySmallestOfMaximum(outputVariable, aggregatedMemberships, samplePoints),
            DefuzzificationMethod.LargestOfMaximum => DefuzzifyLargestOfMaximum(outputVariable, aggregatedMemberships, samplePoints),
            _ => throw new ArgumentException($"Unknown defuzzification method: {method}")
        };
    }

    /// <summary>
    /// Centroid (Center of Gravity) defuzzification
    /// Returns: (Σ μ(x) * x) / (Σ μ(x))
    /// </summary>
    private double DefuzzifyCentroid(
        LinguisticVariable outputVariable,
        Dictionary<string, double> aggregatedMemberships,
        int samplePoints)
    {
        var (values, memberships) = SampleAggregatedOutput(outputVariable, aggregatedMemberships, samplePoints);

        double numerator = 0;
        double denominator = 0;

        for (int i = 0; i < values.Length; i++)
        {
            numerator += memberships[i] * values[i];
            denominator += memberships[i];
        }

        if (denominator == 0)
        {
            // No rules fired, return midpoint
            return (outputVariable.MinValue + outputVariable.MaxValue) / 2;
        }

        return numerator / denominator;
    }

    /// <summary>
    /// Mean of Maximum defuzzification
    /// Returns the average of all points with maximum membership
    /// </summary>
    private double DefuzzifyMeanOfMaximum(
        LinguisticVariable outputVariable,
        Dictionary<string, double> aggregatedMemberships,
        int samplePoints)
    {
        var (values, memberships) = SampleAggregatedOutput(outputVariable, aggregatedMemberships, samplePoints);

        var maxMembership = memberships.Max();
        if (maxMembership == 0)
            return (outputVariable.MinValue + outputVariable.MaxValue) / 2;

        var maxValues = values.Where((v, i) => Math.Abs(memberships[i] - maxMembership) < 1e-6).ToArray();
        return maxValues.Average();
    }

    /// <summary>
    /// Smallest of Maximum defuzzification
    /// Returns the smallest x where membership is maximum
    /// </summary>
    private double DefuzzifySmallestOfMaximum(
        LinguisticVariable outputVariable,
        Dictionary<string, double> aggregatedMemberships,
        int samplePoints)
    {
        var (values, memberships) = SampleAggregatedOutput(outputVariable, aggregatedMemberships, samplePoints);

        var maxMembership = memberships.Max();
        if (maxMembership == 0)
            return outputVariable.MinValue;

        for (int i = 0; i < values.Length; i++)
        {
            if (Math.Abs(memberships[i] - maxMembership) < 1e-6)
                return values[i];
        }

        return outputVariable.MinValue;
    }

    /// <summary>
    /// Largest of Maximum defuzzification
    /// Returns the largest x where membership is maximum
    /// </summary>
    private double DefuzzifyLargestOfMaximum(
        LinguisticVariable outputVariable,
        Dictionary<string, double> aggregatedMemberships,
        int samplePoints)
    {
        var (values, memberships) = SampleAggregatedOutput(outputVariable, aggregatedMemberships, samplePoints);

        var maxMembership = memberships.Max();
        if (maxMembership == 0)
            return outputVariable.MaxValue;

        for (int i = values.Length - 1; i >= 0; i--)
        {
            if (Math.Abs(memberships[i] - maxMembership) < 1e-6)
                return values[i];
        }

        return outputVariable.MaxValue;
    }

    /// <summary>
    /// Samples the aggregated fuzzy output across the output variable's range
    /// </summary>
    private (double[] Values, double[] Memberships) SampleAggregatedOutput(
        LinguisticVariable outputVariable,
        Dictionary<string, double> aggregatedMemberships,
        int samplePoints)
    {
        var step = (outputVariable.MaxValue - outputVariable.MinValue) / (samplePoints - 1);
        var values = new double[samplePoints];
        var memberships = new double[samplePoints];

        for (int i = 0; i < samplePoints; i++)
        {
            values[i] = outputVariable.MinValue + i * step;

            // For each point, calculate the aggregated membership
            // using the maximum of clipped membership functions
            double maxMembership = 0;

            foreach (var (fuzzySetName, clippingHeight) in aggregatedMemberships)
            {
                var fuzzySet = outputVariable.GetFuzzySet(fuzzySetName);
                if (fuzzySet == null)
                    continue;

                // Get membership and clip to the firing strength
                var membership = Math.Min(fuzzySet.GetMembership(values[i]), clippingHeight);
                maxMembership = Math.Max(maxMembership, membership);
            }

            memberships[i] = maxMembership;
        }

        return (values, memberships);
    }
}

/// <summary>
/// Defuzzification methods
/// </summary>
public enum DefuzzificationMethod
{
    /// <summary>
    /// Center of gravity/area (most common)
    /// </summary>
    Centroid,

    /// <summary>
    /// Average of points with maximum membership
    /// </summary>
    MeanOfMaximum,

    /// <summary>
    /// Smallest point with maximum membership
    /// </summary>
    SmallestOfMaximum,

    /// <summary>
    /// Largest point with maximum membership
    /// </summary>
    LargestOfMaximum
}
