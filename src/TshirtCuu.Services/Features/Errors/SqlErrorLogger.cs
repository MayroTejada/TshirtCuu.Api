using Microsoft.Extensions.Logging;
using TshirtCuu.Domain.Features.Errors.Models;
using TshirtCuu.Domain.Features.Errors.Repositories;

namespace TshirtCuu.Services.Features.Errors;

public sealed class SqlErrorLogger(
    IErrorLogRepository repository,
    ILogger<SqlErrorLogger> logger) : IErrorLogger
{
    public Task LogAsync(
        string? path,
        string message,
        string? detail = null,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
    {
        // Fire-and-forget safe: never throw to the request pipeline.
        try
        {
            var msg = string.IsNullOrWhiteSpace(message) ? "(sin mensaje)" : message.Trim();
            var detailText = detail;
            if (exception is not null)
            {
                var chain = exception.ToString();
                detailText = string.IsNullOrWhiteSpace(detailText)
                    ? chain
                    : $"{detailText}\n\n{chain}";
            }

            if (detailText is { Length: > 100_000 })
                detailText = detailText[..100_000] + "…";

            repository.Insert(path, msg, detailText, DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to persist error log for path {Path}: {Message}",
                path,
                message);
        }

        return Task.CompletedTask;
    }

    public IReadOnlyList<ErrorLogEntry> ListLatest(int take = 100)
    {
        try
        {
            return repository.GetLatest(take);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list error logs.");
            return [];
        }
    }
}
