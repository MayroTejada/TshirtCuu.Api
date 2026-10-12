namespace TshirtCuu.Domain.Features.Quotes.Models;

public sealed class Quotation
{
    public required string Id { get; init; }
    public required string CustomerName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required List<ShirtLine> Shirts { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string Status { get; set; } = "Nueva";

    public int TotalUnits => Shirts.Sum(s => s.Quantity);
}
