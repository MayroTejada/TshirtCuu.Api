namespace TshirtCuu.Domain.Features.Catalog.Models;

public sealed class CatalogDesign
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string CategoryId { get; set; }
    public required string Tagline { get; set; }
    public string AccentHex { get; set; } = "#B8F34A";
    public string InkHex { get; set; } = "#0E1116";
}
