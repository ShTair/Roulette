using MessagePack;
using System.Buffers;
using System.IO.Compression;

namespace Roulette.Models;

public static class Serializer
{
    public static string Serialize<T>(T obj)
    {
        using var memoryStream = new MemoryStream();
        using var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Fastest);
        deflateStream.Write(MessagePackSerializer.Serialize(obj));
        deflateStream.Flush();
        return Convert.ToBase64String(memoryStream.GetBuffer().AsSpan()[..(int)memoryStream.Length]);
    }

    public static T Deserialize<T>(string s)
    {
        using var memoryStream = new MemoryStream(Convert.FromBase64String(s));
        using var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Fastest);
        using var dstMemoryStream = new MemoryStream();

        deflateStream.CopyTo(dstMemoryStream);

        var buffer = ArrayPool<byte>.Shared.Rent((int)deflateStream.Length);
        try
        {
            return MessagePackSerializer.Deserialize<T>(dstMemoryStream.GetBuffer().AsMemory()[..(int)dstMemoryStream.Length]);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
