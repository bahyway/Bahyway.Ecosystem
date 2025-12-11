"""
Temperature Control System using Fuzzy Logic

This example demonstrates a complete fuzzy control system that adjusts
fan speed based on temperature and humidity inputs.
"""

import numpy as np
import matplotlib.pyplot as plt
import skfuzzy as fuzz
from skfuzzy import control as ctrl


def create_temperature_controller():
    """
    Create a fuzzy temperature control system.

    Inputs:
        - Temperature (0-40°C)
        - Humidity (0-100%)

    Outputs:
        - Fan Speed (0-100%)

    Returns:
        ctrl.ControlSystemSimulation object
    """
    print("Creating Temperature Control System...")
    print("=" * 60)

    # Define input variables
    temperature = ctrl.Antecedent(np.arange(0, 41, 1), 'temperature')
    humidity = ctrl.Antecedent(np.arange(0, 101, 1), 'humidity')

    # Define output variable
    fan_speed = ctrl.Consequent(np.arange(0, 101, 1), 'fan_speed')

    # Temperature membership functions
    temperature['cold'] = fuzz.trimf(temperature.universe, [0, 0, 20])
    temperature['moderate'] = fuzz.trimf(temperature.universe, [15, 25, 35])
    temperature['hot'] = fuzz.trimf(temperature.universe, [30, 40, 40])

    # Humidity membership functions
    humidity['low'] = fuzz.trimf(humidity.universe, [0, 0, 40])
    humidity['medium'] = fuzz.trimf(humidity.universe, [30, 50, 70])
    humidity['high'] = fuzz.trimf(humidity.universe, [60, 100, 100])

    # Fan speed membership functions
    fan_speed['slow'] = fuzz.trimf(fan_speed.universe, [0, 0, 40])
    fan_speed['medium'] = fuzz.trimf(fan_speed.universe, [30, 50, 70])
    fan_speed['fast'] = fuzz.trimf(fan_speed.universe, [60, 100, 100])

    print("\nMembership Functions Created:")
    print("  Temperature: cold, moderate, hot")
    print("  Humidity: low, medium, high")
    print("  Fan Speed: slow, medium, fast")

    # Define fuzzy rules
    print("\nDefining Rules:")

    rule1 = ctrl.Rule(temperature['cold'] & humidity['low'], fan_speed['slow'])
    print("  Rule 1: IF temp is cold AND humidity is low THEN fan is slow")

    rule2 = ctrl.Rule(temperature['cold'] & humidity['medium'], fan_speed['slow'])
    print("  Rule 2: IF temp is cold AND humidity is medium THEN fan is slow")

    rule3 = ctrl.Rule(temperature['cold'] & humidity['high'], fan_speed['medium'])
    print("  Rule 3: IF temp is cold AND humidity is high THEN fan is medium")

    rule4 = ctrl.Rule(temperature['moderate'] & humidity['low'], fan_speed['slow'])
    print("  Rule 4: IF temp is moderate AND humidity is low THEN fan is slow")

    rule5 = ctrl.Rule(temperature['moderate'] & humidity['medium'], fan_speed['medium'])
    print("  Rule 5: IF temp is moderate AND humidity is medium THEN fan is medium")

    rule6 = ctrl.Rule(temperature['moderate'] & humidity['high'], fan_speed['fast'])
    print("  Rule 6: IF temp is moderate AND humidity is high THEN fan is fast")

    rule7 = ctrl.Rule(temperature['hot'] & humidity['low'], fan_speed['medium'])
    print("  Rule 7: IF temp is hot AND humidity is low THEN fan is medium")

    rule8 = ctrl.Rule(temperature['hot'] & humidity['medium'], fan_speed['fast'])
    print("  Rule 8: IF temp is hot AND humidity is medium THEN fan is fast")

    rule9 = ctrl.Rule(temperature['hot'] & humidity['high'], fan_speed['fast'])
    print("  Rule 9: IF temp is hot AND humidity is high THEN fan is fast")

    # Create control system
    fan_control = ctrl.ControlSystem([
        rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9
    ])

    # Create simulation
    fan_sim = ctrl.ControlSystemSimulation(fan_control)

    print("\nControl System Created Successfully!")
    print("=" * 60)

    return fan_sim, temperature, humidity, fan_speed


def test_controller(fan_sim):
    """Test the controller with various inputs"""
    print("\nTesting Controller with Various Inputs:")
    print("=" * 60)

    test_cases = [
        (15, 30, "Cold, low humidity"),
        (25, 50, "Moderate, medium humidity"),
        (35, 80, "Hot, high humidity"),
        (20, 70, "Cool, high humidity"),
        (38, 40, "Very hot, low humidity"),
    ]

    results = []

    for temp, humid, description in test_cases:
        fan_sim.input['temperature'] = temp
        fan_sim.input['humidity'] = humid
        fan_sim.compute()
        speed = fan_sim.output['fan_speed']

        print(f"\n{description}:")
        print(f"  Temperature: {temp}°C")
        print(f"  Humidity:    {humid}%")
        print(f"  → Fan Speed: {speed:.1f}%")

        results.append((temp, humid, speed))

    return results


