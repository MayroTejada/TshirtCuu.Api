using System.Text;
using TshirtCuu.Services.Features.Errors;

namespace TshirtCuu.Presentation.Features.Errors;

/// <summary>
/// Logs unhandled exceptions to SQL. Logging failures never fail the request further.
/// </summary>
public sealed class ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IErrorLogger errorLogger)
    {
        var path = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            try
            {
                await errorLogger
                    .LogAsync(
                        path,
                        "Excepción no controlada",
                        FormatDetail(context, ex),
                        ex,
                        context.RequestAborted)
                    .ConfigureAwait(false);
            }
            catch (Exception logEx)
            {
                logger.LogError(logEx, "Error logger threw while handling unhandled exception.");
            }

            throw;
        }
    }

    private static string FormatDetail(HttpContext context, Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"StatusAttempt={(context.Response.HasStarted ? "started" : "not-started")}");
        sb.AppendLine($"TraceIdentifier={context.TraceIdentifier}");
        sb.AppendLine(ex.ToString());
        return sb.ToString();
    }
}
