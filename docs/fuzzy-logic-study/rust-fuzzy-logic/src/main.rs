//! Interactive Fuzzy Logic CLI Study Program
//!
//! This program provides step-by-step lessons on fuzzy logic concepts
//! with interactive examples in Rust.

use fuzzy_logic::membership::{FuzzySet, MembershipFunction};
use fuzzy_logic::operations::*;
use fuzzy_logic::defuzzification::*;
use fuzzy_logic::inference::{FuzzyController, FuzzyRule};
use std::io::{self, Write};

fn main() {
    println!("\n{}", "=".repeat(60));
    println!("FUZZY LOGIC INTERACTIVE STUDY PROGRAM (Rust)");
    println!("{}", "=".repeat(60));

    loop {
        println!("\nChoose a lesson:");
        println!();
        println!("  1. Membership Functions");
        println!("  2. Fuzzy Operations");
        println!("  3. Fuzzy Sets");
        println!("  4. Defuzzification Methods");
        println!("  5. Simple Fuzzy Controller");
        println!();
        println!("  0. Exit");
        println!();

        print!("Enter your choice (0-5): ");
        io::stdout().flush().unwrap();

        let mut input = String::new();
        io::stdin().read_line(&mut input).unwrap();

        match input.trim() {
            "1" => lesson_1_membership_functions(),
            "2" => lesson_2_fuzzy_operations(),
            "3" => lesson_3_fuzzy_sets(),
            "4" => lesson_4_defuzzification(),
            "5" => lesson_5_fuzzy_controller(),
            "0" => {
                println!("\nThank you for learning fuzzy logic! 🧠✨");
                break;
            }
            _ => println!("\nInvalid choice. Please try again."),
        }

        wait_for_user();
    }
}

fn wait_for_user() {
    print!("\nPress Enter to continue...");
    io::stdout().flush().unwrap();
    let mut input = String::new();
    io::stdin().read_line(&mut input).unwrap();
}

fn lesson_1_membership_functions() {
    println!("\n{}", "=".repeat(60));
    println!("LESSON 1: Membership Functions");
    println!("{}", "=".repeat(60));
    println!();
    println!("Types of membership functions:");
    println!("  1. Triangular   - Simple, sharp peak");
    println!("  2. Trapezoidal  - Flat top, clear boundaries");
    println!("  3. Gaussian     - Smooth, bell-shaped");
    println!("  4. Sigmoid      - S-shaped curve");
    println!();

    // Triangular
    println!("Creating Triangular membership function:");
    let triangular = MembershipFunction::Triangular {
        a: 0.0,
        b: 5.0,
        c: 10.0,
    };
    println!("  Parameters: a=0, b=5, c=10");
    println!("  Evaluation at x=2.5: {:.3}", triangular.evaluate(2.5));
    println!("  Evaluation at x=5.0: {:.3}", triangular.evaluate(5.0));
    println!("  Evaluation at x=7.5: {:.3}", triangular.evaluate(7.5));

    // Trapezoidal
    println!("\nCreating Trapezoidal membership function:");
    let trapezoidal = MembershipFunction::Trapezoidal {
        a: 0.0,
        b: 2.0,
        c: 8.0,
        d: 10.0,
    };
    println!("  Parameters: a=0, b=2, c=8, d=10");
    println!("  Evaluation at x=1.0: {:.3}", trapezoidal.evaluate(1.0));
    println!("  Evaluation at x=5.0: {:.3}", trapezoidal.evaluate(5.0));
    println!("  Evaluation at x=9.0: {:.3}", trapezoidal.evaluate(9.0));

    // Gaussian
    println!("\nCreating Gaussian membership function:");
    let gaussian = MembershipFunction::Gaussian {
        mean: 5.0,
        sigma: 2.0,
    };
    println!("  Parameters: mean=5, sigma=2");
    println!("  Evaluation at x=3.0: {:.3}", gaussian.evaluate(3.0));
    println!("  Evaluation at x=5.0: {:.3}", gaussian.evaluate(5.0));
    println!("  Evaluation at x=7.0: {:.3}", gaussian.evaluate(7.0));

    // Sigmoid
    println!("\nCreating Sigmoid membership function:");
    let sigmoid = MembershipFunction::Sigmoid { a: 1.0, c: 5.0 };
    println!("  Parameters: a=1, c=5");
    println!("  Evaluation at x=0.0: {:.3}", sigmoid.evaluate(0.0));
    println!("  Evaluation at x=5.0: {:.3}", sigmoid.evaluate(5.0));
    println!("  Evaluation at x=10.0: {:.3}", sigmoid.evaluate(10.0));

    println!("\nKey Points:");
    println!("  - Membership values are always between 0.0 and 1.0");
    println!("  - Each shape serves different purposes");
    println!("  - Triangular is most common for simplicity");
}