def visualize_membership_functions(temperature, humidity, fan_speed):
    """Visualize all membership functions"""
    fig, axes = plt.subplots(3, 1, figsize=(12, 10))

    # Temperature
    for label in temperature.terms:
        axes[0].plot(temperature.universe,
                    temperature[label].mf,
                    linewidth=2,
                    label=label)
    axes[0].set_title('Temperature Membership Functions', fontsize=14)
    axes[0].set_ylabel('Membership Degree')
    axes[0].set_xlabel('Temperature (°C)')
    axes[0].legend()
    axes[0].grid(True, alpha=0.3)

    # Humidity
    for label in humidity.terms:
        axes[1].plot(humidity.universe,
                    humidity[label].mf,
                    linewidth=2,
                    label=label)
    axes[1].set_title('Humidity Membership Functions', fontsize=14)
    axes[1].set_ylabel('Membership Degree')
    axes[1].set_xlabel('Humidity (%)')
    axes[1].legend()
    axes[1].grid(True, alpha=0.3)

    # Fan Speed
    for label in fan_speed.terms:
        axes[2].plot(fan_speed.universe,
                    fan_speed[label].mf,
                    linewidth=2,
                    label=label)
    axes[2].set_title('Fan Speed Membership Functions', fontsize=14)
    axes[2].set_ylabel('Membership Degree')
    axes[2].set_xlabel('Fan Speed (%)')
    axes[2].legend()
    axes[2].grid(True, alpha=0.3)

    plt.tight_layout()
    plt.show()


def visualize_control_surface(fan_sim):
    """Create 3D surface plot showing controller behavior"""
    print("\nGenerating Control Surface...")

    # Create mesh grid
    temp_range = np.arange(0, 41, 2)
    humidity_range = np.arange(0, 101, 5)
    temp_mesh, humidity_mesh = np.meshgrid(temp_range, humidity_range)

    # Compute fan speed for each combination
    fan_speed_mesh = np.zeros_like(temp_mesh)

    for i in range(temp_mesh.shape[0]):
        for j in range(temp_mesh.shape[1]):
            fan_sim.input['temperature'] = temp_mesh[i, j]
            fan_sim.input['humidity'] = humidity_mesh[i, j]
            fan_sim.compute()
            fan_speed_mesh[i, j] = fan_sim.output['fan_speed']

    # Create 3D plot
    fig = plt.figure(figsize=(14, 10))
    ax = fig.add_subplot(111, projection='3d')

    surf = ax.plot_surface(temp_mesh, humidity_mesh, fan_speed_mesh,
                          cmap='viridis', alpha=0.8, edgecolor='none')

    ax.set_xlabel('Temperature (°C)', fontsize=12)
    ax.set_ylabel('Humidity (%)', fontsize=12)
    ax.set_zlabel('Fan Speed (%)', fontsize=12)
    ax.set_title('Fuzzy Control Surface', fontsize=14)

    fig.colorbar(surf, ax=ax, shrink=0.5, aspect=5)

    plt.show()


def visualize_results(results):
    """Visualize test results"""
    temps = [r[0] for r in results]
    humids = [r[1] for r in results]
    speeds = [r[2] for r in results]

    fig, axes = plt.subplots(1, 2, figsize=(14, 5))

    # Temperature vs Fan Speed
    axes[0].scatter(temps, speeds, c=humids, cmap='coolwarm', s=200, alpha=0.7)
    axes[0].set_xlabel('Temperature (°C)', fontsize=12)
    axes[0].set_ylabel('Fan Speed (%)', fontsize=12)
    axes[0].set_title('Temperature vs Fan Speed', fontsize=14)
    axes[0].grid(True, alpha=0.3)
    cbar1 = plt.colorbar(axes[0].collections[0], ax=axes[0])
    cbar1.set_label('Humidity (%)')

    # Humidity vs Fan Speed
    axes[1].scatter(humids, speeds, c=temps, cmap='hot', s=200, alpha=0.7)
    axes[1].set_xlabel('Humidity (%)', fontsize=12)
    axes[1].set_ylabel('Fan Speed (%)', fontsize=12)
    axes[1].set_title('Humidity vs Fan Speed', fontsize=14)
    axes[1].grid(True, alpha=0.3)
    cbar2 = plt.colorbar(axes[1].collections[0], ax=axes[1])
    cbar2.set_label('Temperature (°C)')

    plt.tight_layout()
    plt.show()


def main():
    """Main function to run the temperature control example"""
    print("\n" + "=" * 60)
    print("FUZZY LOGIC TEMPERATURE CONTROL SYSTEM")
    print("=" * 60 + "\n")

    # Create controller
    fan_sim, temperature, humidity, fan_speed = create_temperature_controller()

    # Test controller
    results = test_controller(fan_sim)

    # Visualize membership functions
    print("\nVisualizing Membership Functions...")
    visualize_membership_functions(temperature, humidity, fan_speed)

    # Visualize control surface
    visualize_control_surface(fan_sim)

    # Visualize results
    print("Visualizing Test Results...")
    visualize_results(results)

    print("\n" + "=" * 60)
    print("Temperature Control System Demo Complete!")
    print("=" * 60)


if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print(f"\nError: {e}")
        print("Make sure all dependencies are installed:")
        print("  pip install numpy scikit-fuzzy matplotlib")
