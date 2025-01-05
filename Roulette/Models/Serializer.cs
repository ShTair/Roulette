using MessagePack;
using NeoSmart.Utils;
using System.IO.Compression;

namespace Roulette.Models;

public static class Serializer
{
    public static string Serialize<T>(T obj)
    {
        using var memoryStream = new MemoryStream();
        using var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal);
        deflateStream.Write(MessagePackSerializer.Serialize(obj));
        deflateStream.Flush();

        return UrlBase64.Encode(memoryStream.GetBuffer().AsSpan()[..(int)memoryStream.Length]);
    }

    public static T Deserialize<T>(string s)
    {
        using var memoryStream = new MemoryStream(UrlBase64.Decode(s));
        using var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal);
        using var dstMemoryStream = new MemoryStream();
        deflateStream.CopyTo(dstMemoryStream);

        return MessagePackSerializer.Deserialize<T>(dstMemoryStream.GetBuffer().AsMemory()[..(int)dstMemoryStream.Length]);
    }
}
