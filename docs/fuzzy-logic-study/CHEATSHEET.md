# 📝 Fuzzy Logic Quick Reference Cheatsheet

Fast reference for fuzzy logic concepts and code syntax in Python and Rust.

## 📐 Fuzzy Logic Fundamentals

### Membership Functions

| Type | Description | When to Use |
|------|-------------|-------------|
| **Triangular** | Linear increase, peak, linear decrease | Simple, symmetric categories |
| **Trapezoidal** | Flat peak region | Ranges with certainty zone |
| **Gaussian** | Bell curve shape | Natural phenomena, smooth transitions |
| **Sigmoid** | S-shaped curve | Gradual transitions |

### Fuzzy Operations

| Operation | Formula | Python (NumPy) | Description |
|-----------|---------|----------------|-------------|
| **Union (OR)** | max(A, B) | `np.fmax(A, B)` | Maximum membership |
| **Intersection (AND)** | min(A, B) | `np.fmin(A, B)` | Minimum membership |
| **Complement (NOT)** | 1 - A | `1 - A` | Inverse membership |
| **Algebraic Product** | A × B | `A * B` | T-norm alternative |
| **Algebraic Sum** | A + B - A×B | `A + B - A*B` | S-norm alternative |

---

## 🐍 Python Quick Reference

### Setup & Imports

```python
import numpy as np
import skfuzzy as fuzz
from skfuzzy import control as ctrl
import matplotlib.pyplot as plt
```

### Creating Membership Functions

```python
# Universe of discourse
x = np.arange(0, 11, 1)

# Triangular
low = fuzz.trimf(x, [0, 0, 5])

# Trapezoidal
medium = fuzz.trapmf(x, [3, 4, 6, 7])

# Gaussian
high = fuzz.gaussmf(x, mean=8, sigma=1.5)

# Sigmoid
s_shaped = fuzz.sigmf(x, a=5, b=1)

# Generalized Bell
bell = fuzz.gbellmf(x, a=2, b=4, c=5)
```

### Fuzzy Operations

```python
# Union (OR)
union = np.fmax(set1, set2)

# Intersection (AND)
intersection = np.fmin(set1, set2)

# Complement (NOT)
complement = 1 - set1

# Alpha cut
alpha = 0.5
alpha_cut = x[set1 >= alpha]
```

### Visualization

```python
plt.figure(figsize=(10, 5))
plt.plot(x, low, label='Low')
plt.plot(x, medium, label='Medium')
plt.plot(x, high, label='High')
plt.xlabel('Universe')
plt.ylabel('Membership Degree')
plt.legend()
plt.grid(True)
plt.show()
```

### Control System (scikit-fuzzy)

```python
# Define variables
temperature = ctrl.Antecedent(np.arange(0, 41, 1), 'temperature')
fan_speed = ctrl.Consequent(np.arange(0, 101, 1), 'fan_speed')

# Auto-generate membership functions
temperature.automf(3)  # Low, medium, high
fan_speed.automf(3)

# Or define custom
temperature['cold'] = fuzz.trimf(temperature.universe, [0, 0, 20])
temperature['warm'] = fuzz.trimf(temperature.universe, [10, 25, 35])
temperature['hot'] = fuzz.trimf(temperature.universe, [30, 40, 40])

# Define rules
rule1 = ctrl.Rule(temperature['cold'], fan_speed['low'])
rule2 = ctrl.Rule(temperature['warm'], fan_speed['medium'])
rule3 = ctrl.Rule(temperature['hot'], fan_speed['high'])

# Create control system
fan_ctrl = ctrl.ControlSystem([rule1, rule2, rule3])
fan_sim = ctrl.ControlSystemSimulation(fan_ctrl)

# Compute
fan_sim.input['temperature'] = 25
fan_sim.compute()
print(fan_sim.output['fan_speed'])
```

### Defuzzification Methods

```python
# Centroid (Center of Gravity)
defuzz_val = fuzz.defuzz(x, membership, 'centroid')

# Bisector (divides area in half)
defuzz_val = fuzz.defuzz(x, membership, 'bisector')

# Mean of Maximum
defuzz_val = fuzz.defuzz(x, membership, 'mom')

# Smallest of Maximum
defuzz_val = fuzz.defuzz(x, membership, 'som')

# Largest of Maximum
defuzz_val = fuzz.defuzz(x, membership, 'lom')
```

---

## 🦀 Rust Quick Reference

### Basic Membership Functions

