using TshirtCuu.Domain.Features.Admin.Models;
using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Domain.Features.Quotes.Models;

namespace TshirtCuu.Services.Persistence;

/// <summary>
/// Shared in-memory mock data. SQL Server can replace the repository
/// implementations later without changing Domain or Presentation contracts.
/// </summary>
public sealed class InMemoryStore
{
    private int _quoteSeq = 100;

    public List<CatalogCategory> Categories { get; } = SeedCategories();
    public List<CatalogDesign> Designs { get; } = SeedDesigns();
    public List<ShirtSize> Sizes { get; } = SeedSizes();
    public List<ShirtColor> Colors { get; } = SeedColors();
    public PricingRules Pricing { get; set; } = new();
    public List<Quotation> Quotations { get; } = [];
    public List<AdminUser> Admins { get; } =
    [
        new() { Username = "admin", Password = "admin", Role = "Admin" }
    ];

    public InMemoryStore()
    {
        SeedQuotes();
    }

    public string NextQuoteId()
    {
        _quoteSeq++;
        return $"COT-{_quoteSeq:D3}";
    }

    private void SeedQuotes()
    {
        Quotations.Add(new Quotation
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
        });

        Quotations.Add(new Quotation
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
        });

        Quotations.Add(new Quotation
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
        });
    }

    private static List<CatalogCategory> SeedCategories() =>
    [
        new() { Id = "cat-ciudad", Name = "Ciudad" },
        new() { Id = "cat-ruta", Name = "Ruta" },
        new() { Id = "cat-cultura", Name = "Cultura" },
        new() { Id = "cat-deporte", Name = "Deporte" }
    ];

    private static List<CatalogDesign> SeedDesigns() =>
    [
        new() { Id = "cat-cuu-skyline", Name = "Chihuahua Skyline", CategoryId = "cat-ciudad", Tagline = "Stock · entrega rápida", AccentHex = "#B8F34A" },
        new() { Id = "cat-barrio", Name = "Barrio Centro", CategoryId = "cat-ciudad", Tagline = "Stock · entrega rápida", AccentHex = "#FF6B4A" },
        new() { Id = "cat-noche", Name = "Noche Cuu", CategoryId = "cat-ciudad", Tagline = "Stock · entrega rápida", AccentHex = "#C4B5FD" },
        new() { Id = "cat-ruta", Name = "Ruta 45", CategoryId = "cat-ruta", Tagline = "Stock · entrega rápida", AccentHex = "#6EC6FF" },
        new() { Id = "cat-desierto", Name = "Desierto Norte", CategoryId = "cat-ruta", Tagline = "Stock · entrega rápida", AccentHex = "#F9A825" },
        new() { Id = "cat-cultura", Name = "Cultura Local", CategoryId = "cat-cultura", Tagline = "Stock · entrega rápida", AccentHex = "#FF8A65" },
        new() { Id = "cat-deporte", Name = "Deporte Cuu", CategoryId = "cat-deporte", Tagline = "Stock · entrega rápida", AccentHex = "#66BB6A" }
    ];

    private static List<ShirtSize> SeedSizes() =>
    [
        new() { Id = "xs", Label = "XS" },
        new() { Id = "s", Label = "S" },
        new() { Id = "m", Label = "M" },
        new() { Id = "l", Label = "L" },
        new() { Id = "xl", Label = "XL" },
        new() { Id = "xxl", Label = "XXL" }
    ];

    private static List<ShirtColor> SeedColors() =>
    [
        new() { Id = "arena", Name = "Arena", Hex = "#F3EEE4" },
        new() { Id = "negro", Name = "Negro", Hex = "#111111" },
        new() { Id = "azul", Name = "Azul", Hex = "#1F4B99" },
        new() { Id = "rojo", Name = "Rojo", Hex = "#C62828" },
        new() { Id = "verde", Name = "Verde", Hex = "#2E7D32" },
        new() { Id = "mostaza", Name = "Mostaza", Hex = "#F9A825" },
        new() { Id = "uva", Name = "Uva", Hex = "#6A1B9A" },
        new() { Id = "gris", Name = "Gris", Hex = "#455A64" }
    ];
}
