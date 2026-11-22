namespace Content.Shared._Citadel.Random;

/// <summary>
/// Citadel 64-bit PRNG interface. Derived samples assume uniformly distributed output from <see cref="Next" />.
/// </summary>
public interface ICitadelRandom : ICloneable
{
    object ICloneable.Clone()
    {
        return Clone();
    }

    /// <summary>
    /// Returns the next 64-bit unsigned integer from the PRNG.
    /// <br />
    /// <br />
    /// The PRNG is required, and trusted, to return
    /// a uniform distribution over the 64 bit span.
    /// <br />
    /// All other functions require that for proper entropy.
    /// </summary>
    ulong Next();

    /// <summary>
    /// Copies the current state into an independent generator with the same subsequent sequence.
    /// </summary>
    new ICitadelRandom Clone();

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// </summary>
    double NextDouble()
    {
        return (Next() >> 11) * (1.0 / (1UL << 53));
    }

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// </summary>
    float NextFloat()
    {
        return (Next() >> 40) * (1.0f / (1U << 24));
    }

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// </summary>
    int NextI32()
    {
        return unchecked((int)(Next() >> 32));
    }

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// </summary>
    uint NextU32()
    {
        return (uint)(Next() >> 32);
    }

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// </summary>
    long NextI64()
    {
        return unchecked((long)Next());
    }

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// </summary>
    ulong NextU64()
    {
        return Next();
    }

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// <br />
    /// <br />
    /// This will draw from the generator an arbitrary number of times, to produce an unbiased sample.
    /// </summary>
    long NextBoundedI64(long low, long high)
    {
        return CitadelRandomExtensions.NextBoundedI64(this, low, high);
    }

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// <br />
    /// <br />
    /// This will draw from the generator an arbitrary number of times, to produce an unbiased sample.
    /// </summary>
    ulong NextBoundedU64(ulong low, ulong high)
    {
        return CitadelRandomExtensions.NextBoundedU64(this, low, high);
    }

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// </summary>
    float NextBoundedFloat(float low, float high)
    {
        return CitadelRandomExtensions.NextBoundedFloat(this, low, high);
    }

    /// <summary>
    /// Performs a uniform draw of [low, high)
    /// </summary>
    double NextBoundedDouble(double low, double high)
    {
        return CitadelRandomExtensions.NextBoundedDouble(this, low, high);
    }
}
