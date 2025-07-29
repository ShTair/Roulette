namespace Roulette.Models;

using System.Text.Json;

public static class JsonUtil
{
    public static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);
}
