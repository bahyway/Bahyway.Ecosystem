using System;
// We need to reference the namespace where FileWatcherEventArgs lives
using BahyWay.SharedKernel.Infrastructure.FileWatcher;

namespace BahyWay.SharedKernel.Application.Abstractions
{
    public interface IFileWatcherService : IDisposable
    {
        void StartWatching(
            string directoryPath,
            string fileFilter = "*.*",
            Action<FileWatcherEventArgs>? onFileCreated = null,
            Action<FileWatcherEventArgs>? onFileChanged = null,
            Action<FileWatcherEventArgs>? onFileDeleted = null);

        void StopWatching(string directoryPath);
        string[] GetWatchedDirectories();
    }
}