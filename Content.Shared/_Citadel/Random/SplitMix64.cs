using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Content.Shared._Citadel.Random;

/// <summary>
/// A simple implementation of the SplitMix64 random number generator algorithm.
/// </summary>
/// <param name="seed"></param>
[PublicAPI]
public sealed class SplitMix64(ulong seed) : ICitadelRandom
{
    private const ulong StateMagic = 0x9e3779b97f4a7c15;

    private ulong _state = seed;

    /// <summary>
    /// Generates a random 64-bit integer using the SplitMix64 algorithm.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        _state += StateMagic; // increment the state variable
        var z = _state; // copy the state to a working variable

        // xor the variable with the variable right bit shifted 30
        // then multiply by a constant
        z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;

        // xor the variable with the variable right bit shifted 27
        // then multiply by a constant
        z = (z ^ (z >> 27)) * 0x94d049bb133111eb;

        // return the variable xored with itself right bit shifted 31
        return z ^ (z >> 31);
    }

    /// <summary>
    /// Copies the current state into an independent generator.
    /// </summary>
    public SplitMix64 Clone()
    {
        return (SplitMix64)MemberwiseClone();
    }

    ICitadelRandom ICitadelRandom.Clone()
    {
        return Clone();
    }


    /// <summary>
    /// Jumps the generator forward by a specified number of iterations.
    /// </summary>
    /// <param name="iterations"></param>
    public void Jump(ulong iterations)
    {
        _state += StateMagic * iterations;
    }
}
