using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Content.Shared._Citadel.Random;
using Robust.Shared.Analyzers;

namespace Content.Benchmarks._Citadel;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Virtual]
public class CitadelRandomExtensionsBenchmark
{
    private const int BatchSize = 1024;
    private double _lowDouble, _highDouble;
    private float _lowFloat, _highFloat;
    private long _lowI64, _highI64;
    private ulong _lowU64, _highU64;
    private ICitadelRandom _random = null!;

    [GlobalSetup]
    public void Setup()
    {
        _random = new SplitMix64(42);
        _lowI64 = -500;
        _highI64 = 500;
        _lowU64 = 100;
        _highU64 = 1100;
        _lowFloat = -500;
        _highFloat = 500;
        _lowDouble = -500;
        _highDouble = 500;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("OneDraw")]
    public ulong RawDraw()
    {
        ulong sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _random.Next();
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("OneDraw")]
    public float Float()
    {
        float sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _random.NextFloat();
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("OneDraw")]
    public double Double()
    {
        double sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _random.NextDouble();
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("OneDraw")]
    public float BoundedFloat()
    {
        float sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _random.NextBoundedFloat(_lowFloat, _highFloat);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("OneDraw")]
    public double BoundedDouble()
    {
        double sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _random.NextBoundedDouble(_lowDouble, _highDouble);
        }

        return sum;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("TwoDraws")]
    public ulong RawTwoDraws()
    {
        ulong sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _random.Next();
            sum += _random.Next();
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("TwoDraws")]
    public long BoundedI64()
    {
        long sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _random.NextBoundedI64(_lowI64, _highI64);
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("TwoDraws")]
    public ulong BoundedU64()
    {
        ulong sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _random.NextBoundedU64(_lowU64, _highU64);
        }

        return sum;
    }
}
