namespace TshirtCuu.Domain.Features.Quotes.Models;

public sealed class Quotation
{
    public static readonly string[] AllowedStatuses =
        ["Nueva", "En revisión", "Respondida", "Lista", "Finalizada", "Cancelada"];

    public required string Id { get; init; }
    public required string CustomerName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public List<ShirtLine> Shirts { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Status { get; set; } = "Nueva";

    /// <summary>Admin reply text stored on the quote (no email send).</summary>
    public string? CustomerReply { get; set; }

    public DateTimeOffset? RepliedAt { get; set; }

    public int TotalUnits => Shirts.Sum(s => s.Quantity);

    /// <summary>
    /// Not persisted — filled on create when customer artwork could not be stored
    /// (missing AzureStorage__ConnectionString, missing bytes, or upload error).
    /// </summary>
    public string? ArtworkUploadNote { get; set; }
}
