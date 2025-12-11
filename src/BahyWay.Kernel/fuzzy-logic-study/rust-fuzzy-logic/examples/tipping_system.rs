//! Tipping System Example
//!
//! This example demonstrates a fuzzy logic system for determining
//! appropriate tip percentage based on service quality and food quality.
//!
//! Run with: cargo run --example tipping_system

use fuzzy_logic::membership::MembershipFunction;
use fuzzy_logic::inference::{TwoInputFuzzyController, TwoInputFuzzyRule};

fn main() {
    println!("\n{}", "=".repeat(70));
    println!("FUZZY LOGIC TIPPING SYSTEM");
    println!("{}", "=".repeat(70));
    println!();

    println!("System Overview:");
    println!("  Inputs:  Service Quality (0-10), Food Quality (0-10)");
    println!("  Output:  Tip Percentage (0-30%)");
    println!();

    // Create controller (0-30% tip range)
    let mut controller = TwoInputFuzzyController::new(0.0, 30.0, 100);

    // Define service quality membership functions
    let service_poor = MembershipFunction::Triangular {
        a: 0.0,
        b: 0.0,
        c: 5.0,
    };
    let service_good = MembershipFunction::Triangular {
        a: 0.0,
        b: 5.0,
        c: 10.0,
    };
    let service_excellent = MembershipFunction::Triangular {
        a: 5.0,
        b: 10.0,
        c: 10.0,
    };

    // Define food quality membership functions
    let food_poor = MembershipFunction::Triangular {
        a: 0.0,
        b: 0.0,
        c: 5.0,
    };
    let food_good = MembershipFunction::Triangular {
        a: 0.0,
        b: 5.0,
        c: 10.0,
    };
    let food_excellent = MembershipFunction::Triangular {
        a: 5.0,
        b: 10.0,
        c: 10.0,
    };

    // Define tip percentage membership functions
    let tip_low = MembershipFunction::Triangular {
        a: 0.0,
        b: 0.0,
        c: 13.0,
    };
    let tip_medium = MembershipFunction::Triangular {
        a: 0.0,
        b: 15.0,
        c: 25.0,
    };
    let tip_high = MembershipFunction::Triangular {
        a: 15.0,
        b: 30.0,
        c: 30.0,
    };

    println!("Fuzzy Rules (9 total):");
    println!();

    // Rule Matrix:
    //                 Food Poor   Food Good   Food Excellent
    // Service Poor      Low         Low          Medium
    // Service Good      Low        Medium         High
    // Service Excellent Medium      High          High

    // Rule 1: Poor Service + Poor Food = Low Tip
    println!("  1. IF service is poor AND food is poor THEN tip is low");
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| service_poor.evaluate(x)),
        Box::new(move |x| food_poor.evaluate(x)),
        Box::new(move |x| tip_low.evaluate(x)),
    ));

    // Rule 2: Poor Service + Good Food = Low Tip
    println!("  2. IF service is poor AND food is good THEN tip is low");
    let service_poor2 = service_poor.clone();
    let food_good2 = food_good.clone();
    let tip_low2 = tip_low.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| service_poor2.evaluate(x)),
        Box::new(move |x| food_good2.evaluate(x)),
        Box::new(move |x| tip_low2.evaluate(x)),
    ));

    // Rule 3: Poor Service + Excellent Food = Medium Tip
    println!("  3. IF service is poor AND food is excellent THEN tip is medium");
    let service_poor3 = service_poor.clone();
    let food_excellent3 = food_excellent.clone();
    let tip_medium3 = tip_medium.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| service_poor3.evaluate(x)),
        Box::new(move |x| food_excellent3.evaluate(x)),
        Box::new(move |x| tip_medium3.evaluate(x)),
    ));

    // Rule 4: Good Service + Poor Food = Low Tip
    println!("  4. IF service is good AND food is poor THEN tip is low");
    let service_good4 = service_good.clone();
    let food_poor4 = food_poor.clone();
    let tip_low4 = tip_low.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| service_good4.evaluate(x)),
        Box::new(move |x| food_poor4.evaluate(x)),
        Box::new(move |x| tip_low4.evaluate(x)),
    ));

    // Rule 5: Good Service + Good Food = Medium Tip
    println!("  5. IF service is good AND food is good THEN tip is medium");
    let service_good5 = service_good.clone();
    let food_good5 = food_good.clone();
    let tip_medium5 = tip_medium.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| service_good5.evaluate(x)),
        Box::new(move |x| food_good5.evaluate(x)),
        Box::new(move |x| tip_medium5.evaluate(x)),
    ));

    // Rule 6: Good Service + Excellent Food = High Tip
    println!("  6. IF service is good AND food is excellent THEN tip is high");
    let service_good6 = service_good.clone();
    let food_excellent6 = food_excellent.clone();
    let tip_high6 = tip_high.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| service_good6.evaluate(x)),
        Box::new(move |x| food_excellent6.evaluate(x)),
        Box::new(move |x| tip_high6.evaluate(x)),
    ));

    // Rule 7: Excellent Service + Poor Food = Medium Tip
    println!("  7. IF service is excellent AND food is poor THEN tip is medium");
    let service_excellent7 = service_excellent.clone();
    let food_poor7 = food_poor.clone();
    let tip_medium7 = tip_medium.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| service_excellent7.evaluate(x)),
        Box::new(move |x| food_poor7.evaluate(x)),
        Box::new(move |x| tip_medium7.evaluate(x)),
    ));

    // Rule 8: Excellent Service + Good Food = High Tip
    println!("  8. IF service is excellent AND food is good THEN tip is high");
    let service_excellent8 = service_excellent.clone();
    let food_good8 = food_good.clone();
    let tip_high8 = tip_high.clone();
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| service_excellent8.evaluate(x)),
        Box::new(move |x| food_good8.evaluate(x)),
        Box::new(move |x| tip_high8.evaluate(x)),
    ));

    // Rule 9: Excellent Service + Excellent Food = High Tip
    println!("  9. IF service is excellent AND food is excellent THEN tip is high");
    controller.add_rule(TwoInputFuzzyRule::new(
        Box::new(move |x| service_excellent.evaluate(x)),
        Box::new(move |x| food_excellent.evaluate(x)),
        Box::new(move |x| tip_high.evaluate(x)),
    ));

    println!();
    println!("{}", "=".repeat(70));
    println!("TESTING TIPPING SYSTEM");
    println!("{}", "=".repeat(70));
    println!();

    // Test cases
    let test_cases = vec![
        (3.0, 2.0, "Poor service, poor food"),
        (5.0, 5.0, "Average service and food"),
        (8.0, 9.0, "Great service, excellent food"),
        (2.0, 9.0, "Poor service, but great food"),
        (9.0, 3.0, "Excellent service, mediocre food"),
        (7.0, 7.0, "Good overall experience"),
    ];

    println!("Restaurant Visit Scenarios:");
    println!();

    for (service, food, description) in test_cases {
        let tip = controller.evaluate(service, food);
        println!("{}", description);
        println!("  Service Quality: {}/10", service);
        println!("  Food Quality:    {}/10", food);
        println!("  → Suggested Tip: {:.1}%", tip);

        // Calculate tip amount for a $50 bill
        let bill = 50.0;
        let tip_amount = bill * tip / 100.0;
        println!("  → Tip Amount (on $50 bill): ${:.2}", tip_amount);
        println!();
    }

    println!("{}", "=".repeat(70));
    println!("TIPPING SYSTEM DEMO COMPLETE");
    println!("{}", "=".repeat(70));
    println!();
    println!("Key Observations:");
    println!("  - Service quality has stronger influence than food");
    println!("  - Both factors contribute to final tip percentage");
    println!("  - Smooth gradations - not just 15%, 18%, or 20%");
    println!("  - Captures human decision-making process");
    println!();
    println!("Try modifying the rules or membership functions to");
    println!("reflect your own tipping philosophy!");
    println!();
}
