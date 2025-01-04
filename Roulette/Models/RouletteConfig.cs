namespace Roulette.Models;

public class RouletteConfig
{
    [JsonPropertyName("a")]
    public bool IsAdjustable { get; set; }

    [JsonPropertyName("i")]
    public RouletteItem[] Items { get; set; } = default!;
}
