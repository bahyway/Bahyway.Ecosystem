using BahyWay.SharedKernel.Interfaces;
using BahyWay.SharedKernel.Application.Events;
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
        private readonly string _watchPath = @"C:\BahyWay\LandingZone";

        public FileWatchDogService(ILogger<FileWatchDogService> logger, IMessageBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Directory.Exists(_watchPath)) Directory.CreateDirectory(_watchPath);

            var watcher = new FileSystemWatcher(_watchPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.zip", // Watch Zip files
                EnableRaisingEvents = true
            };

            watcher.Created += async (s, e) => await OnFileArrived(e);

            _logger.LogInformation($"👀 WatchDog monitoring: {_watchPath}");
            return Task.CompletedTask; // Keep running
        }

        private async Task OnFileArrived(FileSystemEventArgs e)
        {
            _logger.LogInformation($"🚨 File Detected: {e.Name}");

            var executionId = Guid.NewGuid().ToString();

            // 1. RELIABLE: Send to ETLWay to do the work
            var workPayload = new FileArrivedEvent
            {
                FilePath = e.FullPath,
                FileName = e.Name,
                Timestamp = DateTime.UtcNow
            };
            await _bus.PushToStreamAsync("ETLWay.IngestQueue", workPayload);

            // 2. VISUAL: Send to KGEditor to draw the particle
            var visualPayload = new FlowParticleEvent
            {
                ExecutionId = executionId,
                FileName = e.Name,
                FromNode = "WatchDog",
                ToNode = "Splitter",
                Status = "Moving",
                ColorHex = "#FFFF00" // Yellow
            };
            await _bus.PublishParticleAsync("Flow.Particles", visualPayload);
        }
    }
}