"""
Fuzzy Logic Utility Library

This module provides reusable functions and classes for working with
fuzzy sets, membership functions, and fuzzy operations.
"""

import numpy as np
import matplotlib.pyplot as plt
import skfuzzy as fuzz
from typing import Callable, List, Tuple


class FuzzySet:
    """
    A fuzzy set with a name and membership function.

    Attributes:
        name (str): Name of the fuzzy set (e.g., "Cold", "Hot")
        membership_fn (Callable): Function that returns membership degree for input
        universe (np.ndarray): Domain of discourse (optional)
    """

    def __init__(self, name: str, membership_fn: Callable[[float], float],
                 universe: np.ndarray = None):
        """
        Initialize a fuzzy set.

        Args:
            name: Name of the fuzzy set
            membership_fn: Function mapping input to membership degree [0, 1]
            universe: Optional array of domain values
        """
        self.name = name
        self.membership_fn = membership_fn
        self.universe = universe

    def membership(self, x: float) -> float:
        """
        Get membership degree for a value.

        Args:
            x: Input value

        Returns:
            Membership degree in [0, 1]
        """
        return np.clip(self.membership_fn(x), 0, 1)

    def __call__(self, x: float) -> float:
        """Allow calling the object directly: fuzzy_set(x)"""
        return self.membership(x)

    def __repr__(self):
        return f"FuzzySet('{self.name}')"


# ============================================================================
# Membership Function Generators
# ============================================================================

def triangular_mf(a: float, b: float, c: float) -> Callable:
    """
    Create a triangular membership function.

    Parameters:
        a: Left foot (where membership starts to rise from 0)
        b: Peak (where membership is 1)
        c: Right foot (where membership returns to 0)

    Returns:
        Function that computes membership for any input x
    """
    def membership(x: float) -> float:
        if x <= a or x >= c:
            return 0.0
        elif x == b:
            return 1.0
        elif a < x < b:
            return (x - a) / (b - a)
        else:  # b < x < c
            return (c - x) / (c - b)

    return membership


def trapezoidal_mf(a: float, b: float, c: float, d: float) -> Callable:
    """
    Create a trapezoidal membership function.

    Parameters:
        a: Left foot
        b: Left shoulder (start of peak)
        c: Right shoulder (end of peak)
        d: Right foot

    Returns:
        Function that computes membership for any input x
    """
    def membership(x: float) -> float:
        if x <= a or x >= d:
            return 0.0
        elif b <= x <= c:
            return 1.0
        elif a < x < b:
            return (x - a) / (b - a)
        else:  # c < x < d
            return (d - x) / (d - c)

    return membership


def gaussian_mf(mean: float, sigma: float) -> Callable:
    """
    Create a Gaussian (bell-shaped) membership function.

    Parameters:
        mean: Center of the bell curve
        sigma: Standard deviation (controls width)

    Returns:
        Function that computes membership for any input x
    """
    def membership(x: float) -> float:
        return np.exp(-0.5 * ((x - mean) / sigma) ** 2)

    return membership


def sigmoid_mf(a: float, c: float) -> Callable:
    """
    Create a sigmoid membership function.

    Parameters:
        a: Controls steepness (larger = steeper)
        c: Center point (inflection point)

    Returns:
        Function that computes membership for any input x
    """
    def membership(x: float) -> float:
        return 1.0 / (1.0 + np.exp(-a * (x - c)))

    return membership


# ============================================================================
# Fuzzy Operations
# ============================================================================

def fuzzy_union(a: float, b: float) -> float:
    """
    Fuzzy union (OR operation) using maximum.

    Args:
        a: Membership degree in set A
        b: Membership degree in set B

    Returns:
        max(a, b)
    """
    return max(a, b)


def fuzzy_intersection(a: float, b: float) -> float:
    """
    Fuzzy intersection (AND operation) using minimum.

    Args:
        a: Membership degree in set A
        b: Membership degree in set B

    Returns:
        min(a, b)
    """
    return min(a, b)


def fuzzy_complement(a: float) -> float:
    """
    Fuzzy complement (NOT operation).

    Args:
        a: Membership degree

    Returns:
        1 - a
    """
    return 1.0 - a


