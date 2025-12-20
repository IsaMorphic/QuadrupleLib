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

namespace QuadrupleLib;

public partial struct Float128
{
    #region Public API (integer related)

    public static bool IsInteger(Float128 value)
    {
        return value == Round(value);
    }

    public static bool IsEvenInteger(Float128 value)
    {
        return IsInteger(value) && value.Exponent >= 1;
    }

    public static bool IsOddInteger(Float128 value)
    {
        return IsInteger(value) && value.Exponent < 1;
    }

    #endregion

    #region Public API (rounding related)

    private static readonly Float128[] _pow10Table;

    public static Float128 Round(Float128 x, int digits, MidpointRounding mode = MidpointRounding.ToEven)
    {
        if (mode != MidpointRounding.ToEven)
        {
            throw new ArgumentException("The specified rounding mode is not supported", nameof(mode));
        }

        if (digits < 0)
        {
            throw new ArgumentOutOfRangeException("Parameter must be greater than or equal to 0.", nameof(digits));
        }

        if (digits >= 38)
        {
            return x;
        }
        else
        {
            Float128 scaled = x * _pow10Table[digits];
            return Round(scaled) / _pow10Table[digits];
        }
    }

    public static Float128 Round(Float128 value)
    {
        Float128 result;
        int unbiasedExponent = value.Exponent;

        if (unbiasedExponent == short.MaxValue) result = value; //NaN, +inf, -inf
        else if (unbiasedExponent < -1) result = 0;
        else if (unbiasedExponent == -1) result = value.RawSignBit ? -1 : 1;
        else if (unbiasedExponent >= 112) result = value;
        else
        {
            result = value;
            bool roundUp = ((result.RawSignificand >> (111 - unbiasedExponent)) & 1) != 0;
            int bitsToErase = 112 - unbiasedExponent;
            result.RawSignificand &= ~((UInt128.One << bitsToErase) - 1);
            if (roundUp) result += 1;
        }

        return result;
    }

    public static Float128 Floor(Float128 value)
    {
        Float128 result;
        int unbiasedExponent = value.Exponent;

        if (unbiasedExponent == short.MaxValue) result = value; //NaN, +inf, -inf
        else if (unbiasedExponent < 0)
        {
            if (value.RawSignBit)
            {
                result = -1;
            }
            else
            {
                result = 0;
            }
        }
        else
        {
            result = value;
            int bitsToErase = 112 - unbiasedExponent;
            result.RawSignificand &= ~((UInt128.One << bitsToErase) - 1);
            if (value.RawSignBit) result -= 1;
        }

        return result;
    }

    public static Float128 Ceiling(Float128 value)
    {
        Float128 result;
        int unbiasedExponent = value.Exponent;

        if (unbiasedExponent == short.MaxValue) result = value; //NaN, +inf, -inf
        else if (unbiasedExponent < 0)
        {
            if (value.RawSignBit)
            {
                result = 0;
            }
            else
            {
                result = 1;
            }
        }
        else
        {
            result = value;
            int bitsToErase = 112 - unbiasedExponent;
            result.RawSignificand &= ~((UInt128.One << bitsToErase) - 1);
            if (value.RawSignBit) result += 1;
        }

        return result;
    }

    #endregion

    #region Public API (conversion operators)

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
                return Zero;
        }

        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, signBit);
    }

    public static explicit operator sbyte(Float128 x)
    {
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return x.RawSignBit ? (sbyte)-n : (sbyte)n;
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
                return Zero;
        }

        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, signBit);
    }

    public static explicit operator short(Float128 x)
    {
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return x.RawSignBit ? (short)-n : (short)n;
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
                return Zero;
        }

        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, signBit);
    }

    public static explicit operator int(Float128 x)
    {
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return x.RawSignBit ? (int)-n : (int)n;
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
                return Zero;
        }

        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, signBit);
    }

    public static explicit operator long(Float128 x)
    {
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return x.RawSignBit ? (long)-n : (long)n;
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
                return Zero;
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
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return x.RawSignBit ? -n : n;
    }

    public static implicit operator Float128(byte n)
    {
        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, false);
    }

    public static explicit operator byte(Float128 x)
    {
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return (byte)(x.RawSignBit ? -n : n);
    }

    public static implicit operator Float128(ushort n)
    {
        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, false);
    }

    public static explicit operator ushort(Float128 x)
    {
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return (ushort)(x.RawSignBit ? -n : n);
    }

    public static implicit operator Float128(uint n)
    {
        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, false);
    }

    public static explicit operator uint(Float128 x)
    {
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return (uint)(x.RawSignBit ? -n : n);
    }

    public static implicit operator Float128(ulong n)
    {
        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128(mantissa << leftShift, 112 - leftShift, false);
    }

    public static explicit operator ulong(Float128 x)
    {
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return (ulong)(x.RawSignBit ? -n : n);
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
        Int128 n = (Int128)x.Significand >> (112 - x.Exponent);
        return (UInt128)(x.RawSignBit ? -n : n);
    }

    #endregion
}
