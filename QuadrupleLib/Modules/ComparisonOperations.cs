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

public partial struct Float128<TAccelerator>
{
    #region Public API (equality & comparison related)

    public override int GetHashCode()
    {
        return HashCode.Combine(_rawBits);
    }

    public int CompareTo(object? obj)
    {
        if (obj is Float128<TAccelerator> other)
        {
            return CompareTo(other);
        }
        else
        {
            throw new ArgumentException($"Provided value must be of type {nameof(Float128<TAccelerator>)}.", nameof(obj));
        }
    }

    public int CompareTo(Float128<TAccelerator> other)
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

    public override bool Equals(object? obj)
    {
        return obj is Float128<TAccelerator> && Equals((Float128<TAccelerator>)obj);
    }

    public bool Equals(Float128<TAccelerator> other)
    {
        return (_rawBits == other._rawBits || (IsZero(this) && IsZero(other))) && (!IsNaN(this) && !IsNaN(other));
    }

    public static bool operator ==(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        return (left.CompareTo(right) < 0) && (!IsNaN(left) && !IsNaN(right));
    }

    public static bool operator >(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        return left.CompareTo(right) > 0 && (!IsNaN(left) && !IsNaN(right));
    }

    public static bool operator <=(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        return left.CompareTo(right) <= 0 && (!IsNaN(left) && !IsNaN(right));
    }

    public static bool operator >=(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        return left.CompareTo(right) >= 0 && (!IsNaN(left) && !IsNaN(right));
    }

    #endregion
}
