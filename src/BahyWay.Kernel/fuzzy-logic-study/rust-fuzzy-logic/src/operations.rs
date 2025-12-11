//! Fuzzy Operations
//!
//! This module provides basic fuzzy set operations including
//! union, intersection, complement, and various T-norms and S-norms.

/// Fuzzy union (OR operation) using maximum
///
/// # Arguments
///
/// * `a` - Membership degree in set A
/// * `b` - Membership degree in set B
///
/// # Returns
///
/// max(a, b)
#[inline]
pub fn fuzzy_union(a: f64, b: f64) -> f64 {
    a.max(b)
}

/// Fuzzy intersection (AND operation) using minimum
///
/// # Arguments
///
/// * `a` - Membership degree in set A
/// * `b` - Membership degree in set B
///
/// # Returns
///
/// min(a, b)
#[inline]
pub fn fuzzy_intersection(a: f64, b: f64) -> f64 {
    a.min(b)
}

/// Fuzzy complement (NOT operation)
///
/// # Arguments
///
/// * `a` - Membership degree
///
/// # Returns
///
/// 1.0 - a
#[inline]
pub fn fuzzy_complement(a: f64) -> f64 {
    1.0 - a
}

/// Algebraic product (T-norm alternative to minimum)
///
/// # Arguments
///
/// * `a` - Membership degree in set A
/// * `b` - Membership degree in set B
///
/// # Returns
///
/// a * b
#[inline]
pub fn algebraic_product(a: f64, b: f64) -> f64 {
    a * b
}

/// Algebraic sum (S-norm alternative to maximum)
///
/// # Arguments
///
/// * `a` - Membership degree in set A
/// * `b` - Membership degree in set B
///
/// # Returns
///
/// a + b - a * b
#[inline]
pub fn algebraic_sum(a: f64, b: f64) -> f64 {
    a + b - a * b
}

/// Bounded sum (S-norm)
///
/// # Arguments
///
/// * `a` - Membership degree in set A
/// * `b` - Membership degree in set B
///
/// # Returns
///
/// min(1, a + b)
#[inline]
pub fn bounded_sum(a: f64, b: f64) -> f64 {
    (a + b).min(1.0)
}

/// Bounded difference (T-norm)
///
/// # Arguments
///
/// * `a` - Membership degree in set A
/// * `b` - Membership degree in set B
///
/// # Returns
///
/// max(0, a + b - 1)
#[inline]
pub fn bounded_difference(a: f64, b: f64) -> f64 {
    (a + b - 1.0).max(0.0)
}

/// Drastic sum (S-norm)
#[inline]
pub fn drastic_sum(a: f64, b: f64) -> f64 {
    if a == 0.0 {
        b
    } else if b == 0.0 {
        a
    } else {
        1.0
    }
}

/// Drastic product (T-norm)
#[inline]
pub fn drastic_product(a: f64, b: f64) -> f64 {
    if a == 1.0 {
        b
    } else if b == 1.0 {
        a
    } else {
        0.0
    }
}

/// Apply fuzzy operation element-wise to vectors
///
/// # Arguments
///
/// * `a` - First vector of membership degrees
/// * `b` - Second vector of membership degrees
/// * `op` - Binary operation function
///
/// # Returns
///
/// Vector of results
pub fn apply_operation<F>(a: &[f64], b: &[f64], op: F) -> Vec<f64>
where
    F: Fn(f64, f64) -> f64,
{
    assert_eq!(a.len(), b.len(), "Vectors must have the same length");
    a.iter()
        .zip(b.iter())
        .map(|(&ai, &bi)| op(ai, bi))
        .collect()
}

/// Alpha cut of a fuzzy set
///
/// Returns indices where membership >= alpha
///
/// # Arguments
///
/// * `membership` - Vector of membership degrees
/// * `alpha` - Threshold value
///
/// # Returns
///
/// Indices where membership >= alpha
pub fn alpha_cut(membership: &[f64], alpha: f64) -> Vec<usize> {
    membership
        .iter()
        .enumerate()
        .filter(|(_, &m)| m >= alpha)
        .map(|(i, _)| i)
        .collect()
}

/// Strong alpha cut of a fuzzy set
///
/// Returns indices where membership > alpha
pub fn strong_alpha_cut(membership: &[f64], alpha: f64) -> Vec<usize> {
    membership
        .iter()
        .enumerate()
        .filter(|(_, &m)| m > alpha)
        .map(|(i, _)| i)
        .collect()
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_fuzzy_union() {
        assert_eq!(fuzzy_union(0.7, 0.5), 0.7);
        assert_eq!(fuzzy_union(0.3, 0.8), 0.8);
        assert_eq!(fuzzy_union(0.5, 0.5), 0.5);
    }

    #[test]
    fn test_fuzzy_intersection() {
        assert_eq!(fuzzy_intersection(0.7, 0.5), 0.5);
        assert_eq!(fuzzy_intersection(0.3, 0.8), 0.3);
        assert_eq!(fuzzy_intersection(0.5, 0.5), 0.5);
    }

    #[test]
    fn test_fuzzy_complement() {
        assert_eq!(fuzzy_complement(0.7), 0.3);
        assert_eq!(fuzzy_complement(0.0), 1.0);
        assert_eq!(fuzzy_complement(1.0), 0.0);
    }

    #[test]
    fn test_algebraic_product() {
        assert_eq!(algebraic_product(0.5, 0.5), 0.25);
        assert_eq!(algebraic_product(0.8, 0.5), 0.4);
    }

    #[test]
    fn test_algebraic_sum() {
        assert!((algebraic_sum(0.5, 0.5) - 0.75).abs() < 1e-10);
        assert!((algebraic_sum(0.3, 0.4) - 0.58).abs() < 1e-10);
    }

    #[test]
    fn test_bounded_operations() {
        assert_eq!(bounded_sum(0.7, 0.5), 1.0);
        assert_eq!(bounded_difference(0.7, 0.5), 0.2);
    }

    #[test]
    fn test_apply_operation() {
        let a = vec![0.3, 0.5, 0.7];
        let b = vec![0.4, 0.6, 0.2];

        let union = apply_operation(&a, &b, fuzzy_union);
        assert_eq!(union, vec![0.4, 0.6, 0.7]);

        let intersection = apply_operation(&a, &b, fuzzy_intersection);
        assert_eq!(intersection, vec![0.3, 0.5, 0.2]);
    }

    #[test]
    fn test_alpha_cut() {
        let membership = vec![0.2, 0.5, 0.8, 0.3, 0.9];
        let cut = alpha_cut(&membership, 0.5);
        assert_eq!(cut, vec![1, 2, 4]);

        let strong_cut = strong_alpha_cut(&membership, 0.5);
        assert_eq!(strong_cut, vec![2, 4]);
    }
}
