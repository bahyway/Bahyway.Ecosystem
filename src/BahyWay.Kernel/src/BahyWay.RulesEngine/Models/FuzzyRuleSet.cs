namespace BahyWay.RulesEngine.Models;

/// <summary>
/// Collection of fuzzy rules with input/output linguistic variables
/// </summary>
public class FuzzyRuleSet
{
    public string Name { get; }
    private readonly Dictionary<string, LinguisticVariable> _inputVariables;
    private readonly Dictionary<string, LinguisticVariable> _outputVariables;
    private readonly List<FuzzyRule> _rules;

    public FuzzyRuleSet(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _inputVariables = new Dictionary<string, LinguisticVariable>(StringComparer.OrdinalIgnoreCase);
        _outputVariables = new Dictionary<string, LinguisticVariable>(StringComparer.OrdinalIgnoreCase);
        _rules = new List<FuzzyRule>();
    }

    public void AddInputVariable(LinguisticVariable variable)
    {
        if (variable == null)
            throw new ArgumentNullException(nameof(variable));
        _inputVariables[variable.Name] = variable;
    }

    public void AddOutputVariable(LinguisticVariable variable)
    {
        if (variable == null)
            throw new ArgumentNullException(nameof(variable));
        _outputVariables[variable.Name] = variable;
    }

    public void AddRule(FuzzyRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
        _rules.Add(rule);
    }

    public void AddRules(IEnumerable<FuzzyRule> rules)
    {
        foreach (var rule in rules)
            AddRule(rule);
    }

    public LinguisticVariable? GetInputVariable(string name)
    {
        _inputVariables.TryGetValue(name, out var variable);
        return variable;
    }

    public LinguisticVariable? GetOutputVariable(string name)
    {
        _outputVariables.TryGetValue(name, out var variable);
        return variable;
    }

    public IReadOnlyList<FuzzyRule> Rules => _rules.AsReadOnly();
    public IEnumerable<string> InputVariableNames => _inputVariables.Keys;
    public IEnumerable<string> OutputVariableNames => _outputVariables.Keys;

    public override string ToString() =>
        $"RuleSet: {Name} ({_inputVariables.Count} inputs, {_outputVariables.Count} outputs, {_rules.Count} rules)";
}
