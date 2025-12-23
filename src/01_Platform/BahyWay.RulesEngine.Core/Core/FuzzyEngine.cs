using System;
using System.Collections.Generic;
using System.Linq;
using BahyWay.RulesEngine.Core.Interfaces;
using BahyWay.RulesEngine.Core.Models;

namespace BahyWay.RulesEngine.Core.Core
{
    public class FuzzyEngine : IFuzzyEngine
    {
        private readonly List<FuzzyRule> _rules = new();

        public void AddRule(FuzzyRule rule)
        {
            _rules.Add(rule);
        }

        public FuzzyEvaluationResult Evaluate(Dictionary<string, double> inputs)
        {
            var result = new FuzzyEvaluationResult();

            double numerator = 0;
            double denominator = 0;

            // Iterate through all rules in the system
            foreach (var rule in _rules)
            {
                // 1. Calculate Firing Strength (How "True" is this rule?)
                // e.g., If FileSize is Small, strength might be 0.8
                double firingStrength = rule.Evaluate(inputs);

                // If the rule applies (strength > 0)
                if (firingStrength > 0)
                {
                    // 2. COLLECT MESSAGES (The UI Feedback)
                    if (!string.IsNullOrEmpty(rule.FeedbackMessage))
                    {
                        // Avoid duplicates if multiple rules trigger the same message
                        if (!result.Messages.Contains(rule.FeedbackMessage))
                        {
                            result.Messages.Add(rule.FeedbackMessage);
                        }
                    }

                    // 3. DEFUZZIFICATION (Calculate Score)
                    // We map the Output Set Name to a numeric value for the Scoreboard
                    double outputValue = GetCentroidForSet(rule.ConsequentFuzzySet);

                    // Weighted Average Calculation
                    numerator += firingStrength * outputValue;
                    denominator += firingStrength;
                }
            }

            // 4. Final Score Calculation
            if (denominator == 0)
            {
                // If no rules fired, we assume perfection (100%) or neutral?
                // For Quality Assurance, usually "Innocent until proven guilty".
                result.Score = 100.0;
            }
            else
            {
                result.Score = numerator / denominator;
            }

            // 5. Determine Status Text
            result.Status = result.Score switch
            {
                < 50 => "Critical",
                < 90 => "Warning",
                _ => "Success"
            };

            return result;
        }

        // Helper: Maps linguistic output terms to numeric scores
        // In a full version, this would read from the LinguisticVariable definition
        private double GetCentroidForSet(string setName)
        {
            return setName.ToLower() switch
            {
                // Bad Outcomes
                "critical" or "low" or "bad" or "zero" => 0.0,

                // Warn Outcomes
                "warning" or "medium" or "average" => 50.0,

                // Good Outcomes
                "success" or "high" or "good" or "excellent" => 100.0,

                // Default
                _ => 50.0
            };
        }
    }
}