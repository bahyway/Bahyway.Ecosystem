# Path to the broken file
$path = "src/Bahyway.SharedKernel/Actors/SensorMonitorActor.cs"

# The Correct C# Code (Using Literal String format)
$content = @'
using System;
using Akka.Actor;
using Bahyway.SharedKernel.AI;

namespace Bahyway.SharedKernel.Actors
{
    public class SensorMonitorActor : ReceiveActor
    {
        private readonly FuzzyEngine _fuzzy;
        // The address of the UI Actor
        private readonly string _uiAddress = "akka.tcp://BahywayUI@localhost:8091/user/bridge";

        public SensorMonitorActor()
        {
            _fuzzy = new FuzzyEngine();
            Receive<SensorReading>(reading => ProcessReading(reading));
        }

        private void ProcessReading(SensorReading reading)
        {
            double pressureRisk = _fuzzy.Trapezoid(reading.Pressure, 70, 80, 100, 110);
            double vibrationRisk = _fuzzy.Trapezoid(reading.Vibration, 40, 50, 100, 100);

            // Logic: IF Pressure is High AND Vibration is High -> PIPE BURST
            double burstProb = _fuzzy.AND(pressureRisk, vibrationRisk);

            string colorToSend = "#333333"; // Default Gray

            if (burstProb > 0.8)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                // Note: Using standard string concatenation to avoid PowerShell confusion
                Console.WriteLine("[CRITICAL] Sending RED ALERT to UI for " + reading.SensorId);
                colorToSend = "#FF0000"; // Red
            }
            else if (vibrationRisk > 0.8)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[WAR ZONE] Sending YELLOW WARNING to UI for " + reading.SensorId);
                colorToSend = "#FFFF00"; // Yellow
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[NORMAL] Sending GREEN STATUS to UI for " + reading.SensorId);
                colorToSend = "#00FF00"; // Green
            }
            Console.ResetColor();

            // SEND TO REMOTE UI
            // We use ActorSelection because the UI might not be open yet
            Context.ActorSelection(_uiAddress).Tell(new NodeColorUpdate(reading.SensorId, colorToSend));
        }
    }
}
'@

# Overwrite the file with the clean code
Set-Content -Path $path -Value $content -Encoding UTF8

Write-Host "✅ SensorMonitorActor.cs has been repaired." -ForegroundColor Green

