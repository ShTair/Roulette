using MessagePack;

namespace Roulette.Models;

[MessagePackObject]
public class RouletteConfig
{
    [Key(0)]
    public bool IsAdjustable { get; set; }

    [Key(1)]
    public RouletteItem[] Items { get; set; } = default!;
}
