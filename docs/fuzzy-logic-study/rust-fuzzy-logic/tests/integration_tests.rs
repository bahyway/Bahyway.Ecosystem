//! Integration Tests for Fuzzy Logic Library
//!
//! This file contains comprehensive tests that verify the library works
//! correctly end-to-end.

use fuzzy_logic::membership::{FuzzySet, MembershipFunction};
use fuzzy_logic::operations::*;
use fuzzy_logic::defuzzification::*;
use fuzzy_logic::inference::{FuzzyController, FuzzyRule};

#[test]
fn test_triangular_membership() {
    let mf = MembershipFunction::Triangular {
        a: 0.0,
        b: 5.0,
        c: 10.0,
    };

    // Test key points
    assert_eq!(mf.evaluate(0.0), 0.0, "Left foot should be 0");
    assert_eq!(mf.evaluate(5.0), 1.0, "Peak should be 1");
    assert_eq!(mf.evaluate(10.0), 0.0, "Right foot should be 0");

    // Test slopes
    assert!((mf.evaluate(2.5) - 0.5).abs() < 1e-10, "Midpoint of left slope");
    assert!((mf.evaluate(7.5) - 0.5).abs() < 1e-10, "Midpoint of right slope");
}

#[test]
fn test_trapezoidal_membership() {
    let mf = MembershipFunction::Trapezoidal {
        a: 0.0,
        b: 2.0,
        c: 8.0,
        d: 10.0,
    };

    assert_eq!(mf.evaluate(0.0), 0.0);
    assert_eq!(mf.evaluate(2.0), 1.0);
    assert_eq!(mf.evaluate(5.0), 1.0, "Should be 1 on plateau");
    assert_eq!(mf.evaluate(8.0), 1.0);
    assert_eq!(mf.evaluate(10.0), 0.0);
}

#[test]
fn test_gaussian_membership() {
    let mf = MembershipFunction::Gaussian {
        mean: 5.0,
        sigma: 2.0,
    };

    assert_eq!(mf.evaluate(5.0), 1.0, "Should be 1 at mean");
    assert!(mf.evaluate(3.0) > 0.5, "Should be > 0.5 at mean-sigma");
    assert!(mf.evaluate(7.0) > 0.5, "Should be > 0.5 at mean+sigma");
    assert!(mf.evaluate(1.0) < 0.2, "Should be low far from mean");
}

#[test]
fn test_sigmoid_membership() {
    let mf = MembershipFunction::Sigmoid { a: 1.0, c: 5.0 };

    assert!((mf.evaluate(5.0) - 0.5).abs() < 1e-10, "Should be 0.5 at center");
    assert!(mf.evaluate(10.0) > 0.9, "Should approach 1");
    assert!(mf.evaluate(0.0) < 0.1, "Should approach 0");
}

#[test]
fn test_fuzzy_operations_basic() {
    let a = 0.7;
    let b = 0.5;

    assert_eq!(fuzzy_union(a, b), 0.7, "Union should be max");
    assert_eq!(fuzzy_intersection(a, b), 0.5, "Intersection should be min");
    assert!((fuzzy_complement(a) - 0.3).abs() < 1e-10, "Complement should be 1-a");
}

#[test]
fn test_algebraic_operations() {
    let a = 0.6;
    let b = 0.5;

    let product = algebraic_product(a, b);
    assert_eq!(product, 0.3, "Algebraic product");

    let sum = algebraic_sum(a, b);
    assert!((sum - 0.8).abs() < 1e-10, "Algebraic sum");
}

#[test]
fn test_bounded_operations() {
    assert_eq!(bounded_sum(0.7, 0.5), 1.0, "Bounded sum should cap at 1");
    assert_eq!(bounded_sum(0.3, 0.4), 0.7, "Bounded sum < 1");

    assert_eq!(bounded_difference(0.7, 0.5), 0.2, "Bounded difference");
    assert_eq!(bounded_difference(0.3, 0.4), 0.0, "Bounded difference floors at 0");
}

#[test]
fn test_alpha_cuts() {
    let membership = vec![0.2, 0.5, 0.8, 0.3, 0.9];

    let cut = alpha_cut(&membership, 0.5);
    assert_eq!(cut, vec![1, 2, 4], "Alpha cut at 0.5");

    let strong_cut = strong_alpha_cut(&membership, 0.5);
    assert_eq!(strong_cut, vec![2, 4], "Strong alpha cut at 0.5");
}

#[test]
fn test_centroid_defuzzification() {
    let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
    let m = vec![0.0, 0.5, 1.0, 0.5, 0.0];

    let result = centroid(&x, &m);
    assert!((result - 2.0).abs() < 1e-10, "Centroid should be at center");
}

#[test]
fn test_mean_of_maximum_defuzzification() {
    let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
    let m = vec![0.0, 0.5, 1.0, 1.0, 0.5];

    let result = mean_of_maximum(&x, &m);
    assert!((result - 2.5).abs() < 1e-10, "MOM should be average of maxima");
}

#[test]
fn test_smallest_and_largest_of_maximum() {
    let x = vec![0.0, 1.0, 2.0, 3.0, 4.0];
    let m = vec![0.0, 0.5, 1.0, 1.0, 0.5];

    let som = smallest_of_maximum(&x, &m);
    assert_eq!(som, 2.0, "SOM should be first maximum");

    let lom = largest_of_maximum(&x, &m);
    assert_eq!(lom, 3.0, "LOM should be last maximum");
}

