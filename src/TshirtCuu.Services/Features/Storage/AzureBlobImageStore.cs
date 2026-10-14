using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TshirtCuu.Services.Features.Storage;

public sealed class AzureBlobImageStore(
    IOptions<AzureStorageOptions> options,
    ILogger<AzureBlobImageStore> logger) : IImageBlobStore
{
    private readonly AzureStorageOptions _options = options.Value;
    private bool _containerEnsured;

    public bool IsConfigured => _options.IsConfigured;

    public async Task<ImageUploadResult> UploadAsync(
        Stream content,
        string contentType,
        string folder,
        string? originalFileName,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var container = await GetContainerAsync(cancellationToken).ConfigureAwait(false);

        var ext = GuessExtension(contentType, originalFileName);
        var safeFolder = SanitizeFolder(folder);
        var blobPath = $"{safeFolder}/{Guid.NewGuid():N}{ext}";
        var blob = container.GetBlobClient(blobPath);

        var headers = new BlobHttpHeaders
        {
            ContentType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType.Trim()
        };

        await blob.UploadAsync(
                content,
                new BlobUploadOptions { HttpHeaders = headers },
                cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("Uploaded image blob {BlobPath} ({ContentType}).", blobPath, headers.ContentType);
        return new ImageUploadResult(blobPath, headers.ContentType);
    }

    public async Task<ImageDownloadResult?> OpenReadAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var path = NormalizePath(blobPath);
        if (path is null) return null;

        var container = await GetContainerAsync(cancellationToken).ConfigureAwait(false);
        var blob = container.GetBlobClient(path);
        if (!await blob.ExistsAsync(cancellationToken).ConfigureAwait(false))
            return null;

        var download = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var contentType = download.Value.Details.ContentType;
        if (string.IsNullOrWhiteSpace(contentType))
            contentType = "application/octet-stream";

        var fileName = Path.GetFileName(path);
        return new ImageDownloadResult(download.Value.Content, contentType, fileName);
    }

    private void EnsureConfigured()
    {
        if (_options.IsConfigured) return;
        throw new InvalidOperationException(
            "Azure Storage is not configured. Set App Setting AzureStorage__ConnectionString " +
            "(storage account connection string). Do not commit secrets. " +
            "Images are stored in the private container 'imagenes'.");
    }

    private async Task<BlobContainerClient> GetContainerAsync(CancellationToken cancellationToken)
    {
        var containerName = string.IsNullOrWhiteSpace(_options.ContainerName)
            ? "imagenes"
            : _options.ContainerName.Trim().ToLowerInvariant();

        var service = new BlobServiceClient(_options.ConnectionString);
        var container = service.GetBlobContainerClient(containerName);

        if (!_containerEnsured)
        {
            // CreateIfNotExists is idempotent; private access only.
            await container.CreateIfNotExistsAsync(
                    PublicAccessType.None,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            _containerEnsured = true;
            logger.LogInformation(
                "Ensured private blob container {Container}.",
                containerName);
        }

        return container;
    }

    private static string SanitizeFolder(string folder)
    {
        var f = (folder ?? "misc").Trim().Trim('/').ToLowerInvariant();
        if (f is not ("catalog" or "artwork"))
            f = "misc";
        return f;
    }

    private static string? NormalizePath(string blobPath)
    {
        if (string.IsNullOrWhiteSpace(blobPath)) return null;
        var path = blobPath.Trim().Replace('\\', '/').TrimStart('/');
        if (path.Contains("..", StringComparison.Ordinal) || path.Contains(':', StringComparison.Ordinal))
            return null;
        if (path.Length > 512) return null;
        return path;
    }

    private static string GuessExtension(string contentType, string? originalFileName)
    {
        var fromName = Path.GetExtension(originalFileName ?? "");
        if (!string.IsNullOrWhiteSpace(fromName) && fromName.Length <= 8)
            return fromName.ToLowerInvariant();

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => ".bin"
        };
    }
}
