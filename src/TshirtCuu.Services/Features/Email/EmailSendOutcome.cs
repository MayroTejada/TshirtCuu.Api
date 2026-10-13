namespace TshirtCuu.Services.Features.Email;

public sealed record EmailSendOutcome(bool Sent, string Note)
{
    public static EmailSendOutcome Skipped(string note) => new(false, note);

    public static EmailSendOutcome Succeeded(string note) => new(true, note);

    public static EmailSendOutcome Failed(string note) => new(false, note);
}
