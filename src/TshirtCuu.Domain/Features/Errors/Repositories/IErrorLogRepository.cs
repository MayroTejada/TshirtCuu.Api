using TshirtCuu.Domain.Features.Errors.Models;

namespace TshirtCuu.Domain.Features.Errors.Repositories;

public interface IErrorLogRepository
{
    /// <summary>Insert one row. Must not throw to callers of the error logger.</summary>
    long Insert(string? path, string message, string? detail, DateTimeOffset? createdAt = null);

    IReadOnlyList<ErrorLogEntry> GetLatest(int take = 100);
}
