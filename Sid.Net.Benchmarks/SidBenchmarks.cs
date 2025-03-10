using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Sid.Net.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SidBenchmarks
{
    [Benchmark(Baseline = true)]
    public void BenchmarkCreate()
    {
        var s = Sid.Create();
    }

    [Benchmark]
    public void BenchmarkCreateWithPrefix()
    {
        var s = Sid.Create("DC.");
    }
}