def algebraic_product(a: float, b: float) -> float:
    """
    Algebraic product (T-norm alternative to min).

    Args:
        a: Membership degree in set A
        b: Membership degree in set B

    Returns:
        a * b
    """
    return a * b


def algebraic_sum(a: float, b: float) -> float:
    """
    Algebraic sum (S-norm alternative to max).

    Args:
        a: Membership degree in set A
        b: Membership degree in set B

    Returns:
        a + b - a*b
    """
    return a + b - a * b


# ============================================================================
# Defuzzification Methods
# ============================================================================

def defuzzify_centroid(x: np.ndarray, membership: np.ndarray) -> float:
    """
    Defuzzify using centroid (center of gravity) method.

    Args:
        x: Array of x values
        membership: Array of membership values

    Returns:
        Crisp output value
    """
    numerator = np.sum(x * membership)
    denominator = np.sum(membership)

    if denominator == 0:
        return 0.0

    return numerator / denominator


def defuzzify_mean_of_maximum(x: np.ndarray, membership: np.ndarray) -> float:
    """
    Defuzzify using mean of maximum (MOM) method.

    Args:
        x: Array of x values
        membership: Array of membership values

    Returns:
        Mean of x values where membership is maximum
    """
    max_membership = np.max(membership)
    max_indices = membership == max_membership
    return np.mean(x[max_indices])


def defuzzify_bisector(x: np.ndarray, membership: np.ndarray) -> float:
    """
    Defuzzify using bisector method (divides area in half).

    Args:
        x: Array of x values
        membership: Array of membership values

    Returns:
        x value that divides area under curve in half
    """
    total_area = np.sum(membership)
    if total_area == 0:
        return 0.0

    cumulative_area = np.cumsum(membership)
    half_area = total_area / 2.0

    # Find index where cumulative area exceeds half
    idx = np.where(cumulative_area >= half_area)[0]
    if len(idx) == 0:
        return x[-1]

    return x[idx[0]]


# ============================================================================
# Visualization Helpers
# ============================================================================

def plot_membership_function(x: np.ndarray, membership: np.ndarray,
                             title: str = "Membership Function",
                             xlabel: str = "Universe",
                             ylabel: str = "Membership Degree"):
    """
    Plot a single membership function.

    Args:
        x: Array of x values
        membership: Array of membership values
        title: Plot title
        xlabel: X-axis label
        ylabel: Y-axis label
    """
    plt.figure(figsize=(10, 5))
    plt.plot(x, membership, 'b', linewidth=2)
    plt.fill_between(x, membership, alpha=0.3)
    plt.xlabel(xlabel, fontsize=12)
    plt.ylabel(ylabel, fontsize=12)
    plt.title(title, fontsize=14)
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])
    plt.tight_layout()
    plt.show()


def plot_multiple_membership_functions(x: np.ndarray,
                                       memberships: List[Tuple[np.ndarray, str]],
                                       title: str = "Membership Functions",
                                       xlabel: str = "Universe",
                                       ylabel: str = "Membership Degree"):
    """
    Plot multiple membership functions on the same graph.

    Args:
        x: Array of x values (shared by all functions)
        memberships: List of (membership_array, label) tuples
        title: Plot title
        xlabel: X-axis label
        ylabel: Y-axis label
    """
    plt.figure(figsize=(12, 6))

    colors = ['b', 'g', 'r', 'c', 'm', 'y', 'k']

    for i, (membership, label) in enumerate(memberships):
        color = colors[i % len(colors)]
        plt.plot(x, membership, color, linewidth=2, label=label)
        plt.fill_between(x, membership, alpha=0.2)

    plt.xlabel(xlabel, fontsize=12)
    plt.ylabel(ylabel, fontsize=12)
    plt.title(title, fontsize=14)
    plt.legend(fontsize=10)
    plt.grid(True, alpha=0.3)
    plt.ylim([-0.1, 1.1])
    plt.tight_layout()
    plt.show()


