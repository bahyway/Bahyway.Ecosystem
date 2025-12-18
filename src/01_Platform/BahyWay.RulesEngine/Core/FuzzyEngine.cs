using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BahyWay.RulesEngine.Interfaces;
using BahyWay.RulesEngine.Models; // <--- CRITICAL for Logic
// Note: We don't need 'using ...Core' because we are INSIDE that namespace

namespace BahyWay.RulesEngine.Core
{
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

            // ... (Rest of your evaluation logic stays the same)
            // Just ensure you have the 'using BahyWay.RulesEngine.Models;' at the top!

            // Fuzzify Logic Placeholder for compilation context:
            var fuzzifiedInputs = Fuzzify(ruleSet, inputs);
            var ruleOutputs = EvaluateRules(ruleSet, fuzzifiedInputs);
            return AggregateAndDefuzzify(ruleSet, ruleOutputs);
        }

        public async Task<Dictionary<string, double>> EvaluateAsync(string ruleSetName, Dictionary<string, double> inputs)
        {
            return await Task.Run(() => Evaluate(ruleSetName, inputs));
        }

        // --- PRIVATE METHODS ---
        // Ensuring these use the classes from the 'Models' namespace we imported
        private Dictionary<string, double> Fuzzify(FuzzyRuleSet ruleSet, Dictionary<string, double> crispInputs)
        {
            var fuzzified = new Dictionary<string, double>();
            foreach (var (inputName, crispValue) in crispInputs)
            {
                var variable = ruleSet.GetInputVariable(inputName);
                if (variable == null) continue;

                // If you get errors here, ensure LinguisticVariable has a Fuzzify method in Models/LinguisticVariable.cs
                var memberships = variable.Fuzzify(crispValue);
                foreach (var (fuzzySetName, membership) in memberships)
                {
                    fuzzified[$"{inputName}_{fuzzySetName}"] = membership;
                }
            }
            return fuzzified;
        }

        private List<(FuzzyRule Rule, double FiringStrength)> EvaluateRules(FuzzyRuleSet ruleSet, Dictionary<string, double> fuzzifiedInputs)
        {
            // Implementation...
            return new List<(FuzzyRule, double)>();
        }

        private Dictionary<string, double> AggregateAndDefuzzify(FuzzyRuleSet ruleSet, List<(FuzzyRule Rule, double FiringStrength)> ruleOutputs)
        {
            // Implementation...
            return new Dictionary<string, double>();
        }
    }
}