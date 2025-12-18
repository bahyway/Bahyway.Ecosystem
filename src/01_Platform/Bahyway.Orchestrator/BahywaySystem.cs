using System;
using Akka.Actor;
using Akka.Configuration;

namespace Bahyway.Orchestrator
{
    public static class BahywaySystem
    {
        // Reference to the running system (The Brain)
        public static ActorSystem? Instance { get; private set; }

        public static void Start()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("⚡ Initializing Bahyway Nervous System...");
            Console.ResetColor();

            // 1. HOCON Configuration (The DNA of the Actor System)
            // We configure it to accept remote messages (TCP)
            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    }
                    remote {
                        dot-netty.tcp {
                            port = 8090
                            hostname = ""localhost""
                        }
                    }
                }
            ");

            // 2. Create the System
            Instance = ActorSystem.Create("BahywaySys", config);

            Console.WriteLine($"   - System Name: {Instance.Name}");
            Console.WriteLine($"   - Listening on: akka.tcp://BahywaySys@localhost:8090");
            Console.WriteLine("   - Status: ONLINE");
            Console.WriteLine();
        }

        public static async Task Stop()
        {
            if (Instance != null)
            {
                await Instance.Terminate();
                Console.WriteLine("🛑 Bahyway System Terminated.");
            }
        }
    }
}