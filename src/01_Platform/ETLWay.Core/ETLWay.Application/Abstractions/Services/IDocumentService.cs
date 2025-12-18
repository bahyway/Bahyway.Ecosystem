using BahyWay.SharedKernel.Domain.Primitives;

namespace ETLWay.Application.Abstractions.Services;

/// <summary>
/// Service interface for document management.
/// REUSABLE: ✅ ETLway, HireWay, NajafCemetery (document handling)
/// PURPOSE: Handles upload, storage, and retrieval of documents and photos
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Uploads a document and returns the URL.
    /// </summary>
    Task<Result<string>> UploadDocumentAsync(
        byte[] content,
        string fileName,
        string contentType,
        string category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a document by URL.
    /// </summary>
    Task<Result<byte[]>> DownloadDocumentAsync(
        string documentUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document by URL.
    /// </summary>
    Task<Result> DeleteDocumentAsync(
        string documentUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a temporary signed URL for secure document access.
    /// </summary>
    Task<Result<string>> GenerateTemporaryUrlAsync(
        string documentUrl,
        TimeSpan expiresIn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a file is an allowed image type and size.
    /// </summary>
    Task<Result> ValidateImageAsync(
        byte[] content,
        string fileName,
        int maxSizeInMb = 5,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Document categories for cemetery system.
/// </summary>
public static class DocumentCategories
{
    public const string BurialPermit = "burial-permit";
    public const string DeathCertificate = "death-certificate";
    public const string NationalId = "national-id";
    public const string Photo = "photo";
    public const string BurialCertificate = "burial-certificate";
    public const string Other = "other";
}
