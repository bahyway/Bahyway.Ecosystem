namespace Bahyway.SharedKernel.Actors
{
    // Input: Sensor Data
    public record SensorReading(string SensorId, double Pressure, double Vibration);

    // Output: AI Decision
    public record RiskAlert(string SensorId, string ThreatType, double Confidence);

    // *** NEW: UI UPDATE MESSAGE ***
    // The Brain tells the Face: "Change Node X to Color Y"
    public record NodeColorUpdate(string NodeId, string ColorHex);
}