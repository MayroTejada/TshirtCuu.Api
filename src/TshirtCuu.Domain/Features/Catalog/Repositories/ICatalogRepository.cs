using TshirtCuu.Domain.Features.Catalog.Models;

namespace TshirtCuu.Domain.Features.Catalog.Repositories;

public interface ICatalogRepository
{
    IReadOnlyList<CatalogCategory> GetCategories();
    CatalogCategory? GetCategory(string id);
    CatalogCategory AddCategory(CatalogCategory category);
    bool UpdateCategory(CatalogCategory category);
    bool DeleteCategory(string id);

    IReadOnlyList<CatalogDesign> GetDesigns(string? categoryId = null, string? query = null);
    CatalogDesign? GetDesign(string id);
    CatalogDesign AddDesign(CatalogDesign design);
    bool UpdateDesign(CatalogDesign design);
    bool DeleteDesign(string id);

    IReadOnlyList<ShirtSize> GetSizes();
    ShirtSize AddSize(ShirtSize size);
    bool UpdateSize(ShirtSize size);
    bool DeleteSize(string id);

    IReadOnlyList<ShirtColor> GetColors();
    ShirtColor AddColor(ShirtColor color);
    bool UpdateColor(ShirtColor color);
    bool DeleteColor(string id);

    PricingRules GetPricingRules();
    PricingRules UpdatePricingRules(PricingRules rules);
}
