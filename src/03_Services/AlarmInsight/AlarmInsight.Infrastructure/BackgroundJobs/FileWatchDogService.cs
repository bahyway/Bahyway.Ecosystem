using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using BahyWay.SharedKernel.Application.Abstractions; // For IFileWatcherService

namespace AlarmInsight.Infrastructure.BackgroundJobs
{
    public class FileWatchDogService : BackgroundService
    {
        // 1. Declare the field (This was missing, causing the error)
        private readonly IFileWatcherService _fileWatcher;

        // Path to watch
        private readonly string _watchPath = @"C:\BahyWay\LandingZone";

        // 2. Inject it via Constructor
        public FileWatchDogService(IFileWatcherService fileWatcher)
        {
            _fileWatcher = fileWatcher;
        }

        // 3. Use it
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Now '_fileWatcher' exists, so this line will work
            _fileWatcher.StartWatching(_watchPath, "*.zip");

            return Task.CompletedTask;
        }
    }
}