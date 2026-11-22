using System;
using Content.Shared._Citadel.Random;
using NUnit.Framework;

namespace Content.Tests.Shared._Citadel.Random;

/// <summary>
/// These tests are specific to the implementation as of right now,
/// unfortunately. So, something like 'test 32 bit'
/// assumes scaling happens by extracting the top 32 bits.
/// <br />
/// <br />
/// That isn't ideal for an interface tests, but.. whatever.
/// </summary>
[TestFixture]
[TestOf(typeof(ICitadelRandom))]
public sealed class CitadelRandomTests
{
    [Test]
    public void Typed32BitMethodsCoverTheFullSignedAndUnsignedRanges()
    {
        // right now, citadel random takes
        // the upper 32 bits for i32/u32.
        ICitadelRandom random = new ScriptedRandom(
            0,
            0x7FFFFFFF00000000,
            0x8000000000000000,
            ulong.MaxValue,
            0,
            0x7FFFFFFF00000000,
            0x8000000000000000,
            ulong.MaxValue);

        Assert.That(random.NextI32(), Is.Zero);
        Assert.That(random.NextI32(), Is.EqualTo(int.MaxValue));
        Assert.That(random.NextI32(), Is.EqualTo(int.MinValue));
        Assert.That(random.NextI32(), Is.EqualTo(-1));
        Assert.That(random.NextU32(), Is.Zero);
        Assert.That(random.NextU32(), Is.EqualTo((uint)int.MaxValue));
        Assert.That(random.NextU32(), Is.EqualTo(1U << 31));
        Assert.That(random.NextU32(), Is.EqualTo(uint.MaxValue));
    }

    [Test]
    public void Typed64BitMethodsCoverTheFullSignedAndUnsignedRanges()
    {
        ICitadelRandom random = new ScriptedRandom(0, long.MaxValue, 1UL << 63, ulong.MaxValue, 0, ulong.MaxValue);

        Assert.That(random.NextI64(), Is.Zero);
        Assert.That(random.NextI64(), Is.EqualTo(long.MaxValue));
        Assert.That(random.NextI64(), Is.EqualTo(long.MinValue));
        Assert.That(random.NextI64(), Is.EqualTo(-1L));
        Assert.That(random.NextU64(), Is.Zero);
        Assert.That(random.NextU64(), Is.EqualTo(ulong.MaxValue));
    }

    [Test]
    public void FloatUses24BitsAndExcludesOne()
    {
        ICitadelRandom random = new ScriptedRandom(0, 1UL << 63, ulong.MaxValue);

        Assert.That(random.NextFloat(), Is.Zero);
        Assert.That(random.NextFloat(), Is.EqualTo(0.5f));
        Assert.That(random.NextFloat(), Is.EqualTo(float.BitDecrement(1)));
    }

    [Test]
    public void SignedBoundsIncludeLowAndExcludeHighAcrossZero()
    {
        // This only works with the current version; because we take
        // two and multiply for the draw-two algorithm, this works to force a low and high value.
        ICitadelRandom random = new ScriptedRandom(0, ulong.MaxValue);

        // because these are power-of-two bound width, it consumes one per.
        Assert.That(random.NextBoundedI64(-8, 8), Is.EqualTo(-8L));
        Assert.That(random.NextBoundedI64(-8, 8), Is.EqualTo(7L));
    }

    // TODO(silicons): add actual tests to test whether the algorithm from
    //                 https://arxiv.org/abs/1805.10941 is working.

