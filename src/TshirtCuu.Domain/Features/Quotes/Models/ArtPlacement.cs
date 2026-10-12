namespace TshirtCuu.Domain.Features.Quotes.Models;

/// <summary>
/// Normalized placement inside the shirt print area.
/// Dx/Dy offset the art center; ScaleX/ScaleY size width/height independently.
/// Scale is the legacy uniform field — used when ScaleX/ScaleY are unset.
/// </summary>
public sealed class ArtPlacement
{
    public double Dx { get; set; }
    public double Dy { get; set; }

    /// <summary>Legacy uniform scale. Prefer ScaleX/ScaleY.</summary>
    public double Scale { get; set; } = 1;

    public double? ScaleX { get; set; }
    public double? ScaleY { get; set; }

    public double ResolvedScaleX => ScaleX ?? Scale;
    public double ResolvedScaleY => ScaleY ?? ScaleX ?? Scale;
}
