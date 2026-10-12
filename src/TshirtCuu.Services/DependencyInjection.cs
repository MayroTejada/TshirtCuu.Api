using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TshirtCuu.Domain.Features.Admin.Models;
using TshirtCuu.Domain.Features.Admin.Repositories;
using TshirtCuu.Domain.Features.Catalog.Repositories;
using TshirtCuu.Domain.Features.Quotes.Repositories;
using TshirtCuu.Services.Features.Admin;
using TshirtCuu.Services.Features.Catalog;
using TshirtCuu.Services.Features.Quotes;
using TshirtCuu.Services.Persistence;

namespace TshirtCuu.Services;

public static class DependencyInjection
{
    public const string SqlConnectionName = "TshirtCuu";

    public static IServiceCollection AddTshirtCuuServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var connectionString = configuration.GetConnectionString(SqlConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Configure ConnectionStrings:{SqlConnectionName} in appsettings.Development.json or user-secrets. " +
                "Use a local SQL Server string for development; for Azure SQL later, set the Azure connection string via config/secrets — do not commit production passwords.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICatalogRepository, SqlCatalogRepository>();
        services.AddScoped<IQuoteRepository, SqlQuoteRepository>();
        services.AddScoped<IAdminUserRepository, SqlAdminUserRepository>();

        services.AddScoped<CatalogService>();
        services.AddScoped<QuoteService>();
        services.AddSingleton<JwtTokenService>();
        services.AddScoped<AdminAuthService>();
        services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();

        return services;
    }
}
