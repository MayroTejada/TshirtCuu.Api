using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(email.From.Trim(), fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(to);

            using var client = new SmtpClient(email.Host.Trim(), email.Port)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(email.Username.Trim(), email.Password)
            };

            cancellationToken.ThrowIfCancellationRequested();
            await client.SendMailAsync(message, cancellationToken).ConfigureAwait(false);

            return EmailSendOutcome.Succeeded($"Correo de “ya quedó” enviado a {to}.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "SMTP failed sending Lista email for quote {QuoteId} to {Email}",
                quote.Id,
                to);

            // Surface the real SMTP/error text (no credentials). Do not fake success.
            var detail = SanitizeError(ex);
            return EmailSendOutcome.Failed($"Correo no enviado a {to}: {detail}");
        }
    }

    private static string SanitizeError(Exception ex)
    {
        var msg = ex.GetBaseException().Message?.Trim() ?? ex.Message;
        if (string.IsNullOrWhiteSpace(msg))
            return "error SMTP desconocido.";

        // Keep it short for UI; never invent a success.
        if (msg.Length > 240)
            msg = msg[..240] + "…";
        return msg;
    }
}
