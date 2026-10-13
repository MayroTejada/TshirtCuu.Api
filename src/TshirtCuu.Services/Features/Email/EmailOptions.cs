namespace TshirtCuu.Services.Features.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "smtp.zoho.com";
    public int Port { get; set; } = 465;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = "T-SHIRTCUU";

    /// <summary>
    /// Inbox for new-quote alerts. Falls back to <see cref="From"/> when empty.
    /// Azure: Email__NotifyTo
    /// </summary>
    public string NotifyTo { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host)
        && Port > 0
        && !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password)
        && !string.IsNullOrWhiteSpace(From);

    /// <summary>Alert destination: NotifyTo, else From.</summary>
    public string ResolveNotifyTo()
    {
        if (!string.IsNullOrWhiteSpace(NotifyTo))
            return NotifyTo.Trim();
        return string.IsNullOrWhiteSpace(From) ? string.Empty : From.Trim();
    }
}
