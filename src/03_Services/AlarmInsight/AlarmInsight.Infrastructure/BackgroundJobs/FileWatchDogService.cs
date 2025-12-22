using Akka.Actor;
using BahyWay.SharedKernel.Application.DTOs;
using BahyWay.SharedKernel.Application.Events;
using BahyWay.SharedKernel.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AlarmInsight.Infrastructure.BackgroundJobs
{
    public class FileWatchDogService : BackgroundService
    {
        private readonly ILogger<FileWatchDogService> _logger;
        private readonly IMessageBus _bus;
        private readonly ActorSystem _actorSystem; // Field for Akka
        private readonly string _watchPath = @"C:\BahyWay\LandingZone";

        // CONSTRUCTOR: Ensure all 3 are injected and assigned!
        public FileWatchDogService(
            ILogger<FileWatchDogService> logger,
            IMessageBus bus,
            ActorSystem actorSystem)
        {
            _logger = logger;
            _bus = bus;
            _actorSystem = actorSystem; // <--- FIX FOR WARNING CS8618
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Directory.Exists(_watchPath)) Directory.CreateDirectory(_watchPath);

            var watcher = new FileSystemWatcher(_watchPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.zip",
                EnableRaisingEvents = true
            };

            watcher.Created += async (s, e) => await OnFileArrived(e);

            _logger.LogInformation($"👀 WatchDog monitoring: {_watchPath}");
            return Task.CompletedTask;
        }

        private async Task OnFileArrived(FileSystemEventArgs e)
        {
            _logger.LogInformation($"🚨 File Detected: {e.Name}");

            var executionId = Guid.NewGuid().ToString();

            // 1. CREATE THE OBJECT (Fixes CS0103 error)
            var fileEvent = new FileArrivedEvent
            {
                ExecutionId = executionId, // <--- PASS THIS ID
                FilePath = e.FullPath,
                FileName = e.Name,
                Timestamp = DateTime.UtcNow,
                Mode = ProcessingMode.FullAutomation, // Default to Auto
                // --- NEW: Inject the Rules Profile ---
                // In production, you would load this from a DB based on the File Source
                RuleConfig = new PipelineRuleConfig
                {
                    CheckFileSize = true,
                    MinSizeBytes = 1024,
                    AnalyzeNulls = true, // Set to FALSE to simulate "Free Tier"
                    StrictSLA = true     // Enable Enterprise checks
                }
            };

            // 2. Send to Redis (Reliable Queue)
            await _bus.PushToStreamAsync("ETLWay.IngestQueue", fileEvent);

            // 3. Send to Visuals (KGEditor)
            var visualPayload = new FlowParticleEvent
            {
                ExecutionId = executionId,
                FileName = e.Name,
                //FromNode = "WatchDog",
                //ToNode = "Splitter",
                FromNode = "LandingZone", // Start exactly at the folder icon
                ToNode = "WatchDog",
                Status = "Moving",
                ColorHex = "#FFFF00", // Yellow
                SourceSystem = "FileWatcher",
                FileType = "Zip"
            };
            await _bus.PublishParticleAsync("Flow.Particles", visualPayload);

            // 4. *** THE BRIDGE ***
            // Hand the message directly to the Actor so it runs NOW
            _actorSystem.ActorSelection("/user/ingest").Tell(fileEvent);
        }
    }
}