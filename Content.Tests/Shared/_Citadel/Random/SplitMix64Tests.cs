using Content.Shared._Citadel.Random;
using NUnit.Framework;

namespace Content.Tests.Shared._Citadel.Random;

[TestFixture(0UL)]
[TestFixture(1UL)]
[TestFixture(0x0123456789ABCDEFUL)]
[TestFixture(ulong.MaxValue)]
[TestOf(typeof(SplitMix64))]
public sealed class SplitMix64Tests(ulong seed)
{
    [Test]
    public void SameSeedProducesSameSequence()
    {
        var first = new SplitMix64(seed);
        var second = new SplitMix64(seed);

        AssertSameSequence(first, second);
    }

    [Test]
    public void BiasCheck()
    {
        var random = new SplitMix64(seed);
        var counts = new uint[64];

        const ulong iterations = 10_000_000;
        for (ulong i = 0; i < iterations; i++)
        {
            var value = random.Next() % (ulong)counts.Length;
            counts[value]++;
        }

        var expected = iterations / (ulong)counts.Length;
        for (var value = 0; value < counts.Length; value++)
        {
            var actual = counts[value];
            var ratio = (double)actual / expected;
            Assert.That(ratio, Is.InRange(0.99, 1.01),
                $"Value {value} is outside of the expected range. Expected {expected}, got {actual}.");
        }
    }

    [TestCase(5UL)]
    [TestCase(10UL)]
    [TestCase(20UL)]
    public void JumpMatchesManualAdvance(ulong amount)
    {
        var manual = new SplitMix64(seed);
        var jumped = manual.Clone();

        for (ulong i = 0; i < amount; i++)
        {
            manual.Next();
        }

        jumped.Jump(amount);

        AssertSameSequence(manual, jumped);
    }

    [Test]
    public void CloneCopiesCurrentStateAndAdvancesIndependently()
    {
        var original = new SplitMix64(seed);
        for (var i = 0; i < 7; i++)
            original.Next();

        var clone = original.Clone();

        Assert.That(clone, Is.Not.SameAs(original));

        for (var i = 0; i < 16; i++)
        {
            var expected = original.Next();
            Assert.That(clone.Next(), Is.EqualTo(expected));
        }
    }

    private static void AssertSameSequence(SplitMix64 expected, SplitMix64 actual)
    {
        for (var i = 0; i < 16; i++)
        {
            Assert.That(actual.Next(), Is.EqualTo(expected.Next()), $"Output {i} differs.");
        }
    }
}
