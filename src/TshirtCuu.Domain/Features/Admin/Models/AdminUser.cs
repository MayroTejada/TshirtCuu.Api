namespace TshirtCuu.Domain.Features.Admin.Models;

public sealed class AdminUser
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public string Role { get; init; } = "Admin";
}
