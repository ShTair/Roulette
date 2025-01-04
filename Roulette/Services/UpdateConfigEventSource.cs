using Microsoft.JSInterop;

namespace Roulette.Services;

public class UpdateConfigEventSource
{
    private static UpdateConfigEventSource? s_obj;

    public event Action<RouletteConfig>? OnUpdateConfig;

    public UpdateConfigEventSource()
    {
        s_obj = this;
    }

    [JSInvokable]
    public static Task UpdateConfigAsync(RouletteConfig config)
    {
        s_obj?.OnUpdateConfig?.Invoke(config);
        return Task.CompletedTask;
    }
}
