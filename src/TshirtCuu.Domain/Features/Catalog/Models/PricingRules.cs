namespace TshirtCuu.Domain.Features.Catalog.Models;

public sealed class PricingRules
{
    public int BaseUnitMxn { get; set; } = 120;
    public int CatalogSideMxn { get; set; } = 25;
    public int CustomSideMxn { get; set; } = 95;
    public int BaseLeadDays { get; set; } = 2;
    public int CatalogExtraDays { get; set; }
    public int CustomExtraDays { get; set; } = 5;
}
