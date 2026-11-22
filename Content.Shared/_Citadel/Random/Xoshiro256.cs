using System.Numerics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Content.Shared._Citadel.Random;

/// <summary>
/// An implementation of the Xoshiro256++ random number generator algorithm.
/// <br />
/// <br />
/// TODO(silicons): Generalize this file to linear pseudorandom number generators. The polynomial terms and degrees are
/// currently hardcoded.
/// </summary>
/// <param name="seed"></param>
[PublicAPI]
public sealed class Xoshiro256 : ICitadelRandom
{
    private static readonly Xoshiro256State StateJumpMagic = CreateStateJumpMagic();

    private static readonly Xoshiro256State StateLongJumpMagic = CreateStateLongJumpMagic();

    private static readonly Xoshiro256State CharacteristicPolynomial = CreateCharacteristicPolynomial();

    private Xoshiro256State _state;

    public Xoshiro256(ulong seed)
    {
        var splitMix = new SplitMix64(seed);

        _state = new Xoshiro256State(splitMix.Next(), splitMix.Next(), splitMix.Next(), splitMix.Next());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        var s0 = _state.V1;
        var s1 = _state.V2;
        var s2 = _state.V3;
        var s3 = _state.V4;

        var result = _rotl(s0 + s3, 23) + s0;

        var t = s1 << 17;

        s2 ^= s0;
        s3 ^= s1;
        s1 ^= s2;
        s0 ^= s3;

        s2 ^= t;

        s3 = _rotl(s3, 45);

        _state.V1 = s0;
        _state.V2 = s1;
        _state.V3 = s2;
        _state.V4 = s3;

        return result;
    }

    ICitadelRandom ICitadelRandom.Clone()
    {
        return Clone();
    }

    /// <summary>
    /// Rotates the bits of a 64-bit unsigned integer to the left by a specified number of positions.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    private static ulong _rotl(ulong x, int k)
    {
        return BitOperations.RotateLeft(x, k);
    }


    /// <summary>
    /// Copies the current state into an independent generator.
    /// </summary>
    public Xoshiro256 Clone()
    {
        return (Xoshiro256)MemberwiseClone();
    }

    private static Xoshiro256State CreateStateJumpMagic()
    {
        return new Xoshiro256State(
            0x180ec6d33cfd0aba,
            0xd5a61266f0c9392c,
            0xa9582618e03fc9aa,
            0x39abdc4529b1661c);
    }

    /// <summary>
    /// Jumps forwards by 2^128 positions.
    /// Can be used to generate 2^128 non-overlapping subsequences for parallel computations.
    /// </summary>
    public void Jump()
    {
        JumpImpl(StateJumpMagic);
    }

    private static Xoshiro256State CreateStateLongJumpMagic()
    {
        return new Xoshiro256State(
            0x76e15d3efefdcbbf,
            0xc5004e441c522fb3,
            0x77710069854ee241,
            0x39109bb02acbe635);
    }

    /// <summary>
    /// Jumps forwards by 2^192 positions.
    /// </summary>
    public void LongJump()
    {
        JumpImpl(StateLongJumpMagic);
    }

    private void JumpImpl(in Xoshiro256State jumpMagic)
    {
        ApplyJumpPolynomial(jumpMagic);
    }

    private static Xoshiro256State CreateCharacteristicPolynomial()
    {
        return new Xoshiro256State(
            0x9d116f2bb0f0f001,
            0x0280002bcefd1a5e,
            0x04b4edcf26259f85,
            0x0003c03c3f3ecb19);
    }

