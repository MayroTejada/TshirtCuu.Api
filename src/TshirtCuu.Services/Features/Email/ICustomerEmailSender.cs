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

    /// <summary>
    /// Alert the shop mailbox (Email__NotifyTo or Email__From) that a new quote arrived.
    /// Never throws — quote create must succeed even if this fails.
    /// </summary>
    Task<EmailSendOutcome> TrySendNewQuoteAlertAsync(
        Quotation quote,
        CancellationToken cancellationToken = default);
}
