namespace TshirtCuu.Services.Features.Quotes;

/// <summary>Outcome of sending (or attempting) the Lista / “ya quedó” customer email.</summary>
public sealed record ListaEmailResult(
    bool Found,
    bool EmailSent,
    string? EmailNote,
    string? To)
{
    public static ListaEmailResult NotFound() => new(false, false, null, null);

    public static ListaEmailResult FromOutcome(bool sent, string note, string? to) =>
        new(true, sent, note, to);
}
