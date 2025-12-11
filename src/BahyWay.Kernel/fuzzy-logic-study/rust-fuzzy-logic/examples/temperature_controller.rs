//! Temperature Controller Example
//!
//! This example demonstrates a fuzzy logic controller for adjusting
//! fan speed based on temperature and humidity.
//!
//! Run with: cargo run --example temperature_controller

use fuzzy_logic::membership::MembershipFunction;
use fuzzy_logic::inference::{TwoInputFuzzyController, TwoInputFuzzyRule};

fn main() {
    println!("\n{}", "=".repeat(70));
    println!("FUZZY LOGIC TEMPERATURE CONTROLLER");
    println!("{}", "=".repeat(70));
    println!();

    println!("System Overview:");
    println!("  Inputs:  Temperature (0-40°C), Humidity (0-100%)");
    println!("  Output:  Fan Speed (0-100%)");
    println!();

    // Create controller
    let mut controller = TwoInputFuzzyController::new(0.0, 100.0, 100);

    // Define temperature membership functions
    let temp_cold = MembershipFunction::Triangular {
        a: 0.0,
        b: 0.0,
        c: 20.0,
    };
    let temp_moderate = MembershipFunction::Triangular {
        a: 15.0,
        b: 25.0,
        c: 35.0,
    };
    let temp_hot = MembershipFunction::Triangular {
        a: 30.0,
        b: 40.0,
        c: 40.0,
    };

    // Define humidity membership functions
    let humidity_low = MembershipFunction::Triangular {
        a: 0.0,
        b: 0.0,
        c: 40.0,
    };
    let humidity_medium = MembershipFunction::Triangular {
        a: 30.0,
        b: 50.0,
        c: 70.0,
    };
    let humidity_high = MembershipFunction::Triangular {
        a: 60.0,
        b: 100.0,
        c: 100.0,
    };

    // Define fan speed membership functions
    let fan_slow = MembershipFunction::Triangular {
        a: 0.0,
        b: 0.0,
        c: 40.0,
    };
    let fan_medium = MembershipFunction::Triangular {
        a: 30.0,
        b: 50.0,
        c: 70.0,
    };
    let fan_fast = MembershipFunction::Triangular {
        a: 60.0,
        b: 100.0,
        c: 100.0,
    };

    println!("Fuzzy Rules (9 total):");
    println!();

    // Rule 1: Cold + Low Humidity = Slow
    println!("  1. IF temp is cold AND humidity is low THEN fan is slow");
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| temp_cold.evaluate(x)),
        Box::new(move |x| humidity_low.evaluate(x)),
        Box::new(move |x| fan_slow.evaluate(x)),
    ));

    // Rule 2: Cold + Medium Humidity = Slow
    println!("  2. IF temp is cold AND humidity is medium THEN fan is slow");
    let temp_cold2 = temp_cold.clone();
    let humidity_medium2 = humidity_medium.clone();
    let fan_slow2 = fan_slow.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| temp_cold2.evaluate(x)),
        Box::new(move |x| humidity_medium2.evaluate(x)),
        Box::new(move |x| fan_slow2.evaluate(x)),
    ));

    // Rule 3: Cold + High Humidity = Medium
    println!("  3. IF temp is cold AND humidity is high THEN fan is medium");
    let temp_cold3 = temp_cold.clone();
    let humidity_high3 = humidity_high.clone();
    let fan_medium3 = fan_medium.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| temp_cold3.evaluate(x)),
        Box::new(move |x| humidity_high3.evaluate(x)),
        Box::new(move |x| fan_medium3.evaluate(x)),
    ));

    // Rule 4: Moderate + Low = Slow
    println!("  4. IF temp is moderate AND humidity is low THEN fan is slow");
    let temp_moderate4 = temp_moderate.clone();
    let humidity_low4 = humidity_low.clone();
    let fan_slow4 = fan_slow.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| temp_moderate4.evaluate(x)),
        Box::new(move |x| humidity_low4.evaluate(x)),
        Box::new(move |x| fan_slow4.evaluate(x)),
    ));

    // Rule 5: Moderate + Medium = Medium
    println!("  5. IF temp is moderate AND humidity is medium THEN fan is medium");
    let temp_moderate5 = temp_moderate.clone();
    let humidity_medium5 = humidity_medium.clone();
    let fan_medium5 = fan_medium.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| temp_moderate5.evaluate(x)),
        Box::new(move |x| humidity_medium5.evaluate(x)),
        Box::new(move |x| fan_medium5.evaluate(x)),
    ));

    // Rule 6: Moderate + High = Fast
    println!("  6. IF temp is moderate AND humidity is high THEN fan is fast");
    let temp_moderate6 = temp_moderate.clone();
    let humidity_high6 = humidity_high.clone();
    let fan_fast6 = fan_fast.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| temp_moderate6.evaluate(x)),
        Box::new(move |x| humidity_high6.evaluate(x)),
        Box::new(move |x| fan_fast6.evaluate(x)),
    ));

    // Rule 7: Hot + Low = Medium
    println!("  7. IF temp is hot AND humidity is low THEN fan is medium");
    let temp_hot7 = temp_hot.clone();
    let humidity_low7 = humidity_low.clone();
    let fan_medium7 = fan_medium.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| temp_hot7.evaluate(x)),
        Box::new(move |x| humidity_low7.evaluate(x)),
        Box::new(move |x| fan_medium7.evaluate(x)),
    ));

    // Rule 8: Hot + Medium = Fast
    println!("  8. IF temp is hot AND humidity is medium THEN fan is fast");
    let temp_hot8 = temp_hot.clone();
    let humidity_medium8 = humidity_medium.clone();
    let fan_fast8 = fan_fast.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| temp_hot8.evaluate(x)),
        Box::new(move |x| humidity_medium8.evaluate(x)),
        Box::new(move |x| fan_fast8.evaluate(x)),
    ));

    // Rule 9: Hot + High = Fast
    println!("  9. IF temp is hot AND humidity is high THEN fan is fast");
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| temp_hot.evaluate(x)),
        Box::new(move |x| humidity_high.evaluate(x)),
        Box::new(move |x| fan_fast.evaluate(x)),
    ));

    println!();
    println!("{}", "=".repeat(70));
    println!("TESTING CONTROLLER");
    println!("{}", "=".repeat(70));
    println!();

    // Test cases
    let test_cases = vec![
        (15.0, 30.0, "Cold day, low humidity"),
        (25.0, 50.0, "Moderate temp, medium humidity"),
        (35.0, 80.0, "Hot day, high humidity"),
        (20.0, 70.0, "Cool but humid"),
        (38.0, 40.0, "Very hot, dry"),
    ];

    for (temp, humidity, description) in test_cases {
        let fan_speed = controller.evaluate(temp, humidity);
        println!("{}", description);
        println!("  Temperature: {}°C", temp);
        println!("  Humidity:    {}%", humidity);
        println!("  → Fan Speed: {:.1}%", fan_speed);
        println!();
    }

    println!("{}", "=".repeat(70));
    println!("CONTROLLER DEMO COMPLETE");
    println!("{}", "=".repeat(70));
    println!();
    println!("Key Observations:");
    println!("  - Fan speed increases with both temperature and humidity");
    println!("  - Smooth transitions (no sudden jumps)");
    println!("  - Human-like reasoning encoded in simple rules");
    println!();
}
