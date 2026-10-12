using Microsoft.AspNetCore.Identity;
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
    public static IServiceCollection AddTshirtCuuServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Fail fast if connection string missing (env ConnectionStrings__TshirtCuu or user-secrets).
        _ = new SqlConnectionFactory(configuration).Create();

        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
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
