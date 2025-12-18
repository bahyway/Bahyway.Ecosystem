$file = "src/Bahyway.SharedKernel/Actors/SystemMessages.cs"
$content = @"
namespace Bahyway.SharedKernel.Actors
{
    // Input: Represents a raw data packet from a Pipeline Sensor
    public record SensorReading(string SensorId, double Pressure, double Vibration);

    // Output: Represents a decision made by the AI
    public record RiskAlert(string SensorId, string ThreatType, double Confidence);

    // *** NEW: UI UPDATE MESSAGE ***
    // The Brain tells the Face: "Change Node X to Color Y"
    public record NodeColorUpdate(string NodeId, string ColorHex);
}
"@
Set-Content -Path $file -Value $content
Write-Host "✅ Protocol Updated: Added NodeColorUpdate message." -ForegroundColor Green
