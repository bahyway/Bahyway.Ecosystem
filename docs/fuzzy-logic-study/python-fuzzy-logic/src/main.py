"""
Interactive Fuzzy Logic Study Program

This program provides step-by-step lessons on fuzzy logic concepts with
interactive visualizations and examples.
"""

import numpy as np
import matplotlib.pyplot as plt
import skfuzzy as fuzz
from skfuzzy import control as ctrl


def clear_screen():
    """Clear the console (works on most terminals)"""
    print("\n" * 2)


def wait_for_user():
    """Wait for user to press Enter"""
    input("\nPress Enter to continue...")


def lesson_1_fuzzy_sets():
    """Lesson 1: Introduction to Fuzzy Sets"""
    clear_screen()
    print("=" * 60)
    print("LESSON 1: Introduction to Fuzzy Sets")
    print("=" * 60)
    print()
    print("Classical Set vs Fuzzy Set Example:")
    print("Let's define 'tall person' for height in centimeters")
    print()

    # Classical set
    print("Classical Set (Boolean):")
    print("  IF height >= 180cm THEN tall")
    print("  Person A (179cm): NOT tall (0)")
    print("  Person B (180cm): tall (1)")
    print("  Person C (181cm): tall (1)")
    print()

    # Fuzzy set
    print("Fuzzy Set (Degree of membership):")
    print("  Person A (179cm): tall (0.95) - almost tall!")
    print("  Person B (180cm): tall (1.0)  - definitely tall")
    print("  Person C (181cm): tall (1.0)  - definitely tall")
    print()
    print("Notice: Fuzzy sets allow gradual transitions!")

    wait_for_user()

    # Visualization
    print("\nVisualizing a fuzzy set for 'tall person'...")

    height = np.arange(150, 201, 1)
    tall_membership = fuzz.trapmf(height, [170, 180, 200, 200])

    plt.figure(figsize=(10, 5))
    plt.plot(height, tall_membership, 'b', linewidth=2)
    plt.fill_between(height, tall_membership, alpha=0.3)
    plt.xlabel('Height (cm)', fontsize=12)
    plt.ylabel('Membership Degree', fontsize=12)
    plt.title('Fuzzy Set: "Tall Person"', fontsize=14)
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])

    # Annotate examples
    plt.plot(179, fuzz.interp_membership(height, tall_membership, 179),
             'ro', markersize=10, label='179cm')
    plt.plot(180, fuzz.interp_membership(height, tall_membership, 180),
             'go', markersize=10, label='180cm')
    plt.plot(181, fuzz.interp_membership(height, tall_membership, 181),
             'mo', markersize=10, label='181cm')
    plt.legend()

    plt.tight_layout()
    plt.show()

    print("\nKey Concepts:")
    print("  - Membership degree: Value between 0 and 1")
    print("  - 0 = not a member, 1 = full member")
    print("  - Values in between = partial membership")
    wait_for_user()


def lesson_2_membership_functions():
    """Lesson 2: Membership Functions"""
    clear_screen()
    print("=" * 60)
    print("LESSON 2: Membership Functions")
    print("=" * 60)
    print()
    print("Types of membership functions:")
    print("  1. Triangular   - Simple, sharp peak")
    print("  2. Trapezoidal  - Flat top, clear boundaries")
    print("  3. Gaussian     - Smooth, bell-shaped")
    print()

    # Create universe
    x = np.arange(0, 11, 0.1)

    # Create different membership functions
    triangular = fuzz.trimf(x, [2, 5, 8])
    trapezoidal = fuzz.trapmf(x, [1, 3, 7, 9])
    gaussian = fuzz.gaussmf(x, 5, 1.5)

    # Plot
    plt.figure(figsize=(12, 8))

    # Triangular
    plt.subplot(3, 1, 1)
    plt.plot(x, triangular, 'b', linewidth=2)
    plt.fill_between(x, triangular, alpha=0.3)
    plt.title('Triangular Membership Function', fontsize=12)
    plt.ylabel('Membership')
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])

    # Trapezoidal
    plt.subplot(3, 1, 2)
    plt.plot(x, trapezoidal, 'g', linewidth=2)
    plt.fill_between(x, trapezoidal, alpha=0.3)
    plt.title('Trapezoidal Membership Function', fontsize=12)
    plt.ylabel('Membership')
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])

    # Gaussian
    plt.subplot(3, 1, 3)
    plt.plot(x, gaussian, 'r', linewidth=2)
    plt.fill_between(x, gaussian, alpha=0.3)
    plt.title('Gaussian Membership Function', fontsize=12)
    plt.ylabel('Membership')
    plt.xlabel('Universe')
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])

    plt.tight_layout()
    plt.show()

    print("\nWhen to use each type:")
    print("  Triangular:   Simple cases, quick computation")
    print("  Trapezoidal:  When there's a 'definitely true' range")
    print("  Gaussian:     Natural phenomena, smooth transitions")
    wait_for_user()


