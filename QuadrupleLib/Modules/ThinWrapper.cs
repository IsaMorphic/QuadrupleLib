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

public readonly struct Float128 : IComparable, IComparable<Float128>, IEquatable<Float128>
{
    private static readonly UInt128 SIGNIFICAND_MASK = UInt128.MaxValue >> 16;
    private static readonly UInt128 SIGNBIT_MASK = (UInt128.MaxValue >> 1) + 1;
    private static readonly ushort EXPONENT_BIAS = short.MaxValue >> 1;

    private readonly UInt128 _rawBits;

    private bool RawSignBit
    {
        get => _rawBits > (UInt128.MaxValue >> 1);
    }

    private ushort RawExponent
    {
        get => (ushort)((_rawBits & ~SIGNBIT_MASK) >> 112);
    }

    private UInt128 RawSignificand
    {
        get => _rawBits & SIGNIFICAND_MASK;
    }

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

    internal Float128(UInt128 rawBits)
    {
        _rawBits = rawBits;
    }

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

    public Float128<TAccelerator> WithAccelerator<TAccelerator>()
        where TAccelerator : IAccelerator
    {
        return new Float128<TAccelerator>(_rawBits);
    }

    private static bool IsNaN(Float128 value)
    {
        return value.RawExponent == short.MaxValue && value.RawSignificand != UInt128.Zero;
    }

    private static bool IsZero(Float128 value)
    {
        return value.RawExponent == 0 && value.RawSignificand == UInt128.Zero;
    }

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

    public static Float128<TAccelerator> WithAccelerator<TAccelerator>(Float128 x)
        where TAccelerator : IAccelerator
    {
        return new Float128<TAccelerator>(x._rawBits);
    }
}
