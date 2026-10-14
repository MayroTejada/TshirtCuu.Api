namespace TshirtCuu.Services.Features.Storage;

/// <summary>
/// Azure Blob Storage settings. Set App Setting / env <c>AzureStorage__ConnectionString</c>.
/// Missing value does not block API startup; uploads return a clear error instead.
/// </summary>
public sealed class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    /// <summary>Storage account connection string (never commit secrets).</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Private container for catalog and customer artwork.</summary>
    public string ContainerName { get; set; } = "imagenes";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);
}
