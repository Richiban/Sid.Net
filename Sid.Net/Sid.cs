using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Sid.Net;

public record struct Sid()
{
    private const int RandomCharCount = 23 - 7;
    private const int Radix = 62;
    private const int TimestampCharCount = 7;
    private static readonly Random Random = new();

    public static string Create(string prefix = "")
    {
        return Create(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), prefix);
    }

    public static string Create(long timestamp, string prefix = "")
    {
        Span<char> buffer = stackalloc char[TimestampCharCount + RandomCharCount + prefix.Length];

        var bufferIndex = 0;

        for (; bufferIndex < prefix.Length; bufferIndex++)
        {
            buffer[bufferIndex] = prefix[bufferIndex];
        }

        for (var (v, i) = (timestamp, bufferIndex + TimestampCharCount - 1); v > 0; v /= Radix, i--)
        {
            var digit = (int)(v % Radix);

            buffer[i] = digit switch
            {
                < 10 => (char)('0' + digit),
                < 36 => (char)('A' + (digit - 10)),
                _ => (char)('a' + (digit - 36))
            };
        }

        bufferIndex += TimestampCharCount;

        for (; bufferIndex < TimestampCharCount + RandomCharCount + prefix.Length; bufferIndex++)
        {
            var digit = Random.Next(0, Radix);

            buffer[bufferIndex] += digit switch
            {
                < 10 => (char)('0' + digit),
                < 36 => (char)('A' + (digit - 10)),
                _ => (char)('a' + (digit - 36))
            };
        }

        return new string(buffer);
    }

    public char[] Prefix { get; set; } = [];
    public long Timestamp { get; set; } = 0L;
    public char[] RandomChars { get; set; } = [];

    public static Sid Parse(string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (input.Length < RandomCharCount)
        {
            throw new ArgumentException(
                $"The input string is too short to be a {nameof(Sid)}",
                nameof(input));
        }

        var prefixLength = input.Length - RandomCharCount - TimestampCharCount;

        var timestamp = 0L;

        foreach (var c in input.Skip(prefixLength).Take(TimestampCharCount))
        {
            timestamp *= 62;

            timestamp += c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'A' and <= 'Z' => 10 + (c - 'A'),
                >= 'a' and <= 'z' => 36 + (c - 'a'),
                _ => throw new Exception($"Invalid char: {c}")
            };
        }

        return new()
        {
            Prefix = input.Take(prefixLength).ToArray(),
            Timestamp = timestamp,
            RandomChars =
                input.Skip(prefixLength + TimestampCharCount).ToArray()
        };
    }
}
