using Microsoft.EntityFrameworkCore;
using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Domain.Features.Catalog.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Catalog;

public sealed class SqlCatalogRepository(AppDbContext db) : ICatalogRepository
{
    public IReadOnlyList<CatalogCategory> GetCategories() =>
        db.Categories.AsNoTracking().OrderBy(c => c.Name).ToList();

    public CatalogCategory? GetCategory(string id) =>
        db.Categories.FirstOrDefault(c => c.Id == id);

    public CatalogCategory AddCategory(CatalogCategory category)
    {
        db.Categories.Add(category);
        db.SaveChanges();
        return category;
    }

    public bool UpdateCategory(CatalogCategory category)
    {
        var existing = db.Categories.FirstOrDefault(c => c.Id == category.Id);
        if (existing is null) return false;
        existing.Name = category.Name;
        db.SaveChanges();
        return true;
    }

    public bool DeleteCategory(string id)
    {
        var existing = db.Categories.FirstOrDefault(c => c.Id == id);
        if (existing is null) return false;
        db.Categories.Remove(existing);
        db.SaveChanges();
        return true;
    }

    public IReadOnlyList<CatalogDesign> GetDesigns(string? categoryId = null, string? query = null)
    {
        IQueryable<CatalogDesign> q = db.Designs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(categoryId))
            q = q.Where(d => d.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(d =>
                d.Name.Contains(term) ||
                d.Tagline.Contains(term));
        }

        return q.OrderBy(d => d.Name).ToList();
    }

    public CatalogDesign? GetDesign(string id) =>
        db.Designs.FirstOrDefault(d => d.Id == id);

    public CatalogDesign AddDesign(CatalogDesign design)
    {
        db.Designs.Add(design);
        db.SaveChanges();
        return design;
    }

    public bool UpdateDesign(CatalogDesign design)
    {
        var existing = db.Designs.FirstOrDefault(d => d.Id == design.Id);
        if (existing is null) return false;
        existing.Name = design.Name;
        existing.CategoryId = design.CategoryId;
        existing.Tagline = design.Tagline;
        existing.AccentHex = design.AccentHex;
        existing.InkHex = design.InkHex;
        db.SaveChanges();
        return true;
    }

    public bool DeleteDesign(string id)
    {
        var existing = db.Designs.FirstOrDefault(d => d.Id == id);
        if (existing is null) return false;
        db.Designs.Remove(existing);
        db.SaveChanges();
        return true;
    }

    public IReadOnlyList<ShirtSize> GetSizes() =>
        db.Sizes.AsNoTracking().OrderBy(s => s.Label).ToList();

    public ShirtSize AddSize(ShirtSize size)
    {
        db.Sizes.Add(size);
        db.SaveChanges();
        return size;
    }

    public bool UpdateSize(ShirtSize size)
    {
        var existing = db.Sizes.FirstOrDefault(s => s.Id == size.Id);
        if (existing is null) return false;
        existing.Label = size.Label;
        db.SaveChanges();
        return true;
    }

    public bool DeleteSize(string id)
    {
        var existing = db.Sizes.FirstOrDefault(s => s.Id == id);
        if (existing is null) return false;
        db.Sizes.Remove(existing);
        db.SaveChanges();
        return true;
    }

    public IReadOnlyList<ShirtColor> GetColors() =>
        db.Colors.AsNoTracking().OrderBy(c => c.Name).ToList();

    public ShirtColor AddColor(ShirtColor color)
    {
        db.Colors.Add(color);
        db.SaveChanges();
        return color;
    }

    public bool UpdateColor(ShirtColor color)
    {
        var existing = db.Colors.FirstOrDefault(c => c.Id == color.Id);
        if (existing is null) return false;
        existing.Name = color.Name;
        existing.Hex = color.Hex;
        db.SaveChanges();
        return true;
    }

    public bool DeleteColor(string id)
    {
        var existing = db.Colors.FirstOrDefault(c => c.Id == id);
        if (existing is null) return false;
        db.Colors.Remove(existing);
        db.SaveChanges();
        return true;
    }

    public PricingRules GetPricingRules() =>
        db.PricingRules.AsNoTracking().FirstOrDefault(p => p.Id == 1)
        ?? new PricingRules { Id = 1 };

    public PricingRules UpdatePricingRules(PricingRules rules)
    {
        rules.Id = 1;
        var existing = db.PricingRules.FirstOrDefault(p => p.Id == 1);
        if (existing is null)
        {
            db.PricingRules.Add(rules);
        }
        else
        {
            existing.BaseUnitMxn = rules.BaseUnitMxn;
            existing.CatalogSideMxn = rules.CatalogSideMxn;
            existing.CustomSideMxn = rules.CustomSideMxn;
            existing.BaseLeadDays = rules.BaseLeadDays;
            existing.CatalogExtraDays = rules.CatalogExtraDays;
            existing.CustomExtraDays = rules.CustomExtraDays;
        }

        db.SaveChanges();
        return GetPricingRules();
    }
}
