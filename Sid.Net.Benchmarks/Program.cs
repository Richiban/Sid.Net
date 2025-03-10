using BenchmarkDotNet.Running;

namespace Sid.Net.Benchmarks;

public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<SidBenchmarks>();
    }
}