```rust
// Triangular
fn trimf(x: f64, a: f64, b: f64, c: f64) -> f64 {
    if x <= a || x >= c { 0.0 }
    else if x == b { 1.0 }
    else if x < b { (x - a) / (b - a) }
    else { (c - x) / (c - b) }
}

// Trapezoidal
fn trapmf(x: f64, a: f64, b: f64, c: f64, d: f64) -> f64 {
    if x <= a || x >= d { 0.0 }
    else if x >= b && x <= c { 1.0 }
    else if x < b { (x - a) / (b - a) }
    else { (d - x) / (d - c) }
}

// Gaussian
fn gaussmf(x: f64, mean: f64, sigma: f64) -> f64 {
    (-(x - mean).powi(2) / (2.0 * sigma.powi(2))).exp()
}

// Sigmoid
fn sigmf(x: f64, a: f64, c: f64) -> f64 {
    1.0 / (1.0 + (-a * (x - c)).exp())
}
```

### Fuzzy Operations

```rust
// Union (OR)
fn fuzzy_or(a: f64, b: f64) -> f64 {
    a.max(b)
}

// Intersection (AND)
fn fuzzy_and(a: f64, b: f64) -> f64 {
    a.min(b)
}

// Complement (NOT)
fn fuzzy_not(a: f64) -> f64 {
    1.0 - a
}

// Algebraic Product
fn algebraic_product(a: f64, b: f64) -> f64 {
    a * b
}

// Algebraic Sum
fn algebraic_sum(a: f64, b: f64) -> f64 {
    a + b - a * b
}
```

### Fuzzy Set Structure

```rust
use std::collections::HashMap;

struct FuzzySet {
    name: String,
    values: HashMap<String, f64>,
}

impl FuzzySet {
    fn new(name: &str) -> Self {
        FuzzySet {
            name: name.to_string(),
            values: HashMap::new(),
        }
    }

    fn add_member(&mut self, element: String, degree: f64) {
        self.values.insert(element, degree.clamp(0.0, 1.0));
    }

    fn membership(&self, element: &str) -> f64 {
        *self.values.get(element).unwrap_or(&0.0)
    }

    fn union(&self, other: &FuzzySet) -> FuzzySet {
        let mut result = FuzzySet::new(&format!("{}_union_{}", self.name, other.name));
        for (k, &v1) in &self.values {
            let v2 = other.membership(k);
            result.add_member(k.clone(), v1.max(v2));
        }
        result
    }

    fn intersection(&self, other: &FuzzySet) -> FuzzySet {
        let mut result = FuzzySet::new(&format!("{}_intersect_{}", self.name, other.name));
        for (k, &v1) in &self.values {
            let v2 = other.membership(k);
            result.add_member(k.clone(), v1.min(v2));
        }
        result
    }
}
```

### Defuzzification

```rust
// Centroid method
fn centroid(x_values: &[f64], membership: &[f64]) -> f64 {
    let numerator: f64 = x_values.iter()
        .zip(membership.iter())
        .map(|(x, m)| x * m)
        .sum();
    let denominator: f64 = membership.iter().sum();

    if denominator == 0.0 { 0.0 } else { numerator / denominator }
}

// Mean of Maximum
fn mean_of_maximum(x_values: &[f64], membership: &[f64]) -> f64 {
    let max_membership = membership.iter()
        .cloned()
        .fold(f64::NEG_INFINITY, f64::max);

    let max_x_values: Vec<f64> = x_values.iter()
        .zip(membership.iter())
        .filter(|(_, &m)| m == max_membership)
        .map(|(&x, _)| x)
        .collect();

    max_x_values.iter().sum::<f64>() / max_x_values.len() as f64
}
```

### Simple Control System

```rust
struct FuzzyController {
    rules: Vec<Rule>,
}

struct Rule {
    condition: Box<dyn Fn(f64) -> f64>,
    conclusion: Box<dyn Fn(f64) -> f64>,
}

impl FuzzyController {
    fn new() -> Self {
        FuzzyController { rules: Vec::new() }
    }

    fn add_rule(&mut self,
                condition: Box<dyn Fn(f64) -> f64>,
                conclusion: Box<dyn Fn(f64) -> f64>) {
        self.rules.push(Rule { condition, conclusion });
    }

    fn evaluate(&self, input: f64) -> f64 {
        let x_range: Vec<f64> = (0..100).map(|i| i as f64).collect();
        let mut aggregated = vec![0.0; x_range.len()];

        for rule in &self.rules {
            let strength = (rule.condition)(input);
            for (i, &x) in x_range.iter().enumerate() {
                let membership = (rule.conclusion)(x);
                aggregated[i] = aggregated[i].max(strength.min(membership));
            }
        }

        centroid(&x_range, &aggregated)
    }
}
```

