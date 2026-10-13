using TshirtCuu.Domain.Features.Quotes.Models;

namespace TshirtCuu.Services.Features.Email;

public interface ICustomerEmailSender
{
    /// <summary>
    /// Notify the customer that their shirt is ready (Lista / “ya quedó”).
    /// Never throws — returns an outcome the API can surface without failing the status save.
    /// </summary>
    Task<EmailSendOutcome> TrySendListaReadyAsync(
        Quotation quote,
        CancellationToken cancellationToken = default);
}
