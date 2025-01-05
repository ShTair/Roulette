using MessagePack;

namespace Roulette.Models;

[MessagePackObject]
public class RouletteItem
{
    [Key(0)]
    public string Name { get; set; } = default!;

    [Key(1)]
    public string? Color { get; set; }

    [Key(2)]
    public int Count { get; set; }
}
