namespace Roulette.Models;

public class RouletteItem
{
    [JsonPropertyName("n")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("c")]
    public string? Color { get; set; }

    [JsonPropertyName("i")]
    public int Count { get; set; }
}
