namespace TshirtCuu.Services.Features.Storage;

public interface IImageBlobStore
{
    bool IsConfigured { get; }

    /// <summary>
    /// Uploads bytes to the private <c>imagenes</c> container.
    /// Returns the blob path (e.g. <c>catalog/{guid}.png</c>).
    /// </summary>
    Task<ImageUploadResult> UploadAsync(
        Stream content,
        string contentType,
        string folder,
        string? originalFileName,
        CancellationToken cancellationToken = default);

    /// <summary>Opens a blob for streaming through the API. Null if missing.</summary>
    Task<ImageDownloadResult?> OpenReadAsync(
        string blobPath,
        CancellationToken cancellationToken = default);

    /// <summary>Best-effort delete (used if quote create fails after an upload attempt).</summary>
    Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default);
}

public sealed record ImageUploadResult(string BlobPath, string ContentType);

public sealed record ImageDownloadResult(Stream Content, string ContentType, string FileName);
