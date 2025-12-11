//! Fuzzy Inference System
//!
//! This module provides a simple implementation of a fuzzy inference system
//! using Mamdani-style inference.

use crate::operations::{fuzzy_intersection, fuzzy_union};
use crate::defuzzification::centroid;

/// A fuzzy rule: IF condition THEN conclusion
pub struct FuzzyRule {
    /// Function that evaluates the antecedent (IF part)
    pub condition: Box<dyn Fn(f64) -> f64>,
    /// Function that evaluates the consequent (THEN part) for a given output value
    pub conclusion: Box<dyn Fn(f64) -> f64>,
}

impl FuzzyRule {
    /// Create a new fuzzy rule
    pub fn new(
        condition: Box<dyn Fn(f64) -> f64>,
        conclusion: Box<dyn Fn(f64) -> f64>,
    ) -> Self {
        FuzzyRule {
            condition,
            conclusion,
        }
    }

    /// Evaluate the rule for a given input
    ///
    /// Returns the rule firing strength
    pub fn evaluate_condition(&self, input: f64) -> f64 {
        (self.condition)(input)
    }

    /// Apply rule strength to conclusion membership function
    pub fn apply(&self, input: f64, output: f64) -> f64 {
        let strength = self.evaluate_condition(input);
        let conclusion_membership = (self.conclusion)(output);
        fuzzy_intersection(strength, conclusion_membership)
    }
}

/// Fuzzy controller with multiple rules
pub struct FuzzyController {
    rules: Vec<FuzzyRule>,
    output_range: (f64, f64, usize), // (min, max, steps)
}

impl FuzzyController {
    /// Create a new fuzzy controller
    ///
    /// # Arguments
    ///
    /// * `output_min` - Minimum output value
    /// * `output_max` - Maximum output value
    /// * `output_steps` - Number of discrete steps in output range
    pub fn new(output_min: f64, output_max: f64, output_steps: usize) -> Self {
        FuzzyController {
            rules: Vec::new(),
            output_range: (output_min, output_max, output_steps),
        }
    }

    /// Add a rule to the controller
    pub fn add_rule(&mut self, rule: FuzzyRule) {
        self.rules.push(rule);
    }

    /// Evaluate the controller for a given input
    ///
    /// Uses Mamdani inference with centroid defuzzification
    pub fn evaluate(&self, input: f64) -> f64 {
        let (min, max, steps) = self.output_range;
        let step_size = (max - min) / (steps as f64);

        // Create output universe
        let output_values: Vec<f64> = (0..steps)
            .map(|i| min + i as f64 * step_size)
            .collect();

        // Aggregate all rules
        let mut aggregated: Vec<f64> = vec![0.0; steps];

        for rule in &self.rules {
            for (i, &output_val) in output_values.iter().enumerate() {
                let rule_output = rule.apply(input, output_val);
                // Aggregation using maximum (fuzzy union)
                aggregated[i] = fuzzy_union(aggregated[i], rule_output);
            }
        }

        // Defuzzify using centroid method
        centroid(&output_values, &aggregated)
    }
}

/// Two-input fuzzy controller
pub struct TwoInputFuzzyController {
    rules: Vec<TwoInputFuzzyRule>,
    output_range: (f64, f64, usize),
}

/// A fuzzy rule with two inputs
pub struct TwoInputFuzzyRule {
    pub condition1: Box<dyn Fn(f64) -> f64>,
    pub condition2: Box<dyn Fn(f64) -> f64>,
    pub conclusion: Box<dyn Fn(f64) -> f64>,
}

impl TwoInputFuzzyRule {
    pub fn new(
        condition1: Box<dyn Fn(f64) -> f64>,
        condition2: Box<dyn Fn(f64) -> f64>,
        conclusion: Box<dyn Fn(f64) -> f64>,
    ) -> Self {
        TwoInputFuzzyRule {
            condition1,
            condition2,
            conclusion,
        }
    }

