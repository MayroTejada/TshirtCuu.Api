namespace TshirtCuu.Domain.Features.Quotes.Models;

public sealed class ShirtLine
{
    public required string Id { get; init; }
    public string QuotationId { get; set; } = string.Empty;

    public required string ColorId { get; set; }
    public required string ColorName { get; set; }
    public required string ColorHex { get; set; }
    public required string SizeId { get; set; }
    public required string SizeLabel { get; set; }
    public int Quantity { get; set; } = 1;

    /// <summary>Required cut: Dama or Caballero.</summary>
    public required string Fit { get; set; }

    public static readonly string[] AllowedFits = ["Dama", "Caballero"];

    public ArtworkSource FrontSource { get; set; } = ArtworkSource.None;
    public string? FrontLabel { get; set; }
    public string? FrontCatalogId { get; set; }
    /// <summary>Private blob path for custom front artwork (API-mediated read).</summary>
    public string? FrontImageBlobPath { get; set; }
    public ArtPlacement FrontPlacement { get; set; } = new();

    public ArtworkSource BackSource { get; set; } = ArtworkSource.None;
    public string? BackLabel { get; set; }
    public string? BackCatalogId { get; set; }
    /// <summary>Private blob path for custom back artwork (API-mediated read).</summary>
    public string? BackImageBlobPath { get; set; }
    public ArtPlacement BackPlacement { get; set; } = new();
}
