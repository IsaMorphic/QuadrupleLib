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

using System.Numerics;
using System.Runtime.CompilerServices;

namespace QuadrupleLib;

public partial struct Float128
{
    #region Public API (complex number related)

    public static bool IsRealNumber(Float128 value)
    {
        return true;
    }

    public static bool IsImaginaryNumber(Float128 value)
    {
        return false;
    }

    public static bool IsComplexNumber(Float128 value)
    {
        return false;
    }

    #endregion

    #region Public API (infinity related)

    public static bool IsFinite(Float128 value)
    {
        return value.RawExponent != short.MaxValue;
    }

    public static bool IsInfinity(Float128 value)
    {
        return value.RawExponent == short.MaxValue && value.RawSignificand == UInt128.Zero;
    }

    public static bool IsPositiveInfinity(Float128 value)
    {
        return !value.RawSignBit && value.RawExponent == short.MaxValue && value.RawSignificand == UInt128.Zero;
    }

    public static bool IsNegativeInfinity(Float128 value)
    {
        return value.RawSignBit && value.RawExponent == short.MaxValue && value.RawSignificand == UInt128.Zero;
    }

    #endregion

    #region Public API (representation related)

    public static bool IsPow2(Float128 value)
    {
        return (value.RawExponent == 0 && UInt128.IsPow2(value.RawSignificand)) ||
            value.RawSignificand == UInt128.Zero;
    }

    public static bool IsCanonical(Float128 value)
    {
        return UInt128.LeadingZeroCount(value.RawSignificand) - 15 == 0;
    }

    public static bool IsNormal(Float128 value)
    {
        return value.RawExponent != 0;
    }

    public static bool IsSubnormal(Float128 value)
    {
        return value.RawExponent == 0;
    }

    public static bool IsNaN(Float128 value)
    {
        return value.RawExponent == short.MaxValue && value.RawSignificand != UInt128.Zero;
    }

    #endregion

    #region Public API (sign-magnitude related)

    public static Float128 operator +(Float128 value)
    {
        return value;
    }

    public static Float128 operator -(Float128 value)
    {
        value.RawSignBit = !value.RawSignBit;
        return value;
    }

    public static bool IsNegative(Float128 value)
    {
        return value.RawSignBit && (value.RawExponent != short.MaxValue || value.RawSignificand == UInt128.Zero);
    }

    public static bool IsPositive(Float128 value)
    {
        return !value.RawSignBit && (value.RawExponent != short.MaxValue || value.RawSignificand == UInt128.Zero);
    }

    public static bool IsZero(Float128 value)
    {
        return value.RawExponent == 0 && value.RawSignificand == UInt128.Zero;
    }

    public static Float128 Abs(Float128 value)
    {
        return new Float128(value._rawBits & ~SIGNBIT_MASK);
    }

    private static Float128 Sign(Float128 value)
    {
        if (IsZero(value))
            return Zero;
        else if (IsPositive(value))
            return One;
        else if (IsNegative(value))
            return NegativeOne;
        else
            return value;
    }

    public static Float128 MaxMagnitude(Float128 x, Float128 y)
    {
        if (x > y)
            return x;
        else
            return y;
    }

    public static Float128 MaxMagnitudeNumber(Float128 x, Float128 y)
    {
        if (IsNaN(x))
            return y;
        else if (IsNaN(y))
            return x;
        else if (x > y)
            return x;
        else
            return y;
    }

    public static Float128 MinMagnitude(Float128 x, Float128 y)
    {
        if (x < y)
            return x;
        else
            return y;
    }

    public static Float128 MinMagnitudeNumber(Float128 x, Float128 y)
    {
        if (IsNaN(x))
            return y;
        else if (IsNaN(y))
            return x;
        else if (x < y)
            return x;
        else
            return y;
    }

    #endregion

    #region Public API (conversion operators)

    public static explicit operator Float128(Half x)
    {
        if (Half.IsNaN(x))
        {
            return _qNaN;
        }
        else if (Half.IsPositiveInfinity(x))
        {
            return _pInf;
        }
        else if (Half.IsNegativeInfinity(x))
        {
            return _nInf;
        }
        else if (x == Half.Zero)
        {
            return Zero;
        }
        else
        {
            ushort bits = Unsafe.As<Half, ushort>(ref x);
            // Note that the shift is sign-extended, hence the test against -1 not 1
            bool negative = (bits & (1U << 15)) != 0;
            int exponent = (int)((bits >> 10) & 0x10U) - 0xf;
            UInt128 mantissa = bits & 0x3ffU;

            return new(mantissa << 102, exponent, negative);
        }
    }

    public static explicit operator Half(Float128 x)
    {
        if (IsNaN(x))
        {
            return Half.NaN;
        }
        else if (IsPositiveInfinity(x) || (x.Exponent >= 0xf && !x.RawSignBit))
        {
            return Half.PositiveInfinity;
        }
        else if (IsNegativeInfinity(x) || (x.Exponent >= 0xf && x.RawSignBit))
        {
            return Half.NegativeInfinity;
        }
        else if (IsSubnormal(x) || x.Exponent <= -0xe)
        {
            return Half.Zero;
        }
        else
        {
            var smallMantissa = (ushort)(x.RawSignificand >> 99);
            smallMantissa &= unchecked((ushort)(ushort.MaxValue << 1));
            smallMantissa |= Math.Min((ushort)(x.RawSignificand & ((UInt128.One << 99) - 1)), (ushort)1);

            if ((((smallMantissa & 1) |
                 ((smallMantissa >> 2) & 1)) &
                 ((smallMantissa >> 1) & 1)) == 1) // check rounding condition
            {
                smallMantissa++;
            }

            ushort result;
            if (BitOperations.TrailingZeroCount(smallMantissa >> 3) == 10)
            {
                result =
                    (ushort)(((ushort)((x.Exponent + 0x10) & 0x3f) << 10) |
                    (x.RawSignBit ? 1 << 15 : 0));
            }
            else
            {
                result =
                    (ushort)((ushort)((smallMantissa >> 3) & 0x3ffU) |
                    ((ushort)((x.Exponent + 0xf) & 0x3f) << 10) |
                    (x.RawSignBit ? 1 << 15 : 0));
            }

            return Unsafe.As<ushort, Half>(ref result);
        }
    }

