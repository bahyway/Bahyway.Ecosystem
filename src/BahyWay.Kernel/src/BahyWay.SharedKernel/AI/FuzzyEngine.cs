using System;

namespace Bahyway.SharedKernel.AI
{
    public class FuzzyEngine
    {
        /// <summary>
        /// Calculates the Fuzzy Membership (0.0 to 1.0) using a Trapezoidal shape.
        /// </summary>
        /// <param name="value">The sensor reading (e.g., Pressure)</param>
        /// <param name="a">Start of rise (0 -> 1)</param>
        /// <param name="b">Start of peak (1.0)</param>
        /// <param name="c">End of peak (1.0)</param>
        /// <param name="d">End of fall (1 -> 0)</param>
        public double Trapezoid(double value, double a, double b, double c, double d)
        {
            if (value <= a || value >= d) return 0.0; // Outside the shape
            if (value >= b && value <= c) return 1.0; // Inside the "Plateau" (Perfect match)

            // Rising edge (Up slope)
            if (value > a && value < b) return (value - a) / (b - a);

            // Falling edge (Down slope)
            return (d - value) / (d - c);
        }

        /// <summary>
        /// Standard logic operators for Fuzzy systems
        /// </summary>
        public double AND(double val1, double val2) => Math.Min(val1, val2);
        public double OR(double val1, double val2) => Math.Max(val1, val2);
        public double NOT(double val) => 1.0 - val;
    }
}