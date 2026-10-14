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

    public async Task<ImageUploadResult> UploadArtworkBase64Async(
        string base64,
        string? contentType,
        string? originalFileName,
        CancellationToken cancellationToken = default)
    {
        var bytes = Convert.FromBase64String(StripDataUrlPrefix(base64));
        await using var stream = new MemoryStream(bytes);
        var type = string.IsNullOrWhiteSpace(contentType) ? "image/png" : contentType.Trim();
        return await store.UploadAsync(stream, type, "artwork", originalFileName, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<ImageDownloadResult?> OpenReadAsync(
        string blobPath,
        CancellationToken cancellationToken = default) =>
        store.OpenReadAsync(blobPath, cancellationToken);

    public Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default) =>
        store.DeleteAsync(blobPath, cancellationToken);

    private static string StripDataUrlPrefix(string value)
    {
        var v = value.Trim();
        var idx = v.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? v[(idx + "base64,".Length)..] : v;
    }
}
