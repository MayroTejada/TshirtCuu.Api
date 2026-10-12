using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Domain.Features.Catalog.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Catalog;

public sealed class InMemoryCatalogRepository(InMemoryStore store) : ICatalogRepository
{
    public IReadOnlyList<CatalogCategory> GetCategories() => store.Categories.ToList();

    public CatalogCategory? GetCategory(string id) =>
        store.Categories.FirstOrDefault(c => c.Id == id);

    public CatalogCategory AddCategory(CatalogCategory category)
    {
        store.Categories.Add(category);
        return category;
    }

    public bool UpdateCategory(CatalogCategory category)
    {
        var existing = GetCategory(category.Id);
        if (existing is null) return false;
        existing.Name = category.Name;
        return true;
    }

    public bool DeleteCategory(string id) =>
        store.Categories.RemoveAll(c => c.Id == id) > 0;

    public IReadOnlyList<CatalogDesign> GetDesigns(string? categoryId = null, string? query = null)
    {
        IEnumerable<CatalogDesign> q = store.Designs;
        if (!string.IsNullOrWhiteSpace(categoryId))
            q = q.Where(d => d.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(d =>
                d.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                d.Tagline.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return q.ToList();
    }

    public CatalogDesign? GetDesign(string id) =>
        store.Designs.FirstOrDefault(d => d.Id == id);

    public CatalogDesign AddDesign(CatalogDesign design)
    {
        store.Designs.Add(design);
        return design;
    }

    public bool UpdateDesign(CatalogDesign design)
    {
        var existing = GetDesign(design.Id);
        if (existing is null) return false;
        existing.Name = design.Name;
        existing.CategoryId = design.CategoryId;
        existing.Tagline = design.Tagline;
        existing.AccentHex = design.AccentHex;
        existing.InkHex = design.InkHex;
        return true;
    }

    public bool DeleteDesign(string id) =>
        store.Designs.RemoveAll(d => d.Id == id) > 0;

    public IReadOnlyList<ShirtSize> GetSizes() => store.Sizes.ToList();

    public ShirtSize AddSize(ShirtSize size)
    {
        store.Sizes.Add(size);
        return size;
    }

    public bool UpdateSize(ShirtSize size)
    {
        var existing = store.Sizes.FirstOrDefault(s => s.Id == size.Id);
        if (existing is null) return false;
        existing.Label = size.Label;
        return true;
    }

    public bool DeleteSize(string id) =>
        store.Sizes.RemoveAll(s => s.Id == id) > 0;

    public IReadOnlyList<ShirtColor> GetColors() => store.Colors.ToList();

    public ShirtColor AddColor(ShirtColor color)
    {
        store.Colors.Add(color);
        return color;
    }

    public bool UpdateColor(ShirtColor color)
    {
        var existing = store.Colors.FirstOrDefault(c => c.Id == color.Id);
        if (existing is null) return false;
        existing.Name = color.Name;
        existing.Hex = color.Hex;
        return true;
    }

    public bool DeleteColor(string id) =>
        store.Colors.RemoveAll(c => c.Id == id) > 0;

    public PricingRules GetPricingRules() => store.Pricing;

    public PricingRules UpdatePricingRules(PricingRules rules)
    {
        store.Pricing = rules;
        return store.Pricing;
    }
}
