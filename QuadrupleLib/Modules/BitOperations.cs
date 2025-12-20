/*
 *  Copyright 2024-2026 Chosen Few Software
 *  This file is part of QuadrupleLib.
 *
 *  QuadrupleLib is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  QuadrupleLib is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with QuadrupleLib.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Runtime.InteropServices;

namespace QuadrupleLib;

public partial struct Float128
{
    #region Public API (storage representation related)

    public int GetExponentByteCount()
    {
        return 2;
    }

    public int GetExponentShortestBitLength()
    {
        return 15 - short.LeadingZeroCount(Exponent);
    }

    public int GetSignificandBitLength()
    {
        return 113 - (int)UInt128.LeadingZeroCount(Significand);
    }

    public int GetSignificandByteCount()
    {
        return 14;
    }

    public bool TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = GetExponentByteCount();
        Span<byte> exponentBytes = destination.Slice(0, bytesWritten);

        bool flag = BitConverter.TryWriteBytes(exponentBytes, Exponent);
        if (BitConverter.IsLittleEndian)
        {
            exponentBytes.Reverse();
        }
        return flag;
    }

    public bool TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = GetExponentByteCount();
        Span<byte> exponentBytes = destination.Slice(0, bytesWritten);

        bool flag = BitConverter.TryWriteBytes(exponentBytes, Exponent);
        if (!BitConverter.IsLittleEndian)
        {
            exponentBytes.Reverse();
        }
        return flag;
    }

    public bool TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = GetSignificandByteCount();
        Span<byte> significandBytes = destination.Slice(0, bytesWritten);

        UInt128 significand = Significand;
        bool flag = MemoryMarshal.TryWrite(significandBytes, in significand);
        if (BitConverter.IsLittleEndian)
        {
            significandBytes.Reverse();
        }
        return flag;
    }

    public bool TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = GetSignificandByteCount();
        Span<byte> significandBytes = destination.Slice(0, bytesWritten);

        UInt128 significand = Significand;
        bool flag = MemoryMarshal.TryWrite(significandBytes, in significand);
        if (!BitConverter.IsLittleEndian)
        {
            significandBytes.Reverse();
        }
        return flag;
    }

    public static Float128 BitDecrement(Float128 x)
    {
        UInt128 newSignificand = x.Significand - 1;
        int normDist = (int)UInt128.LeadingZeroCount(newSignificand) - 15;
        if (normDist > 0)
        {
            return new Float128(newSignificand << normDist, x.Exponent - normDist, x.RawSignBit);
        }
        else
        {
            return new Float128(newSignificand, x.Exponent, x.RawSignBit);
        }
    }

    public static Float128 BitIncrement(Float128 x)
    {
        UInt128 newSignificand = x.Significand + 1;
        int normDist = 15 - (int)UInt128.LeadingZeroCount(newSignificand);
        if (normDist > 0)
        {
            return new Float128(newSignificand >> normDist, x.Exponent + normDist, x.RawSignBit);
        }
        else
        {
            return new Float128(newSignificand, x.Exponent, x.RawSignBit);
        }
    }

    public static Float128 ScaleB(Float128 x, int n)
    {
        if (IsNaN(x)) return _qNaN;
        else if (x == Zero) return Zero;
        else if (n == 0) return x;

        int normDist, newExponent = x.Exponent + n;
        if (newExponent > EXPONENT_BIAS)
        {
            return x.RawSignBit ? _nInf : _pInf;
        }
        else if (newExponent < -EXPONENT_BIAS + 1)
        {
            normDist = newExponent - (-EXPONENT_BIAS + 1);
            UInt128 newSignificand = (x.RawSignificand << 3) >> normDist;

            // set sticky bit
            newSignificand &= UInt128.MaxValue << 1;
            newSignificand |= UInt128.Min(newSignificand & ((UInt128.One << normDist) - 1), 1);

            if ((((newSignificand & 1) |
                 ((newSignificand >> 2) & 1)) &
                 ((newSignificand >> 1) & 1)) == 1) // check rounding condition
            {
                newSignificand++; // increment pth bit from the left
            }

            return new Float128(newSignificand >> 3, -EXPONENT_BIAS + 1, x.RawSignBit);
        }
        else
        {
            normDist = (int)UInt128.LeadingZeroCount(x.Significand) - 15;
            return new Float128(x.RawSignificand << normDist, newExponent - normDist, x.RawSignBit);
        }
    }

    public static Float128 operator &(Float128 left, Float128 right)
    {
        return new Float128(left._rawBits & right._rawBits);
    }

    public static Float128 operator |(Float128 left, Float128 right)
    {
        return new Float128(left._rawBits | right._rawBits);
    }

    public static Float128 operator ^(Float128 left, Float128 right)
    {
        return new Float128(left._rawBits ^ right._rawBits);
    }

    public static Float128 operator ~(Float128 value)
    {
        return new Float128(~value._rawBits);
    }

    #endregion
}