def lesson_3_fuzzy_operations():
    """Lesson 3: Fuzzy Set Operations"""
    clear_screen()
    print("=" * 60)
    print("LESSON 3: Fuzzy Set Operations")
    print("=" * 60)
    print()
    print("Three basic operations:")
    print("  1. UNION (OR)         - Maximum of membership values")
    print("  2. INTERSECTION (AND) - Minimum of membership values")
    print("  3. COMPLEMENT (NOT)   - 1 minus membership value")
    print()

    # Create two fuzzy sets
    x = np.arange(0, 11, 0.1)
    set_a = fuzz.trimf(x, [0, 3, 6])  # "Low"
    set_b = fuzz.trimf(x, [4, 7, 10])  # "High"

    # Operations
    union = np.fmax(set_a, set_b)
    intersection = np.fmin(set_a, set_b)
    complement_a = 1 - set_a

    # Plot
    plt.figure(figsize=(12, 10))

    # Original sets
    plt.subplot(4, 1, 1)
    plt.plot(x, set_a, 'b', linewidth=2, label='Set A (Low)')
    plt.plot(x, set_b, 'r', linewidth=2, label='Set B (High)')
    plt.fill_between(x, set_a, alpha=0.3)
    plt.fill_between(x, set_b, alpha=0.3)
    plt.title('Original Fuzzy Sets', fontsize=12)
    plt.ylabel('Membership')
    plt.legend()
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])

    # Union
    plt.subplot(4, 1, 2)
    plt.plot(x, union, 'g', linewidth=2, label='A ∪ B (Union)')
    plt.fill_between(x, union, alpha=0.3, color='g')
    plt.title('Union (OR) - Maximum', fontsize=12)
    plt.ylabel('Membership')
    plt.legend()
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])

    # Intersection
    plt.subplot(4, 1, 3)
    plt.plot(x, intersection, 'm', linewidth=2, label='A ∩ B (Intersection)')
    plt.fill_between(x, intersection, alpha=0.3, color='m')
    plt.title('Intersection (AND) - Minimum', fontsize=12)
    plt.ylabel('Membership')
    plt.legend()
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])

    # Complement
    plt.subplot(4, 1, 4)
    plt.plot(x, set_a, 'b', linewidth=2, label='Set A')
    plt.plot(x, complement_a, 'c', linewidth=2, label='NOT A (Complement)')
    plt.fill_between(x, complement_a, alpha=0.3, color='c')
    plt.title('Complement (NOT) - 1 minus membership', fontsize=12)
    plt.ylabel('Membership')
    plt.xlabel('Universe')
    plt.legend()
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])

    plt.tight_layout()
    plt.show()

    print("\nExample calculations at x=5:")
    x_val = 5.0
    membership_a = fuzz.interp_membership(x, set_a, x_val)
    membership_b = fuzz.interp_membership(x, set_b, x_val)
    print(f"  Set A membership: {membership_a:.3f}")
    print(f"  Set B membership: {membership_b:.3f}")
    print(f"  Union:            {max(membership_a, membership_b):.3f}")
    print(f"  Intersection:     {min(membership_a, membership_b):.3f}")
    print(f"  Complement of A:  {1 - membership_a:.3f}")
    wait_for_user()


