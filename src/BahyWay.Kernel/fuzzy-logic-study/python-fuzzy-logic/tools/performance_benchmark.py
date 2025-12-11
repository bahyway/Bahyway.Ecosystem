"""
Performance Benchmarking Tool for Fuzzy Logic Operations

This tool measures and compares the performance of various fuzzy logic
operations and implementations.
"""

import time
import numpy as np
import matplotlib.pyplot as plt
import skfuzzy as fuzz
from typing import Callable, List, Tuple


def benchmark_function(func: Callable, iterations: int = 1000,
                      *args, **kwargs) -> Tuple[float, float]:
    """
    Benchmark a function by running it multiple times.

    Args:
        func: Function to benchmark
        iterations: Number of times to run the function
        *args, **kwargs: Arguments to pass to the function

    Returns:
        Tuple of (average_time, total_time) in seconds
    """
    times = []

    for _ in range(iterations):
        start = time.perf_counter()
        func(*args, **kwargs)
        end = time.perf_counter()
        times.append(end - start)

    return np.mean(times), np.sum(times)


def benchmark_membership_functions():
    """Benchmark different membership function types"""
    print("\n" + "=" * 60)
    print("BENCHMARKING MEMBERSHIP FUNCTIONS")
    print("=" * 60)

    x = np.arange(0, 101, 1)
    iterations = 10000

    results = {}

    # Triangular
    print("\nTriangular membership function...")
    avg_time, _ = benchmark_function(
        fuzz.trimf, iterations, x, [0, 50, 100]
    )
    results['Triangular'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Trapezoidal
    print("Trapezoidal membership function...")
    avg_time, _ = benchmark_function(
        fuzz.trapmf, iterations, x, [0, 25, 75, 100]
    )
    results['Trapezoidal'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Gaussian
    print("Gaussian membership function...")
    avg_time, _ = benchmark_function(
        fuzz.gaussmf, iterations, x, 50, 20
    )
    results['Gaussian'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Sigmoid
    print("Sigmoid membership function...")
    avg_time, _ = benchmark_function(
        fuzz.sigmf, iterations, x, 0.1, 50
    )
    results['Sigmoid'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    return results


def benchmark_fuzzy_operations():
    """Benchmark fuzzy operations"""
    print("\n" + "=" * 60)
    print("BENCHMARKING FUZZY OPERATIONS")
    print("=" * 60)

    x = np.arange(0, 101, 1)
    set_a = fuzz.trimf(x, [0, 25, 50])
    set_b = fuzz.trimf(x, [50, 75, 100])
    iterations = 10000

    results = {}

    # Union
    print("\nFuzzy union (maximum)...")
    avg_time, _ = benchmark_function(
        np.fmax, iterations, set_a, set_b
    )
    results['Union'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Intersection
    print("Fuzzy intersection (minimum)...")
    avg_time, _ = benchmark_function(
        np.fmin, iterations, set_a, set_b
    )
    results['Intersection'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Complement
    print("Fuzzy complement...")
    def complement(x):
        return 1 - x
    avg_time, _ = benchmark_function(
        complement, iterations, set_a
    )
    results['Complement'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Algebraic product
    print("Algebraic product...")
    avg_time, _ = benchmark_function(
        np.multiply, iterations, set_a, set_b
    )
    results['Algebraic Product'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    return results


def benchmark_defuzzification():
    """Benchmark defuzzification methods"""
    print("\n" + "=" * 60)
    print("BENCHMARKING DEFUZZIFICATION METHODS")
    print("=" * 60)

    x = np.arange(0, 101, 1)
    membership = fuzz.trimf(x, [20, 50, 80])
    iterations = 1000

    results = {}

    # Centroid
    print("\nCentroid method...")
    avg_time, _ = benchmark_function(
        fuzz.defuzz, iterations, x, membership, 'centroid'
    )
    results['Centroid'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Bisector
    print("Bisector method...")
    avg_time, _ = benchmark_function(
        fuzz.defuzz, iterations, x, membership, 'bisector'
    )
    results['Bisector'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Mean of Maximum
    print("Mean of Maximum method...")
    avg_time, _ = benchmark_function(
        fuzz.defuzz, iterations, x, membership, 'mom'
    )
    results['MOM'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Smallest of Maximum
    print("Smallest of Maximum method...")
    avg_time, _ = benchmark_function(
        fuzz.defuzz, iterations, x, membership, 'som'
    )
    results['SOM'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Largest of Maximum
    print("Largest of Maximum method...")
    avg_time, _ = benchmark_function(
        fuzz.defuzz, iterations, x, membership, 'lom'
    )
    results['LOM'] = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    return results


def benchmark_vectorized_vs_loop():
    """Compare vectorized operations vs loops"""
    print("\n" + "=" * 60)
    print("VECTORIZED vs LOOP COMPARISON")
    print("=" * 60)

    x = np.arange(0, 1001, 1)
    iterations = 100

    # Vectorized
    print("\nVectorized triangular membership function...")
    def vectorized_trimf():
        return fuzz.trimf(x, [0, 500, 1000])

    avg_time, _ = benchmark_function(vectorized_trimf, iterations)
    vectorized_time = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    # Loop
    print("Loop-based triangular membership function...")
    def loop_trimf():
        result = np.zeros_like(x, dtype=float)
        a, b, c = 0, 500, 1000
        for i, xi in enumerate(x):
            if xi <= a or xi >= c:
                result[i] = 0
            elif xi == b:
                result[i] = 1
            elif xi < b:
                result[i] = (xi - a) / (b - a)
            else:
                result[i] = (c - xi) / (c - b)
        return result

    avg_time, _ = benchmark_function(loop_trimf, iterations)
    loop_time = avg_time
    print(f"  Average time: {avg_time*1e6:.2f} μs")

    speedup = loop_time / vectorized_time
    print(f"\nSpeedup (vectorized): {speedup:.2f}x faster")

    return {'Vectorized': vectorized_time, 'Loop': loop_time}


def visualize_results(mf_results, op_results, defuzz_results):
    """Create visualization of benchmark results"""
    fig, axes = plt.subplots(2, 2, figsize=(14, 10))

    # Membership Functions
    ax = axes[0, 0]
    names = list(mf_results.keys())
    times = [mf_results[name] * 1e6 for name in names]  # Convert to μs
    bars = ax.bar(names, times, color='skyblue', edgecolor='navy')
    ax.set_ylabel('Time (μs)', fontsize=12)
    ax.set_title('Membership Function Performance', fontsize=14)
    ax.tick_params(axis='x', rotation=45)
    ax.grid(axis='y', alpha=0.3)

    # Add value labels on bars
    for bar in bars:
        height = bar.get_height()
        ax.text(bar.get_x() + bar.get_width()/2., height,
               f'{height:.2f}', ha='center', va='bottom', fontsize=10)

    # Fuzzy Operations
    ax = axes[0, 1]
    names = list(op_results.keys())
    times = [op_results[name] * 1e6 for name in names]
    bars = ax.bar(names, times, color='lightgreen', edgecolor='darkgreen')
    ax.set_ylabel('Time (μs)', fontsize=12)
    ax.set_title('Fuzzy Operation Performance', fontsize=14)
    ax.tick_params(axis='x', rotation=45)
    ax.grid(axis='y', alpha=0.3)

    for bar in bars:
        height = bar.get_height()
        ax.text(bar.get_x() + bar.get_width()/2., height,
               f'{height:.2f}', ha='center', va='bottom', fontsize=10)

    # Defuzzification
    ax = axes[1, 0]
    names = list(defuzz_results.keys())
    times = [defuzz_results[name] * 1e6 for name in names]
    bars = ax.bar(names, times, color='lightcoral', edgecolor='darkred')
    ax.set_ylabel('Time (μs)', fontsize=12)
    ax.set_title('Defuzzification Method Performance', fontsize=14)
    ax.tick_params(axis='x', rotation=45)
    ax.grid(axis='y', alpha=0.3)

    for bar in bars:
        height = bar.get_height()
        ax.text(bar.get_x() + bar.get_width()/2., height,
               f'{height:.2f}', ha='center', va='bottom', fontsize=10)

    # Summary
    ax = axes[1, 1]
    ax.axis('off')
    summary_text = "Performance Summary\n\n"
    summary_text += "Fastest Membership Function:\n"
    fastest_mf = min(mf_results, key=mf_results.get)
    summary_text += f"  {fastest_mf}: {mf_results[fastest_mf]*1e6:.2f} μs\n\n"

    summary_text += "Fastest Operation:\n"
    fastest_op = min(op_results, key=op_results.get)
    summary_text += f"  {fastest_op}: {op_results[fastest_op]*1e6:.2f} μs\n\n"

    summary_text += "Fastest Defuzzification:\n"
    fastest_defuzz = min(defuzz_results, key=defuzz_results.get)
    summary_text += f"  {fastest_defuzz}: {defuzz_results[fastest_defuzz]*1e6:.2f} μs\n\n"

    summary_text += "Recommendations:\n"
    summary_text += f"• Use {fastest_mf} for speed\n"
    summary_text += "• Vectorize operations with NumPy\n"
    summary_text += f"• {fastest_defuzz} is fastest defuzz method\n"

    ax.text(0.1, 0.9, summary_text, fontsize=11, verticalalignment='top',
           family='monospace', bbox=dict(boxstyle='round', facecolor='wheat', alpha=0.5))

    plt.tight_layout()
    plt.show()


def main():
    """Main benchmarking function"""
    print("\n" + "=" * 60)
    print("FUZZY LOGIC PERFORMANCE BENCHMARK TOOL")
    print("=" * 60)
    print("\nThis tool measures the performance of various fuzzy logic")
    print("operations to help you optimize your implementations.")
    print("\nNote: Results may vary based on system performance.")

    # Run benchmarks
    mf_results = benchmark_membership_functions()
    op_results = benchmark_fuzzy_operations()
    defuzz_results = benchmark_defuzzification()
    vec_results = benchmark_vectorized_vs_loop()

    # Print summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)
    print("\nFastest in each category:")
    print(f"  Membership Function: {min(mf_results, key=mf_results.get)}")
    print(f"  Fuzzy Operation:     {min(op_results, key=op_results.get)}")
    print(f"  Defuzzification:     {min(defuzz_results, key=defuzz_results.get)}")
    print(f"\nVectorization speedup: {vec_results['Loop']/vec_results['Vectorized']:.2f}x")

    # Visualize
    print("\nGenerating visualization...")
    visualize_results(mf_results, op_results, defuzz_results)

    print("\n" + "=" * 60)
    print("Benchmarking Complete!")
    print("=" * 60)


if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print(f"\nError: {e}")
        print("Make sure all dependencies are installed:")
        print("  pip install numpy scikit-fuzzy matplotlib")
