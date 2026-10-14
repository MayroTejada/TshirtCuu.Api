namespace TshirtCuu.Domain.Features.Errors.Models;

public sealed class ErrorLogEntry
{
    public long Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string? Path { get; init; }
    public required string Message { get; init; }
    public string? Detail { get; init; }
}
