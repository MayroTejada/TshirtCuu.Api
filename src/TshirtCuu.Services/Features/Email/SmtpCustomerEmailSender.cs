using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TshirtCuu.Domain.Features.Quotes.Models;

namespace TshirtCuu.Services.Features.Email;

public sealed class SmtpCustomerEmailSender(
    IOptions<EmailOptions> options,
    ILogger<SmtpCustomerEmailSender> logger) : ICustomerEmailSender
{
    public async Task<EmailSendOutcome> TrySendListaReadyAsync(
        Quotation quote,
        CancellationToken cancellationToken = default)
    {
        var email = options.Value;
        if (!email.IsConfigured)
        {
            return EmailSendOutcome.Skipped(
                "Correo no enviado: faltan ajustes Email__Username, Email__Password y/o Email__From.");
        }

        if (string.IsNullOrWhiteSpace(quote.Email))
        {
            return EmailSendOutcome.Skipped(
                "Correo no enviado: la cotización no tiene email de cliente.");
        }

        var to = quote.Email.Trim();
        var fromName = string.IsNullOrWhiteSpace(email.FromName) ? "T-SHIRTCUU" : email.FromName.Trim();
        var subject = $"Tu camisa ya quedó · {fromName}";
        var body =
            $"Hola {quote.CustomerName.Trim()},\n\n" +
            $"Tu cotización {quote.Id} ya está lista: la camisa ya quedó.\n\n" +
            $"Puedes pasar a recogerla o escribirnos para coordinar la entrega.\n\n" +
            $"— {fromName}";

        return await SendAsync(
                to,
                subject,
                body,
                fromName,
                quote.Id,
                kind: "Lista",
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EmailSendOutcome> TrySendNewQuoteAlertAsync(
        Quotation quote,
        CancellationToken cancellationToken = default)
    {
        var email = options.Value;
        if (!email.IsConfigured)
        {
            return EmailSendOutcome.Skipped(
                "Alerta de cotización no enviada: faltan ajustes Email__Username, Email__Password y/o Email__From.");
        }

        var to = email.ResolveNotifyTo();
        if (string.IsNullOrWhiteSpace(to))
        {
            return EmailSendOutcome.Skipped(
                "Alerta de cotización no enviada: Email__NotifyTo y Email__From están vacíos.");
        }

        var fromName = string.IsNullOrWhiteSpace(email.FromName) ? "T-SHIRTCUU" : email.FromName.Trim();
        var subject = $"Nueva cotización {quote.Id} · {fromName}";
        var body =
            $"Nueva solicitud de cotización.\n\n" +
            $"Folio: {quote.Id}\n" +
            $"Cliente: {quote.CustomerName.Trim()}\n" +
            $"Email: {quote.Email.Trim()}\n" +
            $"Teléfono: {quote.Phone.Trim()}\n";

        return await SendAsync(
                to,
                subject,
                body,
                fromName,
                quote.Id,
                kind: "NewQuoteAlert",
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<EmailSendOutcome> SendAsync(
        string to,
        string subject,
        string body,
        string fromName,
        string quoteId,
        string kind,
        CancellationToken cancellationToken)
    {
        var email = options.Value;

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, email.From.Trim()));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            var socketOptions = SecureSocketOptionsForPort(email.Port);

            await client
                .ConnectAsync(email.Host.Trim(), email.Port, socketOptions, cancellationToken)
                .ConfigureAwait(false);

            await client
                .AuthenticateAsync(email.Username.Trim(), email.Password, cancellationToken)
                .ConfigureAwait(false);

            await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
            await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            var okNote = kind == "NewQuoteAlert"
                ? $"Alerta de cotización enviada a {to} (folio {quoteId})."
                : $"Correo de “ya quedó” enviado a {to}.";

            logger.LogInformation(
                "SMTP {Kind} ok for quote {QuoteId} → {To}: {Note}",
                kind,
                quoteId,
                to,
                okNote);

            return EmailSendOutcome.Succeeded(okNote);
        }
        catch (Exception ex)
        {
            var detail = FormatFullException(ex);
            var failNote = kind == "NewQuoteAlert"
                ? $"Alerta de cotización no enviada a {to}: {detail}"
                : $"Correo no enviado a {to}: {detail}";

            logger.LogWarning(
                ex,
                "SMTP {Kind} failed for quote {QuoteId} → {To}: {Note}",
                kind,
                quoteId,
                to,
                failNote);

            // Full SMTP exception text (no credentials). Never fake success.
            return EmailSendOutcome.Failed(failNote);
        }
    }

    /// <summary>
    /// Zoho / common SMTP: 465 = implicit SSL, 587 = STARTTLS.
    /// System.Net.Mail EnableSsl only does STARTTLS and breaks on 465.
    /// </summary>
    internal static SecureSocketOptions SecureSocketOptionsForPort(int port) =>
        port switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            587 => SecureSocketOptions.StartTls,
            _ => SecureSocketOptions.Auto
        };

    /// <summary>Full exception chain for emailNote (messages only — no secrets).</summary>
    internal static string FormatFullException(Exception ex)
    {
        var parts = new List<string>();
        for (var cur = ex; cur != null; cur = cur.InnerException)
        {
            var type = cur.GetType().Name;
            var msg = (cur.Message ?? string.Empty).Trim();
            parts.Add(string.IsNullOrEmpty(msg) ? type : $"{type}: {msg}");
        }

        return parts.Count == 0 ? "error SMTP desconocido." : string.Join(" → ", parts);
    }
}
