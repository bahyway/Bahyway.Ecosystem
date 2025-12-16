using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Akka.Actor;

namespace AlarmInsight.Infrastructure.BackgroundJobs
{
    public class FileWatchDogService : BackgroundService
    {
        private readonly ILogger<FileWatchDogService> _logger;
        private readonly FileSystemWatcher _watcher;
        private readonly IActorRef _etlOrchestrator; // Reference to ETLWay Actor

        public FileWatchDogService(ILogger<FileWatchDogService> logger, ActorSystem actorSystem)
        {
            _logger = logger;

            // Connect to the ETLWay Actor (The "Hands")
            // In a real scenario, you might use Akka.Remote string path here
            _etlOrchestrator = actorSystem.ActorSelection("/user/PipelineOrchestrator").ResolveOne(TimeSpan.FromSeconds(5)).Result;

            // Setup the Watcher
            _watcher = new FileSystemWatcher(@"C:\BahyWay\LandingZone")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.zip" // Only watch ZIP files
            };

            _watcher.Created += OnFileCreated;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _watcher.EnableRaisingEvents = true;
            _logger.LogInformation("👀 WatchDog is monitoring /LandingZone for ZIP files...");
            return Task.CompletedTask;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"🚨 EVENT DETECTED: File Arrived - {e.Name}");

            // 1. Send signal to ETLWay to start processing
            _etlOrchestrator.Tell(new IngestZipFileCommand { FilePath = e.FullPath });

            // 2. (Optional) Send signal to KGEditor to spawn a Particle
            // This could be done via SignalR or EventBus
        }
    }
}