fn lesson_2_fuzzy_operations() {
    println!("\n{}", "=".repeat(60));
    println!("LESSON 2: Fuzzy Operations");
    println!("{}", "=".repeat(60));
    println!();
    println!("Three basic operations:");
    println!("  1. UNION (OR)         - Maximum");
    println!("  2. INTERSECTION (AND) - Minimum");
    println!("  3. COMPLEMENT (NOT)   - 1 minus membership");
    println!();

    let a = 0.7;
    let b = 0.5;

    println!("Given two membership degrees:");
    println!("  a = {}", a);
    println!("  b = {}", b);
    println!();

    println!("Union (a OR b):");
    println!("  Result: {:.3}", fuzzy_union(a, b));
    println!("  Formula: max(a, b)");
    println!();

    println!("Intersection (a AND b):");
    println!("  Result: {:.3}", fuzzy_intersection(a, b));
    println!("  Formula: min(a, b)");
    println!();

    println!("Complement (NOT a):");
    println!("  Result: {:.3}", fuzzy_complement(a));
    println!("  Formula: 1 - a");
    println!();

    println!("Alternative T-norms and S-norms:");
    println!();
    println!("Algebraic Product (T-norm):");
    println!("  Result: {:.3}", algebraic_product(a, b));
    println!("  Formula: a * b");
    println!();

    println!("Algebraic Sum (S-norm):");
    println!("  Result: {:.3}", algebraic_sum(a, b));
    println!("  Formula: a + b - a*b");
    println!();

    println!("Bounded Sum:");
    println!("  Result: {:.3}", bounded_sum(a, b));
    println!("  Formula: min(1, a + b)");
}

fn lesson_3_fuzzy_sets() {
    println!("\n{}", "=".repeat(60));
    println!("LESSON 3: Fuzzy Sets");
    println!("{}", "=".repeat(60));
    println!();
    println!("Creating fuzzy sets for temperature control:");
    println!();

    // Create fuzzy sets
    let cold = FuzzySet::new(
        "Cold",
        MembershipFunction::Triangular {
            a: 0.0,
            b: 0.0,
            c: 20.0,
        },
    );

    let warm = FuzzySet::new(
        "Warm",
        MembershipFunction::Triangular {
            a: 10.0,
            b: 25.0,
            c: 35.0,
        },
    );

    let hot = FuzzySet::new(
        "Hot",
        MembershipFunction::Triangular {
            a: 30.0,
            b: 40.0,
            c: 40.0,
        },
    );

    println!("Fuzzy Sets Created:");
    println!("  1. Cold:  {}", cold.membership_function.description());
    println!("  2. Warm:  {}", warm.membership_function.description());
    println!("  3. Hot:   {}", hot.membership_function.description());
    println!();

    // Test with different temperatures
    let test_temps = vec![15.0, 25.0, 35.0];
    println!("Testing membership degrees:");
    println!();

    for temp in test_temps {
        println!("At temperature {}°C:", temp);
        println!("  Cold: {:.3}", cold.membership(temp));
        println!("  Warm: {:.3}", warm.membership(temp));
        println!("  Hot:  {:.3}", hot.membership(temp));
        println!();
    }

    println!("Notice how:");
    println!("  - At 15°C: Mostly cold");
    println!("  - At 25°C: Mix of warm (and a bit cold/hot)");
    println!("  - At 35°C: Mix of warm and hot");
}

