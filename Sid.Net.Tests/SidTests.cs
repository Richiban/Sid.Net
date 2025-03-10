using Xunit.Abstractions;

namespace Sid.Net;

public class SidTests(ITestOutputHelper output)
{
    [Fact]
    public void A()
    {
        var timestampA = DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch;
        var timestamp = timestampA.Ticks;
        var a = DateTimeOffset.UtcNow.Ticks;
        output.WriteLine("Timestamp: {0}", timestamp);
        // for (var (v, i) = (timestamp, bufferIndex + TimestampCharCount - 1); v > 0; v /= Radix, i--)
        // {
        //     var digit = (int)(v % Radix);
        //
        //     buffer[i] = digit switch
        //     {
        //         < 10 => (char)('0' + digit),
        //         < 36 => (char)('A' + (digit - 10)),
        //         _ => (char)('a' + (digit - 36))
        //     };
        // }
    }

    [Fact]
    public void OutputLength()
    {
        var sid = Sid.Create();
        output.WriteLine($"Generated SID: {sid}");
        sid.Length.ShouldBe(23);
    }

    [Fact]
    public void OutputLengthWithPrefix()
    {
        var sid = Sid.Create("AB.");
        output.WriteLine($"Generated SID: {sid}");

        sid.Length.ShouldBe(26);
    }

    [Fact]
    public void StartsWithPrefix()
    {
        var sid = Sid.Create("AB.");
        output.WriteLine($"Generated SID: {sid}");

        sid.ShouldStartWith("AB.");
    }

    [Fact]
    public void AllCharsAreValid()
    {
        var sid = Sid.Create();
        output.WriteLine($"Generated SID: {sid}");

        sid.ShouldAllBe(c => Char.IsLetterOrDigit(c));
    }

    [Fact]
    public void TimestampIsPreserved()
    {
        var originalTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sid = Sid.Create(originalTimestamp);
        output.WriteLine($"Generated SID: {sid}");
        var outTimestamp = 0L;

        foreach (var c in sid.Take(7))
        {
            outTimestamp *= 62;

            outTimestamp += c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'A' and <= 'Z' => 10 + (c - 'A'),
                >= 'a' and <= 'z' => 36 + (c - 'a'),
                _ => throw new Exception($"Invalid char: {c}")
            };
        }

        outTimestamp.ShouldBe(originalTimestamp);
    }

    [Fact]
    public void SortOrder()
    {
        var sids = Enumerable.Range(0, 100).Select(_ => Sid.Create()).ToList();

        sids.ShouldBe(sids.OrderBy(sid => sid, StringComparer.Ordinal));
    }

    [Fact]
    public void Parse()
    {
        var originalTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sid = Sid.Create(originalTimestamp);
        var parsed = Sid.Parse(sid);
        output.WriteLine($"Generated SID: {sid}");

        parsed.ShouldSatisfyAllConditions(
            it => it.Timestamp.ShouldBe(originalTimestamp),
            it => it.RandomChars.Length.ShouldBe(14),
            it => it.Prefix.ShouldBeEmpty(),
            it => it.Counter.ShouldBe(0));
    }

    [Fact]
    public void ParseTwiceIncrementsCounter()
    {
        var originalTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var parsed1 = Sid.Parse(Sid.Create(originalTimestamp));
        var parsed2 = Sid.Parse(Sid.Create(originalTimestamp));

        parsed1.Counter.ShouldBe(0);
        parsed2.Counter.ShouldBe(1);
    }

    [Fact]
    public void ParseWithPrefix()
    {
        var originalTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sid = Sid.Create(originalTimestamp, "AB.");
        output.WriteLine($"Generated SID: {sid}");
        var parsed = Sid.Parse(sid);
        output.WriteLine($"Parsed SID: {parsed}");

        parsed.ShouldSatisfyAllConditions(
            it => it.Timestamp.ShouldBe(originalTimestamp),
            it => it.RandomChars.Length.ShouldBe(14),
            it => it.Prefix.ShouldBe(['A', 'B', '.']),
            it => it.Counter.ShouldBe(0));
    }
}
