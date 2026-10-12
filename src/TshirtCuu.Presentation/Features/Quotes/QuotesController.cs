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

    /// <summary>Admin: update quotation status.</summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateStatus(string id, [FromBody] UpdateStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest(new { error = "Status is required." });

        return quotes.UpdateStatus(id, request.Status.Trim()) ? NoContent() : NotFound();
    }
}

public sealed record UpdateStatusRequest(string Status);
