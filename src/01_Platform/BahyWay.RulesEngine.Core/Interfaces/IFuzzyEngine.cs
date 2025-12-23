using System.Collections.Generic;
using BahyWay.RulesEngine.Core.Models;

namespace BahyWay.RulesEngine.Core.Interfaces
{
    public interface IFuzzyEngine
    {
        /// <summary>
        /// Adds a fuzzy rule to the engine.
        /// </summary>
        void AddRule(FuzzyRule rule);

        /// <summary>
        /// Evaluates the input values against the rules and returns a score + messages.
        /// </summary>
        /// <param name="inputs">Dictionary of input variable names and their crisp values.</param>
        /// <returns>A result containing the defuzzified score and any feedback messages.</returns>
        FuzzyEvaluationResult Evaluate(Dictionary<string, double> inputs);
    }

    /// <summary>
    /// The detailed result of a fuzzy evaluation.
    /// </summary>
    public class FuzzyEvaluationResult
    {
        public double Score { get; set; } // 0.0 to 100.0
        public string Status { get; set; } // "Critical", "Warning", "Success"

        // The list of messages triggered by specific rules (e.g. "File too small")
        public List<string> Messages { get; set; } = new();
    }
}