namespace TshirtCuu.Services.Features.Admin;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "tshirtcuu-api";
    public string Audience { get; set; } = "tshirtcuu-admin";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 480;
}
