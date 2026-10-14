using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Domain.Features.Catalog.Repositories;

namespace TshirtCuu.Services.Features.Catalog;

public sealed class CatalogService(ICatalogRepository repository)
{
    public IReadOnlyList<CatalogCategory> ListCategories() => repository.GetCategories();

    public CatalogCategory CreateCategory(string name)
    {
        var category = new CatalogCategory
        {
            Id = $"cat-{Guid.NewGuid():N}"[..12],
            Name = name.Trim()
        };
        return repository.AddCategory(category);
    }

    public bool RenameCategory(string id, string name)
    {
        var existing = repository.GetCategory(id);
        if (existing is null) return false;
        existing.Name = name.Trim();
        return repository.UpdateCategory(existing);
    }

    public bool RemoveCategory(string id) => repository.DeleteCategory(id);

    public IReadOnlyList<CatalogDesign> ListDesigns(string? categoryId, string? query) =>
        repository.GetDesigns(categoryId, query);

    public CatalogDesign? GetDesign(string id) => repository.GetDesign(id);

    public CatalogDesign CreateDesign(CatalogDesign design)
    {
        if (string.IsNullOrWhiteSpace(design.Id))
        {
            design = new CatalogDesign
            {
                Id = $"des-{Guid.NewGuid():N}"[..12],
                Name = design.Name,
                CategoryId = design.CategoryId,
                Tagline = design.Tagline,
                AccentHex = design.AccentHex,
                InkHex = design.InkHex,
                ImageBlobPath = design.ImageBlobPath
            };
        }

        return repository.AddDesign(design);
    }

    public bool UpdateDesign(CatalogDesign design) => repository.UpdateDesign(design);

    public bool RemoveDesign(string id) => repository.DeleteDesign(id);

    public IReadOnlyList<ShirtSize> ListSizes() => repository.GetSizes();

    public ShirtSize CreateSize(string label)
    {
        var size = new ShirtSize
        {
            Id = label.Trim().ToLowerInvariant(),
            Label = label.Trim().ToUpperInvariant()
        };
        return repository.AddSize(size);
    }

    public bool UpdateSize(ShirtSize size) => repository.UpdateSize(size);

    public bool RemoveSize(string id) => repository.DeleteSize(id);

    public IReadOnlyList<ShirtColor> ListColors() => repository.GetColors();

    public ShirtColor CreateColor(string name, string hex)
    {
        var color = new ShirtColor
        {
            Id = name.Trim().ToLowerInvariant().Replace(' ', '-'),
            Name = name.Trim(),
            Hex = hex.Trim()
        };
        return repository.AddColor(color);
    }

    public bool UpdateColor(ShirtColor color) => repository.UpdateColor(color);

    public bool RemoveColor(string id) => repository.DeleteColor(id);

    public PricingRules GetPricing() => repository.GetPricingRules();

    public PricingRules UpdatePricing(PricingRules rules) => repository.UpdatePricingRules(rules);
}
