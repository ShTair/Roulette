using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.JSInterop;

namespace Roulette.Models;

public partial class RouletteConfig
{

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = "";

    public List<RouletteItem> Items { get; set; } = [];

    public bool AutoAdjustSize { get; set; } = true;

    public int ItemMultiplier { get; set; } = 1;

    public bool ShowCountList { get; set; } = false;

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex ColorRegex();

    private static void EnsureItemColors(IEnumerable<RouletteConfig> configs)
    {
        foreach (var cfg in configs)
        {
            string? prevColor = null;
            foreach (var item in cfg.Items)
            {
                if (string.IsNullOrWhiteSpace(item.BackgroundColor) || !ColorRegex().IsMatch(item.BackgroundColor))
                {
                    item.BackgroundColor = RouletteItem.RandomColor(prevColor);
                }
                prevColor = item.BackgroundColor;
            }
        }
    }

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
                    var cfg = el.Deserialize<RouletteConfig>(JsonUtil.WebOptions) ?? new RouletteConfig();
                    if (!el.TryGetProperty("autoAdjustSize", out _) &&
                        !el.TryGetProperty(nameof(AutoAdjustSize), out _))
                    {
                        cfg.AutoAdjustSize = true;
                    }
                    if (!el.TryGetProperty("itemMultiplier", out _) &&
                        !el.TryGetProperty(nameof(ItemMultiplier), out _))
                    {
                        cfg.ItemMultiplier = 1;
                    }
                    if (!el.TryGetProperty("showCountList", out _) &&
                        !el.TryGetProperty(nameof(ShowCountList), out _))
                    {
                        cfg.ShowCountList = false;
                    }
                    list.Add(cfg);
                }
                EnsureItemColors(list);
                return list;
            }
        }
        catch { }

        try
        {
            var legacy = JsonSerializer.Deserialize<Dictionary<string, RouletteItem[]>>(json, JsonUtil.WebOptions);
            if (legacy is { })
            {
                list = [.. legacy.Select(kvp => new RouletteConfig
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = kvp.Key,
                    Items = [.. kvp.Value],
                    AutoAdjustSize = true,
                    ItemMultiplier = 1
                })];
                EnsureItemColors(list);
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
        var json = JsonSerializer.Serialize(configs, JsonUtil.WebOptions);
        await js.InvokeVoidAsync("localStorage.setItem", "rouletteConfigs", json);
    }
}