    [Test]
    public void EqualBoundsReturnLowAndConsumeTheNormalDrawCount()
    {
        var source = new ScriptedRandom(0, ulong.MaxValue);
        ICitadelRandom random = source;
        Assert.That(random.NextBoundedFloat(-7, -7), Is.EqualTo(-7f));
        Assert.That(source.DrawCount, Is.EqualTo(1));
        Assert.That(random.NextBoundedDouble(7, 7), Is.EqualTo(7.0));
        Assert.That(source.DrawCount, Is.EqualTo(2));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void DifferentValidBoundsKeepGeneratorsInSync(bool useXoshiro)
    {
        ICitadelRandom ranged = useXoshiro ? new Xoshiro256(42) : new SplitMix64(42);
        var equal = ranged.Clone();

        for (var i = 0; i < 16; i++)
        {
            // Equal and power-of-two integer bounds consume one draw, so this works
            Assert.That(equal.NextBoundedI64(-7, -7), Is.EqualTo(-7L));
            ranged.NextBoundedI64(-7, 9);
            Assert.That(equal.Next(), Is.EqualTo(ranged.Next()));

            Assert.That(equal.NextBoundedU64(7, 7), Is.EqualTo(7UL));
            ranged.NextBoundedU64(7, 23);
            Assert.That(equal.Next(), Is.EqualTo(ranged.Next()));

            Assert.That(equal.NextBoundedFloat(-7, -7), Is.EqualTo(-7f));
            ranged.NextBoundedFloat(-float.MaxValue, float.MaxValue);
            Assert.That(equal.Next(), Is.EqualTo(ranged.Next()));

            Assert.That(equal.NextBoundedDouble(7, 7), Is.EqualTo(7.0));
            ranged.NextBoundedDouble(0, double.BitIncrement(0));
            Assert.That(equal.Next(), Is.EqualTo(ranged.Next()));
        }
    }

    [Test]
    public void BoundedBiasCheck()
    {
        ICitadelRandom random = new SplitMix64(46);
        var counts = new uint[64];

        const ulong iterations = 10_000_000;
        for (ulong i = 0; i < iterations; i++)
        {
            var val = random.NextBoundedU64(0, 64);
            counts[val]++;
        }

        for (var i = 0; i < counts.Length; i++)
        {
            var expected = iterations / (ulong)counts.Length;
            var actual = counts[i];
            var ratio = (double)actual / expected;
            Assert.That(ratio,
                Is.InRange(0.99, 1.01),
                $"Value {i} is outside of the expected range. Expected {expected}, got {actual}.");
        }
    }

    [Test]
    public void ReversedBoundsThrowWithoutConsumingRandomness()
    {
        var source = new ScriptedRandom();
        ICitadelRandom random = source;

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextBoundedI64(2, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextBoundedU64(2, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextBoundedFloat(2, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextBoundedDouble(2, 1));
        Assert.That(source.DrawCount, Is.Zero);
    }

    [TestCase(double.NaN, 1)]
    [TestCase(0, double.NaN)]
    [TestCase(double.NegativeInfinity, 1)]
    [TestCase(0, double.PositiveInfinity)]
    public void FloatingBoundsMustBeFinite(double low, double high)
    {
        var source = new ScriptedRandom();
        ICitadelRandom random = source;

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextBoundedFloat((float)low, (float)high));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextBoundedDouble(low, high));
        Assert.That(source.DrawCount, Is.Zero);
    }

    [Test]
    public void FloatingBoundsIncludeLowAndMidpoint()
    {
        ICitadelRandom random = new ScriptedRandom(0, 1UL << 63, 0, 1UL << 63);

        Assert.That(random.NextBoundedFloat(-10, 10), Is.EqualTo(-10f));
        Assert.That(random.NextBoundedFloat(-10, 10), Is.Zero);
        Assert.That(random.NextBoundedDouble(-10, 10), Is.EqualTo(-10.0));
        Assert.That(random.NextBoundedDouble(-10, 10), Is.Zero);
    }

    /// <summary>
    /// Tests that total scaled width turning into infinity
    /// will still work with the scaling system to produce finite results
    /// </summary>
    [Test]
    public void FloatingBoundsDoNotOverflowWhenTheIntervalWidthIsInfinite()
    {
        ICitadelRandom random = new ScriptedRandom(0, 1UL << 63, ulong.MaxValue, 0, 1UL << 63, ulong.MaxValue);

        Assert.That(random.NextBoundedFloat(-float.MaxValue, float.MaxValue), Is.EqualTo(-float.MaxValue));
        Assert.That(random.NextBoundedFloat(-float.MaxValue, float.MaxValue), Is.Zero);
        Assert.That(random.NextBoundedFloat(-float.MaxValue, float.MaxValue), Is.LessThan(float.MaxValue));
        Assert.That(random.NextBoundedDouble(-double.MaxValue, double.MaxValue), Is.EqualTo(-double.MaxValue));
        Assert.That(random.NextBoundedDouble(-double.MaxValue, double.MaxValue), Is.Zero);
        Assert.That(random.NextBoundedDouble(-double.MaxValue, double.MaxValue), Is.LessThan(double.MaxValue));
    }

    [Test]
    public void FloatingBoundsExcludeHighEvenForAdjacentEndpoints()
    {
        ICitadelRandom random = new ScriptedRandom(ulong.MaxValue, ulong.MaxValue);

        Assert.That(random.NextBoundedFloat(1, float.BitIncrement(1)), Is.EqualTo(1f));
        Assert.That(random.NextBoundedDouble(1, double.BitIncrement(1)), Is.EqualTo(1.0));
    }

    [Test]
    public void DoubleUsesAnExclusiveUpperBound()
    {
        ICitadelRandom random = new ScriptedRandom(0, 1UL << 63, ulong.MaxValue);

        Assert.That(random.NextDouble(), Is.Zero);
        Assert.That(random.NextDouble(), Is.EqualTo(0.5));
        Assert.That(random.NextDouble(), Is.EqualTo(double.BitDecrement(1)));
    }

    private sealed class ScriptedRandom(params ulong[] values) : ICitadelRandom
    {
        public int DrawCount { get; private set; }

        public ulong Next()
        {
            return values[DrawCount++];
        }

        public ICitadelRandom Clone()
        {
            return (ScriptedRandom)MemberwiseClone();
        }
    }
}
