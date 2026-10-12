using System.Data;
using Dapper;
using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Domain.Features.Catalog.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Catalog;

public sealed class SqlCatalogRepository(ISqlConnectionFactory connections) : ICatalogRepository
{
    public IReadOnlyList<CatalogCategory> GetCategories()
    {
        using var db = connections.Create();
        return db.Query<CatalogCategory>(
            "dbo.usp_Categories_GetAll",
            commandType: CommandType.StoredProcedure).AsList();
    }

    public CatalogCategory? GetCategory(string id)
    {
        using var db = connections.Create();
        return db.QuerySingleOrDefault<CatalogCategory>(
            "dbo.usp_Categories_GetById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    public CatalogCategory AddCategory(CatalogCategory category)
    {
        using var db = connections.Create();
        return db.QuerySingle<CatalogCategory>(
            "dbo.usp_Categories_Insert",
            new { category.Id, category.Name },
            commandType: CommandType.StoredProcedure);
    }

    public bool UpdateCategory(CatalogCategory category)
    {
        using var db = connections.Create();
        var rows = db.QuerySingle<int>(
            "dbo.usp_Categories_Update",
            new { category.Id, category.Name },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public bool DeleteCategory(string id)
    {
        using var db = connections.Create();
        var rows = db.QuerySingle<int>(
            "dbo.usp_Categories_Delete",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public IReadOnlyList<CatalogDesign> GetDesigns(string? categoryId = null, string? query = null)
    {
        using var db = connections.Create();
        return db.Query<CatalogDesign>(
            "dbo.usp_Designs_GetAll",
            new { CategoryId = categoryId, Query = query },
            commandType: CommandType.StoredProcedure).AsList();
    }

    public CatalogDesign? GetDesign(string id)
    {
        using var db = connections.Create();
        return db.QuerySingleOrDefault<CatalogDesign>(
            "dbo.usp_Designs_GetById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    public CatalogDesign AddDesign(CatalogDesign design)
    {
        using var db = connections.Create();
        return db.QuerySingle<CatalogDesign>(
            "dbo.usp_Designs_Insert",
            new
            {
                design.Id,
                design.Name,
                design.CategoryId,
                design.Tagline,
                design.AccentHex,
                design.InkHex
            },
            commandType: CommandType.StoredProcedure);
    }

    public bool UpdateDesign(CatalogDesign design)
    {
        using var db = connections.Create();
        var rows = db.QuerySingle<int>(
            "dbo.usp_Designs_Update",
            new
            {
                design.Id,
                design.Name,
                design.CategoryId,
                design.Tagline,
                design.AccentHex,
                design.InkHex
            },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public bool DeleteDesign(string id)
    {
        using var db = connections.Create();
        var rows = db.QuerySingle<int>(
            "dbo.usp_Designs_Delete",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public IReadOnlyList<ShirtSize> GetSizes()
    {
        using var db = connections.Create();
        return db.Query<ShirtSize>(
            "dbo.usp_Sizes_GetAll",
            commandType: CommandType.StoredProcedure).AsList();
    }

    public ShirtSize AddSize(ShirtSize size)
    {
        using var db = connections.Create();
        return db.QuerySingle<ShirtSize>(
            "dbo.usp_Sizes_Insert",
            new { size.Id, size.Label },
            commandType: CommandType.StoredProcedure);
    }

    public bool UpdateSize(ShirtSize size)
    {
        using var db = connections.Create();
        var rows = db.QuerySingle<int>(
            "dbo.usp_Sizes_Update",
            new { size.Id, size.Label },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public bool DeleteSize(string id)
    {
        using var db = connections.Create();
        var rows = db.QuerySingle<int>(
            "dbo.usp_Sizes_Delete",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public IReadOnlyList<ShirtColor> GetColors()
    {
        using var db = connections.Create();
        return db.Query<ShirtColor>(
            "dbo.usp_Colors_GetAll",
            commandType: CommandType.StoredProcedure).AsList();
    }

    public ShirtColor AddColor(ShirtColor color)
    {
        using var db = connections.Create();
        return db.QuerySingle<ShirtColor>(
            "dbo.usp_Colors_Insert",
            new { color.Id, color.Name, color.Hex },
            commandType: CommandType.StoredProcedure);
    }

    public bool UpdateColor(ShirtColor color)
    {
        using var db = connections.Create();
        var rows = db.QuerySingle<int>(
            "dbo.usp_Colors_Update",
            new { color.Id, color.Name, color.Hex },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public bool DeleteColor(string id)
    {
        using var db = connections.Create();
        var rows = db.QuerySingle<int>(
            "dbo.usp_Colors_Delete",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public PricingRules GetPricingRules()
    {
        using var db = connections.Create();
        return db.QuerySingleOrDefault<PricingRules>(
                   "dbo.usp_Pricing_Get",
                   commandType: CommandType.StoredProcedure)
               ?? new PricingRules { Id = 1 };
    }

    public PricingRules UpdatePricingRules(PricingRules rules)
    {
        using var db = connections.Create();
        return db.QuerySingle<PricingRules>(
            "dbo.usp_Pricing_Upsert",
            new
            {
                rules.BaseUnitMxn,
                rules.CatalogSideMxn,
                rules.CustomSideMxn,
                rules.BaseLeadDays,
                rules.CatalogExtraDays,
                rules.CustomExtraDays
            },
            commandType: CommandType.StoredProcedure);
    }
}
