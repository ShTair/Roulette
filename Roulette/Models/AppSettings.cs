using System.Text.Json;
using Microsoft.JSInterop;

namespace Roulette.Models;

public class AppSettings
{
    public int BoardSize { get; set; } = 300;

    public static AppSettings FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new AppSettings();
        try
        {
            return JsonSerializer.Deserialize<AppSettings>(json, JsonUtil.WebOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static async Task<AppSettings> LoadAsync(IJSRuntime js)
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", "appSettings");
        return FromJson(json);
    }

    public static async Task SaveAsync(IJSRuntime js, AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonUtil.WebOptions);
        await js.InvokeVoidAsync("localStorage.setItem", "appSettings", json);
    }
}
