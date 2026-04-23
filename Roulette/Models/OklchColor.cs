namespace Roulette.Models;

public readonly record struct OklchColor(float L, float C, float H)
{
    public static readonly OklchColor Black = new(0f, 0f, 0f);
    public static readonly OklchColor White = new(1f, 0f, 0f);

    public string ToCss() => $"oklch({L * 100:F2}% {C:F4} {H:F2})";

    public string ToHex() => ColorUtil.OklchToHex(L, C, H);
}