---

## 🎯 Common Patterns

### Temperature Controller Pattern

```python
# Python
if temp < 20:    # Cold
    fan_speed = 20
elif temp < 30:  # Warm - fuzzy zone!
    # Linear interpolation
    fan_speed = 20 + (temp - 20) * (80 - 20) / (30 - 20)
else:            # Hot
    fan_speed = 80
```

```rust
// Rust
let fan_speed = if temp < 20.0 {
    20.0
} else if temp < 30.0 {
    20.0 + (temp - 20.0) * (80.0 - 20.0) / (30.0 - 20.0)
} else {
    80.0
};
```

### Rule Evaluation Pattern

**IF-THEN Rules:**
```
IF temperature IS hot AND humidity IS high
THEN fan_speed IS very_fast

IF temperature IS cold OR humidity IS low
THEN fan_speed IS slow
```

**Python:**
```python
# Evaluate antecedent
hot_degree = temp_mf['hot'][temp_input]
high_humidity = humidity_mf['high'][humidity_input]

# AND operation
rule_strength = np.fmin(hot_degree, high_humidity)

# Apply to consequent
output = np.fmin(rule_strength, fan_mf['very_fast'])
```

**Rust:**
```rust
let hot_degree = trimf(temp_input, 30.0, 40.0, 40.0);
let high_humidity = trimf(humidity_input, 60.0, 80.0, 100.0);

// AND operation
let rule_strength = hot_degree.min(high_humidity);

// Apply to consequent
let output = (0..100)
    .map(|speed| {
        let membership = trimf(speed as f64, 80.0, 100.0, 100.0);
        rule_strength.min(membership)
    })
    .collect::<Vec<f64>>();
```

---

## 💡 Best Practices

### Design Tips

1. **Universe of Discourse:** Choose appropriate ranges for your variables
   - Too narrow: Loss of precision
   - Too wide: Unnecessary computation

2. **Membership Functions:**
   - Use 3-7 linguistic terms per variable
   - Ensure sufficient overlap (20-50%)
   - Keep shapes simple for interpretation

3. **Rules:**
   - Start with obvious extremes
   - Add intermediate rules for smoothness
   - Keep rule base manageable (< 50 rules)

4. **Defuzzification:**
   - Centroid: Most common, balanced
   - MOM: When you want average peak response
   - Bisector: When you want to split area equally

### Performance Tips

**Python:**
```python
# Use NumPy operations (vectorized)
result = np.fmin(set1, set2)  # ✓ Fast

# Avoid loops
# result = [min(a, b) for a, b in zip(set1, set2)]  # ✗ Slow
```

**Rust:**
```rust
// Inline small functions
#[inline]
fn fuzzy_and(a: f64, b: f64) -> f64 {
    a.min(b)
}

// Use iterators instead of loops
let result: Vec<f64> = values.iter()
    .map(|&x| trimf(x, 0.0, 5.0, 10.0))
    .collect();
```

---

## 🔍 Debugging Tips

### Check Membership Values

```python
# Python - Should be in [0, 1]
assert 0 <= membership_value <= 1, "Invalid membership!"
```

```rust
// Rust - Clamp values
let membership = value.clamp(0.0, 1.0);
```

### Visualize Everything

```python
# Plot at every step
plt.plot(x, membership_function, label='Debug')
plt.axvline(input_value, color='r', linestyle='--')
plt.legend()
plt.show()
```

### Test Edge Cases

- Zero input
- Maximum input
- Boundary values between membership functions
- All zeros (should not crash defuzzification)

---

## 📊 Typical Values

| Domain | Universe Range | Linguistic Terms |
|--------|---------------|------------------|
| Temperature (°C) | 0-50 | Cold, Warm, Hot |
| Speed (km/h) | 0-120 | Slow, Medium, Fast |
| Percentage | 0-100 | Low, Medium, High |
| Distance (m) | 0-10 | Near, Medium, Far |
| Quality | 0-10 | Poor, Average, Good |

---

**Remember:** Fuzzy logic is about modeling human reasoning. If it doesn't make intuitive sense, revisit your membership functions and rules! 🧠
