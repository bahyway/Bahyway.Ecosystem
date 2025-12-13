using BahyWay.RulesEngine.Interfaces;
using BahyWay.RulesEngine.Models;
using Microsoft.Extensions.Logging;

namespace BahyWay.RulesEngine.Core;

/// <summary>
/// Main fuzzy inference engine implementation
/// </summary>
public class FuzzyEngine : IFuzzyEngine
{
    private readonly Dictionary<string, FuzzyRuleSet> _ruleSets;
    private readonly ILogger<FuzzyEngine>? _logger;
    private readonly DefuzzificationEngine _defuzzifier;

    public FuzzyEngine(ILogger<FuzzyEngine>? logger = null)
    {
        _ruleSets = new Dictionary<string, FuzzyRuleSet>(StringComparer.OrdinalIgnoreCase);
        _logger = logger;
        _defuzzifier = new DefuzzificationEngine();
    }

    public void AddRuleSet(string name, FuzzyRuleSet ruleSet)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        if (ruleSet == null)
            throw new ArgumentNullException(nameof(ruleSet));

        _ruleSets[name] = ruleSet;
        _logger?.LogInformation("Added rule set: {RuleSetName} with {RuleCount} rules", name, ruleSet.Rules.Count);
    }

    public FuzzyRuleSet? GetRuleSet(string name)
    {
        _ruleSets.TryGetValue(name, out var ruleSet);
        return ruleSet;
    }

    public IEnumerable<string> GetRuleSetNames() => _ruleSets.Keys;

    public Dictionary<string, double> Evaluate(string ruleSetName, Dictionary<string, double> inputs)
    {
        var ruleSet = GetRuleSet(ruleSetName);
        if (ruleSet == null)
            throw new ArgumentException($"Rule set '{ruleSetName}' not found", nameof(ruleSetName));

        _logger?.LogDebug("Evaluating rule set: {RuleSetName} with inputs: {@Inputs}", ruleSetName, inputs);

        // Step 1: Fuzzification - Convert crisp inputs to fuzzy memberships
        var fuzzifiedInputs = Fuzzify(ruleSet, inputs);

        // Step 2: Rule Evaluation - Evaluate all rules
        var ruleOutputs = EvaluateRules(ruleSet, fuzzifiedInputs);

        // Step 3: Aggregation and Defuzzification - Combine rule outputs and defuzzify
        var outputs = AggregateAndDefuzzify(ruleSet, ruleOutputs);

        _logger?.LogDebug("Evaluation complete. Outputs: {@Outputs}", outputs);

        return outputs;
    }

    public async Task<Dictionary<string, double>> EvaluateAsync(string ruleSetName, Dictionary<string, double> inputs)
    {
        // For now, just wrap synchronous call
        // In the future, could parallelize rule evaluation
        return await Task.Run(() => Evaluate(ruleSetName, inputs));
    }

    /// <summary>
    /// Step 1: Fuzzification - Convert crisp inputs to fuzzy membership degrees
    /// </summary>
    private Dictionary<string, double> Fuzzify(FuzzyRuleSet ruleSet, Dictionary<string, double> crispInputs)
    {
        var fuzzified = new Dictionary<string, double>();

        foreach (var (inputName, crispValue) in crispInputs)
        {
            var variable = ruleSet.GetInputVariable(inputName);
            if (variable == null)
            {
                _logger?.LogWarning("Input variable '{InputName}' not found in rule set", inputName);
                continue;
            }

            // Get membership degrees for all fuzzy sets of this variable
            var memberships = variable.Fuzzify(crispValue);
            foreach (var (fuzzySetName, membership) in memberships)
            {
                var key = $"{inputName}_{fuzzySetName}";
                fuzzified[key] = membership;
                _logger?.LogTrace("Fuzzified: {Key} = {Membership:F3}", key, membership);
            }
        }

        return fuzzified;
    }

    /// <summary>
    /// Step 2: Rule Evaluation - Evaluate all rules and determine firing strengths
    /// </summary>
    private List<(FuzzyRule Rule, double FiringStrength)> EvaluateRules(
        FuzzyRuleSet ruleSet,
        Dictionary<string, double> fuzzifiedInputs)
    {
        var results = new List<(FuzzyRule, double)>();

        foreach (var rule in ruleSet.Rules)
        {
            var firingStrength = rule.Evaluate(fuzzifiedInputs);
            results.Add((rule, firingStrength));

            _logger?.LogTrace("Rule '{RuleName}' fired with strength {FiringStrength:F3}",
                rule.Name, firingStrength);
        }

        return results;
    }

    /// <summary>
    /// Step 3: Aggregation and Defuzzification
    /// </summary>
    private Dictionary<string, double> AggregateAndDefuzzify(
        FuzzyRuleSet ruleSet,
        List<(FuzzyRule Rule, double FiringStrength)> ruleOutputs)
    {
        var outputs = new Dictionary<string, double>();

        // Group rules by output variable
        var groupedByOutput = ruleOutputs
            .GroupBy(ro => ro.Rule.ConsequentVariable);

        foreach (var group in groupedByOutput)
        {
            var outputVarName = group.Key;
            var outputVar = ruleSet.GetOutputVariable(outputVarName);

            if (outputVar == null)
            {
                _logger?.LogWarning("Output variable '{OutputVarName}' not found", outputVarName);
                continue;
            }

            // Aggregate rule outputs using maximum (Mamdani-style)
            var aggregatedMemberships = new Dictionary<string, double>();

            foreach (var (rule, firingStrength) in group)
            {
                var fuzzySetName = rule.ConsequentFuzzySet;
                if (!aggregatedMemberships.ContainsKey(fuzzySetName))
                    aggregatedMemberships[fuzzySetName] = 0.0;

                // Use maximum for aggregation
                aggregatedMemberships[fuzzySetName] = Math.Max(
                    aggregatedMemberships[fuzzySetName],
                    firingStrength
                );
            }

            // Defuzzify to get crisp output
            var crispOutput = _defuzzifier.Defuzzify(outputVar, aggregatedMemberships);
            outputs[outputVarName] = crispOutput;

            _logger?.LogDebug("Output '{OutputVarName}' defuzzified to {CrispOutput:F3}",
                outputVarName, crispOutput);
        }

        return outputs;
    }

    public void AddRuleSet(string name, FuzzyRuleSet ruleSet)
    {
        throw new NotImplementedException();
    }

    FuzzyRuleSet? IFuzzyEngine.GetRuleSet(string name)
    {
        throw new NotImplementedException();
    }

    public void AddRuleSet(string name, FuzzyRuleSet ruleSet)
    {
        throw new NotImplementedException();
    }

    FuzzyRuleSet? IFuzzyEngine.GetRuleSet(string name)
    {
        throw new NotImplementedException();
    }

    public void AddRuleSet(string name, FuzzyRuleSet ruleSet)
    {
        throw new NotImplementedException();
    }
}
