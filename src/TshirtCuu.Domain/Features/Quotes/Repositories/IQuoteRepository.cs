using TshirtCuu.Domain.Features.Quotes.Models;

namespace TshirtCuu.Domain.Features.Quotes.Repositories;

public interface IQuoteRepository
{
    IReadOnlyList<Quotation> GetAll();
    Quotation? GetById(string id);
    Quotation Add(Quotation quotation);
    bool UpdateStatus(string id, string status);
    Quotation? SaveReply(string id, string customerReply);
    string NextQuoteId();
}
