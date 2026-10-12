using Microsoft.EntityFrameworkCore;
using TshirtCuu.Domain.Features.Quotes.Models;
using TshirtCuu.Domain.Features.Quotes.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Quotes;

public sealed class SqlQuoteRepository(AppDbContext db) : IQuoteRepository
{
    public IReadOnlyList<Quotation> GetAll() =>
        db.Quotations
            .AsNoTracking()
            .Include(q => q.Shirts)
            .OrderByDescending(q => q.CreatedAt)
            .ToList();

    public Quotation? GetById(string id) =>
        db.Quotations
            .Include(q => q.Shirts)
            .FirstOrDefault(q => q.Id == id);

    public Quotation Add(Quotation quotation)
    {
        foreach (var shirt in quotation.Shirts)
            shirt.QuotationId = quotation.Id;

        db.Quotations.Add(quotation);
        db.SaveChanges();
        return GetById(quotation.Id)!;
    }

    public bool UpdateStatus(string id, string status)
    {
        var existing = db.Quotations.FirstOrDefault(q => q.Id == id);
        if (existing is null) return false;
        existing.Status = status;
        db.SaveChanges();
        return true;
    }

    public string NextQuoteId()
    {
        var ids = db.Quotations.AsNoTracking().Select(q => q.Id).ToList();
        var max = 100;
        foreach (var id in ids)
        {
            if (id.StartsWith("COT-", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(id[4..], out var n)
                && n > max)
            {
                max = n;
            }
        }

        return $"COT-{max + 1:D3}";
    }
}
