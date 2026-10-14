namespace TshirtCuu.Services.Features.Quotes;

public sealed class ArtworkAttachResult
{
    public bool Found { get; private init; } = true;
    public bool Stored { get; private init; }
    public string? BlobPath { get; private init; }
    public string? Side { get; private init; }
    public string? Error { get; private init; }
    public int StatusCode { get; private init; } = 200;

    public static ArtworkAttachResult Ok(string blobPath, string side) => new()
    {
        Stored = true,
        BlobPath = blobPath,
        Side = side,
        StatusCode = 200
    };

    public static ArtworkAttachResult NotFound() => new()
    {
        Found = false,
        StatusCode = 404,
        Error = "Quote or shirt line not found."
    };

    public static ArtworkAttachResult BadSide() => new()
    {
        StatusCode = 400,
        Error = "side must be 'front' or 'back'."
    };

    public static ArtworkAttachResult BadFile(string message) => new()
    {
        StatusCode = 400,
        Error = message
    };

    public static ArtworkAttachResult StorageNotConfigured() => new()
    {
        StatusCode = 503,
        Error =
            "Azure Storage is not configured. Set App Setting AzureStorage__ConnectionString " +
            "(or ConnectionStrings__AzureStorage). The quote exists; artwork was not stored."
    };

    public static ArtworkAttachResult PathSaveFailed(string detail) => new()
    {
        StatusCode = 500,
        Error =
            "Image reached Azure Storage but the blob path was not saved on the shirt line. " +
            "Apply TShirtMigration tables/ImageBlobPaths.sql if FrontImageBlobPath/BackImageBlobPath " +
            "columns are missing. " + detail
    };

    public static ArtworkAttachResult Failed(string detail) => new()
    {
        StatusCode = 503,
        Error = detail
    };
}
