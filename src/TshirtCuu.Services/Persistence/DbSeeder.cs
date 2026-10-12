using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TshirtCuu.Domain.Features.Admin.Models;
using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Domain.Features.Quotes.Models;

namespace TshirtCuu.Services.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminUser>>();

        if (!await db.Categories.AnyAsync(ct))
        {
            db.Categories.AddRange(
                new CatalogCategory { Id = "cat-ciudad", Name = "Ciudad" },
                new CatalogCategory { Id = "cat-ruta", Name = "Ruta" },
                new CatalogCategory { Id = "cat-cultura", Name = "Cultura" },
                new CatalogCategory { Id = "cat-deporte", Name = "Deporte" });
        }

        if (!await db.Designs.AnyAsync(ct))
        {
            db.Designs.AddRange(
                new CatalogDesign { Id = "cat-cuu-skyline", Name = "Chihuahua Skyline", CategoryId = "cat-ciudad", Tagline = "Stock · entrega rápida", AccentHex = "#B8F34A" },
                new CatalogDesign { Id = "cat-barrio", Name = "Barrio Centro", CategoryId = "cat-ciudad", Tagline = "Stock · entrega rápida", AccentHex = "#FF6B4A" },
                new CatalogDesign { Id = "cat-noche", Name = "Noche Cuu", CategoryId = "cat-ciudad", Tagline = "Stock · entrega rápida", AccentHex = "#C4B5FD" },
                new CatalogDesign { Id = "cat-ruta", Name = "Ruta 45", CategoryId = "cat-ruta", Tagline = "Stock · entrega rápida", AccentHex = "#6EC6FF" },
                new CatalogDesign { Id = "cat-desierto", Name = "Desierto Norte", CategoryId = "cat-ruta", Tagline = "Stock · entrega rápida", AccentHex = "#F9A825" },
                new CatalogDesign { Id = "cat-cultura", Name = "Cultura Local", CategoryId = "cat-cultura", Tagline = "Stock · entrega rápida", AccentHex = "#FF8A65" },
                new CatalogDesign { Id = "cat-deporte", Name = "Deporte Cuu", CategoryId = "cat-deporte", Tagline = "Stock · entrega rápida", AccentHex = "#66BB6A" });
        }

        if (!await db.Sizes.AnyAsync(ct))
        {
            db.Sizes.AddRange(
                new ShirtSize { Id = "xs", Label = "XS" },
                new ShirtSize { Id = "s", Label = "S" },
                new ShirtSize { Id = "m", Label = "M" },
                new ShirtSize { Id = "l", Label = "L" },
                new ShirtSize { Id = "xl", Label = "XL" },
                new ShirtSize { Id = "xxl", Label = "XXL" });
        }

        if (!await db.Colors.AnyAsync(ct))
        {
            db.Colors.AddRange(
                new ShirtColor { Id = "arena", Name = "Arena", Hex = "#F3EEE4" },
                new ShirtColor { Id = "negro", Name = "Negro", Hex = "#111111" },
                new ShirtColor { Id = "azul", Name = "Azul", Hex = "#1F4B99" },
                new ShirtColor { Id = "rojo", Name = "Rojo", Hex = "#C62828" },
                new ShirtColor { Id = "verde", Name = "Verde", Hex = "#2E7D32" },
                new ShirtColor { Id = "mostaza", Name = "Mostaza", Hex = "#F9A825" },
                new ShirtColor { Id = "uva", Name = "Uva", Hex = "#6A1B9A" },
                new ShirtColor { Id = "gris", Name = "Gris", Hex = "#455A64" });
        }

        if (!await db.PricingRules.AnyAsync(ct))
        {
            db.PricingRules.Add(new PricingRules { Id = 1 });
        }

        if (!await db.AdminUsers.AnyAsync(ct))
        {
            var admin = new AdminUser
            {
                Username = "admin",
                PasswordHash = string.Empty,
                Role = "Admin"
            };
            admin.PasswordHash = hasher.HashPassword(admin, "admin");
            db.AdminUsers.Add(admin);
        }

        if (!await db.Quotations.AnyAsync(ct))
        {
            db.Quotations.AddRange(SampleQuotes());
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Database seed completed (idempotent).");
    }

    private static IEnumerable<Quotation> SampleQuotes()
    {
        yield return new Quotation
        {
            Id = "COT-098",
            CustomerName = "Ana Ruiz",
            Email = "ana@example.com",
            Phone = "6141112233",
            Status = "Nueva",
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-5),
            Shirts =
            [
                new ShirtLine
                {
                    Id = "s1",
                    QuotationId = "COT-098",
                    ColorId = "negro",
                    ColorName = "Negro",
                    ColorHex = "#111111",
                    SizeId = "m",
                    SizeLabel = "M",
                    Quantity = 12,
                    FrontSource = ArtworkSource.Catalog,
                    FrontCatalogId = "cat-cuu-skyline",
                    FrontLabel = "Chihuahua Skyline",
                    FrontPlacement = new ArtPlacement(),
                    BackSource = ArtworkSource.None,
                    BackPlacement = new ArtPlacement()
                },
                new ShirtLine
                {
                    Id = "s2",
                    QuotationId = "COT-098",
                    ColorId = "arena",
                    ColorName = "Arena",
                    ColorHex = "#F3EEE4",
                    SizeId = "l",
                    SizeLabel = "L",
                    Quantity = 8,
                    FrontSource = ArtworkSource.Custom,
                    FrontLabel = "logo-cliente.png",
                    FrontPlacement = new ArtPlacement { Dx = 0.05, Scale = 1.1 },
                    BackSource = ArtworkSource.Catalog,
                    BackCatalogId = "cat-ruta",
                    BackLabel = "Ruta 45",
                    BackPlacement = new ArtPlacement()
                }
            ]
        };

        yield return new Quotation
        {
            Id = "COT-097",
            CustomerName = "Carlos Méndez",
            Email = "carlos@example.com",
            Phone = "6142223344",
            Status = "En revisión",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            Shirts =
            [
                new ShirtLine
                {
                    Id = "s3",
                    QuotationId = "COT-097",
                    ColorId = "azul",
                    ColorName = "Azul",
                    ColorHex = "#1F4B99",
                    SizeId = "xl",
                    SizeLabel = "XL",
                    Quantity = 25,
                    FrontSource = ArtworkSource.Catalog,
                    FrontCatalogId = "cat-barrio",
                    FrontLabel = "Barrio Centro",
                    FrontPlacement = new ArtPlacement(),
                    BackSource = ArtworkSource.None,
                    BackPlacement = new ArtPlacement()
                }
            ]
        };

        yield return new Quotation
        {
            Id = "COT-096",
            CustomerName = "Sofía Delgado",
            Email = "sofia@example.com",
            Phone = "6143334455",
            Status = "Respondida",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
            Shirts =
            [
                new ShirtLine
                {
                    Id = "s4",
                    QuotationId = "COT-096",
                    ColorId = "rojo",
                    ColorName = "Rojo",
                    ColorHex = "#C62828",
                    SizeId = "s",
                    SizeLabel = "S",
                    Quantity = 4,
                    FrontSource = ArtworkSource.Custom,
                    FrontLabel = "evento.pdf",
                    FrontPlacement = new ArtPlacement { Dy = -0.04, Scale = 0.9 },
                    BackSource = ArtworkSource.None,
                    BackPlacement = new ArtPlacement()
                }
            ]
        };
    }
}