#[test]
fn test_fuzzy_set_creation() {
    let fuzzy_set = FuzzySet::new(
        "Temperature Cold",
        MembershipFunction::Triangular {
            a: 0.0,
            b: 0.0,
            c: 20.0,
        },
    );

    assert_eq!(fuzzy_set.name, "Temperature Cold");
    assert_eq!(fuzzy_set.membership(0.0), 1.0);
    assert_eq!(fuzzy_set.membership(10.0), 0.5);
    assert_eq!(fuzzy_set.membership(20.0), 0.0);
}

#[test]
fn test_simple_fuzzy_controller() {
    let mut controller = FuzzyController::new(0.0, 100.0, 100);

    // Create simple rules
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
    controller.add_rule(FuzzyRule::new(
        Box::new(move |x| cold.evaluate(x)),
        Box::new(move |x| low.evaluate(x)),
    ));

    // Rule 2: IF hot THEN high
    controller.add_rule(FuzzyRule::new(
        Box::new(move |x| hot.evaluate(x)),
        Box::new(move |x| high.evaluate(x)),
    ));

    // Test
    let output_cold = controller.evaluate(10.0);
    let output_hot = controller.evaluate(35.0);

    assert!(output_cold < 50.0, "Cold input should produce low output");
    assert!(output_hot > 50.0, "Hot input should produce high output");
}

#[test]
fn test_multiple_membership_functions() {
    let mf_types = vec![
        MembershipFunction::Triangular {
            a: 0.0,
            b: 5.0,
            c: 10.0,
        },
        MembershipFunction::Trapezoidal {
            a: 0.0,
            b: 2.0,
            c: 8.0,
            d: 10.0,
        },
        MembershipFunction::Gaussian {
            mean: 5.0,
            sigma: 2.0,
        },
        MembershipFunction::Sigmoid { a: 1.0, c: 5.0 },
    ];

    // All should evaluate to valid membership degrees
    for mf in mf_types {
        let val = mf.evaluate(5.0);
        assert!(
            val >= 0.0 && val <= 1.0,
            "Membership should be in [0, 1]"
        );
    }
}

#[test]
fn test_defuzzification_methods_consistency() {
    let x: Vec<f64> = (0..=100).map(|i| i as f64).collect();
    let m: Vec<f64> = x
        .iter()
        .map(|&xi| {
            let mf = MembershipFunction::Triangular {
                a: 25.0,
                b: 50.0,
                c: 75.0,
            };
            mf.evaluate(xi)
        })
        .collect();

    let c = centroid(&x, &m);
    let mom = mean_of_maximum(&x, &m);

    // For symmetric triangular, centroid and MOM should be close
    assert!((c - 50.0).abs() < 5.0, "Centroid near peak");
    assert_eq!(mom, 50.0, "MOM at peak");
}

#[test]
fn test_controller_with_overlapping_rules() {
    let mut controller = FuzzyController::new(0.0, 100.0, 100);

    // Overlapping membership functions
    let low = MembershipFunction::Triangular {
        a: 0.0,
        b: 0.0,
        c: 30.0,
    };
    let medium = MembershipFunction::Triangular {
        a: 20.0,
        b: 50.0,
        c: 80.0,
    };
    let high = MembershipFunction::Triangular {
        a: 70.0,
        b: 100.0,
        c: 100.0,
    };

    let out_low = MembershipFunction::Triangular {
        a: 0.0,
        b: 0.0,
        c: 40.0,
    };
    let out_medium = MembershipFunction::Triangular {
        a: 30.0,
        b: 50.0,
        c: 70.0,
    };
    let out_high = MembershipFunction::Triangular {
        a: 60.0,
        b: 100.0,
        c: 100.0,
    };

    controller.add_rule(FuzzyRule::new(
        Box::new(move |x| low.evaluate(x)),
        Box::new(move |x| out_low.evaluate(x)),
    ));

    controller.add_rule(FuzzyRule::new(
        Box::new(move |x| medium.evaluate(x)),
        Box::new(move |x| out_medium.evaluate(x)),
    ));

    controller.add_rule(FuzzyRule::new(
        Box::new(move |x| high.evaluate(x)),
        Box::new(move |x| out_high.evaluate(x)),
    ));

    // Test at overlap points - should produce smooth transitions
    let result_25 = controller.evaluate(25.0);
    let result_50 = controller.evaluate(50.0);
    let result_75 = controller.evaluate(75.0);

    assert!(result_25 < result_50, "Output should increase");
    assert!(result_50 < result_75, "Output should increase");
}

#[test]
fn test_edge_cases() {
    // Test with zero membership everywhere
    let x = vec![0.0, 1.0, 2.0, 3.0];
    let m = vec![0.0, 0.0, 0.0, 0.0];

    let c = centroid(&x, &m);
    assert!(c.is_finite(), "Should handle zero membership");

    // Test with single point
    let x_single = vec![5.0];
    let m_single = vec![1.0];
    let c_single = centroid(&x_single, &m_single);
    assert_eq!(c_single, 5.0, "Single point centroid");
}