def lesson_4_inference_systems():
    """Lesson 4: Fuzzy Inference Systems"""
    clear_screen()
    print("=" * 60)
    print("LESSON 4: Fuzzy Inference Systems")
    print("=" * 60)
    print()
    print("A Fuzzy Inference System (FIS) has:")
    print("  1. Fuzzification    - Convert crisp input to fuzzy")
    print("  2. Rule Evaluation  - Apply IF-THEN rules")
    print("  3. Aggregation      - Combine rule outputs")
    print("  4. Defuzzification  - Convert fuzzy output to crisp")
    print()
    print("Example: Simple Temperature Controller")
    print("  Input:  Temperature (°C)")
    print("  Output: Fan Speed (%)")
    print()

    # Create fuzzy variables
    temperature = ctrl.Antecedent(np.arange(0, 41, 1), 'temperature')
    fan_speed = ctrl.Consequent(np.arange(0, 101, 1), 'fan_speed')

    # Create membership functions
    temperature['cold'] = fuzz.trimf(temperature.universe, [0, 0, 20])
    temperature['warm'] = fuzz.trimf(temperature.universe, [10, 25, 35])
    temperature['hot'] = fuzz.trimf(temperature.universe, [30, 40, 40])

    fan_speed['low'] = fuzz.trimf(fan_speed.universe, [0, 0, 50])
    fan_speed['medium'] = fuzz.trimf(fan_speed.universe, [20, 50, 80])
    fan_speed['high'] = fuzz.trimf(fan_speed.universe, [50, 100, 100])

    # Define rules
    print("Rules:")
    print("  IF temperature is cold THEN fan_speed is low")
    print("  IF temperature is warm THEN fan_speed is medium")
    print("  IF temperature is hot  THEN fan_speed is high")
    print()

    rule1 = ctrl.Rule(temperature['cold'], fan_speed['low'])
    rule2 = ctrl.Rule(temperature['warm'], fan_speed['medium'])
    rule3 = ctrl.Rule(temperature['hot'], fan_speed['high'])

    # Create control system
    fan_ctrl = ctrl.ControlSystem([rule1, rule2, rule3])
    fan_sim = ctrl.ControlSystemSimulation(fan_ctrl)

    # Test with different inputs
    test_temps = [15, 25, 35]
    print("Testing the system:")
    for temp in test_temps:
        fan_sim.input['temperature'] = temp
        fan_sim.compute()
        speed = fan_sim.output['fan_speed']
        print(f"  Temperature: {temp}°C → Fan Speed: {speed:.1f}%")

    wait_for_user()

    # Visualize membership functions
    plt.figure(figsize=(12, 8))

    plt.subplot(2, 1, 1)
    for label in temperature.terms:
        temperature[label].view()
    plt.title('Input: Temperature (°C)', fontsize=12)
    plt.ylabel('Membership')

    plt.subplot(2, 1, 2)
    for label in fan_speed.terms:
        fan_speed[label].view()
    plt.title('Output: Fan Speed (%)', fontsize=12)
    plt.ylabel('Membership')
    plt.xlabel('Universe')

    plt.tight_layout()
    plt.show()

    print("\nKey Points:")
    print("  - Rules are intuitive (like human reasoning)")
    print("  - System handles smooth transitions automatically")
    print("  - No need for complex mathematical models")
    wait_for_user()


def main_menu():
    """Main menu for the interactive study program"""
    while True:
        clear_screen()
        print("=" * 60)
        print("FUZZY LOGIC INTERACTIVE STUDY PROGRAM")
        print("=" * 60)
        print()
        print("Choose a lesson:")
        print()
        print("  1. Introduction to Fuzzy Sets")
        print("  2. Membership Functions")
        print("  3. Fuzzy Set Operations")
        print("  4. Fuzzy Inference Systems")
        print()
        print("  0. Exit")
        print()

        choice = input("Enter your choice (0-4): ").strip()

        if choice == '1':
            lesson_1_fuzzy_sets()
        elif choice == '2':
            lesson_2_membership_functions()
        elif choice == '3':
            lesson_3_fuzzy_operations()
        elif choice == '4':
            lesson_4_inference_systems()
        elif choice == '0':
            print("\nThank you for learning fuzzy logic! 🧠✨")
            break
        else:
            print("\nInvalid choice. Please try again.")
            wait_for_user()


if __name__ == "__main__":
    print("\nWelcome to the Fuzzy Logic Interactive Study Program!")
    print("Make sure matplotlib is configured to show plots.")
    print()
    input("Press Enter to start...")

    try:
        main_menu()
    except KeyboardInterrupt:
        print("\n\nProgram interrupted. Goodbye!")
    except Exception as e:
        print(f"\n\nAn error occurred: {e}")
        print("Please check your Python environment and dependencies.")
