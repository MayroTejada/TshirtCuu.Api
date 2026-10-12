using Microsoft.AspNetCore.Identity;
using TshirtCuu.Domain.Features.Admin.Models;
using TshirtCuu.Domain.Features.Admin.Repositories;

namespace TshirtCuu.Services.Features.Admin;

public sealed class AdminAuthService(
    IAdminUserRepository users,
    JwtTokenService tokens,
    IPasswordHasher<AdminUser> passwordHasher)
{
    public LoginResult? Login(string username, string password)
    {
        var user = users.FindByUsername(username);
        if (user is null) return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result is PasswordVerificationResult.Failed)
            return null;

        var token = tokens.CreateToken(user);
        return new LoginResult(token, user.Username, user.Role);
    }
}

public sealed record LoginResult(string AccessToken, string Username, string Role);
