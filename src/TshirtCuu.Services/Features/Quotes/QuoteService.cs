using Microsoft.Extensions.Logging;
using TshirtCuu.Domain.Features.Quotes.Models;
using TshirtCuu.Domain.Features.Quotes.Repositories;
using TshirtCuu.Services.Features.Email;

namespace TshirtCuu.Services.Features.Quotes;

public sealed class QuoteService(
    IQuoteRepository repository,
    ICustomerEmailSender emailSender,
    ILogger<QuoteService> logger)
{
    public IReadOnlyList<Quotation> List() => repository.GetAll();

    public Quotation? Get(string id) => repository.GetById(id);

    /// <summary>
    /// Persists the quote first. Then attempts a shop-inbox alert email;
    /// alert failures are logged and never fail the create.
    /// </summary>
    public async Task<Quotation> CreateAsync(
        CreateQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ArgumentException("CustomerName is required.");
        if (request.Shirts is null || request.Shirts.Count == 0)
            throw new ArgumentException("At least one shirt is required.");

        foreach (var shirt in request.Shirts)
        {
            var fit = shirt.Fit?.Trim() ?? "";
            if (!ShirtLine.AllowedFits.Contains(fit, StringComparer.Ordinal))
                throw new ArgumentException("Each shirt Fit must be Dama or Caballero.");
        }

        var id = repository.NextQuoteId();
        var quotation = new Quotation
        {
            Id = id,
            CustomerName = request.CustomerName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Status = "Nueva",
            CreatedAt = DateTimeOffset.UtcNow,
            Shirts = request.Shirts.Select(s => MapShirt(s, id)).ToList()
        };

        var saved = repository.Add(quotation);

        // Alert must not undo a successful save. Outcome is only for logs.
        try
        {
            var outcome = await emailSender
                .TrySendNewQuoteAlertAsync(saved, cancellationToken)
                .ConfigureAwait(false);

            if (outcome.Sent)
            {
                logger.LogInformation(
                    "New-quote alert for {QuoteId}: sent. {Note}",
                    saved.Id,
                    outcome.Note);
            }
            else
            {
                logger.LogWarning(
                    "New-quote alert for {QuoteId}: not sent. {Note}",
                    saved.Id,
                    outcome.Note);
            }
        }
        catch (Exception ex)
        {
            // Sender should not throw; belt-and-suspenders so create still returns 201.
            logger.LogError(
                ex,
                "New-quote alert for {QuoteId} threw unexpectedly; quote was saved.",
                saved.Id);
        }

        return saved;
    }

    public async Task<UpdateStatusResult> UpdateStatusAsync(
        string id,
        string status,
        CancellationToken cancellationToken = default)
    {
        var normalized = status.Trim();
        if (!Quotation.AllowedStatuses.Contains(normalized, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "Status must be one of: Nueva, En revisión, Respondida, Lista, Finalizada, Cancelada.");
        }

        if (!repository.UpdateStatus(id, normalized))
            return UpdateStatusResult.NotFound();

        if (!string.Equals(normalized, "Lista", StringComparison.Ordinal))
            return UpdateStatusResult.Saved(normalized);

        var quote = repository.GetById(id);
        if (quote is null)
        {
            // Status row was updated; email cannot run without the quote payload.
            return UpdateStatusResult.Saved(
                normalized,
                emailSent: false,
                emailNote:
                    "Correo no enviado: no se pudo leer la cotización tras guardar Lista. " +
                    "El estado Lista sí se guardó.",
                to: null);
        }

        var to = string.IsNullOrWhiteSpace(quote.Email) ? null : quote.Email.Trim();
        var outcome = await emailSender
            .TrySendListaReadyAsync(quote, cancellationToken)
            .ConfigureAwait(false);

        var note = outcome.Note;
        if (!outcome.Sent)
            note = $"{outcome.Note} El estado Lista sí se guardó.";

        return UpdateStatusResult.Saved(normalized, outcome.Sent, note, to);
    }

    /// <summary>
    /// Resend the same Lista / “ya quedó” email to the quote’s customer address
    /// (same Zoho SMTP path as Marcar lista). Does not change status.
    /// </summary>
    public async Task<ListaEmailResult> ResendListaEmailAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var quote = repository.GetById(id);
        if (quote is null)
            return ListaEmailResult.NotFound();

        var to = string.IsNullOrWhiteSpace(quote.Email) ? null : quote.Email.Trim();
        var outcome = await emailSender
            .TrySendListaReadyAsync(quote, cancellationToken)
            .ConfigureAwait(false);

        return ListaEmailResult.FromOutcome(outcome.Sent, outcome.Note, to);
    }

    public Quotation? SaveReply(string id, string customerReply)
    {
        if (string.IsNullOrWhiteSpace(customerReply))
            throw new ArgumentException("CustomerReply is required.");

        return repository.SaveReply(id, customerReply.Trim());
    }

    private static ShirtLine MapShirt(CreateShirtLineRequest s, string quotationId) => new()
    {
        Id = string.IsNullOrWhiteSpace(s.Id) ? Guid.NewGuid().ToString("N")[..8] : s.Id,
        QuotationId = quotationId,
        ColorId = s.ColorId,
        ColorName = s.ColorName,
        ColorHex = s.ColorHex,
        SizeId = s.SizeId,
        SizeLabel = s.SizeLabel,
        Quantity = Math.Max(1, s.Quantity),
        Fit = s.Fit.Trim(),
        FrontSource = s.FrontSource,
        FrontLabel = s.FrontLabel,
        FrontCatalogId = s.FrontCatalogId,
        FrontPlacement = s.FrontPlacement ?? new ArtPlacement(),
        BackSource = s.BackSource,
        BackLabel = s.BackLabel,
        BackCatalogId = s.BackCatalogId,
        BackPlacement = s.BackPlacement ?? new ArtPlacement()
    };
}

public sealed class CreateQuoteRequest
{
    public required string CustomerName { get; init; }
    public required string Email { get; init; }
    public required string Phone { get; init; }
    public required List<CreateShirtLineRequest> Shirts { get; init; }
}

public sealed class CreateShirtLineRequest
{
    public string? Id { get; init; }
    public required string ColorId { get; init; }
    public required string ColorName { get; init; }
    public required string ColorHex { get; init; }
    public required string SizeId { get; init; }
    public required string SizeLabel { get; init; }
    public int Quantity { get; init; } = 1;
    public required string Fit { get; init; }
    public ArtworkSource FrontSource { get; init; } = ArtworkSource.None;
    public string? FrontLabel { get; init; }
    public string? FrontCatalogId { get; init; }
    public ArtPlacement? FrontPlacement { get; init; }
    public ArtworkSource BackSource { get; init; } = ArtworkSource.None;
    public string? BackLabel { get; init; }
    public string? BackCatalogId { get; init; }
    public ArtPlacement? BackPlacement { get; init; }
}
