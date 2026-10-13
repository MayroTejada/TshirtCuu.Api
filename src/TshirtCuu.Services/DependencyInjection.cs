using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TshirtCuu.Domain.Features.Admin.Models;
using TshirtCuu.Domain.Features.Admin.Repositories;
using TshirtCuu.Domain.Features.Catalog.Repositories;
using TshirtCuu.Domain.Features.Quotes.Repositories;
using TshirtCuu.Services.Features.Admin;
using TshirtCuu.Services.Features.Catalog;
using TshirtCuu.Services.Features.Email;
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
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        // Validate presence only — do not open a SQL connection at startup.
        var connectionString = configuration.GetConnectionString(SqlConnectionFactory.ConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            const string message =
                "STARTUP FAILED: ConnectionStrings:TshirtCuu is missing or empty. " +
                "Set App Setting / env var ConnectionStrings__TshirtCuu " +
                "(Azure SQL). Do not commit passwords. " +
                "Schema/procs: https://github.com/MayroTejada/TShirtMigration";
            Console.Error.WriteLine(message);
            throw new InvalidOperationException(message);
        }

        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<ICatalogRepository, SqlCatalogRepository>();
        services.AddScoped<IQuoteRepository, SqlQuoteRepository>();
        services.AddScoped<IAdminUserRepository, SqlAdminUserRepository>();

        services.AddScoped<CatalogService>();
        services.AddScoped<QuoteService>();
        services.AddSingleton<ICustomerEmailSender, SmtpCustomerEmailSender>();
        services.AddSingleton<JwtTokenService>();
        services.AddScoped<AdminAuthService>();
        services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();

        return services;
    }
}
