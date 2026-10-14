using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Services.Features.Catalog;
using TshirtCuu.Services.Features.Storage;

namespace TshirtCuu.Presentation.Features.Catalog;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController(
    CatalogService catalog,
    ImageStorageService images) : ControllerBase
{
    [HttpGet("categories")]
    [AllowAnonymous]
    public ActionResult<IReadOnlyList<CatalogCategory>> Categories() =>
        Ok(catalog.ListCategories());

    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public ActionResult<CatalogCategory> CreateCategory([FromBody] NameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required." });
        return Ok(catalog.CreateCategory(request.Name));
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult RenameCategory(string id, [FromBody] NameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required." });
        return catalog.RenameCategory(id, request.Name) ? NoContent() : NotFound();
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteCategory(string id) =>
        catalog.RemoveCategory(id) ? NoContent() : NotFound();

    [HttpGet("designs")]
    [AllowAnonymous]
    public ActionResult<IReadOnlyList<CatalogDesign>> Designs(
        [FromQuery] string? categoryId,
        [FromQuery] string? q) =>
        Ok(catalog.ListDesigns(categoryId, q));

    [HttpGet("designs/{id}")]
    [AllowAnonymous]
    public ActionResult<CatalogDesign> Design(string id)
    {
        var design = catalog.GetDesign(id);
        return design is null ? NotFound() : Ok(design);
    }

    [HttpPost("designs")]
    [Authorize(Roles = "Admin")]
    public ActionResult<CatalogDesign> CreateDesign([FromBody] CatalogDesign design)
    {
        if (string.IsNullOrWhiteSpace(design.Name) || string.IsNullOrWhiteSpace(design.CategoryId))
            return BadRequest(new { error = "Name and CategoryId are required." });

        var created = catalog.CreateDesign(new CatalogDesign
        {
            Id = string.IsNullOrWhiteSpace(design.Id) ? $"des-{Guid.NewGuid():N}"[..12] : design.Id,
            Name = design.Name,
            CategoryId = design.CategoryId,
            Tagline = design.Tagline ?? "Stock",
            AccentHex = design.AccentHex,
            InkHex = design.InkHex,
            ImageBlobPath = design.ImageBlobPath
        });
        return Ok(created);
    }

    /// <summary>
    /// Admin catalog upload: stores the image in private Azure blob storage, then inserts the design.
    /// </summary>
    [HttpPost("designs/upload")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<CatalogDesign>> UploadDesign(
        [FromForm] string name,
        [FromForm] string categoryId,
        [FromForm] IFormFile? file,
        [FromForm] string? tagline,
        [FromForm] string? accentHex,
        [FromForm] string? inkHex,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(categoryId))
            return BadRequest(new { error = "Name and CategoryId are required." });
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "file is required." });
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only image/* uploads are accepted." });

        if (!images.IsConfigured)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error =
                    "Azure Storage is not configured. Set App Setting AzureStorage__ConnectionString. " +
                    "The API is running; catalog image upload is unavailable until that setting is set."
            });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var uploaded = await images
                .UploadCatalogAsync(stream, file.ContentType, file.FileName, cancellationToken)
                .ConfigureAwait(false);

            var created = catalog.CreateDesign(new CatalogDesign
            {
                Id = $"des-{Guid.NewGuid():N}"[..12],
                Name = name.Trim(),
                CategoryId = categoryId.Trim(),
                Tagline = string.IsNullOrWhiteSpace(tagline) ? "Stock · subido por admin" : tagline.Trim(),
                AccentHex = string.IsNullOrWhiteSpace(accentHex) ? "#B8F34A" : accentHex.Trim(),
                InkHex = string.IsNullOrWhiteSpace(inkHex) ? "#0E1116" : inkHex.Trim(),
                ImageBlobPath = uploaded.BlobPath
            });
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }
    }

    [HttpPut("designs/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateDesign(string id, [FromBody] CatalogDesign design)
    {
        var updated = new CatalogDesign
        {
            Id = id,
            Name = design.Name,
            CategoryId = design.CategoryId,
            Tagline = design.Tagline,
            AccentHex = design.AccentHex,
            InkHex = design.InkHex,
            ImageBlobPath = design.ImageBlobPath
        };
        return catalog.UpdateDesign(updated) ? NoContent() : NotFound();
    }

    [HttpDelete("designs/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteDesign(string id) =>
        catalog.RemoveDesign(id) ? NoContent() : NotFound();

    [HttpGet("sizes")]
    [AllowAnonymous]
    public ActionResult<IReadOnlyList<ShirtSize>> Sizes() => Ok(catalog.ListSizes());

    [HttpPost("sizes")]
    [Authorize(Roles = "Admin")]
    public ActionResult<ShirtSize> CreateSize([FromBody] NameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name (label) is required." });
        return Ok(catalog.CreateSize(request.Name));
    }

    [HttpPut("sizes/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateSize(string id, [FromBody] NameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name (label) is required." });
        return catalog.UpdateSize(new ShirtSize { Id = id, Label = request.Name.Trim() })
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("sizes/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteSize(string id) =>
        catalog.RemoveSize(id) ? NoContent() : NotFound();

    [HttpGet("colors")]
    [AllowAnonymous]
    public ActionResult<IReadOnlyList<ShirtColor>> Colors() => Ok(catalog.ListColors());

    [HttpPost("colors")]
    [Authorize(Roles = "Admin")]
    public ActionResult<ShirtColor> CreateColor([FromBody] ColorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Hex))
            return BadRequest(new { error = "Name and Hex are required." });
        return Ok(catalog.CreateColor(request.Name, request.Hex));
    }

    [HttpPut("colors/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateColor(string id, [FromBody] ColorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Hex))
            return BadRequest(new { error = "Name and Hex are required." });
        return catalog.UpdateColor(new ShirtColor
        {
            Id = id,
            Name = request.Name.Trim(),
            Hex = request.Hex.Trim()
        })
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("colors/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteColor(string id) =>
        catalog.RemoveColor(id) ? NoContent() : NotFound();

    [HttpGet("pricing")]
    [AllowAnonymous]
    public ActionResult<PricingRules> Pricing() => Ok(catalog.GetPricing());

    [HttpPut("pricing")]
    [Authorize(Roles = "Admin")]
    public ActionResult<PricingRules> UpdatePricing([FromBody] PricingRules rules) =>
        Ok(catalog.UpdatePricing(rules));
}

public sealed record NameRequest(string Name);

public sealed record ColorRequest(string Name, string Hex);
