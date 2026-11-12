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

using QuadrupleLib.Annotations;

namespace QuadrupleLib;

public partial struct Float128<TAccelerator>
{
    #region Public API (integer related)

    [MakeNonGeneric]
    public static bool IsInteger(Float128<TAccelerator> value)
    {
        if (IsZero(value))
        {
            return true;
        }
        else
        {
            int m = (int)UInt128.TrailingZeroCount(value.Significand);
            int exp = value.Exponent + m - 112;
            return exp >= 0;
        }
    }

    [MakeNonGeneric]
    public static bool IsEvenInteger(Float128<TAccelerator> value)
    {
        if (IsZero(value))
        {
            return true;
        }
        else
        {
            int m = (int)UInt128.TrailingZeroCount(value.Significand);
            int exp = value.Exponent + m - 112, adj = exp - m;
            switch (adj)
            {
                case > 0:
                    return exp >= 0 && (((Utilities.UInt256)value.Significand << adj) & 1) == 0;
                case <= 0:
                    return exp >= 0 && ((value.Significand >> -adj) & 1) == 0;
            }
        }
    }

    [MakeNonGeneric]
    public static bool IsOddInteger(Float128<TAccelerator> value)
    {
        int m = (int)UInt128.TrailingZeroCount(value.Significand);
        int exp = value.Exponent + m - 112, adj = exp - m;
        switch (adj)
        {
            case > 0:
                return exp >= 0 && (((Utilities.UInt256)value.Significand << adj) & 1) != 0;
            case <= 0:
                return exp >= 0 && ((value.Significand >> -adj) & 1) != 0;
        }
    }

    #endregion

    #region Public API (rounding related)

    private static readonly Float128<TAccelerator>[] _pow10Table;

