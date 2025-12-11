using BahyWay.RulesEngine.Core;
using BahyWay.RulesEngine.FuzzyLogic;
using BahyWay.RulesEngine.Models;

namespace BahyWay.RulesEngine.Examples;

/// <summary>
/// Example fuzzy logic system for classifying alarm severity
/// Inputs: ErrorFrequency (0-100), SystemImpact (0-10), HistoricalPattern (0-10)
/// Output: Severity (0-4) mapped to {Info, Warning, Error, Critical}
/// </summary>
public static class AlarmClassificationExample
{
    public static FuzzyRuleSet CreateAlarmClassificationRuleSet()
    {
        var ruleSet = new FuzzyRuleSet("AlarmClassification");

        // Define Input Variable 1: Error Frequency (errors per minute)
        var errorFrequency = new LinguisticVariable("ErrorFrequency", 0, 100);

        errorFrequency.AddFuzzySet(new FuzzySet(
            "Low",
            new TrapezoidalMembershipFunction("Low", 0, 0, 20, 40)
        ));

        errorFrequency.AddFuzzySet(new FuzzySet(
            "Medium",
            new TriangularMembershipFunction("Medium", 30, 50, 70)
        ));

        errorFrequency.AddFuzzySet(new FuzzySet(
            "High",
            new TrapezoidalMembershipFunction("High", 60, 80, 100, 100)
        ));

        ruleSet.AddInputVariable(errorFrequency);

        // Define Input Variable 2: System Impact (0-10 scale)
        var systemImpact = new LinguisticVariable("SystemImpact", 0, 10);

        systemImpact.AddFuzzySet(new FuzzySet(
            "Minor",
            new TrapezoidalMembershipFunction("Minor", 0, 0, 2, 4)
        ));

        systemImpact.AddFuzzySet(new FuzzySet(
            "Moderate",
            new TriangularMembershipFunction("Moderate", 3, 5, 7)
        ));

        systemImpact.AddFuzzySet(new FuzzySet(
            "Critical",
            new TrapezoidalMembershipFunction("Critical", 6, 8, 10, 10)
        ));

        ruleSet.AddInputVariable(systemImpact);

        // Define Input Variable 3: Historical Pattern (0-10 scale)
        var historicalPattern = new LinguisticVariable("HistoricalPattern", 0, 10);

        historicalPattern.AddFuzzySet(new FuzzySet(
            "Rare",
            new TrapezoidalMembershipFunction("Rare", 0, 0, 2, 4)
        ));

        historicalPattern.AddFuzzySet(new FuzzySet(
            "Occasional",
            new TriangularMembershipFunction("Occasional", 3, 5, 7)
        ));

        historicalPattern.AddFuzzySet(new FuzzySet(
            "Frequent",
            new TrapezoidalMembershipFunction("Frequent", 6, 8, 10, 10)
        ));

        ruleSet.AddInputVariable(historicalPattern);

        // Define Output Variable: Severity (0-4)
        // 0-1: Info, 1-2: Warning, 2-3: Error, 3-4: Critical
        var severity = new LinguisticVariable("Severity", 0, 4);

        severity.AddFuzzySet(new FuzzySet(
            "Info",
            new TriangularMembershipFunction("Info", 0, 0, 1)
        ));

        severity.AddFuzzySet(new FuzzySet(
            "Warning",
            new TriangularMembershipFunction("Warning", 0.5, 1.5, 2.5)
        ));

        severity.AddFuzzySet(new FuzzySet(
            "Error",
            new TriangularMembershipFunction("Error", 1.5, 2.5, 3.5)
        ));

        severity.AddFuzzySet(new FuzzySet(
            "Critical",
            new TriangularMembershipFunction("Critical", 3, 4, 4)
        ));

        ruleSet.AddOutputVariable(severity);

        // Define Fuzzy Rules
        AddAlarmRules(ruleSet);

        return ruleSet;
    }

