using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TshirtCuu.Domain.Features.Quotes.Models;
using TshirtCuu.Services.Features.Quotes;

namespace TshirtCuu.Presentation.Features.Quotes;

[ApiController]
[Route("api/quotes")]
public sealed class QuotesController(QuoteService quotes) : ControllerBase
{
    /// <summary>
    /// Public: submit a new quotation request.
    /// Custom artwork is stored in private Azure blobs only after this create succeeds
    /// (base64 on the body and/or follow-up multipart POST …/artwork).
    /// Also attempts a MailKit alert to Email__NotifyTo (or Email__From); alert failures do not fail the create.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [RequestSizeLimit(40_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 40_000_000)]
    public async Task<ActionResult<Quotation>> Create(
        [FromBody] CreateQuoteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await quotes.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Public: attach customer “Subir propio” artwork after the quote was created.
    /// Uploads to private container <c>imagenes</c> and saves the blob path on the shirt line.
    /// </summary>
    [HttpPost("{quoteId}/shirts/{shirtId}/artwork")]
    [AllowAnonymous]
    [RequestSizeLimit(20_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 20_000_000)]
    public async Task<IActionResult> UploadShirtArtwork(
        string quoteId,
        string shirtId,
        [FromQuery] string side,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "file is required." });

        await using var stream = file.OpenReadStream();
        var result = await quotes
            .AttachShirtArtworkAsync(
                quoteId,
                shirtId,
                side,
                stream,
                file.ContentType,
                file.FileName,
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.Found) return NotFound(new { error = result.Error });
        if (result.StatusCode == 400) return BadRequest(new { error = result.Error });
        if (result.StatusCode >= 500)
            return StatusCode(result.StatusCode, new { error = result.Error });
        if (!result.Stored)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(new
        {
            blobPath = result.BlobPath,
            side = result.Side,
            contentType = file.ContentType
        });
    }

    /// <summary>Admin: list all quotations.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult<IReadOnlyList<Quotation>> List() => Ok(quotes.List());

    /// <summary>Admin: get one quotation.</summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public ActionResult<Quotation> GetById(string id)
    {
        var quote = quotes.Get(id);
        return quote is null ? NotFound() : Ok(quote);
    }

    /// <summary>
    /// Admin: update quotation status (Nueva | En revisión | Respondida | Lista | Finalizada | Cancelada).
    /// When status is Lista, attempts Zoho SMTP “ya quedó” email from Email__* settings
    /// (same send path as POST …/lista-email). Missing SMTP or send errors do not fail the status save.
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest(new { error = "Status is required." });

        try
        {
            var result = await quotes.UpdateStatusAsync(id, request.Status, cancellationToken);
            if (!result.Found)
                return NotFound();

            return Ok(new UpdateStatusResponse(
                result.Status!,
                result.EmailSent,
                result.EmailNote,
                result.To));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Admin: resend the Lista / “ya quedó” email to the quote’s customer address
    /// via the same Zoho SMTP path as Marcar lista. Does not change status.
    /// Returns destination + real send result (never fakes success).
    /// </summary>
    [HttpPost("{id}/lista-email")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResendListaEmail(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await quotes.ResendListaEmailAsync(id, cancellationToken);
        if (!result.Found)
            return NotFound();

        return Ok(new ListaEmailResponse(
            result.EmailSent,
            result.EmailNote,
            result.To));
    }

    /// <summary>
    /// Admin: save customer reply text on the quote (no email).
    /// Sets status to Respondida and records RepliedAt.
    /// </summary>
    [HttpPut("{id}/reply")]
    [Authorize(Roles = "Admin")]
    public ActionResult<Quotation> SaveReply(string id, [FromBody] SaveReplyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerReply))
            return BadRequest(new { error = "CustomerReply is required." });

        try
        {
            var updated = quotes.SaveReply(id, request.CustomerReply);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public sealed record UpdateStatusRequest(string Status);

public sealed record UpdateStatusResponse(
    string Status,
    bool EmailSent,
    string? EmailNote,
    string? To);

public sealed record ListaEmailResponse(bool EmailSent, string? EmailNote, string? To);

public sealed record SaveReplyRequest(string CustomerReply);
