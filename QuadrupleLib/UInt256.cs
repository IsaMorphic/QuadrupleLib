/*
 *  Copyright 2025 Chosen Few Software
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

namespace QuadrupleLib
{
    internal struct UInt256 : IEquatable<UInt256>, IComparable<UInt256>, IComparable
    {
        private static readonly UInt256 _zero = new();
        public static UInt256 Zero => _zero;

        private static readonly UInt256 _one = new(UInt128.One, UInt128.Zero);
        public static UInt256 One => _one;

        private static readonly UInt256 _minValue = _zero;
        public static UInt256 MinValue => _minValue;

        private static readonly UInt256 _maxValue = ~_zero;
        public static UInt256 MaxValue => _maxValue;


#if BIGENDIAN
        private readonly UInt128 _hi;
        private readonly UInt128 _lo;
#else
        private readonly UInt128 _lo;
        private readonly UInt128 _hi;
#endif
        private UInt256(UInt128 lo, UInt128 hi) 
        {
            _lo = lo;
            _hi = hi;
        }

        public static implicit operator UInt256(UInt128 n) 
        {
            return new UInt256(n, UInt128.Zero);
        }

        public static explicit operator UInt128(UInt256 n) 
        {
            return n._lo;
        }

        public static implicit operator UInt256(ulong n)
        {
            return new UInt256(n, UInt128.Zero);
        }

        public static explicit operator ulong(UInt256 n)
        {
            return (ulong)n._lo;
        }

        public static UInt256 operator +(UInt256 a, UInt256 b) 
        {
            UInt128 lo = a._lo + b._lo;
            UInt128 carry = (UInt128)Int128.Max(0, lo.CompareTo(a._lo));
            UInt128 hi = a._hi + b._hi + carry;
            return new(lo, hi);
        }

        public static UInt256 operator ++(UInt256 n) 
        {
            return n + One;
        }

        public static UInt256 operator -(UInt256 a, UInt256 b)
        {
            UInt128 lo = a._lo - b._lo;
            UInt128 borrow = (UInt128)Int128.Max(0, a._lo.CompareTo(lo));
            UInt128 hi = a._hi - b._hi - borrow;
            return new(lo, hi);
        }

        public static UInt256 operator --(UInt256 n)
        {
            return n - One;
        }

        public static UInt256 operator >>(UInt256 n, int amt) 
        {
            switch (amt) 
            {
                case >= 128:
                    return new(n._hi >> (amt - 128), UInt128.Zero);
                default:
                    return new((n._lo >> amt) | ((n._hi & (UInt128.MaxValue >> (128 - amt))) << (128 - amt)), n._hi >> amt);
            }
        }

        public static UInt256 operator <<(UInt256 n, int amt)
        {
            switch (amt)
            {
                case >= 128:
                    return new(n._lo << (amt - 128), UInt128.Zero);
                default:
                    return new(n._lo << amt, (n._hi << amt) | (n._lo & (UInt128.MaxValue << (128 - amt))));
            }
        }

        public static UInt256 operator &(UInt256 a, UInt256 b) 
        {
            return new(a._lo & b._lo, a._hi & b._hi);
        }

        public static UInt256 operator |(UInt256 a, UInt256 b)
        {
            return new(a._lo | b._lo, a._hi | b._hi);
        }

        public static UInt256 operator ^(UInt256 a, UInt256 b)
        {
            return new(a._lo ^ b._lo, a._hi ^ b._hi);
        }

        public static UInt256 operator ~(UInt256 n)
        {
            return new(~n._lo, ~n._hi);
        }

        public static UInt256 LeadingZeroCount() 
        { 
            // TODO: division scaling by implementing this
        }

        public static bool operator >(UInt256 a, UInt256 b) 
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <(UInt256 a, UInt256 b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >=(UInt256 a, UInt256 b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <=(UInt256 a, UInt256 b)
        {
            return a.CompareTo(b) <= 0;
        }

        public int CompareTo(UInt256 other)
        {
            return _hi == other._hi ? _lo.CompareTo(other._lo) : _hi.CompareTo(other._hi);
        }

        public int CompareTo(object? obj)
        {
            return obj is UInt256 @int ? CompareTo(@int) :
                throw new InvalidOperationException();
        }

        public static bool operator ==(UInt256 left, UInt256 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UInt256 left, UInt256 right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is UInt256 @int && Equals(@int);
        }

        public bool Equals(UInt256 other)
        {
            return _lo.Equals(other._lo) &&
                   _hi.Equals(other._hi);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_lo, _hi);
        }
    }
}