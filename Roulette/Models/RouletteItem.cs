using System.Text.Json.Serialization;

namespace Roulette.Models;

public class RouletteItem
{
    public string Text { get; set; } = "";

    public string Color { get; set; } = "";

    public int Count { get; set; }

    public double Size { get; set; } = 1;

    public RouletteItemState State { get; set; } = RouletteItemState.Locked;

    [JsonIgnore]
    public double Weight { get; set; } = 1;

    private static readonly Random s_rand = new();

    public static RouletteItem Create(string text = "", string? baseColor = null)
    {
        return new RouletteItem
        {
            Text = text,
            Color = RandomColor(baseColor),
            Size = 1,
            State = RouletteItemState.Locked
        };
    }

    public static string RandomColor(string? baseColor = null)
    {
        double l = 0.95;
        double c = 0.02;
        if (!string.IsNullOrWhiteSpace(baseColor))
        {
            var (l0, c0, _) = ColorUtil.HexToOklch(baseColor);
            l = l0;
            c = c0;
        }
        var h = s_rand.NextDouble() * 360;
        return ColorUtil.OklchToHex(l, c, h);
    }
}

public enum RouletteItemState
{
    Enabled,
    Locked,
    Disabled
}
