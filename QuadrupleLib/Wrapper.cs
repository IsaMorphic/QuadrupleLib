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

public struct Float128 : IComparable, IComparable<Float128>, IEquatable<Float128>
{
    #region Private API (constants)

    private static readonly UInt128 SIGNIFICAND_MASK = UInt128.MaxValue >> 16;
    private static readonly UInt128 EXPONENT_MASK = ~SIGNIFICAND_MASK ^ SIGNBIT_MASK;
    private static readonly UInt128 SIGNBIT_MASK = (UInt128.MaxValue >> 1) + 1;
    private static readonly ushort EXPONENT_BIAS = short.MaxValue >> 1;
    private static readonly Float128 _qNaN = new Float128(UInt128.One, short.MaxValue, false);
    private static readonly Float128 _pInf = new Float128(UInt128.Zero, short.MaxValue, false);
    private static readonly Float128 _nInf = new Float128(UInt128.Zero, short.MaxValue, true);

    #endregion

    #region Raw storage properties

    private UInt128 _rawBits;

    private bool RawSignBit
    {
        get => _rawBits > (UInt128.MaxValue >> 1);
        set
        {
            _rawBits = value ?
                _rawBits | SIGNBIT_MASK :
                _rawBits & ~SIGNBIT_MASK;
        }
    }

    private ushort RawExponent
    {
        get => (ushort)((_rawBits & ~SIGNBIT_MASK) >> 112);
        set
        {
            _rawBits = _rawBits & ~EXPONENT_MASK | ((UInt128)value << 112);
        }
    }

    private UInt128 RawSignificand
    {
        get => _rawBits & SIGNIFICAND_MASK;
        set
        {
            _rawBits = (_rawBits & ~SIGNIFICAND_MASK) | value;
        }
    }

    #endregion

    #region Raw storage constructors

    internal Float128(UInt128 rawBits)
    {
        _rawBits = rawBits;
    }

    #endregion

    #region Representational properties

    private short Exponent
    {
        get
        {
            if (RawExponent == 0) // check if subnormal
            {
                return (short)(-EXPONENT_BIAS + 1);
            }
            else if (RawExponent != short.MaxValue)
            {
                return (short)(RawExponent - EXPONENT_BIAS);
            }
            else
            {
                return short.MaxValue;
            }
        }
    }

    private UInt128 Significand => RawExponent == 0 ? RawSignificand : RawSignificand | (SIGNIFICAND_MASK + 1);

    #endregion

    #region Representational constructors

    private Float128(UInt128 rawSignificand, int exponent, bool rawSignBit)
    {
        RawSignificand = rawSignificand;

        if (exponent == -EXPONENT_BIAS + 1)
        {
            RawExponent = 0;
        }
        else if (exponent != short.MaxValue)
        {
            RawExponent = (ushort)(exponent + EXPONENT_BIAS);
        }
        else
        {
            RawExponent = (ushort)short.MaxValue;
        }

        RawSignBit = rawSignBit;
    }

    #endregion

    #region Public APIs (comparison related)

    public override int GetHashCode()
    {
        return HashCode.Combine(_rawBits);
    }

    public override bool Equals(object? obj)
    {
        return obj is Float128 x && Equals(x);
    }

    public bool Equals(Float128 other)
    {
        return (_rawBits == other._rawBits || (IsZero(this) && IsZero(other))) && (!IsNaN(this) && !IsNaN(other));
    }

    public int CompareTo(object? obj)
    {
        if (obj is Float128 other)
        {
            return CompareTo(other);
        }
        else
        {
            throw new ArgumentException($"Provided value must be of type {nameof(Float128)}.", nameof(obj));
        }
    }

    public int CompareTo(Float128 other)
    {
        if (Equals(other))
            return 0;
        else if (RawSignBit == other.RawSignBit)
        {
            var expCmp = Exponent.CompareTo(other.Exponent);
            return (RawSignBit ? -1 : 1) * (expCmp == 0
                ? Significand.CompareTo(other.Significand)
                : expCmp);
        }
        else
        {
            return Math.Sign((RawSignBit ? -1 : 1) - (other.RawSignBit ? -1 : 1));
        }
    }

    #endregion

    #region Public APIs (wrapper related)

    public Float128 WithAccelerator<TAccelerator>()
        where TAccelerator : IAccelerator
    {
        return new Float128(_rawBits);
    }

    public static Float128<TAccelerator> WithAccelerator<TAccelerator>(Float128 x)
        where TAccelerator : IAccelerator
    {
        return new Float128<TAccelerator>(x._rawBits);
    }

    #endregion

    #region Public API (representation related)

    public static bool IsZero(Float128 value)
    {
        return value.RawExponent == 0 && value.RawSignificand == UInt128.Zero;
    }

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

    #region Public APIs (comparison operators)

    public static bool operator ==(Float128 left, Float128 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Float128 left, Float128 right)
    {
        return !(left == right);
    }

    public static bool operator <(Float128 left, Float128 right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Float128 left, Float128 right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Float128 left, Float128 right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Float128 left, Float128 right)
    {
        return left.CompareTo(right) >= 0;
    }

    #endregion

    #region Public API (integer conversion operators)

