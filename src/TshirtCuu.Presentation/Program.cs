using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TshirtCuu.Services;
using TshirtCuu.Services.Features.Admin;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "T-shirtCuu API",
        Version = "v1",
        Description = "API for quotes, catalog, and admin auth. Persistence: Azure SQL via stored procedures (see TShirtMigration)."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

try
{
    builder.Services.AddTshirtCuuServices(builder.Configuration);
}
catch (InvalidOperationException ex)
{
    // Ensure Azure Log Stream / stdout shows the reason for HTTP 500.30 process exit.
    Console.Error.WriteLine(ex.Message);
    throw;
}

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
{
    const string message =
        "STARTUP FAILED: Jwt:SigningKey is missing or shorter than 32 characters. " +
        "Set App Setting / env var Jwt__SigningKey (min 32 chars). " +
        "Do not commit production secrets. " +
        "Note: appsettings.Development.json JWT is not loaded when ASPNETCORE_ENVIRONMENT=Production.";
    Console.Error.WriteLine(message);
    throw new InvalidOperationException(message);
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(
                "https://tshirtcuu.web.app",
                "https://tshirtcuu.firebaseapp.com")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Swagger UI is enabled in all environments (including Azure Production) so /swagger works live.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "T-shirtCuu API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    name = "T-shirtCuu API",
    docs = "/swagger",
    auth = "POST /api/admin/auth/login",
    persistence = "Azure SQL stored procedures (TShirtMigration)",
    images = "Private Azure blob container 'imagenes' via /api/images (AzureStorage__ConnectionString)",
    schemaRepo = "https://github.com/MayroTejada/TShirtMigration"
}));

app.Logger.LogInformation(
    "T-shirtCuu API starting. Environment={Environment}. SQL is opened only on first request (stored procedures).",
    app.Environment.EnvironmentName);

app.Run();
