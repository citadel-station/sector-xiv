using System.Numerics;
using System.Runtime.CompilerServices;

namespace Content.Shared._Citadel.Random;

public static class CitadelRandomExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long NextBoundedI64(ICitadelRandom random, long low, long high)
    {
        if (low > high)
            throw new ArgumentOutOfRangeException(nameof(high), "The upper bound must be at least the lower bound.");
        // Unsigned arithmetic also handles intervals spanning more than long.MaxValue values.
        var width = unchecked((ulong)high - (ulong)low);

        return unchecked((long)((ulong)low + NextBoundedU64(random, width)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong NextBoundedU64(ICitadelRandom random, ulong low, ulong high)
    {
        if (low > high)
            throw new ArgumentOutOfRangeException(nameof(high), "The upper bound must be at least the lower bound.");
        return low + NextBoundedU64(random, high - low);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong NextBoundedU64(ICitadelRandom random, ulong bound)
    {
        // Algorithm based off of
        // https://arxiv.org/pdf/1805.10941 ; algorithm 5

        // Let L = 64
        // Let s = bound

        // x ← random integer in [0, 2^L)
        var raw = random.Next();

        // fast path: power of 2
        if (BitOperations.IsPow2(bound))
            return raw & (bound - 1);

        // m ← x * s
        // l ← m mod 2^L
        var result = Math.BigMul(raw, bound, out var low);

        // if l < s then
        if (low < bound)
        {
            // t ← (2^L - s) mod s
            var threshold = unchecked(0UL - bound) % bound;
            // while l < t do
            while (low < threshold)
            {
                // x ← random integer in [0, 2^L)
                raw = random.Next();
                // m ← x * s
                // l ← m mod 2^L
                result = Math.BigMul(raw, bound, out low);
            }
        }

        // return m / 2^L
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float NextBoundedFloat(ICitadelRandom random, float low, float high)
    {
        ValidateFloatingBounds(low, high);
        var sample = random.NextFloat();

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return low == high ? low : ScaleFloating(low, high, sample);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double NextBoundedDouble(ICitadelRandom random, double low, double high)
    {
        ValidateFloatingBounds(low, high);
        var sample = random.NextDouble();

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return low == high ? low : ScaleFloating(low, high, sample);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateFloatingBounds<T>(T low, T high) where T : IFloatingPointIeee754<T>
    {
        if (!T.IsFinite(low))
            throw new ArgumentOutOfRangeException(nameof(low), "Bounds must be finite.");
        if (!T.IsFinite(high) || low > high)
        {
            throw new ArgumentOutOfRangeException(nameof(high),
                "The upper bound must be finite and at least the lower bound.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ScaleFloating<T>(T low, T high, T sample) where T : IFloatingPointIeee754<T>
    {
        var width = high - low;
        var value = T.IsFinite(width)
            ? low + width * sample
            // If the width is infinite, high - low overflowed,
            // so use a different strategy; scale the endpoints
            // and then sample.
            : low * (T.One - sample) + high * sample;

        // Scaling can round to high, preserve exclusive upper bound semantics
        return value >= high ? T.BitDecrement(high) : value;
    }
}
