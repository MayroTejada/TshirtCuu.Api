namespace TshirtCuu.Domain.Features.Quotes.Models;

public sealed class ShirtLine
{
    public required string Id { get; init; }
    public required string ColorId { get; set; }
    public required string ColorName { get; set; }
    public required string ColorHex { get; set; }
    public required string SizeId { get; set; }
    public required string SizeLabel { get; set; }
    public int Quantity { get; set; } = 1;

    public ArtworkSource FrontSource { get; set; } = ArtworkSource.None;
    public string? FrontLabel { get; set; }
    public string? FrontCatalogId { get; set; }
    public ArtPlacement FrontPlacement { get; set; } = new();

    public ArtworkSource BackSource { get; set; } = ArtworkSource.None;
    public string? BackLabel { get; set; }
    public string? BackCatalogId { get; set; }
    public ArtPlacement BackPlacement { get; set; } = new();
}