    public static implicit operator Float128(float x)
    {
        if (float.IsNaN(x))
        {
            return _qNaN;
        }
        else if (float.IsPositiveInfinity(x))
        {
            return _pInf;
        }
        else if (float.IsNegativeInfinity(x))
        {
            return _nInf;
        }
        else if (x == 0.0)
        {
            return Zero;
        }
        else
        {
            uint bits = Unsafe.As<float, uint>(ref x);
            // Note that the shift is sign-extended, hence the test against -1 not 1
            bool negative = (bits & (1U << 31)) != 0;
            int exponent = (int)((bits >> 23) & 0xffU) - 0x7f;
            UInt128 mantissa = bits & 0x7fffffU;

            return new(mantissa << 89, exponent, negative);
        }
    }

    public static explicit operator float(Float128 x)
    {
        if (IsNaN(x))
        {
            return float.NaN;
        }
        else if (IsPositiveInfinity(x) || (x.Exponent >= 0x7f && !x.RawSignBit))
        {
            return float.PositiveInfinity;
        }
        else if (IsNegativeInfinity(x) || (x.Exponent >= 0x7f && x.RawSignBit))
        {
            return float.NegativeInfinity;
        }
        else if (IsSubnormal(x) || x.Exponent <= -0xfe)
        {
            return 0.0f;
        }
        else
        {
            var smallMantissa = (uint)(x.RawSignificand >> 86);
            smallMantissa &= uint.MaxValue << 1;
            smallMantissa |= Math.Min((uint)(x.RawSignificand & ((UInt128.One << 86) - 1)), 1);

            if ((((smallMantissa & 1) |
                 ((smallMantissa >> 2) & 1)) &
                 ((smallMantissa >> 1) & 1)) == 1) // check rounding condition
            {
                smallMantissa++;
            }

            uint result;
            if (BitOperations.TrailingZeroCount(smallMantissa >> 3) == 23)
            {
                result =
                    ((uint)((x.Exponent + 0x80) & 0xff) << 23) |
                    (x.RawSignBit ? 1U << 31 : 0);
            }
            else
            {
                result =
                    ((smallMantissa >> 3) & 0x7fffffU) |
                    ((uint)((x.Exponent + 0x7f) & 0xff) << 23) |
                    (x.RawSignBit ? 1U << 31 : 0);
            }

            return Unsafe.As<uint, float>(ref result);
        }
    }

    public static implicit operator Float128(double x)
    {
        if (double.IsNaN(x))
        {
            return _qNaN;
        }
        else if (double.IsPositiveInfinity(x))
        {
            return _pInf;
        }
        else if (double.IsNegativeInfinity(x))
        {
            return _nInf;
        }
        else if (x == 0.0)
        {
            return Zero;
        }
        else
        {
            ulong bits = Unsafe.As<double, ulong>(ref x);
            // Note that the shift is sign-extended, hence the test against -1 not 1
            bool negative = (bits & (1UL << 63)) != 0;
            int exponent = (int)((bits >> 52) & 0x7ffUL) - 0x3ff;
            UInt128 mantissa = bits & 0xfffffffffffffUL;

            return new(mantissa << 60, exponent, negative);
        }
    }

    public static explicit operator double(Float128 x)
    {
        if (IsNaN(x))
        {
            return double.NaN;
        }
        else if (IsPositiveInfinity(x) || (x.Exponent >= 0x3ff && !x.RawSignBit))
        {
            return double.PositiveInfinity;
        }
        else if (IsNegativeInfinity(x) || (x.Exponent >= 0x3ff && x.RawSignBit))
        {
            return double.NegativeInfinity;
        }
        else if (IsSubnormal(x) || x.Exponent <= -0x3fe)
        {
            return 0.0;
        }
        else
        {
            var smallMantissa = (ulong)(x.RawSignificand >> 57);
            smallMantissa &= ulong.MaxValue << 1;
            smallMantissa |= Math.Min((ulong)(x.RawSignificand & ((UInt128.One << 57) - 1)), 1);

            if ((((smallMantissa & 1) |
                 ((smallMantissa >> 2) & 1)) &
                 ((smallMantissa >> 1) & 1)) == 1) // check rounding condition
            {
                smallMantissa++;
            }

            ulong result;
            if (BitOperations.TrailingZeroCount(smallMantissa >> 3) == 52)
            {
                result =
                    ((ulong)((x.Exponent + 0x400) & 0x7ff) << 52) |
                    (x.RawSignBit ? 1UL << 63 : 0);
            }
            else
            {
                result =
                    ((smallMantissa >> 3) & 0xfffffffffffffUL) |
                    ((ulong)((x.Exponent + 0x3ff) & 0x7ff) << 52) |
                    (x.RawSignBit ? 1UL << 63 : 0);
            }

            return Unsafe.As<ulong, double>(ref result);
        }
    }

    #endregion
}
