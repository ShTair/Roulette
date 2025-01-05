using MessagePack;

namespace Roulette.Models;

public static class Serializer
{
    public static string Serialize<T>(T obj)
    {
        var buffer = MessagePackSerializer.Serialize(obj);
        return Convert.ToBase64String(buffer);
    }

    public static T Deserialize<T>(string s)
    {
        var buffer = Convert.FromBase64String(s);
        return MessagePackSerializer.Deserialize<T>(buffer);
    }
}
