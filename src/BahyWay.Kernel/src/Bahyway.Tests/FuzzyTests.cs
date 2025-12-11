using Xunit;
using Bahyway.SharedKernel.AI;

namespace Bahyway.Tests
{
    public class FuzzyTests
    {
        private readonly FuzzyEngine _fuzzy = new FuzzyEngine();

        [Fact]
        public void Should_Detect_Critical_Burst_Risk()
        {
            // --- DEFINITION (From your DSL) ---
            // Pressure Critical: Starts rising at 70, Peak at 80-100
            double a = 70, b = 80, c = 100, d = 110;

            // --- SCENARIO 1: Normal Operation (Pressure 50) ---
            double normalState = _fuzzy.Trapezoid(50, a, b, c, d);
            Assert.Equal(0.0, normalState); // Should be 0 risk

            // --- SCENARIO 2: Dangerous Spike (Pressure 90) ---
            double criticalState = _fuzzy.Trapezoid(90, a, b, c, d);
            Assert.Equal(1.0, criticalState); // 100% Critical
        }

        [Fact]
        public void Should_Filter_False_Alarm_In_WarZone()
        {
            // SCENARIO: Explosion nearby causing ground shock
            double pressureReading = 40.0;   // Normal Pressure
            double vibrationReading = 95.0;  // EXTREME Vibration (Shockwave)

            // 1. Evaluate Pressure Risk (Trapezoid: 70, 80, 100, 110)
            double pressureRisk = _fuzzy.Trapezoid(pressureReading, 70, 80, 100, 110);

            // 2. Evaluate Vibration Risk (Trapezoid: 50, 70, 100, 100)
            double vibrationRisk = _fuzzy.Trapezoid(vibrationReading, 50, 70, 100, 100);

            // 3. COMBINE: A Pipe Burst requires BOTH High Pressure AND High Vibration
            double pipeBurstProbability = _fuzzy.AND(pressureRisk, vibrationRisk);

            // 4. Assert
            // Even though vibration is 100% (1.0), Pressure is 0% (0.0).
            // Logic: Min(1.0, 0.0) = 0.0
            System.Console.WriteLine($"Vibration Risk: {vibrationRisk}");
            System.Console.WriteLine($"Pressure Risk: {pressureRisk}");
            System.Console.WriteLine($"Burst Probability: {pipeBurstProbability}");

            Assert.Equal(0.0, pipeBurstProbability);
            // Result: The system successfully IGNORES the explosion vibration.
        }
    }
}