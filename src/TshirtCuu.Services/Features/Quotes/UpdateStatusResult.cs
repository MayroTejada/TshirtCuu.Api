namespace TshirtCuu.Services.Features.Quotes;

public sealed record UpdateStatusResult(
    bool Found,
    string? Status,
    bool EmailSent,
    string? EmailNote,
    string? To)
{
    public static UpdateStatusResult NotFound() => new(false, null, false, null, null);

    public static UpdateStatusResult Saved(
        string status,
        bool emailSent = false,
        string? emailNote = null,
        string? to = null) =>
        new(true, status, emailSent, emailNote, to);
}
