using BahyWay.RulesEngine.Models; // <--- IMPORTANT: Point this to where FuzzyRuleSet.cs lives
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BahyWay.RulesEngine.Interfaces
{
    public interface IFuzzyEngine
    {
        // CRUD for Rules
        void AddRuleSet(string name, FuzzyRuleSet ruleSet);
        FuzzyRuleSet GetRuleSet(string name);
        IEnumerable<string> GetRuleSetNames();

        // Evaluation Methods
        /// <summary>
        /// Evaluates fuzzy rules with given inputs (Async)
        /// </summary>
        Task<Dictionary<string, double>> EvaluateAsync(string ruleSetName, Dictionary<string, double> inputs);

        /// <summary>
        /// Synchronous evaluation of fuzzy rules
        /// </summary>
        Dictionary<string, double> Evaluate(string ruleSetName, Dictionary<string, double> inputs);
    }
}