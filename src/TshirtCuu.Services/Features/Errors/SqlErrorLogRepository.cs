using System.Data;
using Dapper;
using TshirtCuu.Domain.Features.Errors.Models;
using TshirtCuu.Domain.Features.Errors.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Errors;

public sealed class SqlErrorLogRepository(ISqlConnectionFactory connections) : IErrorLogRepository
{
    public long Insert(string? path, string message, string? detail, DateTimeOffset? createdAt = null)
    {
        using var db = connections.Create();
        return db.QuerySingle<long>(
            "dbo.usp_ErrorLogs_Insert",
            new
            {
                Path = path,
                Message = message,
                Detail = detail,
                CreatedAt = createdAt
            },
            commandType: CommandType.StoredProcedure);
    }

    public IReadOnlyList<ErrorLogEntry> GetLatest(int take = 100)
    {
        using var db = connections.Create();
        return db.Query<ErrorLogEntry>(
            "dbo.usp_ErrorLogs_GetLatest",
            new { Take = take },
            commandType: CommandType.StoredProcedure).AsList();
    }
}