    public static implicit operator Float128(sbyte n)
    {
        bool signBit;
        UInt128 mantissa;
        switch (n.CompareTo(0))
        {
            case > 0:
                mantissa = (UInt128)n;
                signBit = false;
                break;
            case < 0:
                mantissa = (UInt128)(-n);
                signBit = true;
                break;
            default:
                return default;
        }

        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, signBit);
    }

    public static explicit operator sbyte(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
            return x.RawSignBit ? (sbyte)-n : (sbyte)n;
        }
        else
        {
            return 0;
        }
    }

    public static implicit operator Float128(short n)
    {
        bool signBit;
        UInt128 mantissa;
        switch (n.CompareTo(0))
        {
            case > 0:
                mantissa = (UInt128)n;
                signBit = false;
                break;
            case < 0:
                mantissa = (UInt128)(-n);
                signBit = true;
                break;
            default:
                return default;
        }

        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, signBit);
    }

    public static explicit operator short(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
            return x.RawSignBit ? (short)-n : (short)n;
        }
        else
        {
            return 0;
        }
    }

    public static implicit operator Float128(int n)
    {
        bool signBit;
        UInt128 mantissa;
        switch (n.CompareTo(0))
        {
            case > 0:
                mantissa = (UInt128)n;
                signBit = false;
                break;
            case < 0:
                mantissa = (UInt128)(-n);
                signBit = true;
                break;
            default:
                return default;
        }

        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, signBit);
    }

    public static explicit operator int(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
            return x.RawSignBit ? (int)-n : (int)n;
        }
        else
        {
            return 0;
        }
    }

    public static implicit operator Float128(long n)
    {
        bool signBit;
        UInt128 mantissa;
        switch (n.CompareTo(0L))
        {
            case > 0:
                mantissa = (UInt128)n;
                signBit = false;
                break;
            case < 0:
                mantissa = (UInt128)(-n);
                signBit = true;
                break;
            default:
                return default;
        }

        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, signBit);
    }

    public static explicit operator long(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
            return x.RawSignBit ? (long)-n : (long)n;
        }
        else
        {
            return 0;
        }
    }

    public static explicit operator Float128(Int128 n)
    {
        bool signBit;
        UInt128 mantissa;
        switch (n.CompareTo(Int128.Zero))
        {
            case > 0:
                mantissa = (UInt128)n;
                signBit = false;
                break;
            case < 0:
                mantissa = (UInt128)(-n);
                signBit = true;
                break;
            default:
                return default;
        }

        UInt128 rounded;
        int expShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        switch (expShift)
        {
            case 0:
                rounded = mantissa << 3;
                break;
            case > 0:
                rounded = mantissa << (expShift + 3);
                break;
            case < 0:
                rounded = mantissa >> (-expShift - 3);
                rounded &= UInt128.MaxValue << 1;
                rounded |= UInt128.Min(mantissa & ((UInt128.One << (113 + expShift)) - 1), 1);
                if ((((rounded & 1) |
                     ((rounded >> 2) & 1)) &
                     ((rounded >> 1) & 1)) == 1) // check rounding condition
                {
                    rounded++;
                }
                break;
        }

        return new Float128(rounded >> 3, 112 - expShift, signBit);
    }

    public static explicit operator Int128(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
            return x.RawSignBit ? -n : n;
        }
        else
        {
            return 0;
        }
    }

    public static implicit operator Float128(byte n)
    {
        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, false);
    }

    public static explicit operator byte(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> rightShift;
            return (byte)(x.RawSignBit ? -n : n);
        }
        else
        {
            return 0;
        }
    }

    public static implicit operator Float128(ushort n)
    {
        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, false);
    }

    public static explicit operator ushort(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> rightShift;
            return (ushort)(x.RawSignBit ? -n : n);
        }
        else
        {
            return 0;
        }
    }

    public static implicit operator Float128(uint n)
    {
        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, false);
    }

    public static explicit operator uint(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> rightShift;
            return (uint)(x.RawSignBit ? -n : n);
        }
        else
        {
            return 0;
        }
    }

    public static implicit operator Float128(ulong n)
    {
        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, false);
    }

    public static explicit operator ulong(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> rightShift;
            return (ulong)(x.RawSignBit ? -n : n);
        }
        else
        {
            return 0;
        }
    }

    public static explicit operator Float128(UInt128 n)
    {
        UInt128 rounded;
        int expShift = (int)UInt128.LeadingZeroCount(n) - 15;
        switch (expShift)
        {
            case 0:
                rounded = n << 3;
                break;
            case > 0:
                rounded = n << (expShift + 3);
                break;
            case < 0:
                rounded = n >> (-expShift - 3);
                rounded &= UInt128.MaxValue << 1;
                rounded |= UInt128.Min(n & ((UInt128.One << (113 + expShift)) - 1), 1);
                if ((((rounded & 1) |
                     ((rounded >> 2) & 1)) &
                     ((rounded >> 1) & 1)) == 1) // check rounding condition
                {
                    rounded++;
                }
                break;
        }

        return new Float128(rounded >> 3, 112 - expShift, false);
    }

    public static explicit operator UInt128(Float128 x)
    {
        int rightShift = 112 - x.Exponent;
        if (rightShift <= 128)
        {
            Int128 n = (Int128)x.Significand >> rightShift;
            return (UInt128)(x.RawSignBit ? -n : n);
        }
        else
        {
            return 0;
        }
    }

    #endregion

    #region Public API (floating-point conversion operators)

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
            return default;
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
            return default;
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
            return default;
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
