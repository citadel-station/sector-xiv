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

        for (var i = 0; i < 4; i++)
        {
            _state[i] = splitMix.Next();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        var s0 = _state[0];
        var s1 = _state[1];
        var s2 = _state[2];
        var s3 = _state[3];

        var result = _rotl(s0 + s3, 23) + s0;

        var t = s1 << 17;

        s2 ^= s0;
        s3 ^= s1;
        s1 ^= s2;
        s0 ^= s3;

        s2 ^= t;

        s3 = _rotl(s3, 45);

        _state[0] = s0;
        _state[1] = s1;
        _state[2] = s2;
        _state[3] = s3;

        return result;
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

    ICitadelRandom ICitadelRandom.Clone()
    {
        return Clone();
    }

    private static Xoshiro256State CreateStateJumpMagic()
    {
        var stateJumpMagic = new Xoshiro256State();
        stateJumpMagic[0] = 0x180ec6d33cfd0aba;
        stateJumpMagic[1] = 0xd5a61266f0c9392c;
        stateJumpMagic[2] = 0xa9582618e03fc9aa;
        stateJumpMagic[3] = 0x39abdc4529b1661c;
        return stateJumpMagic;
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
        var stateLongJumpMagic = new Xoshiro256State();
        stateLongJumpMagic[0] = 0x76e15d3efefdcbbf;
        stateLongJumpMagic[1] = 0xc5004e441c522fb3;
        stateLongJumpMagic[2] = 0x77710069854ee241;
        stateLongJumpMagic[3] = 0x39109bb02acbe635;
        return stateLongJumpMagic;
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
        var newState = new Xoshiro256State();
        newState[0] = 0;
        newState[1] = 0;
        newState[2] = 0;
        newState[3] = 0;

        for (var i = 0; i < 4; i++)
        {
            for (var b = 0; b < 64; b++)
            {
                if ((jumpMagic[i] & (1UL << b)) != 0)
                {
                    newState[0] ^= _state[0];
                    newState[1] ^= _state[1];
                    newState[2] ^= _state[2];
                    newState[3] ^= _state[3];
                }

                Next();
            }
        }

        for (var i = 0; i < 4; i++)
        {
            _state[i] = newState[i];
        }
    }

    private static Xoshiro256State CreateCharacteristicPolynomial()
    {
        var characteristicPolynomial = new Xoshiro256State();
        characteristicPolynomial[0] = 0x9d116f2bb0f0f001;
        characteristicPolynomial[1] = 0x0280002bcefd1a5e;
        characteristicPolynomial[2] = 0x04b4edcf26259f85;
        characteristicPolynomial[3] = 0x0003c03c3f3ecb19;
        return characteristicPolynomial;
    }

    // ReSharper disable once IdentifierTypo
    private static void F2X_mulx(ref Xoshiro256State state)
    {
        ulong carry = 0;
        for (var i = 0; i < 4; i++)
        {
            var nextCarry = state[i] >> 63;
            state[i] = (state[i] << 1) | carry;
            carry = nextCarry;
        }

        var top = (int)carry;

        if (top != 0)
        {
            for (var i = 0; i < 4; i++)
            {
                state[i] ^= CharacteristicPolynomial[i];
            }
        }
    }

    private static void F2X_mulmod(ref Xoshiro256State a, in Xoshiro256State b)
    {
        var r = new Xoshiro256State();
        r[0] = 0;
        r[1] = 0;
        r[2] = 0;
        r[3] = 0;

        for (var k = 255; k >= 0; k--)
        {
            F2X_mulx(ref r);
            if (((a[k >> 6] >> (k & 63)) & 1) != 0)
            {
                for (var i = 0; i < 4; i++)
                {
                    r[i] ^= b[i];
                }
            }
        }

        for (var i = 0; i < 4; i++)
        {
            a[i] = r[i];
        }
    }

    private static void F2X_jumpPolyCE(ulong c, uint e, ref Xoshiro256State result)
    {
        result[0] = 1;
        result[1] = 0;
        result[2] = 0;
        result[3] = 0;

        for (var k = 63; k >= 0; k--)
        {
            F2X_mulmod(ref result, result);
            if (((c >> k) & 1) != 0)
                F2X_mulx(ref result);
        }

        while (e-- > 0)
        {
            F2X_mulmod(ref result, result);
        }
    }

    private static Xoshiro256State F2X_jumpPolyN(in Xoshiro256State jump)
    {
        var result = new Xoshiro256State();

        result[0] = 1;
        result[1] = 0;
        result[2] = 0;
        result[3] = 0;

        for (var k = 255; k >= 0; k--)
        {
            F2X_mulmod(ref result, result);
            if (((jump[k >> 6] >> (k & 63)) & 1) != 0)
                F2X_mulx(ref result);
        }

        return result;
    }

    private void ApplyJumpPolynomial(in Xoshiro256State poly)
    {
        var newState = new Xoshiro256State();
        newState[0] = 0;
        newState[1] = 0;
        newState[2] = 0;
        newState[3] = 0;

        for (var i = 0; i < 4; i++)
        {
            for (var b = 0; b < 64; b++)
            {
                if ((poly[i] & (1UL << b)) != 0)
                {
                    newState[0] ^= _state[0];
                    newState[1] ^= _state[1];
                    newState[2] ^= _state[2];
                    newState[3] ^= _state[3];
                }

                Next();
            }
        }

        for (var i = 0; i < 4; i++)
        {
            _state[i] = newState[i];
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
    /// n = jump[0] + jump[1] * 2^64 + ... positions.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    public void JumpN(ulong a, ulong b, ulong c, ulong d)
    {
        var jump = new Xoshiro256State();
        jump[0] = a;
        jump[1] = b;
        jump[2] = c;
        jump[3] = d;

        var poly = F2X_jumpPolyN(jump);
        ApplyJumpPolynomial(poly);
    }

    [InlineArray(4)]
    public struct Xoshiro256State
    {
        public ulong value;
    }
}
