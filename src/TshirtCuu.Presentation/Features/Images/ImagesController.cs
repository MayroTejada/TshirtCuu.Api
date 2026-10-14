using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TshirtCuu.Services.Features.Storage;

namespace TshirtCuu.Presentation.Features.Images;

[ApiController]
[Route("api/images")]
public sealed class ImagesController(ImageStorageService images) : ControllerBase
{
    /// <summary>
    /// Admin catalog image upload into the private <c>imagenes</c> container.
    /// Customer artwork is not accepted here — it is stored only when POST /api/quotes succeeds.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile? file,
        [FromQuery] string kind = "catalog",
        CancellationToken cancellationToken = default)
    {
        if (!images.IsConfigured)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error =
                    "Azure Storage is not configured. Set App Setting AzureStorage__ConnectionString. " +
                    "The API is running; image upload is unavailable until that setting is set."
            });
        }

        var normalized = (kind ?? "catalog").Trim().ToLowerInvariant();
        if (normalized != "catalog")
        {
            return BadRequest(new
            {
                error =
                    "Only kind=catalog is accepted on this endpoint. " +
                    "Customer artwork is uploaded when POST /api/quotes succeeds."
            });
        }

        if (file is null || file.Length == 0)
            return BadRequest(new { error = "file is required." });

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only image/* uploads are accepted." });

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await images
                .UploadCatalogAsync(stream, file.ContentType, file.FileName, cancellationToken)
                .ConfigureAwait(false);

            return Ok(new
            {
                blobPath = result.BlobPath,
                contentType = result.ContentType
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Stream a private blob through the API (no public Azure URL).
    /// Path example: <c>catalog/{guid}.png</c>.
    /// </summary>
    [HttpGet("{*blobPath}")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        if (!images.IsConfigured)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error =
                    "Azure Storage is not configured. Set App Setting AzureStorage__ConnectionString."
            });
        }

        if (string.IsNullOrWhiteSpace(blobPath))
            return BadRequest(new { error = "blobPath is required." });

        try
        {
            var result = await images.OpenReadAsync(blobPath, cancellationToken).ConfigureAwait(false);
            if (result is null) return NotFound(new { error = "Image not found." });

            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }
    }
}
