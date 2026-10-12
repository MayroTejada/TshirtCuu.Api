using Microsoft.Data.SqlClient;

namespace TshirtCuu.Services.Persistence;

public interface ISqlConnectionFactory
{
    SqlConnection Create();
}
