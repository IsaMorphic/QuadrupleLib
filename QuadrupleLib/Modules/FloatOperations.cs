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

using QuadrupleLib.Annotations;

public partial struct Float128<TAccelerator>
{
    #region Public API (complex number related)

    public static bool IsRealNumber(Float128<TAccelerator> value)
    {
        return true;
    }

    public static bool IsImaginaryNumber(Float128<TAccelerator> value)
    {
        return false;
    }

    public static bool IsComplexNumber(Float128<TAccelerator> value)
    {
        return false;
    }

    #endregion

    #region Public API (infinity related)

    [MakeNonGeneric]
    public static bool IsFinite(Float128<TAccelerator> value)
    {
        return value.RawExponent != short.MaxValue;
    }

    [MakeNonGeneric]
    public static bool IsInfinity(Float128<TAccelerator> value)
    {
        return value.RawExponent == short.MaxValue && value.RawSignificand == UInt128.Zero;
    }

    [MakeNonGeneric]
    public static bool IsPositiveInfinity(Float128<TAccelerator> value)
    {
        return !value.RawSignBit && value.RawExponent == short.MaxValue && value.RawSignificand == UInt128.Zero;
    }

    [MakeNonGeneric]
    public static bool IsNegativeInfinity(Float128<TAccelerator> value)
    {
        return value.RawSignBit && value.RawExponent == short.MaxValue && value.RawSignificand == UInt128.Zero;
    }

    #endregion

    #region Public API (representation related)

    public static bool IsPow2(Float128<TAccelerator> value)
    {
        return value > Zero && ((value.RawExponent == 0 && UInt128.IsPow2(value.RawSignificand)) ||
            (value.RawSignificand == UInt128.Zero && value.RawExponent != short.MaxValue));
    }

    public static bool IsCanonical(Float128<TAccelerator> value)
    {
        return true;
    }

    [MakeNonGeneric]
    public static bool IsNormal(Float128<TAccelerator> value)
    {
        return value.RawExponent != 0 && value.RawExponent != short.MaxValue;
    }

    [MakeNonGeneric]
    public static bool IsSubnormal(Float128<TAccelerator> value)
    {
        return value.RawExponent == 0 && value.RawSignificand != UInt128.Zero;
    }

    [MakeNonGeneric]
    public static bool IsNaN(Float128<TAccelerator> value)
    {
        return value.RawExponent == short.MaxValue && value.RawSignificand != UInt128.Zero;
    }

    #endregion

    #region Public API (sign-magnitude related)

