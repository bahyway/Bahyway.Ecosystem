using BahyWay.SharedKernel.Domain.Primitives;

namespace BahyWay.SharedKernel.Application.Abstractions;

/// <summary>
/// File storage service abstraction for uploading, downloading, and managing files.
/// REUSABLE: ✅ ETLway, HireWay, NajafCemetery, SmartForesight
/// STORAGE: Local filesystem, Azure Blob, AWS S3, etc.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file from a stream.
    /// </summary>
    Task<Result<string>> UploadAsync(
        Stream fileStream,
        string fileName,
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file as a stream.
    /// </summary>
    Task<Result<Stream>> DownloadAsync(
        string filePath,
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    Task<Result> DeleteAsync(
        string filePath,
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    Task<bool> ExistsAsync(
        string filePath,
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file metadata (size, created date, etc.).
    /// </summary>
    Task<Result<FileMetadata>> GetMetadataAsync(
        string filePath,
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all files in a container/folder.
    /// </summary>
    Task<Result<IEnumerable<string>>> ListFilesAsync(
        string containerName,
        string? prefix = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a file to another location.
    /// </summary>
    Task<Result> CopyAsync(
        string sourceFilePath,
        string destinationFilePath,
        string sourceContainer,
        string destinationContainer,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a file to another location.
    /// </summary>
    Task<Result> MoveAsync(
        string sourceFilePath,
        string destinationFilePath,
        string sourceContainer,
        string destinationContainer,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// File metadata information.
/// </summary>
public class FileMetadata
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Standard container names for different projects.
/// </summary>
public static class StorageContainers
{
    // ETLway
    public const string ETLInbox = "etl-inbox";
    public const string ETLProcessing = "etl-processing";
    public const string ETLArchive = "etl-archive";
    public const string ETLError = "etl-error";

    // HireWay
    public const string Resumes = "resumes";
    public const string CoverLetters = "cover-letters";
    public const string Certificates = "certificates";

    // NajafCemetery
    public const string BurialDocuments = "burial-documents";
    public const string Photos = "photos";

    // SmartForesight
    public const string Models = "ml-models";
    public const string Datasets = "datasets";
}