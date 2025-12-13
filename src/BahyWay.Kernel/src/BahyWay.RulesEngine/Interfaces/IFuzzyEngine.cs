using System.Collections.Generic;
using System.Threading.Tasks;
using BahyWay.RulesEngine.Core; // <--- Pointing to where FuzzyRuleSet lives

namespace BahyWay.RulesEngine.Interfaces
{
    public interface IFuzzyEngine
    {
        // CRUD for Rules
        void AddRuleSet(string name, FuzzyRuleSet ruleSet);

        // Note: Added '?' to allow null return, fixing CS0738
        FuzzyRuleSet? GetRuleSet(string name);

        IEnumerable<string> GetRuleSetNames();

        // Evaluation Methods
        Task<Dictionary<string, double>> EvaluateAsync(string ruleSetName, Dictionary<string, double> inputs);
        Dictionary<string, double> Evaluate(string ruleSetName, Dictionary<string, double> inputs);
    }
}