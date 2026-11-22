using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Content.Shared._Citadel.Random;
using Robust.Shared.Analyzers;

namespace Content.Benchmarks._Citadel;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Virtual]
public class CitadelRandomBenchmark
{
    private const int BatchSize = 1024;

    private Random _random = null!;
    private Random _seededRandom = null!;
    private SplitMix64 _splitMix;
    private Xoshiro256 _xoshiro;

    [GlobalSetup]
    public void Setup()
    {
        _random = new Random();
        _seededRandom = new Random(42);
        _splitMix = new SplitMix64(42);
        _xoshiro = new Xoshiro256(42);
    }

    // Use 63-bit outputs for the custom generators to match NextInt64's bit width.
    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Integer")]
    public ulong SystemRandomInteger()
    {
        ulong sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += (ulong)_random.NextInt64();
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Integer")]
    public ulong SeededSystemRandomInteger()
    {
        ulong sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += (ulong)_seededRandom.NextInt64();
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Integer")]
    public ulong SplitMix64Integer()
    {
        ulong sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _splitMix.Next() >> 1;
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Integer")]
    public ulong Xoshiro256Integer()
    {
        ulong sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _xoshiro.Next() >> 1;
        }

        return sum;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Double")]
    public double SystemRandomDouble()
    {
        double sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _random.NextDouble();
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Double")]
    public double SeededSystemRandomDouble()
    {
        double sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += _seededRandom.NextDouble();
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Double")]
    public double SplitMix64Double()
    {
        ICitadelRandom random = _splitMix;
        double sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += random.NextDouble();
        }

        return sum;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    [BenchmarkCategory("Double")]
    public double Xoshiro256Double()
    {
        ICitadelRandom random = _xoshiro;
        double sum = 0;
        for (var i = 0; i < BatchSize; i++)
        {
            sum += random.NextDouble();
        }

        return sum;
    }
}
