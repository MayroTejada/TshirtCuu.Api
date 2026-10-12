using System.Data;
using Dapper;
using TshirtCuu.Domain.Features.Admin.Models;
using TshirtCuu.Domain.Features.Admin.Repositories;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services.Features.Admin;

public sealed class SqlAdminUserRepository(ISqlConnectionFactory connections) : IAdminUserRepository
{
    public AdminUser? FindByUsername(string username)
    {
        using var db = connections.Create();
        return db.QuerySingleOrDefault<AdminUser>(
            "dbo.usp_AdminUsers_GetByUsername",
            new { Username = username },
            commandType: CommandType.StoredProcedure);
    }
}
