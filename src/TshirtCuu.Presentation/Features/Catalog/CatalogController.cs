using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Services.Features.Catalog;

namespace TshirtCuu.Presentation.Features.Catalog;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController(CatalogService catalog) : ControllerBase
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
            InkHex = design.InkHex
        });
        return Ok(created);
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
            InkHex = design.InkHex
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
