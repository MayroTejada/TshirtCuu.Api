namespace TshirtCuu.Services.Features.Storage;

public sealed class ImageStorageService(IImageBlobStore store)
{
    public bool IsConfigured => store.IsConfigured;

    public Task<ImageUploadResult> UploadCatalogAsync(
        Stream content,
        string contentType,
        string? originalFileName,
        CancellationToken cancellationToken = default) =>
        store.UploadAsync(content, contentType, "catalog", originalFileName, cancellationToken);

    public Task<ImageUploadResult> UploadArtworkAsync(
        Stream content,
        string contentType,
        string? originalFileName,
        CancellationToken cancellationToken = default) =>
        store.UploadAsync(content, contentType, "artwork", originalFileName, cancellationToken);

    public Task<ImageDownloadResult?> OpenReadAsync(
        string blobPath,
        CancellationToken cancellationToken = default) =>
        store.OpenReadAsync(blobPath, cancellationToken);
}
