using System;
using System.IO;
using System.Threading.Tasks;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Application.Events;
using BahyWay.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;
using Akka.Actor; // <--- REQUIRED

namespace BahyWay.SharedKernel.Infrastructure.FileWatcher
{
    public class FileWatcherEventArgs : EventArgs
    {
        public string FullPath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public WatcherChangeTypes ChangeType { get; set; }
    }

    public class FileWatcherService : IFileWatcherService
    {
        private readonly ILogger<FileWatcherService> _logger;
        private readonly IMessageBus _messageBus;
        private readonly ActorSystem _actorSystem; // <--- NEW DEPENDENCY
        private FileSystemWatcher? _watcher;
        private string _currentPath = "";

        // INJECT ActorSystem
        public FileWatcherService(ILogger<FileWatcherService> logger, IMessageBus messageBus, ActorSystem actorSystem)
        {
            _logger = logger;
            _messageBus = messageBus;
            _actorSystem = actorSystem;
        }

        public void StartWatching(
            string directoryPath,
            string fileFilter = "*.*",
            Action<FileWatcherEventArgs>? onFileCreated = null,
            Action<FileWatcherEventArgs>? onFileChanged = null,
            Action<FileWatcherEventArgs>? onFileDeleted = null)
        {
            try
            {
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                _currentPath = directoryPath;
                StopWatching(directoryPath);

                _watcher = new FileSystemWatcher(directoryPath)
                {
                    Filter = fileFilter,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = false
                };

                _watcher.Created += async (sender, e) =>
                {
                    // 1. Run internal logic
                    await HandleFileArrived(e);

                    // 2. Run external callback
                    onFileCreated?.Invoke(new FileWatcherEventArgs
                    {
                        FullPath = e.FullPath,
                        Name = e.Name,
                        ChangeType = e.ChangeType
                    });
                };

                _logger.LogInformation($"[FileWatcher] Started watching: {directoryPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FileWatcher] Failed to start");
            }
        }

        private async Task HandleFileArrived(FileSystemEventArgs e)
        {
            await Task.Delay(500); // Debounce

            var executionId = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow;

            Console.WriteLine($"[FileWatcher] 🟢 Detected: {e.Name}. Waking up Actor...");

            // 1. Prepare Payload
            var fileEvent = new FileArrivedEvent
            {
                ExecutionId = executionId,
                FileName = e.Name,
                FilePath = e.FullPath,
                Timestamp = timestamp,
                Mode = ProcessingMode.FullAutomation,
                RuleConfig = new BahyWay.SharedKernel.Application.DTOs.PipelineRuleConfig()
            };

            // 2. Send to Redis (Audit Trail)
            await _messageBus.PushToStreamAsync("ETLWay.IngestQueue", fileEvent);

            // 3. Send Visuals to UI (The Yellow Dot)
            var visualEvent = new FlowParticleEvent
            {
                ExecutionId = executionId,
                FileName = e.Name,
                FromNode = "LandingZone",
                ToNode = "WatchDog",
                Status = "Moving",
                ColorHex = "#FFFF00",
                SourceSystem = "FileWatcher",
                FileType = "Zip",
                Timestamp = timestamp
            };
            await _messageBus.PublishParticleAsync("Flow.Particles", visualEvent);

            // 4. *** THE BRIDGE RESTORED ***
            // Wake up the Actor immediately
            _actorSystem.ActorSelection("/user/generated_pipeline").Tell(fileEvent);
        }

        public void StopWatching(string directoryPath)
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        public string[] GetWatchedDirectories()
        {
            return string.IsNullOrEmpty(_currentPath) ? Array.Empty<string>() : new[] { _currentPath };
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}