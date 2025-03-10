using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Sid.Net.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SidBenchmarks
{
    [Benchmark(Baseline = true)]
    public void Create()
    {
        var s = Sid.Create();
    }

    [Benchmark]
    public void CreateWithPrefix()
    {
        var s = Sid.Create("DC.");
    }
}
