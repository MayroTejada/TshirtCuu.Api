using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddSingleton<InMemoryStore>();
        services.AddSingleton<ICatalogRepository, InMemoryCatalogRepository>();
        services.AddSingleton<IQuoteRepository, InMemoryQuoteRepository>();
        services.AddSingleton<IAdminUserRepository, InMemoryAdminUserRepository>();

        services.AddSingleton<CatalogService>();
        services.AddSingleton<QuoteService>();
        services.AddSingleton<JwtTokenService>();
        services.AddSingleton<AdminAuthService>();

        return services;
    }
}
