using System.Data;
using Dapper;
using TshirtCuu.Domain.Features.Quotes.Models;
using TshirtCuu.Domain.Features.Quotes.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Quotes;

public sealed class SqlQuoteRepository(ISqlConnectionFactory connections) : IQuoteRepository
{
    public IReadOnlyList<Quotation> GetAll()
    {
        using var db = connections.Create();
        using var multi = db.QueryMultiple(
            "dbo.usp_Quotes_GetAll",
            commandType: CommandType.StoredProcedure);
        var quotes = multi.Read<Quotation>().AsList();
        var shirts = multi.Read<ShirtLineRow>().AsList();
        AttachShirts(quotes, shirts);
        return quotes;
    }

    public Quotation? GetById(string id)
    {
        using var db = connections.Create();
        using var multi = db.QueryMultiple(
            "dbo.usp_Quotes_GetById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
        var quote = multi.ReadSingleOrDefault<Quotation>();
        if (quote is null) return null;
        var shirts = multi.Read<ShirtLineRow>().AsList();
        AttachShirts([quote], shirts);
        return quote;
    }

    public Quotation Add(Quotation quotation)
    {
        using var db = connections.Create();
        db.Open();
        using var tx = db.BeginTransaction();

        db.Execute(
            "dbo.usp_Quotes_Insert",
            new
            {
                quotation.Id,
                quotation.CustomerName,
                quotation.Email,
                quotation.Phone,
                quotation.Status,
                quotation.CreatedAt
            },
            tx,
            commandType: CommandType.StoredProcedure);

        foreach (var s in quotation.Shirts)
        {
            s.QuotationId = quotation.Id;
            db.Execute(
                "dbo.usp_ShirtLines_Insert",
                new
                {
                    s.Id,
                    s.QuotationId,
                    s.ColorId,
                    s.ColorName,
                    s.ColorHex,
                    s.SizeId,
                    s.SizeLabel,
                    s.Quantity,
                    s.Fit,
                    FrontSource = s.FrontSource.ToString(),
                    s.FrontLabel,
                    s.FrontCatalogId,
                    FrontDx = s.FrontPlacement.Dx,
                    FrontDy = s.FrontPlacement.Dy,
                    FrontScale = s.FrontPlacement.ResolvedScaleX,
                    FrontScaleY = s.FrontPlacement.ResolvedScaleY,
                    BackSource = s.BackSource.ToString(),
                    s.BackLabel,
                    s.BackCatalogId,
                    BackDx = s.BackPlacement.Dx,
                    BackDy = s.BackPlacement.Dy,
                    BackScale = s.BackPlacement.ResolvedScaleX,
                    BackScaleY = s.BackPlacement.ResolvedScaleY
                },
                tx,
                commandType: CommandType.StoredProcedure);
        }

        tx.Commit();
        return GetById(quotation.Id)!;
    }

    public bool UpdateStatus(string id, string status)
    {
        using var db = connections.Create();
        var rows = db.QuerySingle<int>(
            "dbo.usp_Quotes_UpdateStatus",
            new { Id = id, Status = status },
            commandType: CommandType.StoredProcedure);
        return rows > 0;
    }

    public Quotation? SaveReply(string id, string customerReply)
    {
        using var db = connections.Create();
        using var multi = db.QueryMultiple(
            "dbo.usp_Quotes_SaveReply",
            new { Id = id, CustomerReply = customerReply },
            commandType: CommandType.StoredProcedure);
        var rows = multi.ReadSingle<int>();
        if (rows <= 0) return null;
        var quote = multi.ReadSingleOrDefault<Quotation>();
        if (quote is null) return null;
        // Reload with shirts for a complete response.
        return GetById(id);
    }

    public string NextQuoteId()
    {
        using var db = connections.Create();
        return db.QuerySingle<string>(
            "dbo.usp_Quotes_NextId",
            commandType: CommandType.StoredProcedure);
    }

    private static void AttachShirts(IList<Quotation> quotes, IList<ShirtLineRow> rows)
    {
        var byQuote = rows.GroupBy(r => r.QuotationId).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var q in quotes)
        {
            if (!byQuote.TryGetValue(q.Id, out var list))
            {
                q.Shirts = [];
                continue;
            }

            q.Shirts = list.Select(r => r.ToShirtLine()).ToList();
        }
    }

    private sealed class ShirtLineRow
    {
        public string Id { get; init; } = "";
        public string QuotationId { get; init; } = "";
        public string ColorId { get; init; } = "";
        public string ColorName { get; init; } = "";
        public string ColorHex { get; init; } = "";
        public string SizeId { get; init; } = "";
        public string SizeLabel { get; init; } = "";
        public int Quantity { get; init; }
        public string Fit { get; init; } = "Caballero";
        public string FrontSource { get; init; } = "None";
        public string? FrontLabel { get; init; }
        public string? FrontCatalogId { get; init; }
        public double FrontDx { get; init; }
        public double FrontDy { get; init; }
        public double FrontScale { get; init; } = 1;
        public double? FrontScaleY { get; init; }
        public string BackSource { get; init; } = "None";
        public string? BackLabel { get; init; }
        public string? BackCatalogId { get; init; }
        public double BackDx { get; init; }
        public double BackDy { get; init; }
        public double BackScale { get; init; } = 1;
        public double? BackScaleY { get; init; }

        public ShirtLine ToShirtLine() => new()
        {
            Id = Id,
            QuotationId = QuotationId,
            ColorId = ColorId,
            ColorName = ColorName,
            ColorHex = ColorHex,
            SizeId = SizeId,
            SizeLabel = SizeLabel,
            Quantity = Quantity,
            Fit = string.IsNullOrWhiteSpace(Fit) ? "Caballero" : Fit,
            FrontSource = Enum.TryParse<ArtworkSource>(FrontSource, true, out var fs) ? fs : ArtworkSource.None,
            FrontLabel = FrontLabel,
            FrontCatalogId = FrontCatalogId,
            FrontPlacement = new ArtPlacement
            {
                Dx = FrontDx,
                Dy = FrontDy,
                Scale = FrontScale,
                ScaleX = FrontScale,
                ScaleY = FrontScaleY ?? FrontScale
            },
            BackSource = Enum.TryParse<ArtworkSource>(BackSource, true, out var bs) ? bs : ArtworkSource.None,
            BackLabel = BackLabel,
            BackCatalogId = BackCatalogId,
            BackPlacement = new ArtPlacement
            {
                Dx = BackDx,
                Dy = BackDy,
                Scale = BackScale,
                ScaleX = BackScale,
                ScaleY = BackScaleY ?? BackScale
            }
        };
    }
}
