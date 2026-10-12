using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TshirtCuu.Services.Persistence;

public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public const string ConnectionName = "TshirtCuu";

    public SqlConnection Create()
    {
        var cs = configuration.GetConnectionString(ConnectionName);
        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                $"Configure ConnectionStrings:{ConnectionName} via environment variable " +
                "ConnectionStrings__TshirtCuu or user-secrets. Do not commit passwords. " +
                "Schema/procs live in https://github.com/MayroTejada/TShirtMigration");
        }

        return new SqlConnection(cs);
    }
}
