using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TshirtCuu.Services.Persistence;

/// <summary>
/// Design-time factory for <c>dotnet ef migrations</c>. Uses the same localhost
/// placeholder as Development — no Azure password required to generate migrations.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("TSHIRTCUU_SQL")
            ?? "Server=localhost,1433;Database=TshirtCuu;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
