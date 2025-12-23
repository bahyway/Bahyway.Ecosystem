using System.Collections.Generic;
using BahyWay.RulesEngine.Core.Models; // <--- THIS WAS MISSING

namespace BahyWay.RulesEngine.Core.Core
{
    public class FuzzyRuleSet
    {
        public string Name { get; set; }
        public List<FuzzyRule> Rules { get; private set; }

        public FuzzyRuleSet(string name)
        {
            Name = name;
            Rules = new List<FuzzyRule>();
        }

        public void AddRule(FuzzyRule rule)
        {
            Rules.Add(rule);
        }
    }
}