    /// Evaluate rule with AND operation on conditions
    pub fn apply(&self, input1: f64, input2: f64, output: f64) -> f64 {
        let strength1 = (self.condition1)(input1);
        let strength2 = (self.condition2)(input2);
        let combined_strength = fuzzy_intersection(strength1, strength2);
        let conclusion_membership = (self.conclusion)(output);
        fuzzy_intersection(combined_strength, conclusion_membership)
    }
}

impl TwoInputFuzzyController {
    pub fn new(output_min: f64, output_max: f64, output_steps: usize) -> Self {
        TwoInputFuzzyController {
            rules: Vec::new(),
            output_range: (output_min, output_max, output_steps),
        }
    }

    pub fn add_rule(&mut self, rule: TwoInputFuzzyRule) {
        self.rules.push(rule);
    }

    pub fn evaluate(&self, input1: f64, input2: f64) -> f64 {
        let (min, max, steps) = self.output_range;
        let step_size = (max - min) / (steps as f64);

        let output_values: Vec<f64> = (0..steps)
            .map(|i| min + i as f64 * step_size)
            .collect();

        let mut aggregated: Vec<f64> = vec![0.0; steps];

        for rule in &self.rules {
            for (i, &output_val) in output_values.iter().enumerate() {
                let rule_output = rule.apply(input1, input2, output_val);
                aggregated[i] = fuzzy_union(aggregated[i], rule_output);
            }
        }

        centroid(&output_values, &aggregated)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::membership::MembershipFunction;

    #[test]
    fn test_simple_controller() {
        let mut controller = FuzzyController::new(0.0, 100.0, 100);

        // Create simple rules: cold -> low, hot -> high
        let cold = MembershipFunction::Triangular {
            a: 0.0,
            b: 0.0,
            c: 20.0,
        };
        let hot = MembershipFunction::Triangular {
            a: 30.0,
            b: 40.0,
            c: 40.0,
        };
        let low = MembershipFunction::Triangular {
            a: 0.0,
            b: 0.0,
            c: 50.0,
        };
        let high = MembershipFunction::Triangular {
            a: 50.0,
            b: 100.0,
            c: 100.0,
        };

        // Rule 1: IF cold THEN low
        let rule1 = FuzzyRule::new(
            Box::new(move |x| cold.evaluate(x)),
            Box::new(move |x| low.evaluate(x)),
        );

        // Rule 2: IF hot THEN high
        let rule2 = FuzzyRule::new(
            Box::new(move |x| hot.evaluate(x)),
            Box::new(move |x| high.evaluate(x)),
        );

        controller.add_rule(rule1);
        controller.add_rule(rule2);

        // Test
        let output_cold = controller.evaluate(10.0);
        let output_hot = controller.evaluate(35.0);

        assert!(output_cold < 50.0, "Cold input should produce low output");
        assert!(output_hot > 50.0, "Hot input should produce high output");
    }

    #[test]
    fn test_two_input_controller() {
        let mut controller = TwoInputFuzzyController::new(0.0, 100.0, 100);

        let cold = MembershipFunction::Triangular {
            a: 0.0,
            b: 0.0,
            c: 20.0,
        };
        let high_humidity = MembershipFunction::Triangular {
            a: 60.0,
            b: 100.0,
            c: 100.0,
        };
        let fast = MembershipFunction::Triangular {
            a: 60.0,
            b: 100.0,
            c: 100.0,
        };

        // Rule: IF temp is cold AND humidity is high THEN fan is fast
        let rule = TwoInputFuzzyRule::new(
            Box::new(move |x| cold.evaluate(x)),
            Box::new(move |x| high_humidity.evaluate(x)),
            Box::new(move |x| fast.evaluate(x)),
        );

        controller.add_rule(rule);

        let output = controller.evaluate(15.0, 80.0);
        assert!(output > 50.0, "Should produce high output");
    }
}