def plot_fuzzy_operations(x: np.ndarray, set_a: np.ndarray, set_b: np.ndarray,
                          label_a: str = "Set A", label_b: str = "Set B"):
    """
    Visualize fuzzy union, intersection, and complement operations.

    Args:
        x: Array of x values
        set_a: Membership array for set A
        set_b: Membership array for set B
        label_a: Label for set A
        label_b: Label for set B
    """
    union = np.fmax(set_a, set_b)
    intersection = np.fmin(set_a, set_b)
    complement_a = 1 - set_a

    fig, axes = plt.subplots(4, 1, figsize=(12, 10))

    # Original sets
    axes[0].plot(x, set_a, 'b', linewidth=2, label=label_a)
    axes[0].plot(x, set_b, 'r', linewidth=2, label=label_b)
    axes[0].fill_between(x, set_a, alpha=0.3)
    axes[0].fill_between(x, set_b, alpha=0.3)
    axes[0].set_title('Original Sets', fontsize=12)
    axes[0].set_ylabel('Membership')
    axes[0].legend()
    axes[0].grid(True, alpha=0.3)
    axes[0].set_ylim([-0.1, 1.1])

    # Union
    axes[1].plot(x, union, 'g', linewidth=2, label='A ∪ B (Union)')
    axes[1].fill_between(x, union, alpha=0.3, color='g')
    axes[1].set_title('Union (OR) - Maximum', fontsize=12)
    axes[1].set_ylabel('Membership')
    axes[1].legend()
    axes[1].grid(True, alpha=0.3)
    axes[1].set_ylim([-0.1, 1.1])

    # Intersection
    axes[2].plot(x, intersection, 'm', linewidth=2, label='A ∩ B (Intersection)')
    axes[2].fill_between(x, intersection, alpha=0.3, color='m')
    axes[2].set_title('Intersection (AND) - Minimum', fontsize=12)
    axes[2].set_ylabel('Membership')
    axes[2].legend()
    axes[2].grid(True, alpha=0.3)
    axes[2].set_ylim([-0.1, 1.1])

    # Complement
    axes[3].plot(x, set_a, 'b', linewidth=2, label=label_a)
    axes[3].plot(x, complement_a, 'c', linewidth=2, label=f'NOT {label_a}')
    axes[3].fill_between(x, complement_a, alpha=0.3, color='c')
    axes[3].set_title('Complement (NOT) - 1 minus membership', fontsize=12)
    axes[3].set_ylabel('Membership')
    axes[3].set_xlabel('Universe')
    axes[3].legend()
    axes[3].grid(True, alpha=0.3)
    axes[3].set_ylim([-0.1, 1.1])

    plt.tight_layout()
    plt.show()


# ============================================================================
# Example Usage
# ============================================================================

def example_usage():
    """Demonstrate the utility library"""
    print("Fuzzy Logic Utility Library - Example Usage\n")

    # Create universe
    x = np.arange(0, 101, 1)

    # Create fuzzy sets using utility functions
    print("1. Creating fuzzy sets...")
    cold_fn = triangular_mf(0, 0, 50)
    warm_fn = triangular_mf(25, 50, 75)
    hot_fn = triangular_mf(50, 100, 100)

    cold = np.array([cold_fn(xi) for xi in x])
    warm = np.array([warm_fn(xi) for xi in x])
    hot = np.array([hot_fn(xi) for xi in x])

    # Visualize
    plot_multiple_membership_functions(
        x,
        [(cold, 'Cold'), (warm, 'Warm'), (hot, 'Hot')],
        title='Temperature Membership Functions',
        xlabel='Temperature (°C)'
    )

    # Test membership
    print("\n2. Testing membership degrees at 30°C:")
    test_val = 30
    print(f"   Cold: {cold_fn(test_val):.3f}")
    print(f"   Warm: {warm_fn(test_val):.3f}")
    print(f"   Hot:  {hot_fn(test_val):.3f}")

    # Fuzzy operations
    print("\n3. Fuzzy operations:")
    a, b = 0.7, 0.5
    print(f"   Union({a}, {b}) = {fuzzy_union(a, b)}")
    print(f"   Intersection({a}, {b}) = {fuzzy_intersection(a, b)}")
    print(f"   Complement({a}) = {fuzzy_complement(a)}")

    # Defuzzification
    print("\n4. Defuzzification example:")
    test_membership = warm
    centroid = defuzzify_centroid(x, test_membership)
    mom = defuzzify_mean_of_maximum(x, test_membership)
    print(f"   Centroid: {centroid:.2f}")
    print(f"   Mean of Maximum: {mom:.2f}")


if __name__ == "__main__":
    example_usage()
