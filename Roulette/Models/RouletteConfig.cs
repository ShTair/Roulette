using System.Text.Json;
using Microsoft.JSInterop;

namespace Roulette.Models;

public class RouletteConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = "";

    public List<RouletteItem> Items { get; set; } = [];

    public bool AutoAdjustSize { get; set; } = true;

    public static List<RouletteConfig> FromJson(string? json)
    {
        var list = new List<RouletteConfig>();
        if (string.IsNullOrEmpty(json)) return list;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var cfg = el.Deserialize<RouletteConfig>() ?? new RouletteConfig();
                    if (!el.TryGetProperty(nameof(AutoAdjustSize), out _))
                    {
                        cfg.AutoAdjustSize = true;
                    }
                    list.Add(cfg);
                }
                return list;
            }
        }
        catch { }

        try
        {
            var legacy = JsonSerializer.Deserialize<Dictionary<string, RouletteItem[]>>(json);
            if (legacy is { })
            {
                list = [.. legacy.Select(kvp => new RouletteConfig
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = kvp.Key,
                    Items = [.. kvp.Value],
                    AutoAdjustSize = true
                })];
            }
        }
        catch { }

        return list;
    }

    public static async Task<List<RouletteConfig>> LoadAsync(IJSRuntime js)
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", "rouletteConfigs");
        return FromJson(json);
    }

    public static async Task SaveAsync(IJSRuntime js, IEnumerable<RouletteConfig> configs)
    {
        var json = JsonSerializer.Serialize(configs);
        await js.InvokeVoidAsync("localStorage.setItem", "rouletteConfigs", json);
    }
}