fn lesson_4_defuzzification() {
    println!("\n{}", "=".repeat(60));
    println!("LESSON 4: Defuzzification Methods");
    println!("{}", "=".repeat(60));
    println!();
    println!("Converting fuzzy sets back to crisp values:");
    println!();

    // Create sample data
    let x_values: Vec<f64> = (0..=100).map(|i| i as f64).collect();
    let membership: Vec<f64> = x_values
        .iter()
        .map(|&x| {
            // Triangular membership centered at 50
            if x <= 25.0 {
                0.0
            } else if x <= 50.0 {
                (x - 25.0) / 25.0
            } else if x <= 75.0 {
                (75.0 - x) / 25.0
            } else {
                0.0
            }
        })
        .collect();

    println!("Using a triangular fuzzy set (peak at 50):");
    println!();

    let c = centroid(&x_values, &membership);
    println!("1. Centroid (COG):");
    println!("   Result: {:.2}", c);
    println!("   Method: Center of area under curve");
    println!();

    let mom = mean_of_maximum(&x_values, &membership);
    println!("2. Mean of Maximum (MOM):");
    println!("   Result: {:.2}", mom);
    println!("   Method: Average of all maximum points");
    println!();

    let som = smallest_of_maximum(&x_values, &membership);
    println!("3. Smallest of Maximum:");
    println!("   Result: {:.2}", som);
    println!();

    let lom = largest_of_maximum(&x_values, &membership);
    println!("4. Largest of Maximum:");
    println!("   Result: {:.2}", lom);
    println!();

    let bis = bisector(&x_values, &membership);
    println!("5. Bisector:");
    println!("   Result: {:.2}", bis);
    println!("   Method: Divides area in half");
    println!();

    println!("When to use:");
    println!("  - Centroid: Most common, balanced");
    println!("  - MOM: When you want the peak response");
    println!("  - Bisector: For equal area division");
}

fn lesson_5_fuzzy_controller() {
    println!("\n{}", "=".repeat(60));
    println!("LESSON 5: Simple Fuzzy Controller");
    println!("{}", "=".repeat(60));
    println!();
    println!("Creating a temperature-to-fan-speed controller:");
    println!();

    // Create controller
    let mut controller = FuzzyController::new(0.0, 100.0, 100);

    // Define membership functions
    let cold = MembershipFunction::Triangular {
        a: 0.0,
        b: 0.0,
        c: 20.0,
    };
    let warm = MembershipFunction::Triangular {
        a: 10.0,
        b: 25.0,
        c: 35.0,
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
    let medium = MembershipFunction::Triangular {
        a: 20.0,
        b: 50.0,
        c: 80.0,
    };
    let high = MembershipFunction::Triangular {
        a: 50.0,
        b: 100.0,
        c: 100.0,
    };

    println!("Rules:");
    println!("  1. IF temperature is cold THEN fan_speed is low");
    println!("  2. IF temperature is warm THEN fan_speed is medium");
    println!("  3. IF temperature is hot  THEN fan_speed is high");
    println!();

    // Add rules
    controller.add_rule(FuzzyRule::new(
        Box::new(move |x| cold.evaluate(x)),
        Box::new(move |x| low.evaluate(x)),
    ));

    controller.add_rule(FuzzyRule::new(
        Box::new(move |x| warm.evaluate(x)),
        Box::new(move |x| medium.evaluate(x)),
    ));

    controller.add_rule(FuzzyRule::new(
        Box::new(move |x| hot.evaluate(x)),
        Box::new(move |x| high.evaluate(x)),
    ));

    // Test controller
    println!("Testing controller:");
    let test_temps = vec![15.0, 25.0, 35.0];

    for temp in test_temps {
        let fan_speed = controller.evaluate(temp);
        println!("  Temperature: {}°C → Fan Speed: {:.1}%", temp, fan_speed);
    }

    println!();
    println!("Notice the smooth transitions!");
    println!("This is the power of fuzzy logic - no abrupt jumps.");
}