    [MakeNonGeneric]
    public static Float128<TAccelerator> operator +(Float128<TAccelerator> value)
    {
        return value;
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> operator -(Float128<TAccelerator> value)
    {
        value.RawSignBit = !value.RawSignBit;
        return value;
    }

    [MakeNonGeneric]
    public static bool IsNegative(Float128<TAccelerator> value)
    {
        return value.RawSignBit && (value.RawExponent != short.MaxValue || value.RawSignificand == UInt128.Zero);
    }

    [MakeNonGeneric]
    public static bool IsPositive(Float128<TAccelerator> value)
    {
        return !value.RawSignBit && (value.RawExponent != short.MaxValue || value.RawSignificand == UInt128.Zero);
    }

    [MakeNonGeneric]
    public static bool IsZero(Float128<TAccelerator> value)
    {
        return value.RawExponent == 0 && value.RawSignificand == UInt128.Zero;
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> Abs(Float128<TAccelerator> value)
    {
        return new Float128<TAccelerator>(value._rawBits & ~SIGNBIT_MASK);
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> Sign(Float128<TAccelerator> value)
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

    [MakeNonGeneric]
    public static Float128<TAccelerator> MaxMagnitude(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        if (IsNaN(x) || IsNaN(y))
            return _qNaN;
        else if (Abs(x) > Abs(y))
            return x;
        else
            return y;
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> MaxMagnitudeNumber(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        if (IsNaN(x))
            return y;
        else if (IsNaN(y))
            return x;
        else if (Abs(x) > Abs(y))
            return x;
        else
            return y;
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> MinMagnitude(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        if (IsNaN(x) || IsNaN(y))
            return _qNaN;
        else if (Abs(x) < Abs(y))
            return x;
        else
            return y;
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> MinMagnitudeNumber(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        if (IsNaN(x))
            return y;
        else if (IsNaN(y))
            return x;
        else if (Abs(x) < Abs(y))
            return x;
        else
            return y;
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> Max(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        if (IsNaN(x) || IsNaN(y))
            return _qNaN;
        else
            return x > y ? x : y;
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> Min(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        if (IsNaN(x) || IsNaN(y))
            return _qNaN;
        else
            return x < y ? x : y;
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> CopySign(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        return new Float128<TAccelerator>((x._rawBits & ~SIGNBIT_MASK) | (y._rawBits & SIGNBIT_MASK));
    }

    #endregion

    #region Public API (conversion operators)

    [MakeNonGeneric]
    public static explicit operator Float128<TAccelerator>(Half x)
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
            ushort bits = Unsafe.As<Half, ushort>(ref x);
            bool negative = (bits & (1U << 15)) != 0;
            return negative ? NegativeZero : Zero;
        }
        else
        {
            ushort bits = Unsafe.As<Half, ushort>(ref x);
            bool negative = (bits & (1U << 15)) != 0;
            int exponent = (int)((bits >> 10) & 0x1fU) - 0xf;
            UInt128 mantissa = bits & 0x3ffU;

            if (exponent == -0xf)
            {
                int finalAdjust = (int)UInt128.LeadingZeroCount(mantissa) - 15;
                return new(mantissa << finalAdjust, exponent - finalAdjust + 103, negative);
            }
            else
            {
                return new(mantissa << 102, exponent, negative);
            }
        }
    }

    [MakeNonGeneric]
    public static explicit operator Half(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return Half.NaN;
        }
        else if (IsPositiveInfinity(x) || (x.Exponent > 0xf && !x.RawSignBit))
        {
            return Half.PositiveInfinity;
        }
        else if (IsNegativeInfinity(x) || (x.Exponent > 0xf && x.RawSignBit))
        {
            return Half.NegativeInfinity;
        }
        else if (x.Exponent < -0x19)
        {
            return x.RawSignBit ? Half.NegativeZero : Half.Zero;
        }
        else
        {
            // keep 10 fraction bits plus guard, round and sticky bits; subnormal results shift further right
            int shiftDist = 99 - Math.Min(x.Exponent + 0xe, 0);
            uint smallMantissa = (uint)(x.Significand >> shiftDist);

            // set sticky bit
            smallMantissa |= (x.Significand & ((UInt128.One << shiftDist) - 1)) != UInt128.Zero ? 1U : 0U;

            // round to nearest, ties to even
            bool roundUp = ((smallMantissa >> 2) & 1) == 1 && (smallMantissa & 0b1011) != 0;
            smallMantissa = (smallMantissa >> 3) + (roundUp ? 1U : 0U);

            // the implicit bit carries into the exponent field, so a rounding carry needs no special case
            int biasedExponent = x.Exponent < -0xe ? 0 : x.Exponent + 0xe;
            ushort result = (ushort)(((uint)biasedExponent << 10) + smallMantissa | (x.RawSignBit ? 1U << 15 : 0U));

            return Unsafe.As<ushort, Half>(ref result);
        }
    }

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(float x)
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
        else if (x == 0.0f)
        {
            uint bits = Unsafe.As<float, uint>(ref x);
            bool negative = (bits & (1U << 31)) != 0;
            return negative ? NegativeZero : Zero;
        }
        else
        {
            uint bits = Unsafe.As<float, uint>(ref x);
            bool negative = (bits & (1U << 31)) != 0;
            int exponent = (int)((bits >> 23) & 0xffU) - 0x7f;
            UInt128 mantissa = bits & 0x7fffffU;

            if (exponent == -0x7f)
            {
                int finalAdjust = (int)UInt128.LeadingZeroCount(mantissa) - 15;
                return new(mantissa << finalAdjust, exponent - finalAdjust + 90, negative);
            }
            else
            {
                return new(mantissa << 89, exponent, negative);
            }
        }
    }

    [MakeNonGeneric]
    public static explicit operator float(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return float.NaN;
        }
        else if (IsPositiveInfinity(x) || (x.Exponent >= 0x80 && !x.RawSignBit))
        {
            return float.PositiveInfinity;
        }
        else if (IsNegativeInfinity(x) || (x.Exponent >= 0x80 && x.RawSignBit))
        {
            return float.NegativeInfinity;
        }
        else if (x.Exponent < -0x96)
        {
            return x.RawSignBit ? -0.0f : 0.0f;
        }
        else if (x.Exponent < -0x7e)
        {
            int shift = -0x25 - x.Exponent;
            UInt128 remainder = x.Significand & ((UInt128.One << shift) - 1);
            UInt128 halfway = UInt128.One << (shift - 1);

            uint result = (uint)(x.Significand >> shift);
            if (remainder > halfway || (remainder == halfway && (result & 1) == 1))
            {
                result++;
            }

            result |= x.RawSignBit ? 1U << 31 : 0;

            return Unsafe.As<uint, float>(ref result);
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

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(double x)
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
            ulong bits = Unsafe.As<double, ulong>(ref x);
            bool negative = (bits & (1UL << 63)) != 0;
            return negative ? NegativeZero : Zero;
        }
        else
        {
            ulong bits = Unsafe.As<double, ulong>(ref x);
            bool negative = (bits & (1UL << 63)) != 0;
            int exponent = (int)((bits >> 52) & 0x7ffUL) - 0x3ff;
            UInt128 mantissa = bits & 0xfffffffffffffUL;

            if (exponent == -0x3ff)
            {
                int finalAdjust = (int)UInt128.LeadingZeroCount(mantissa) - 15;
                return new(mantissa << finalAdjust, exponent - finalAdjust + 61, negative);
            }
            else
            {
                return new(mantissa << 60, exponent, negative);
            }
        }
    }

    [MakeNonGeneric]
    public static explicit operator double(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return double.NaN;
        }
        else if (IsPositiveInfinity(x) || (x.Exponent >= 0x400 && !x.RawSignBit))
        {
            return double.PositiveInfinity;
        }
        else if (IsNegativeInfinity(x) || (x.Exponent >= 0x400 && x.RawSignBit))
        {
            return double.NegativeInfinity;
        }
        else if (x.Exponent < -0x433)
        {
            return x.RawSignBit ? -0.0 : 0.0;
        }
        else if (x.Exponent < -0x3fe)
        {
            int shift = -0x3c2 - x.Exponent;
            UInt128 remainder = x.Significand & ((UInt128.One << shift) - 1);
            UInt128 halfway = UInt128.One << (shift - 1);

            ulong result = (ulong)(x.Significand >> shift);
            if (remainder > halfway || (remainder == halfway && (result & 1) == 1))
            {
                result++;
            }

            result |= x.RawSignBit ? 1UL << 63 : 0;

            return Unsafe.As<ulong, double>(ref result);
        }
        else
        {
            UInt128 remainder = x.RawSignificand & ((UInt128.One << 60) - 1);
            UInt128 halfway = UInt128.One << 59;

            ulong smallMantissa = (ulong)(x.RawSignificand >> 60);
            if (remainder > halfway || (remainder == halfway && (smallMantissa & 1) == 1))
            {
                smallMantissa++;
            }

            // a carry out of the mantissa increments the exponent field
            ulong result = ((ulong)(x.Exponent + 0x3ff) << 52) + smallMantissa;
            result |= x.RawSignBit ? 1UL << 63 : 0;

            return Unsafe.As<ulong, double>(ref result);
        }
    }

    #endregion
}
