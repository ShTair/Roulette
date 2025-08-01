using System.Text.Json;
using Microsoft.JSInterop;

namespace Roulette.Models;

public class AppSettings
{
    public double AutoStopDelayMinSeconds { get; set; } = 2.0;
    public double AutoStopDelayMaxSeconds { get; set; } = 3.0;
    public double StopDurationSeconds { get; set; } = 2.0;
    public double StartSpeed { get; set; } = 18.0;
    public string BorderColor { get; set; } = "#808080";

    private const string StorageKey = "appSettings";

    public static async Task<AppSettings> LoadAsync(IJSRuntime js)
    {
        try
        {
            var json = await js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(json)) return new AppSettings();
            var cfg = JsonSerializer.Deserialize<AppSettings>(json, JsonUtil.WebOptions);
            return cfg ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static async Task SaveAsync(IJSRuntime js, AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonUtil.WebOptions);
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}
