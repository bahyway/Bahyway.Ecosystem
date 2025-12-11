namespace BahyWay.SharedKernel.Application.Abstractions;

/// <summary>
/// File watcher service for monitoring file system changes.
/// REUSABLE: ✅ PRIMARY: ETLway (large ZIP file processing)
/// USE CASE: Detects when new files arrive, triggers background processing
/// </summary>
public interface IFileWatcherService
{
    /// <summary>
    /// Starts watching a directory for file changes.
    /// </summary>
    void StartWatching(
        string directoryPath,
        string fileFilter = "*.*",
        Action<FileWatcherEventArgs> onFileCreated = null,
        Action<FileWatcherEventArgs> onFileChanged = null,
        Action<FileWatcherEventArgs> onFileDeleted = null);

    /// <summary>
    /// Stops watching a directory.
    /// </summary>
    void StopWatching(string directoryPath);

    /// <summary>
    /// Stops all watchers.
    /// </summary>
    void StopAll();

    /// <summary>
    /// Gets list of currently watched directories.
    /// </summary>
    IEnumerable<string> GetWatchedDirectories();
}

/// <summary>
/// Event args for file watcher events.
/// </summary>
public class FileWatcherEventArgs
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public FileWatcherChangeType ChangeType { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public long? FileSizeBytes { get; set; }
}

/// <summary>
/// Types of file changes.
/// </summary>
public enum FileWatcherChangeType
{
    Created = 1,
    Changed = 2,
    Deleted = 3,
    Renamed = 4
}

/// <summary>
/// Configuration for file watcher.
/// </summary>
public class FileWatcherOptions
{
    /// <summary>
    /// Directory to watch.
    /// </summary>
    public string WatchDirectory { get; set; } = string.Empty;

    /// <summary>
    /// File filter (e.g., "*.zip", "*.xml").
    /// </summary>
    public string FileFilter { get; set; } = "*.*";

    /// <summary>
    /// Watch subdirectories.
    /// </summary>
    public bool IncludeSubdirectories { get; set; } = false;

    /// <summary>
    /// Minimum file size to trigger event (bytes).
    /// Useful to avoid processing incomplete file writes.
    /// </summary>
    public long MinimumFileSizeBytes { get; set; } = 0;

    /// <summary>
    /// Delay before triggering event (to ensure file is fully written).
    /// </summary>
    public TimeSpan StabilizationDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Enable notification for file created events.
    /// </summary>
    public bool NotifyOnCreated { get; set; } = true;

    /// <summary>
    /// Enable notification for file changed events.
    /// </summary>
    public bool NotifyOnChanged { get; set; } = false;

    /// <summary>
    /// Enable notification for file deleted events.
    /// </summary>
    public bool NotifyOnDeleted { get; set; } = false;
}
