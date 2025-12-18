using System;
using System.Collections.Generic;
using System.Linq;
using BahyWay.RulesEngine.Models; // Import Models to see LinguisticVariable/FuzzyRule

namespace BahyWay.RulesEngine.Core
{
    public class FuzzyRuleSet
    {
        public string Name { get; set; }
        public List<FuzzyRule> Rules { get; set; } = new List<FuzzyRule>();
        public List<LinguisticVariable> Inputs { get; set; } = new List<LinguisticVariable>();
        public List<LinguisticVariable> Outputs { get; set; } = new List<LinguisticVariable>();

        // --- FIX 1: Constructor ---
        // This fixes error CS1729: 'FuzzyRuleSet' does not contain a constructor that takes 1 arguments
        public FuzzyRuleSet(string name)
        {
            Name = name;
        }

        // --- FIX 2: Helper Methods ---
        // These fix error CS1061: 'FuzzyRuleSet' does not contain a definition for 'Add...'

        public void AddInputVariable(LinguisticVariable variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            Inputs.Add(variable);
        }

        public void AddOutputVariable(LinguisticVariable variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            Outputs.Add(variable);
        }

        public void AddRule(FuzzyRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            Rules.Add(rule);
        }

        // --- Lookup Logic ---
        public LinguisticVariable? GetInputVariable(string name)
        {
            return Inputs.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public LinguisticVariable? GetOutputVariable(string name)
        {
            return Outputs.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}