using System.Text.Json;
using Microsoft.JSInterop;

namespace Roulette.Models;

public class RouletteConfig
{

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = "";

    public List<RouletteItem> Items { get; set; } = [];

    public bool AutoAdjustSize { get; set; } = true;

    public double AutoAdjustExponent { get; set; } = 1.0;

    public int ItemMultiplier { get; set; } = 1;

    public bool ShowCountList { get; set; } = false;

    public bool AutoStop { get; set; } = true;

    private static void EnsureItemColors(IEnumerable<RouletteConfig> configs)
    {
        foreach (var cfg in configs)
        {
            OklchColor? prevColor = null;
            foreach (var item in cfg.Items)
            {
                if (item.BackgroundOklch == null)
                {
                    item.BackgroundOklch = RouletteItem.RandomOklchColor(prevColor);
                }
                prevColor = item.BackgroundOklch;
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
                    if (!el.TryGetProperty("autoAdjustExponent", out _) &&
                        !el.TryGetProperty(nameof(AutoAdjustExponent), out _))
                    {
                        cfg.AutoAdjustExponent = 1.0;
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
                    if (!el.TryGetProperty("autoStop", out _) &&
                        !el.TryGetProperty(nameof(AutoStop), out _))
                    {
                        cfg.AutoStop = true;
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
                    AutoAdjustExponent = 1.0,
                    ItemMultiplier = 1,
                    AutoStop = true
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
