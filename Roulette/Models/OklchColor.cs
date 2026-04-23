namespace Roulette.Models;

public readonly record struct OklchColor(double L, double C, double H)
{
    public static readonly OklchColor Black = new(0, 0, 0);
    public static readonly OklchColor White = new(1, 0, 0);

    public string ToCss() => $"oklch({L * 100:F2}% {C:F4} {H:F2})";

    public string ToHex() => ColorUtil.OklchToHex(L, C, H);
}
