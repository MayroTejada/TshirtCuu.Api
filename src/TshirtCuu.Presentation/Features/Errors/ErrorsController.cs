using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TshirtCuu.Domain.Features.Errors.Models;
using TshirtCuu.Services.Features.Errors;

namespace TshirtCuu.Presentation.Features.Errors;

[ApiController]
[Route("api/admin/errors")]
[Authorize(Roles = "Admin")]
public sealed class ErrorsController(IErrorLogger errorLogger) : ControllerBase
{
    /// <summary>Admin: latest API error log rows (newest first).</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<ErrorLogEntry>> List([FromQuery] int take = 100)
    {
        if (take < 1) take = 100;
        if (take > 500) take = 500;
        return Ok(errorLogger.ListLatest(take));
    }
}
