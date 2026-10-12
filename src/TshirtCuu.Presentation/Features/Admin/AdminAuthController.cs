using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TshirtCuu.Services.Features.Admin;

namespace TshirtCuu.Presentation.Features.Admin;

[ApiController]
[Route("api/admin/auth")]
public sealed class AdminAuthController(AdminAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Username and password are required." });

        var result = auth.Login(request.Username.Trim(), request.Password);
        if (result is null)
            return Unauthorized(new { error = "Invalid credentials." });

        return Ok(new LoginResponse(result.AccessToken, result.Username, result.Role, "Bearer"));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Admin")]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name,
            role = User.IsInRole("Admin") ? "Admin" : null
        });
    }
}

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(string AccessToken, string Username, string Role, string TokenType);
