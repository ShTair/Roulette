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

    public static RouletteItem Create(string text = "")
    {
        return new RouletteItem
        {
            Text = text,
            Color = RandomColor(),
            Size = 1,
            State = RouletteItemState.Locked
        };
    }

    public static string RandomColor()
    {
        // Choose colors with good readability
        var hue = s_rand.NextDouble() * 360;            // 0 - 360
        var saturation = 0.6 + s_rand.NextDouble() * 0.3; // 60% - 90%
        var lightness = 0.5 + s_rand.NextDouble() * 0.2;  // 50% - 70%
        return HslToHex(hue, saturation, lightness);
    }

    private static string HslToHex(double h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = l - c / 2;
        double r1, g1, b1;
        if (h < 60) { r1 = c; g1 = x; b1 = 0; }
        else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
        else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
        else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
        else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
        else { r1 = c; g1 = 0; b1 = x; }
        int r = (int)Math.Round((r1 + m) * 255);
        int g = (int)Math.Round((g1 + m) * 255);
        int b = (int)Math.Round((b1 + m) * 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}

public enum RouletteItemState
{
    Enabled,
    Locked,
    Disabled
}
