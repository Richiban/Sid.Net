using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Sid.Net;

public record struct Sid()
{
    /// <summary>
    /// Not including prefix
    /// </summary>
    private const int TotalCharCount = 23;

    private const int Radix = 62;
    private const int TimestampCharCount = 7;
    private const int CounterCharCount = 2;
    private const int RandomCharCount = TotalCharCount - TimestampCharCount - CounterCharCount;
    private static readonly Random Random = new();
    private static int _counter;
    private static long _lastMillisecond;
    private static readonly object Lock = new();

    public char[] Prefix { get; set; } = [];
    public long Timestamp { get; set; } = 0L;
    public int Counter { get; set; } = 0;
    public char[] RandomChars { get; set; } = [];

    public static string Create(string prefix = "")
    {
        return Create(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), prefix);
    }

    private static int GetCounter(long timestamp)
    {
        lock (Lock)
        {
            if (timestamp == _lastMillisecond)
            {
                _counter++;
            }
            else
            {
                _lastMillisecond = timestamp;
                _counter = 0;
            }

            return _counter;
        }
    }

    public static string Create(long timestamp, string prefix = "")
    {
        var counter = GetCounter(timestamp);
        Span<char> buffer = stackalloc char[TotalCharCount + prefix.Length];

        var bufferIndex = 0;

        for (; bufferIndex < prefix.Length; bufferIndex++)
        {
            buffer[bufferIndex] = prefix[bufferIndex];
        }

        for (var (v, i) = (timestamp, bufferIndex + TimestampCharCount - 1); v > 0; v /= Radix, i--)
        {
            buffer[i] = EncodeDigit((int)(v % Radix));
        }

        bufferIndex += TimestampCharCount;

        for (var i = bufferIndex + CounterCharCount - 1; i >= bufferIndex; i--, counter /= Radix)
        {
            buffer[i] = EncodeDigit(counter % Radix);
        }

        bufferIndex += CounterCharCount;

        for (; bufferIndex < TotalCharCount + prefix.Length; bufferIndex++)
        {
            buffer[bufferIndex] += EncodeDigit(Random.Next(0, Radix));
        }

        return new string(buffer);
    }

    private static char EncodeDigit(int digit) =>
        digit switch
        {
            < 10 => (char)('0' + digit),
            < 36 => (char)('A' + (digit - 10)),
            _ => (char)('a' + (digit - 36))
        };

    private static int DecodeDigit(char c) =>
        c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'Z' => 10 + (c - 'A'),
            >= 'a' and <= 'z' => 36 + (c - 'a'),
            _ => throw new Exception($"Invalid char: {c}")
        };

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

        var prefixLength = input.Length - TotalCharCount;

        var timestamp = 0L;
        var inputIndex = prefixLength;
        var limit = inputIndex + TimestampCharCount;

        for (; inputIndex < limit; inputIndex++)
        {
            timestamp *= Radix;
            timestamp += DecodeDigit(input[inputIndex]);
        }

        var counter = 0;

        limit = inputIndex + CounterCharCount;

        for (; inputIndex < limit; inputIndex++)
        {
            counter *= Radix;
            counter += DecodeDigit(input[inputIndex]);
        }

        return new()
        {
            Prefix = input.Take(prefixLength).ToArray(),
            Timestamp = timestamp,
            Counter = counter,
            RandomChars = input.Skip(inputIndex).ToArray(),
        };
    }
}
