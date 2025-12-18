//! Membership Functions
//!
//! This module provides various types of membership functions used in fuzzy logic.

use std::f64::consts::E;

/// Types of membership functions
#[derive(Debug, Clone)]
pub enum MembershipFunction {
    /// Triangular membership function
    /// - a: Left foot (membership = 0)
    /// - b: Peak (membership = 1)
    /// - c: Right foot (membership = 0)
    Triangular { a: f64, b: f64, c: f64 },

    /// Trapezoidal membership function
    /// - a: Left foot
    /// - b: Left shoulder (start of plateau)
    /// - c: Right shoulder (end of plateau)
    /// - d: Right foot
    Trapezoidal { a: f64, b: f64, c: f64, d: f64 },

    /// Gaussian membership function
    /// - mean: Center of the bell curve
    /// - sigma: Standard deviation (controls width)
    Gaussian { mean: f64, sigma: f64 },

    /// Sigmoid membership function
    /// - a: Controls steepness (larger = steeper)
    /// - c: Center point (inflection point)
    Sigmoid { a: f64, c: f64 },
}

impl MembershipFunction {
    /// Evaluate the membership function at a given point
    ///
    /// # Arguments
    ///
    /// * `x` - The point at which to evaluate the function
    ///
    /// # Returns
    ///
    /// The membership degree in the range [0.0, 1.0]
    pub fn evaluate(&self, x: f64) -> f64 {
        match self {
            MembershipFunction::Triangular { a, b, c } => {
                Self::triangular(x, *a, *b, *c)
            }
            MembershipFunction::Trapezoidal { a, b, c, d } => {
                Self::trapezoidal(x, *a, *b, *c, *d)
            }
            MembershipFunction::Gaussian { mean, sigma } => {
                Self::gaussian(x, *mean, *sigma)
            }
            MembershipFunction::Sigmoid { a, c } => {
                Self::sigmoid(x, *a, *c)
            }
        }
    }

    /// Triangular membership function
    #[inline]
    fn triangular(x: f64, a: f64, b: f64, c: f64) -> f64 {
        if x <= a || x >= c {
            0.0
        } else if x == b {
            1.0
        } else if x < b {
            (x - a) / (b - a)
        } else {
            (c - x) / (c - b)
        }
    }

    /// Trapezoidal membership function
    #[inline]
    fn trapezoidal(x: f64, a: f64, b: f64, c: f64, d: f64) -> f64 {
        if x <= a || x >= d {
            0.0
        } else if x >= b && x <= c {
            1.0
        } else if x < b {
            (x - a) / (b - a)
        } else {
            (d - x) / (d - c)
        }
    }

    /// Gaussian membership function
    #[inline]
    fn gaussian(x: f64, mean: f64, sigma: f64) -> f64 {
        E.powf(-0.5 * ((x - mean) / sigma).powi(2))
    }

    /// Sigmoid membership function
    #[inline]
    fn sigmoid(x: f64, a: f64, c: f64) -> f64 {
        1.0 / (1.0 + E.powf(-a * (x - c)))
    }

    /// Get a textual description of the membership function
    pub fn description(&self) -> String {
        match self {
            MembershipFunction::Triangular { a, b, c } => {
                format!("Triangular(a={}, b={}, c={})", a, b, c)
            }
            MembershipFunction::Trapezoidal { a, b, c, d } => {
                format!("Trapezoidal(a={}, b={}, c={}, d={})", a, b, c, d)
            }
            MembershipFunction::Gaussian { mean, sigma } => {
                format!("Gaussian(mean={}, sigma={})", mean, sigma)
            }
            MembershipFunction::Sigmoid { a, c } => {
                format!("Sigmoid(a={}, c={})", a, c)
            }
        }
    }
}

/// A fuzzy set with a name and membership function
#[derive(Debug, Clone)]
pub struct FuzzySet {
    pub name: String,
    pub membership_function: MembershipFunction,
}

impl FuzzySet {
    /// Create a new fuzzy set
    pub fn new(name: impl Into<String>, membership_function: MembershipFunction) -> Self {
        FuzzySet {
            name: name.into(),
            membership_function,
        }
    }

    /// Evaluate membership degree for a value
    pub fn membership(&self, x: f64) -> f64 {
        self.membership_function.evaluate(x)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_triangular() {
        let mf = MembershipFunction::Triangular {
            a: 0.0,
            b: 5.0,
            c: 10.0,
        };

        assert_eq!(mf.evaluate(0.0), 0.0);
        assert_eq!(mf.evaluate(5.0), 1.0);
        assert_eq!(mf.evaluate(10.0), 0.0);
        assert!((mf.evaluate(2.5) - 0.5).abs() < 1e-10);
        assert!((mf.evaluate(7.5) - 0.5).abs() < 1e-10);
    }

    #[test]
    fn test_trapezoidal() {
        let mf = MembershipFunction::Trapezoidal {
            a: 0.0,
            b: 2.0,
            c: 8.0,
            d: 10.0,
        };

        assert_eq!(mf.evaluate(0.0), 0.0);
        assert_eq!(mf.evaluate(2.0), 1.0);
        assert_eq!(mf.evaluate(5.0), 1.0);
        assert_eq!(mf.evaluate(8.0), 1.0);
        assert_eq!(mf.evaluate(10.0), 0.0);
        assert!((mf.evaluate(1.0) - 0.5).abs() < 1e-10);
    }

    #[test]
    fn test_gaussian() {
        let mf = MembershipFunction::Gaussian {
            mean: 5.0,
            sigma: 2.0,
        };

        assert_eq!(mf.evaluate(5.0), 1.0);
        assert!(mf.evaluate(3.0) > 0.5);
        assert!(mf.evaluate(7.0) > 0.5);
        assert!(mf.evaluate(1.0) < 0.2);
    }

    #[test]
    fn test_sigmoid() {
        let mf = MembershipFunction::Sigmoid { a: 1.0, c: 5.0 };

        assert!((mf.evaluate(5.0) - 0.5).abs() < 1e-10);
        assert!(mf.evaluate(10.0) > 0.9);
        assert!(mf.evaluate(0.0) < 0.1);
    }

    #[test]
    fn test_fuzzy_set() {
        let fuzzy_set = FuzzySet::new(
            "Cold",
            MembershipFunction::Triangular {
                a: 0.0,
                b: 0.0,
                c: 20.0,
            },
        );

        assert_eq!(fuzzy_set.name, "Cold");
        assert_eq!(fuzzy_set.membership(0.0), 1.0);
        assert_eq!(fuzzy_set.membership(10.0), 0.5);
        assert_eq!(fuzzy_set.membership(20.0), 0.0);
    }
}
