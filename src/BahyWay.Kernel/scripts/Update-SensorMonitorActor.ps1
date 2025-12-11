$monitorPath = "src/Bahyway.SharedKernel/Actors/SensorMonitorActor.cs"
$monitorCode = @"
using System;
using Akka.Actor;
using Bahyway.SharedKernel.AI;

namespace Bahyway.SharedKernel.Actors
{
    public class SensorMonitorActor : ReceiveActor
    {
        private readonly FuzzyEngine _fuzzy;
        // The address of the UI Actor
        private readonly string _uiAddress = ""akka.tcp://BahywayUI@localhost:8091/user/bridge"";

        public SensorMonitorActor()
        {
            _fuzzy = new FuzzyEngine();
            Receive<SensorReading>(reading => ProcessReading(reading));
        }

        private void ProcessReading(SensorReading reading)
        {
            double pressureRisk = _fuzzy.Trapezoid(reading.Pressure, 70, 80, 100, 110);
            double vibrationRisk = _fuzzy.Trapezoid(reading.Vibration, 40, 50, 100, 100);
            double burstProb = _fuzzy.AND(pressureRisk, vibrationRisk);

            string colorToSend = ""#333333""; // Default Gray

            if (burstProb > 0.8)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($""[🔥 CRITICAL] Sending RED ALERT to UI for {reading.SensorId}..."");
                colorToSend = ""#FF0000""; // Red
            }
            else if (vibrationRisk > 0.8)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($""[🛡️ WAR ZONE] Sending YELLOW WARNING to UI for {reading.SensorId}..."");
                colorToSend = ""#FFFF00""; // Yellow
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($""[✅ NORMAL] Sending GREEN STATUS to UI for {reading.SensorId}..."");
                colorToSend = ""#00FF00""; // Green
            }
            Console.ResetColor();

            // SEND TO REMOTE UI
            // We use ActorSelection because the UI might not be open yet
            Context.ActorSelection(_uiAddress).Tell(new NodeColorUpdate(reading.SensorId, colorToSend));
        }
    }
}
"@
Set-Content -Path $monitorPath -Value $monitorCode
Write-Host "✅ SensorMonitorActor updated to broadcast to UI." -ForegroundColor Green
