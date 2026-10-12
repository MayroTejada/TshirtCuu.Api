namespace TshirtCuu.Domain.Features.Catalog.Models;

public sealed class ShirtColor
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string Hex { get; set; }
}
