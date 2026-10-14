using Microsoft.Extensions.Logging;
using TshirtCuu.Domain.Features.Catalog.Repositories;
using TshirtCuu.Domain.Features.Quotes.Models;
using TshirtCuu.Domain.Features.Quotes.Repositories;
using TshirtCuu.Services.Features.Email;
using TshirtCuu.Services.Features.Storage;

namespace TshirtCuu.Services.Features.Quotes;

public sealed class QuoteService(
    IQuoteRepository repository,
    ICatalogRepository catalog,
    ICustomerEmailSender emailSender,
    ImageStorageService images,
    ILogger<QuoteService> logger)
{
    public IReadOnlyList<Quotation> List() => repository.GetAll();

    public Quotation? Get(string id) => repository.GetById(id);

    /// <summary>
    /// Persists the quote first. Customer artwork is uploaded to private Azure storage
    /// only after the quote row is saved — leaving without submit stores nothing.
    /// Then emails the shop alert and a customer receipt; email failures never fail create.
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
        // Blob paths are not accepted from the client on create — artwork is uploaded
        // only after this insert succeeds.
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

        await AttachCustomerArtworkAsync(saved, request.Shirts, cancellationToken)
            .ConfigureAwait(false);

        // Reload so response includes blob paths when uploads succeeded.
        saved = repository.GetById(saved.Id) ?? saved;

        int? estimatedTotal = null;
        try
        {
            estimatedTotal = QuotePricing.TryEstimateTotalMxn(catalog.GetPricingRules(), saved);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Could not load pricing for customer receipt on {QuoteId}; sending without total.",
                saved.Id);
        }

        // Emails must not undo a successful save. Outcomes are only for logs.
        await LogEmailOutcomeAsync(
                "New-quote alert",
                saved.Id,
                () => emailSender.TrySendNewQuoteAlertAsync(saved, cancellationToken))
            .ConfigureAwait(false);

        await LogEmailOutcomeAsync(
                "Customer quote receipt",
                saved.Id,
                () => emailSender.TrySendCustomerQuoteReceivedAsync(
                    saved,
                    estimatedTotal,
                    cancellationToken))
            .ConfigureAwait(false);

        return saved;
    }

    /// <summary>
    /// Upload custom front/back images only after the quote exists. Missing Azure Storage
    /// does not fail the quote — paths stay null.
    /// </summary>
    private async Task AttachCustomerArtworkAsync(
        Quotation saved,
        IReadOnlyList<CreateShirtLineRequest> requests,
        CancellationToken cancellationToken)
    {
        if (!images.IsConfigured)
        {
            logger.LogInformation(
                "Skipping customer artwork upload for {QuoteId}: AzureStorage__ConnectionString not set.",
                saved.Id);
            return;
        }

        var byId = requests
            .Where(r => !string.IsNullOrWhiteSpace(r.Id))
            .GroupBy(r => r.Id!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        for (var i = 0; i < saved.Shirts.Count; i++)
        {
            var shirt = saved.Shirts[i];
            CreateShirtLineRequest? req = null;
            if (byId.TryGetValue(shirt.Id, out var matched))
                req = matched;
            else if (i < requests.Count)
                req = requests[i];
            if (req is null) continue;

            string? frontPath = null;
            string? backPath = null;

            if (shirt.FrontSource == ArtworkSource.Custom &&
                !string.IsNullOrWhiteSpace(req.FrontImageBase64))
            {
                frontPath = await TryUploadArtworkAsync(
                        saved.Id,
                        shirt.Id,
                        "front",
                        req.FrontImageBase64,
                        req.FrontImageContentType,
                        req.FrontImageFileName ?? req.FrontLabel,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            if (shirt.BackSource == ArtworkSource.Custom &&
                !string.IsNullOrWhiteSpace(req.BackImageBase64))
            {
                backPath = await TryUploadArtworkAsync(
                        saved.Id,
                        shirt.Id,
                        "back",
                        req.BackImageBase64,
                        req.BackImageContentType,
                        req.BackImageFileName ?? req.BackLabel,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            if (frontPath is null && backPath is null) continue;

            repository.UpdateShirtImagePaths(saved.Id, shirt.Id, frontPath, backPath);
            shirt.FrontImageBlobPath = frontPath;
            shirt.BackImageBlobPath = backPath;
        }
    }

    private async Task<string?> TryUploadArtworkAsync(
        string quoteId,
        string shirtId,
        string side,
        string base64,
        string? contentType,
        string? fileName,
        CancellationToken cancellationToken)
    {
        try
        {
            var uploaded = await images
                .UploadArtworkBase64Async(base64, contentType, fileName, cancellationToken)
                .ConfigureAwait(false);
            logger.LogInformation(
                "Stored customer {Side} artwork for {QuoteId}/{ShirtId} at {BlobPath}.",
                side,
                quoteId,
                shirtId,
                uploaded.BlobPath);
            return uploaded.BlobPath;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Customer {Side} artwork upload failed for {QuoteId}/{ShirtId}; quote remains saved.",
                side,
                quoteId,
                shirtId);
            return null;
        }
    }

    private async Task LogEmailOutcomeAsync(
        string label,
        string quoteId,
        Func<Task<EmailSendOutcome>> send)
    {
        try
        {
            var outcome = await send().ConfigureAwait(false);
            if (outcome.Sent)
            {
                logger.LogInformation(
                    "{Label} for {QuoteId}: sent. {Note}",
                    label,
                    quoteId,
                    outcome.Note);
            }
            else
            {
                logger.LogWarning(
                    "{Label} for {QuoteId}: not sent. {Note}",
                    label,
                    quoteId,
                    outcome.Note);
            }
        }
        catch (Exception ex)
        {
            // Sender should not throw; belt-and-suspenders so create still returns 201.
            logger.LogError(
                ex,
                "{Label} for {QuoteId} threw unexpectedly; quote was saved.",
                label,
                quoteId);
        }
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
        // Never trust client blob paths on create — upload happens after insert.
        FrontImageBlobPath = null,
        FrontPlacement = s.FrontPlacement ?? new ArtPlacement(),
        BackSource = s.BackSource,
        BackLabel = s.BackLabel,
        BackCatalogId = s.BackCatalogId,
        BackImageBlobPath = null,
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
    /// <summary>Custom front image (base64). Uploaded only after the quote is saved.</summary>
    public string? FrontImageBase64 { get; init; }
    public string? FrontImageContentType { get; init; }
    public string? FrontImageFileName { get; init; }
    public ArtworkSource BackSource { get; init; } = ArtworkSource.None;
    public string? BackLabel { get; init; }
    public string? BackCatalogId { get; init; }
    public ArtPlacement? BackPlacement { get; init; }
    /// <summary>Custom back image (base64). Uploaded only after the quote is saved.</summary>
    public string? BackImageBase64 { get; init; }
    public string? BackImageContentType { get; init; }
    public string? BackImageFileName { get; init; }
}
