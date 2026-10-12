using TshirtCuu.Domain.Features.Quotes.Models;
using TshirtCuu.Domain.Features.Quotes.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Quotes;

public sealed class InMemoryQuoteRepository(InMemoryStore store) : IQuoteRepository
{
    public IReadOnlyList<Quotation> GetAll() =>
        store.Quotations.OrderByDescending(q => q.CreatedAt).ToList();

    public Quotation? GetById(string id) =>
        store.Quotations.FirstOrDefault(q => q.Id == id);

    public Quotation Add(Quotation quotation)
    {
        store.Quotations.Insert(0, quotation);
        return quotation;
    }

    public bool UpdateStatus(string id, string status)
    {
        var existing = GetById(id);
        if (existing is null) return false;
        existing.Status = status;
        return true;
    }
}
