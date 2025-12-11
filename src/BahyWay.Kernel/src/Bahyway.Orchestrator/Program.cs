using System;
using System.IO;
using Akka.Actor;
using Bahyway.SharedKernel.Compiler;
using Bahyway.SharedKernel.Actors;

namespace Bahyway.Orchestrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Start the Actor System (The Nervous System)
            BahywaySystem.Start();

            // 2. Run the Interactive Loop
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("🤖 COMMAND CENTER:");
                Console.WriteLine("   [1] Compile 'cemetery.akk' (DSL -> SQL)");
                Console.WriteLine("   [2] Monitor Sensor Status (Actor Check)");
                Console.WriteLine("   [Q] Quit");
                Console.ResetColor();
                Console.Write("> ");

                var input = Console.ReadLine()?.ToUpper();

                if (input == "Q") break;

                switch (input)
                {
                    case "1":
                        RunCompiler();
                        break;
                    case "2":
                        CheckSystemHealth();
                        break;
                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
                Console.WriteLine();
            }

            // 3. Graceful Shutdown
            await BahywaySystem.Stop();
        }

        static void RunCompiler()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(basePath, "cemetery.akk");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"❌ Error: File '{filePath}' not found.");
                return;
            }

            try
            {
                Console.WriteLine("🧠 Parsing DSL...");
                var parser = new AkkadianParser();
                var model = parser.Parse(File.ReadAllText(filePath));

                var generator = new AkkadianGenerator();
                var result = generator.Generate(model);

                string outputDir = Path.Combine(basePath, "output");
                Directory.CreateDirectory(outputDir);
                File.WriteAllText(Path.Combine(outputDir, "datavault.sql"), result.SqlScript);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ SQL Generated in: {outputDir}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        static void CheckSystemHealth()
        {
            if (BahywaySystem.Instance != null)
            {
                Console.WriteLine($"⚡ Uptime: {BahywaySystem.Instance.Uptime}");

                // 1. Spawn the Monitor Actor
                var monitor = BahywaySystem.Instance.ActorOf(Props.Create(() => new SensorMonitorActor()), "pipe-monitor");

                Console.WriteLine("🧪 Sending Test Signals to Actor...");

                // Scenario A: Normal Operation
                monitor.Tell(new SensorReading("Sensor_01", Pressure: 45, Vibration: 5));

                // Scenario B: Nearby Explosion (False Alarm)
                // High Vibration, Low Pressure -> Should be Filtered
                monitor.Tell(new SensorReading("Sensor_02", Pressure: 42, Vibration: 95));

                // Scenario C: Pipe Burst (Real Threat)
                // High Vibration + High Pressure -> Should Alert
                monitor.Tell(new SensorReading("Sensor_03", Pressure: 92, Vibration: 88));

                // Give actors time to process before menu reappears
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}