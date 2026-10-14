namespace TshirtCuu.Services.Features.Errors;

/// <summary>
/// Writes API errors to SQL via stored procedure. Implementations must never throw.
/// </summary>
public interface IErrorLogger
{
    Task LogAsync(
        string? path,
        string message,
        string? detail = null,
        Exception? exception = null,
        CancellationToken cancellationToken = default);

    IReadOnlyList<Domain.Features.Errors.Models.ErrorLogEntry> ListLatest(int take = 100);
}
