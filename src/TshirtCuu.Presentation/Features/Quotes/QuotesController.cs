using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TshirtCuu.Domain.Features.Quotes.Models;
using TshirtCuu.Services.Features.Quotes;

namespace TshirtCuu.Presentation.Features.Quotes;

[ApiController]
[Route("api/quotes")]
public sealed class QuotesController(QuoteService quotes) : ControllerBase
{
    /// <summary>Public: submit a new quotation request.</summary>
    [HttpPost]
    [AllowAnonymous]
    public ActionResult<Quotation> Create([FromBody] CreateQuoteRequest request)
    {
        try
        {
            var created = quotes.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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

    /// <summary>Admin: update quotation status (Nueva | En revisión | Respondida).</summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateStatus(string id, [FromBody] UpdateStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest(new { error = "Status is required." });

        try
        {
            return quotes.UpdateStatus(id, request.Status) ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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

public sealed record SaveReplyRequest(string CustomerReply);
