using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Domain.Features.Quotes.Models;

namespace TshirtCuu.Services.Features.Quotes;

/// <summary>Estimates quote totals from catalog pricing rules (same model as the Flutter mock).</summary>
public static class QuotePricing
{
    public static int SideSurchargeMxn(PricingRules rules, ArtworkSource source) =>
        source switch
        {
            ArtworkSource.Catalog => rules.CatalogSideMxn,
            ArtworkSource.Custom => rules.CustomSideMxn,
            _ => 0
        };

    public static int UnitPriceMxn(PricingRules rules, ShirtLine shirt) =>
        rules.BaseUnitMxn
        + SideSurchargeMxn(rules, shirt.FrontSource)
        + SideSurchargeMxn(rules, shirt.BackSource);

    public static int LineTotalMxn(PricingRules rules, ShirtLine shirt) =>
        UnitPriceMxn(rules, shirt) * Math.Max(1, shirt.Quantity);

    public static int? TryEstimateTotalMxn(PricingRules? rules, Quotation quote)
    {
        if (rules is null || quote.Shirts.Count == 0)
            return null;

        var total = 0;
        foreach (var shirt in quote.Shirts)
            total += LineTotalMxn(rules, shirt);
        return total;
    }
}