    public static Float128<TAccelerator> Round(Float128<TAccelerator> x, int digits, MidpointRounding mode = MidpointRounding.ToEven)
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
            Float128<TAccelerator> scaled = x * _pow10Table[digits];
            return Round(scaled) / _pow10Table[digits];
        }
    }

    public static Float128<TAccelerator> Round(Float128<TAccelerator> value)
    {
        Float128<TAccelerator> result;

        int unbiasedExponent = value.Exponent;
        switch (unbiasedExponent)
        {
            case short.MaxValue:
                result = value; //NaN, +inf, -inf
                break;
            case < 0:
                result = 0;
                break;
            case >= 112:
                result = value;
                break;
            default:
                result = value;
                bool roundUp = ((result.RawSignificand >> (111 - unbiasedExponent)) & 1) != 0;
                int bitsToErase = 112 - unbiasedExponent;
                result.RawSignificand &= ~((UInt128.One << bitsToErase) - 1);
                if (roundUp && IsOddInteger(result)) result += value.RawSignBit ? -1 : 1;
                break;
        }

        return result;
    }

    public static Float128<TAccelerator> Floor(Float128<TAccelerator> value)
    {
        Float128<TAccelerator> result;

        if (IsZero(value))
        {
            return value;
        }

        int unbiasedExponent = value.Exponent;
        switch (unbiasedExponent)
        {
            case short.MaxValue:
                result = value; //NaN, +inf, -inf
                break;
            case < 0:
                result = value.RawSignBit ? -1 : 0;
                break;
            default:
                result = value;
                int bitsToErase = 112 - unbiasedExponent;
                result.RawSignificand &= ~((UInt128.One << bitsToErase) - 1);
                if (value.RawSignBit && result != value) result -= 1;
                break;
        }

        return result;
    }

    public static Float128<TAccelerator> Ceiling(Float128<TAccelerator> value)
    {
        Float128<TAccelerator> result;

        if (IsZero(value))
        {
            return value;
        }

        int unbiasedExponent = value.Exponent;
        switch (unbiasedExponent)
        {
            case short.MaxValue:
                result = value; //NaN, +inf, -inf
                break;
            case < 0:
                result = value.RawSignBit ? 0 : 1;
                break;
            default:
                result = value;
                int bitsToErase = 112 - unbiasedExponent;
                result.RawSignificand &= ~((UInt128.One << bitsToErase) - 1);

                // From @GreatCoder1000 (https://github.com/GreatCoder1000):
                // if positive and we lost a fractional part, bump up by 1,
                // for negative values, truncation toward zero already yields the ceiling
                if (!value.RawSignBit && result != value) result += 1;
                break;
        }

        return result;
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> Truncate(Float128<TAccelerator> value)
    {
        Float128<TAccelerator> result;

        if (IsZero(value))
        {
            return value;
        }

        int unbiasedExponent = value.Exponent;
        switch (unbiasedExponent)
        {
            case short.MaxValue:
                result = value; //NaN, +inf, -inf
                break;
            case < 0:
                result = value.RawSignBit ? NegativeZero : Zero;
                break;
            default:
                result = value;
                int bitsToErase = 112 - unbiasedExponent;
                result.RawSignificand &= ~((UInt128.One << bitsToErase) - 1);
                break;
        }

        return result;
    }

    #endregion

    #region Public API (conversion operators)

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(sbyte n)
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
        return new Float128<TAccelerator>(mantissa << leftShift, 112 - leftShift, signBit);
    }

    [MakeNonGeneric]
    public static explicit operator sbyte(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || Truncate(x) > sbyte.MaxValue || Truncate(x) < sbyte.MinValue)
        {
            throw new OverflowException();
        }

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

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(short n)
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
        return new Float128<TAccelerator>(mantissa << leftShift, 112 - leftShift, signBit);
    }

    [MakeNonGeneric]
    public static explicit operator short(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || Truncate(x) > short.MaxValue || Truncate(x) < short.MinValue)
        {
            throw new OverflowException();
        }

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

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(int n)
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
        return new Float128<TAccelerator>(mantissa << leftShift, 112 - leftShift, signBit);
    }

    [MakeNonGeneric]
    public static explicit operator int(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || Truncate(x) > int.MaxValue || Truncate(x) < int.MinValue)
        {
            throw new OverflowException();
        }

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

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(long n)
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
        return new Float128<TAccelerator>(mantissa << leftShift, 112 - leftShift, signBit);
    }

    [MakeNonGeneric]
    public static explicit operator long(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || Truncate(x) > long.MaxValue || Truncate(x) < long.MinValue)
        {
            throw new OverflowException();
        }

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

    [MakeNonGeneric]
    public static explicit operator Float128<TAccelerator>(Int128 n)
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

        return new Float128<TAccelerator>(rounded >> 3, 112 - expShift, signBit);
    }

    [MakeNonGeneric]
    public static explicit operator Int128(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || 
            Truncate(x) > (Float128<TAccelerator>)Int128.MaxValue ||
            Truncate(x) < (Float128<TAccelerator>)Int128.MinValue)
        {
            throw new OverflowException();
        }

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

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(byte n)
    {
        if(n == 0) return Zero;

        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128<TAccelerator>(mantissa << leftShift, 112 - leftShift, false);
    }

    [MakeNonGeneric]
    public static explicit operator byte(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || Truncate(x) > byte.MaxValue || Truncate(x) < byte.MinValue)
        {
            throw new OverflowException();
        }

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

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(ushort n)
    {
        if(n == 0) return Zero;

        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128<TAccelerator>(mantissa << leftShift, 112 - leftShift, false);
    }

    [MakeNonGeneric]
    public static explicit operator ushort(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || Truncate(x) > ushort.MaxValue || Truncate(x) < ushort.MinValue)
        {
            throw new OverflowException();
        }

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

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(uint n)
    {
        if(n == 0) return Zero;

        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128<TAccelerator>(mantissa << leftShift, 112 - leftShift, false);
    }

    [MakeNonGeneric]
    public static explicit operator uint(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || Truncate(x) > uint.MaxValue || Truncate(x) < uint.MinValue)
        {
            throw new OverflowException();
        }

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

    [MakeNonGeneric]
    public static implicit operator Float128<TAccelerator>(ulong n)
    {
        if(n == 0) return Zero;

        UInt128 mantissa = (UInt128)n;
        int leftShift = (int)UInt128.LeadingZeroCount(mantissa) - 15;
        return new Float128<TAccelerator>(mantissa << leftShift, 112 - leftShift, false);
    }

    [MakeNonGeneric]
    public static explicit operator ulong(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || Truncate(x) > ulong.MaxValue || Truncate(x) < ulong.MinValue)
        {
            throw new OverflowException();
        }

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

    [MakeNonGeneric]
    public static explicit operator Float128<TAccelerator>(UInt128 n)
    {
        if(n == 0) return Zero;
        
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

        return new Float128<TAccelerator>(rounded >> 3, 112 - expShift, false);
    }

    [MakeNonGeneric]
    public static explicit operator UInt128(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || 
            Truncate(x) > (Float128<TAccelerator>)UInt128.MaxValue || 
            Truncate(x) < (Float128<TAccelerator>)UInt128.MinValue)
        {
            throw new OverflowException();
        }

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
}
