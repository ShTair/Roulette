using System.Text.Encodings.Web;
using System.Text.Json;

namespace Roulette.Models;

public static class JsonUtil
{
    public static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
