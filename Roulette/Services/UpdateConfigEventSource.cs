using Microsoft.JSInterop;

namespace Roulette.Services;

public class UpdateConfigEventSource
{
    private static UpdateConfigEventSource? s_obj;

    public event Action<string>? OnUpdateConfig;

    public UpdateConfigEventSource()
    {
        s_obj = this;
    }

    [JSInvokable]
    public static Task UpdateConfigAsync(string data)
    {
        s_obj?.OnUpdateConfig?.Invoke(data);
        return Task.CompletedTask;
    }
}