    // ReSharper disable once IdentifierTypo
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void F2X_mulx(ref Xoshiro256State state)
    {
        var carry = state.V4 >> 63;
        state.V4 = (state.V4 << 1) | (state.V3 >> 63);
        state.V3 = (state.V3 << 1) | (state.V2 >> 63);
        state.V2 = (state.V2 << 1) | (state.V1 >> 63);
        state.V1 <<= 1;

        if (carry != 0)
        {
            state.V1 ^= CharacteristicPolynomial.V1;
            state.V2 ^= CharacteristicPolynomial.V2;
            state.V3 ^= CharacteristicPolynomial.V3;
            state.V4 ^= CharacteristicPolynomial.V4;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void F2X_mulmod(ref Xoshiro256State a, in Xoshiro256State b)
    {
        var r = new Xoshiro256State();
        F2X_mulmodWord(a.V4, b, ref r);
        F2X_mulmodWord(a.V3, b, ref r);
        F2X_mulmodWord(a.V2, b, ref r);
        F2X_mulmodWord(a.V1, b, ref r);
        a = r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void F2X_mulmodWord(ulong word, in Xoshiro256State b, ref Xoshiro256State result)
    {
        for (var bit = 63; bit >= 0; bit--)
        {
            F2X_mulx(ref result);
            if (((word >> bit) & 1) != 0)
            {
                result.V1 ^= b.V1;
                result.V2 ^= b.V2;
                result.V3 ^= b.V3;
                result.V4 ^= b.V4;
            }
        }
    }

    private static void F2X_jumpPolyCE(ulong c, uint e, ref Xoshiro256State result)
    {
        result = new Xoshiro256State(1, 0, 0, 0);

        F2X_jumpPolyWord(c, ref result);

        while (e-- > 0)
        {
            F2X_mulmod(ref result, result);
        }
    }

    private static Xoshiro256State F2X_jumpPolyN(in Xoshiro256State jump)
    {
        var result = new Xoshiro256State(1, 0, 0, 0);
        F2X_jumpPolyWord(jump.V4, ref result);
        F2X_jumpPolyWord(jump.V3, ref result);
        F2X_jumpPolyWord(jump.V2, ref result);
        F2X_jumpPolyWord(jump.V1, ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void F2X_jumpPolyWord(ulong word, ref Xoshiro256State result)
    {
        for (var bit = 63; bit >= 0; bit--)
        {
            F2X_mulmod(ref result, result);
            if (((word >> bit) & 1) != 0)
                F2X_mulx(ref result);
        }
    }

    private void ApplyJumpPolynomial(in Xoshiro256State poly)
    {
        var newState = new Xoshiro256State();
        AccumulateJumpWord(poly.V1, ref newState);
        AccumulateJumpWord(poly.V2, ref newState);
        AccumulateJumpWord(poly.V3, ref newState);
        AccumulateJumpWord(poly.V4, ref newState);
        _state = newState;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AccumulateJumpWord(ulong word, ref Xoshiro256State result)
    {
        for (var bit = 0; bit < 64; bit++)
        {
            if ((word & (1UL << bit)) != 0)
            {
                result.V1 ^= _state.V1;
                result.V2 ^= _state.V2;
                result.V3 ^= _state.V3;
                result.V4 ^= _state.V4;
            }

            Next();
        }
    }

    /// <summary>
    /// Jumps forwards by c * 2^e positions.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="e"></param>
    // ReSharper disable once InconsistentNaming
    public void JumpCE(ulong c, uint e)
    {
        var poly = new Xoshiro256State();
        F2X_jumpPolyCE(c, e, ref poly);
        ApplyJumpPolynomial(poly);
    }

    /// <summary>
    /// General arbitrary-jump function; Jumps forwards by
    /// n = a + b * 2^64 + c * 2^128 + d * 2^192 positions.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    public void JumpN(ulong a, ulong b, ulong c, ulong d)
    {
        var jump = new Xoshiro256State(a, b, c, d);

        var poly = F2X_jumpPolyN(jump);
        ApplyJumpPolynomial(poly);
    }

    public struct Xoshiro256State(ulong v1, ulong v2, ulong v3, ulong v4)
    {
        public ulong V1 = v1;
        public ulong V2 = v2;
        public ulong V3 = v3;
        public ulong V4 = v4;
    }
}
