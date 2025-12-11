namespace BahyWay.RulesEngine.Interfaces;

/// <summary>
/// Main interface for fuzzy inference engine
/// </summary>
public interface IFuzzyEngine
{
    /// <summary>
    /// Evaluates fuzzy rules with given inputs
    /// </summary>
    /// <param name="ruleSetName">Name of the rule set to evaluate</param>
    /// <param name="inputs">Dictionary of input variable names and their crisp values</param>
    /// <returns>Dictionary of output variable names and their defuzzified values</returns>
    Task<Dictionary<string, double>> EvaluateAsync(string ruleSetName, Dictionary<string, double> inputs);

    /// <summary>
    /// Synchronous evaluation of fuzzy rules
    /// </summary>
    Dictionary<string, double> Evaluate(string ruleSetName, Dictionary<string, double> inputs);

    /// <summary>
    /// Adds a rule set to the engine
    /// </summary>
    void AddRuleSet(string name, FuzzyRuleSet ruleSet);

    /// <summary>
    /// Gets a rule set by name
    /// </summary>
    FuzzyRuleSet? GetRuleSet(string name);

    /// <summary>
    /// Gets all rule set names
    /// </summary>
    IEnumerable<string> GetRuleSetNames();
}
