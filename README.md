# Sid.Net
A tiny library for time-sortable, URL-safe string IDs.

The format of a SID is as follows:

```
[<prefix>]<timestamp><counter><randomChars>
```

Where 
* `prefix` is an optional string supplied by the caller
* `timestamp` is a base62-encoded UNIX timestamp in which the SID was created 
* `counter` is a simple incrementing counter that ensures sortability within the same millisecond
* `randomChars` is a string of 16 random base62 characters.

The timestamp is the number of milliseconds since the Unix epoch, encoded in such a way that SIDs will naturally sort into the order in which they were created. Remember that in .NET you must use `StringComparer.Ordinal` to sort SIDs correctly (by default strings are sorted using the current `CultureInfo`, which usually means a sort order that ignores case).

The entropy of a SID is at least that of a v7 GUID (which also includes a timestamp component), but a SID is more compact whilst remaining URL-safe.

| Component | String length | Possible values | Entropy        |
| --------- | ------------- | --------------- | -------------- |
| Guid      | 36            | 2^122           | 122 bits       |
| Guid v7   | 36            | 2^122           | 74 bits per ms |
| SID       | 22            | 2^137           | 84 bits per ms |

## Usage

Creating a new ID is as simple as calling `Sid.Create()`.

```csharp
using Sid.Net;
string s = Sid.Create(); // Uf2WHnIaa6R9Hiw3HABMMLH
```

Prefixed IDs are nice; you can also provide a prefix to the ID by calling `Sid.Create("prefix.")`.

```csharp
string s = Sid.Create("prefix"); // prefixUf2WICe9roQFNb8RRVe0Y1O
```

If desired, a generated SID can be parsed into a `Sid` struct instance if you'd like to deconstruct the SID into the inputs that were used to create it:

```csharp
Sid parsed = Sid.Parse("AB.Uf2WHnIaa6R9Hiw3HABMMLH");
long timestamp = parsed.Timestamp; 
    // 1625097600000

char[] prefix = parsed.Prefix; 
    // ['A', 'B', '.']

char[] randomChars = parsed.RandomChars; 
    // ['a', 'a', '6', 'R', '9', 'H', 'i', 'w', '3', 'H', 'A','B', 'M', 'M', 'L', 'H']
```

You can also easily convert the timestamp to a DateTimeOffset:

```csharp
DateTimeOffset dt = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
    // 2021-07-01T00:00:00.0000000+00:00
```

## It's fast

Sid.NET is extremely fast, taking less than 300ns on a dev laptop to generate a new ID, even with a prefix. It also allocates no memory beside the returned string, making it appropriate for high-performance applications.

| Method                    |     Mean |   Error |   StdDev |   Median | Ratio | RatioSD | Rank | Allocated | Alloc Ratio |
| ------------------------- | -------: | ------: | -------: | -------: | ----: | ------: | ---: | --------: | ----------: |
| Create           | 256.3 ns | 6.22 ns | 17.43 ns | 251.0 ns |  1.00 |    0.09 |    1 |      72 B |        1.00 |
| CreateWithPrefix | 263.3 ns | 6.99 ns | 20.05 ns | 257.4 ns |  1.03 |    0.10 |    1 |      80 B |        1.11 |

## Caveats

Since it's possible for mulitple SIDs to be created in the same millisecond, a counter is included in the SID to ensure that each SID is unique. The counter is a base62-encoded number that is incremented (in a thread-safe way) each time a new SID is created in the same millisecond. The counter has two characters, allowing for 3844 SIDs to be created in the same millisecond whilst remaining sortable. Creating more SIDs per millisecond will work, but they will not order correctly as the counter will overflow.