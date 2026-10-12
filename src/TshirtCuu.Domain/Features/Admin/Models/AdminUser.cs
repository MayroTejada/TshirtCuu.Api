namespace TshirtCuu.Domain.Features.Admin.Models;

public sealed class AdminUser
{
    public required string Username { get; init; }

    /// <summary>ASP.NET Identity password hash (never store plaintext).</summary>
    public required string PasswordHash { get; set; }

    public string Role { get; set; } = "Admin";
}
