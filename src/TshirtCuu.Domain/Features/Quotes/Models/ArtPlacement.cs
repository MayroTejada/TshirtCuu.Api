namespace TshirtCuu.Domain.Features.Quotes.Models;

/// <summary>
/// Normalized placement inside the shirt print area.
/// Dx/Dy are offsets of the art center as a fraction of the print zone.
/// </summary>
public sealed class ArtPlacement
{
    public double Dx { get; set; }
    public double Dy { get; set; }
    public double Scale { get; set; } = 1;
}
