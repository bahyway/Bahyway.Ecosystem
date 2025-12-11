using BahyWay.RulesEngine.Interfaces;

namespace BahyWay.RulesEngine.FuzzyLogic;

/// <summary>
/// Gaussian membership function defined by mean (center) and sigma (standard deviation)
/// μ(x) = exp(-(x-c)²/(2σ²))
/// </summary>
public class GaussianMembershipFunction : IMembershipFunction
{
    public string Name { get; }
    public double Mean { get; }   // Center
    public double Sigma { get; }  // Standard deviation

    public GaussianMembershipFunction(string name, double mean, double sigma)
    {
        if (sigma <= 0)
            throw new ArgumentException("Sigma must be positive", nameof(sigma));

        Name = name ?? throw new ArgumentNullException(nameof(name));
        Mean = mean;
        Sigma = sigma;
    }

    public double Evaluate(double x)
    {
        var diff = x - Mean;
        return Math.Exp(-(diff * diff) / (2 * Sigma * Sigma));
    }

    public (double Min, double Max) GetSupport()
    {
        // Support extends to about 3 standard deviations
        var range = 3 * Sigma;
        return (Mean - range, Mean + range);
    }

    public (double Min, double Max)? GetCore()
    {
        // For Gaussian, core is a single point at the mean
        return (Mean, Mean);
    }

    public override string ToString() => $"Gaussian(μ={Mean:F2}, σ={Sigma:F2})";
}
