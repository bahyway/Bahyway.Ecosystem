//! Fuzzy Logic Library
//!
//! This library provides implementations of fuzzy logic concepts including:
//! - Membership functions (triangular, trapezoidal, Gaussian, sigmoid)
//! - Fuzzy operations (union, intersection, complement)
//! - Fuzzy inference systems
//! - Defuzzification methods
//!
//! # Example
//!
//! ```
//! use fuzzy_logic::membership::MembershipFunction;
//! use fuzzy_logic::operations::{fuzzy_union, fuzzy_intersection};
//!
//! let temp_cold = MembershipFunction::Triangular { a: 0.0, b: 0.0, c: 20.0 };
//! let temp_warm = MembershipFunction::Triangular { a: 10.0, b: 25.0, c: 35.0 };
//!
//! let cold_degree = temp_cold.evaluate(15.0);
//! let warm_degree = temp_warm.evaluate(15.0);
//!
//! let union = fuzzy_union(cold_degree, warm_degree);
//! ```

pub mod membership;
pub mod operations;
pub mod inference;
pub mod defuzzification;

// Re-export commonly used types and functions
pub use membership::MembershipFunction;
pub use operations::{fuzzy_union, fuzzy_intersection, fuzzy_complement};
pub use defuzzification::{centroid, mean_of_maximum, bisector};

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_basic_membership() {
        let mf = MembershipFunction::Triangular {
            a: 0.0,
            b: 5.0,
            c: 10.0,
        };

        assert_eq!(mf.evaluate(5.0), 1.0);
        assert_eq!(mf.evaluate(0.0), 0.0);
        assert_eq!(mf.evaluate(10.0), 0.0);
        assert!((mf.evaluate(2.5) - 0.5).abs() < 1e-10);
    }

    #[test]
    fn test_fuzzy_operations() {
        let a = 0.7;
        let b = 0.5;

        assert_eq!(fuzzy_union(a, b), 0.7);
        assert_eq!(fuzzy_intersection(a, b), 0.5);
        assert_eq!(fuzzy_complement(a), 0.3);
    }
}