    private static void AddAlarmRules(FuzzyRuleSet ruleSet)
    {
        // Rule 1: Low frequency, minor impact -> Info
        ruleSet.AddRule(new FuzzyRule(
            "LowFreq_MinorImpact_Info",
            inputs => FuzzyOperations.And(
                inputs["ErrorFrequency_Low"],
                inputs["SystemImpact_Minor"]
            ),
            "Severity",
            "Info"
        ));

        // Rule 2: Low frequency, moderate impact -> Warning
        ruleSet.AddRule(new FuzzyRule(
            "LowFreq_ModerateImpact_Warning",
            inputs => FuzzyOperations.And(
                inputs["ErrorFrequency_Low"],
                inputs["SystemImpact_Moderate"]
            ),
            "Severity",
            "Warning"
        ));

        // Rule 3: Medium frequency, minor impact -> Warning
        ruleSet.AddRule(new FuzzyRule(
            "MediumFreq_MinorImpact_Warning",
            inputs => FuzzyOperations.And(
                inputs["ErrorFrequency_Medium"],
                inputs["SystemImpact_Minor"]
            ),
            "Severity",
            "Warning"
        ));

        // Rule 4: Medium frequency, moderate impact -> Error
        ruleSet.AddRule(new FuzzyRule(
            "MediumFreq_ModerateImpact_Error",
            inputs => FuzzyOperations.And(
                inputs["ErrorFrequency_Medium"],
                inputs["SystemImpact_Moderate"]
            ),
            "Severity",
            "Error"
        ));

        // Rule 5: High frequency OR critical impact -> Critical
        ruleSet.AddRule(new FuzzyRule(
            "HighFreq_OR_CriticalImpact_Critical",
            inputs => FuzzyOperations.Or(
                inputs["ErrorFrequency_High"],
                inputs["SystemImpact_Critical"]
            ),
            "Severity",
            "Critical"
        ));

        // Rule 6: High frequency AND critical impact -> Critical (with emphasis)
        ruleSet.AddRule(new FuzzyRule(
            "HighFreq_AND_CriticalImpact_Critical",
            inputs => FuzzyOperations.Very(FuzzyOperations.And(
                inputs["ErrorFrequency_High"],
                inputs["SystemImpact_Critical"]
            )),
            "Severity",
            "Critical",
            weight: 1.0
        ));

        // Rule 7: Frequent historical pattern increases severity
        ruleSet.AddRule(new FuzzyRule(
            "FrequentPattern_CriticalImpact_Critical",
            inputs => FuzzyOperations.And(
                inputs["HistoricalPattern_Frequent"],
                inputs["SystemImpact_Critical"]
            ),
            "Severity",
            "Critical"
        ));

        // Rule 8: Rare pattern with low impact -> Info
        ruleSet.AddRule(new FuzzyRule(
            "RarePattern_MinorImpact_Info",
            inputs => FuzzyOperations.And(
                inputs["HistoricalPattern_Rare"],
                inputs["SystemImpact_Minor"]
            ),
            "Severity",
            "Info"
        ));

        // Rule 9: Medium frequency, critical impact -> Critical
        ruleSet.AddRule(new FuzzyRule(
            "MediumFreq_CriticalImpact_Critical",
            inputs => FuzzyOperations.And(
                inputs["ErrorFrequency_Medium"],
                inputs["SystemImpact_Critical"]
            ),
            "Severity",
            "Critical"
        ));

        // Rule 10: Low frequency, rare pattern, critical impact -> Error
        ruleSet.AddRule(new FuzzyRule(
            "LowFreq_RarePattern_CriticalImpact_Error",
            inputs => FuzzyOperations.And(
                FuzzyOperations.And(
                    inputs["ErrorFrequency_Low"],
                    inputs["HistoricalPattern_Rare"]
                ),
                inputs["SystemImpact_Critical"]
            ),
            "Severity",
            "Error"
        ));
    }

    /// <summary>
    /// Demonstrates the alarm classification system
    /// </summary>
    public static void RunExample()
    {
        Console.WriteLine("=== Alarm Classification Fuzzy Logic Example ===\n");

        // Create the fuzzy engine
        var engine = new FuzzyEngine();

        // Add the alarm classification rule set
        var ruleSet = CreateAlarmClassificationRuleSet();
        engine.AddRuleSet("AlarmClassification", ruleSet);

        // Test scenarios
        var scenarios = new[]
        {
            new { Name = "Low priority alarm", ErrorFreq = 5.0, Impact = 1.0, Pattern = 1.0 },
            new { Name = "Medium priority alarm", ErrorFreq = 50.0, Impact = 5.0, Pattern = 5.0 },
            new { Name = "High priority alarm", ErrorFreq = 85.0, Impact = 8.0, Pattern = 7.0 },
            new { Name = "Critical system failure", ErrorFreq = 95.0, Impact = 9.5, Pattern = 9.0 },
            new { Name = "Rare but critical", ErrorFreq = 10.0, Impact = 9.0, Pattern = 1.0 },
        };

        foreach (var scenario in scenarios)
        {
            var inputs = new Dictionary<string, double>
            {
                ["ErrorFrequency"] = scenario.ErrorFreq,
                ["SystemImpact"] = scenario.Impact,
                ["HistoricalPattern"] = scenario.Pattern
            };

            var outputs = engine.Evaluate("AlarmClassification", inputs);
            var severityValue = outputs["Severity"];

            var severityLabel = severityValue switch
            {
                < 1.0 => "Info",
                < 2.0 => "Warning",
                < 3.0 => "Error",
                _ => "Critical"
            };

            Console.WriteLine($"Scenario: {scenario.Name}");
            Console.WriteLine($"  Inputs:");
            Console.WriteLine($"    Error Frequency: {scenario.ErrorFreq:F1}");
            Console.WriteLine($"    System Impact: {scenario.Impact:F1}");
            Console.WriteLine($"    Historical Pattern: {scenario.Pattern:F1}");
            Console.WriteLine($"  Output:");
            Console.WriteLine($"    Severity Score: {severityValue:F2}");
            Console.WriteLine($"    Severity Label: {severityLabel}");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Maps fuzzy severity score to enum
    /// </summary>
    public static AlarmSeverity MapToSeverity(double severityScore)
    {
        return severityScore switch
        {
            < 1.0 => AlarmSeverity.Info,
            < 2.0 => AlarmSeverity.Warning,
            < 3.0 => AlarmSeverity.Error,
            _ => AlarmSeverity.Critical
        };
    }
}

/// <summary>
/// Alarm severity levels
/// </summary>
public enum AlarmSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}
