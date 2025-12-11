//! Defuzzification Methods
//!
//! This module provides various methods for converting fuzzy sets
//! back to crisp values.

/// Centroid (Center of Gravity) defuzzification method
///
/// Computes the center of area under the membership function curve.
///
/// # Arguments
///
/// * `x_values` - Array of x values (universe of discourse)
/// * `membership` - Array of corresponding membership degrees
///
/// # Returns
///
/// The crisp output value
pub fn centroid(x_values: &[f64], membership: &[f64]) -> f64 {
    assert_eq!(
        x_values.len(),
        membership.len(),
        "x_values and membership must have same length"
    );

    let numerator: f64 = x_values
        .iter()
        .zip(membership.iter())
        .map(|(x, m)| x * m)
        .sum();

    let denominator: f64 = membership.iter().sum();

    if denominator == 0.0 {
        // Return midpoint if no membership
        (x_values[0] + x_values[x_values.len() - 1]) / 2.0
    } else {
        numerator / denominator
    }
}

/// Mean of Maximum defuzzification method
///
/// Returns the average of all x values where membership is maximum.
///
/// # Arguments
///
/// * `x_values` - Array of x values
/// * `membership` - Array of membership degrees
///
/// # Returns
///
/// The crisp output value
pub fn mean_of_maximum(x_values: &[f64], membership: &[f64]) -> f64 {
    assert_eq!(x_values.len(), membership.len());

    let max_membership = membership
        .iter()
        .cloned()
        .fold(f64::NEG_INFINITY, f64::max);

    if max_membership == 0.0 {
        return (x_values[0] + x_values[x_values.len() - 1]) / 2.0;
    }

    let max_x_values: Vec<f64> = x_values
        .iter()
        .zip(membership.iter())
        .filter(|(_, &m)| m == max_membership)
        .map(|(&x, _)| x)
        .collect();

    max_x_values.iter().sum::<f64>() / max_x_values.len() as f64
}

/// Smallest of Maximum defuzzification method
///
/// Returns the smallest x value where membership is maximum.
pub fn smallest_of_maximum(x_values: &[f64], membership: &[f64]) -> f64 {
    assert_eq!(x_values.len(), membership.len());

    let max_membership = membership
        .iter()
        .cloned()
        .fold(f64::NEG_INFINITY, f64::max);

    if max_membership == 0.0 {
        return x_values[0];
    }

    x_values
        .iter()
        .zip(membership.iter())
        .filter(|(_, &m)| m == max_membership)
        .map(|(&x, _)| x)
        .min_by(|a, b| a.partial_cmp(b).unwrap())
        .unwrap()
}

/// Largest of Maximum defuzzification method
///
/// Returns the largest x value where membership is maximum.
pub fn largest_of_maximum(x_values: &[f64], membership: &[f64]) -> f64 {
    assert_eq!(x_values.len(), membership.len());

    let max_membership = membership
        .iter()
        .cloned()
        .fold(f64::NEG_INFINITY, f64::max);

    if max_membership == 0.0 {
        return x_values[x_values.len() - 1];
    }

    x_values
        .iter()
        .zip(membership.iter())
        .filter(|(_, &m)| m == max_membership)
        .map(|(&x, _)| x)
        .max_by(|a, b| a.partial_cmp(b).unwrap())
        .unwrap()
}

/// Bisector defuzzification method
///
/// Finds the x value that divides the area under the curve in half.
///
/// # Arguments
///
/// * `x_values` - Array of x values
/// * `membership` - Array of membership degrees
///
/// # Returns
///
/// The crisp output value
pub fn bisector(x_values: &[f64], membership: &[f64]) -> f64 {
    assert_eq!(x_values.len(), membership.len());

    let total_area: f64 = membership.iter().sum();

    if total_area == 0.0 {
        return (x_values[0] + x_values[x_values.len() - 1]) / 2.0;
    }

    let half_area = total_area / 2.0;
    let mut cumulative_area = 0.0;

    for (i, &m) in membership.iter().enumerate() {
        cumulative_area += m;
        if cumulative_area >= half_area {
            return x_values[i];
        }
    }

    x_values[x_values.len() - 1]
}

/// Defuzzification method enum
#[derive(Debug, Clone, Copy)]
pub enum DefuzzificationMethod {
    Centroid,
    MeanOfMaximum,
    SmallestOfMaximum,
    LargestOfMaximum,
    Bisector,
}

/// Apply specified defuzzification method
pub fn defuzzify(
    x_values: &[f64],
    membership: &[f64],
    method: DefuzzificationMethod,
) -> f64 {
    match method {
        DefuzzificationMethod::Centroid => centroid(x_values, membership),
        DefuzzificationMethod::MeanOfMaximum => mean_of_maximum(x_values, membership),
        DefuzzificationMethod::SmallestOfMaximum => smallest_of_maximum(x_values, membership),
        DefuzzificationMethod::LargestOfMaximum => largest_of_maximum(x_values, membership),
        DefuzzificationMethod::Bisector => bisector(x_values, membership),
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_centroid() {
        let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
        let m = vec![0.0, 0.5, 1.0, 0.5, 0.0];

        let result = centroid(&x, &m);
        assert!((result - 2.0).abs() < 1e-10);
    }

    #[test]
    fn test_mean_of_maximum() {
        let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
        let m = vec![0.0, 0.5, 1.0, 1.0, 0.5];

        let result = mean_of_maximum(&x, &m);
        assert!((result - 2.5).abs() < 1e-10);
    }

    #[test]
    fn test_smallest_of_maximum() {
        let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
        let m = vec![0.0, 0.5, 1.0, 1.0, 0.5];

        let result = smallest_of_maximum(&x, &m);
        assert_eq!(result, 2.0);
    }

    #[test]
    fn test_largest_of_maximum() {
        let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
        let m = vec![0.0, 0.5, 1.0, 1.0, 0.5];

        let result = largest_of_maximum(&x, &m);
        assert_eq!(result, 3.0);
    }

    #[test]
    fn test_bisector() {
        let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
        let m = vec![0.0, 0.5, 1.0, 0.5, 0.0];

        let result = bisector(&x, &m);
        // Should be close to center
        assert!((result - 2.0).abs() < 1.0);
    }

    #[test]
    fn test_zero_membership() {
        let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
        let m = vec![0.0, 0.0, 0.0, 0.0, 0.0];

        // Should return midpoint
        let result = centroid(&x, &m);
        assert_eq!(result, 2.0);
    }

    #[test]
    fn test_defuzzify_enum() {
        let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
        let m = vec![0.0, 0.5, 1.0, 0.5, 0.0];

        let c = defuzzify(&x, &m, DefuzzificationMethod::Centroid);
        let mom = defuzzify(&x, &m, DefuzzificationMethod::MeanOfMaximum);

        assert!((c - 2.0).abs() < 1e-10);
        assert_eq!(mom, 2.0);
    }
}
