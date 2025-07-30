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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace QuadrupleLib
{
    public struct Float128 : IBinaryFloatingPointIeee754<Float128>
    {
        #region Useful constants

        private static readonly UInt128 SIGNIFICAND_MASK = UInt128.MaxValue >> 16;
        private static readonly UInt128 EXPONENT_MASK = ~SIGNIFICAND_MASK ^ SIGNBIT_MASK;
        private static readonly UInt128 SIGNBIT_MASK = ~(UInt128.MaxValue >> 1);
        private static readonly ushort EXPONENT_BIAS = short.MaxValue >> 1;

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

        private Float128(UInt128 rawBits)
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

        #region Static constructor

        static Float128()
        {
            // Rounding related
            Float128 pow10 = One;
            List<Float128> pow10List = new();
            for (int i = 0; i < 38; i++)
            {
                pow10List.Add(pow10);
                pow10 *= 10;
            }
            _pow10Table = pow10List.ToArray();

            // CoRDiC implementation
            _thetaTable = Enumerable.Range(0, SINCOS_ITER_COUNT)
                .Select(AtanPow2).ToArray();
            _K_n = ComputeK(SINCOS_ITER_COUNT);
        }

        #endregion

        #region Public API (constants)

        private static readonly Float128 _qNaN = new Float128(UInt128.One, short.MaxValue, false);
        private static readonly Float128 _sNaN = new Float128(UInt128.One, short.MaxValue, true);

        private static readonly Float128 _pInf = new Float128(UInt128.Zero, short.MaxValue, false);
        private static readonly Float128 _nInf = new Float128(UInt128.Zero, short.MaxValue, true);

        public static Float128 PositiveInfinity => _pInf;
        public static Float128 NegativeInfinity => _nInf;
        public static Float128 NaN => _qNaN;

        private static readonly Float128 _epsilon = new Float128(UInt128.One, -EXPONENT_BIAS + 1, false);
        public static Float128 Epsilon => _epsilon;

        public static Float128 One => 1.0;
        public static Float128 Zero => new();
        public static Float128 NegativeOne => -1.0;

        public static int Radix => 2;
        public static Float128 AdditiveIdentity => Zero;
        public static Float128 MultiplicativeIdentity => One;

        public static Float128 NegativeZero => new(UInt128.Zero, 0, true);

        private static readonly Float128 _e = Parse("2.7182818284590452353602874713526625");
        public static Float128 E => _e;

        private static readonly Float128 _pi = Parse("3.1415926535897932384626433832795028");
        public static Float128 Pi => _pi;

        private static readonly Float128 _tau = Parse("6.2831853071795864769252867665590058");
        public static Float128 Tau => _tau;

        #endregion

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

        #region Public API (equality & comparison related)

        public override int GetHashCode()
        {
            return HashCode.Combine(_rawBits);
        }

        public int CompareTo(object? obj)
        {
            if (obj is Float128 other)
            {
                return CompareTo(other);
            }
            else
            {
                throw new InvalidOperationException();
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

        public override bool Equals(object? obj)
        {
            return obj is Float128 && Equals((Float128)obj);
        }

        public bool Equals(Float128 other)
        {
            return (_rawBits == other._rawBits || (IsZero(this) && IsZero(other))) && (!IsNaN(this) && !IsNaN(other));
        }

        public static bool operator ==(Float128 left, Float128 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Float128 left, Float128 right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Float128 left, Float128 right)
        {
            return (left.CompareTo(right) < 0) && (!IsNaN(left) && !IsNaN(right));
        }

        public static bool operator >(Float128 left, Float128 right)
        {
            return left.CompareTo(right) > 0 && (!IsNaN(left) && !IsNaN(right));
        }

        public static bool operator <=(Float128 left, Float128 right)
        {
            return left.CompareTo(right) <= 0 && (!IsNaN(left) && !IsNaN(right));
        }

        public static bool operator >=(Float128 left, Float128 right)
        {
            return left.CompareTo(right) >= 0 && (!IsNaN(left) && !IsNaN(right));
        }

        #endregion

        #region Private API: Full-width 256-bit Multiplication Utility

        [StructLayout(LayoutKind.Sequential)]
        private struct BigMul128
        {

#if BIGENDIAN
            public uint _3;
            public uint _2;
            public uint _1;
            public uint _0;
#else
            public uint _0;
            public uint _1;
            public uint _2;
            public uint _3;
#endif

            private static BigMul128 Add(BigMul128 left, BigMul128 right)
            {
                uint carry;
                BigMul128 result = new BigMul128();

                result._0 = left._0 + right._0;

                carry = (uint)Math.Max(0, left._0.CompareTo(result._0));
                result._1 = left._1 + right._1 + carry;

                carry = (uint)Math.Max(0, left._1.CompareTo(result._1));
                result._2 = left._2 + right._2 + carry;

                carry = (uint)Math.Max(0, left._2.CompareTo(result._2));
                result._3 = left._3 + right._3 + carry;

                return result;
            }

            private static BigMul128 Multiply(ulong left, uint right)
            {
                var result = new BigMul128();

                ulong prod1 = (uint)left * (ulong)right;
                result._0 = (uint)prod1;

                ulong prod2 = (left >> 32) * right;
                result._1 = (uint)prod2 + (uint)(prod1 >> 32);
                result._2 = (uint)(prod2 >> 32) + (uint)Math.Max(0, ((uint)prod2).CompareTo((uint)prod2 + (uint)(prod1 >> 32)));

                return result;
            }

            public static BigMul128 Multiply(ulong left, ulong right)
            {
                var leftProd = Multiply(left, (uint)right);
                var rightProd = Multiply(left, (uint)(right >> 32));

                var rightShift = new BigMul128 // 32-bit left-shift
                {
                    _1 = rightProd._0,
                    _2 = rightProd._1,
                    _3 = rightProd._2,
                };

                return Add(leftProd, rightShift);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BigMul256
        {

#if BIGENDIAN
            public ulong _3;
            public ulong _2;
            public ulong _1;
            public ulong _0;
#else
            public ulong _0;
            public ulong _1;
            public ulong _2;
            public ulong _3;
#endif

            private static BigMul256 Add(BigMul256 left, BigMul256 right)
            {
                ulong carry;
                BigMul256 result = new BigMul256();

                result._0 = left._0 + right._0;

                carry = (ulong)Math.Max(0, left._0.CompareTo(result._0));
                result._1 = left._1 + right._1 + carry;

                carry = (ulong)Math.Max(0, left._1.CompareTo(result._1));
                result._2 = left._2 + right._2 + carry;

                carry = (ulong)Math.Max(0, left._2.CompareTo(result._2));
                result._3 = left._3 + right._3 + carry;

                return result;
            }

            private static BigMul256 Multiply(UInt128 left, ulong right)
            {
                var result = new BigMul256();

                BigMul128 prod1 = BigMul128.Multiply((ulong)left, right);

                var lo1 = prod1._0 | ((ulong)prod1._1 << 32);
                var hi1 = prod1._2 | ((ulong)prod1._3 << 32);

                result._0 = lo1;

                BigMul128 prod2 = BigMul128.Multiply((ulong)(left >> 64), right);

                var lo2 = prod2._0 | ((ulong)prod2._1 << 32);
                var hi2 = prod2._2 | ((ulong)prod2._3 << 32);

                result._1 = lo2 + hi1;
                result._2 = hi2 + (ulong)Math.Max(0, lo2.CompareTo(lo2 + hi1));

                return result;
            }

            public static BigMul256 Multiply(UInt128 left, UInt128 right)
            {
                var leftProd = Multiply(left, (ulong)right);
                var rightProd = Multiply(left, (ulong)(right >> 64));

                var rightShift = new BigMul256 // 64-bit left-shift
                {
                    _1 = rightProd._0,
                    _2 = rightProd._1,
                    _3 = rightProd._2,
                };

                return Add(leftProd, rightShift);
            }
        }

        #endregion

        #region Private API: Software-based 128-bit Division Routines

        private static (ulong lo, ulong hi) Divide(UInt128 n, ulong d, out ulong r)
        {
            (UInt128 quot, UInt128 rem) = UInt128.DivRem(n, d);

            ulong loBits = (ulong)quot;
            ulong hiBits = (ulong)(quot >> 64);
            r = (ulong)rem;

            return (loBits, hiBits);
        }

        private static UInt128 Divide(UInt128 n, UInt128 d, out UInt128 r)
        {
            var dLoBits = (ulong)d;
            var dHiBits = (ulong)(d >> 64);

            if (d != 0 && d > n)
            {
                r = n;
                return UInt128.Zero;
            }
            else if (d == n)
            {
                r = UInt128.Zero;
                return UInt128.One;
            }
            else if (dHiBits > 0)
            {
                // Base 2^64 long division (lol)
                UInt128 q = UInt128.Zero;
                r = n;

                while (r >= d)
                {
                    var p = Divide(r, dHiBits, out ulong _);
                    if (p.hi == 0)
                    {
                        q += UInt128.One;
                        r -= d;
                        break;
                    }

                    var pHi = p.hi;
                    var prod = BigMul256.Multiply(d, pHi);
                    UInt128 s = prod._0 | ((UInt128)prod._1 << 64);
                    while (r < s)
                    {
                        prod = BigMul256.Multiply(d, pHi >>= 1);
                        s = prod._0 | ((UInt128)prod._1 << 64);
                    }

                    q += pHi;
                    r -= s;
                }

                return q;
            }
            else
            {
                var q = Divide(n, dLoBits, out ulong r0);
                r = r0;
                return q.lo | ((UInt128)q.hi << 64);
            }
        }

        #endregion

        #region Public API (arithmetic related)

        public static Float128 FusedMultiplyAdd(Float128 left, Float128 right, Float128 addend)
        {
            var prodSign = left.RawSignBit != right.RawSignBit;
            var prodExponent = left.Exponent + right.Exponent;

            var bigSignificand = BigMul256.Multiply(left.Significand, right.Significand);

            var bigLo = bigSignificand._0 | ((UInt128)bigSignificand._1 << 64);
            var bigHi = bigSignificand._2 | ((UInt128)bigSignificand._3 << 64);

            var lowBits = bigLo & (UInt128.MaxValue >> 16);
            var highBits = (bigHi << 19) | (bigLo >> 109);

            // normalize output
            int normDist;
            if ((highBits >> 3) != 0 && IsNormal(left) && IsNormal(right))
            {
                normDist = (short)(UInt128.LeadingZeroCount(highBits >> 3) - 15);
                if (normDist > 0)
                    highBits <<= normDist;
                else if (normDist < 0)
                    highBits >>= -normDist;
                prodExponent -= normDist;
            }
            else if ((highBits >> 3) == 0)
            {
                prodExponent = (short)(-EXPONENT_BIAS + 1);
                normDist = 0;
            }
            else
            {
                normDist = prodExponent - (-EXPONENT_BIAS + 1);
                if (normDist > 0)
                    highBits <<= normDist;
                else if (normDist < 0)
                    highBits >>= normDist;
                prodExponent -= normDist;
            }

            // set sticky bit
            highBits &= UInt128.MaxValue << 1;
            highBits |= UInt128.Min(lowBits + (normDist < 0 ? highBits & ((UInt128.One << -normDist) - 1) : 0), 1);

            bool leftSign = prodSign;
            int leftExponent = prodExponent;
            UInt128 leftSignificand = highBits;

            bool rightSign = addend.RawSignBit;
            int rightExponent = addend.Exponent;
            UInt128 rightSignificand = addend.Significand << 3;

            if (leftExponent < rightExponent)
            {
                var (tempSign, tempExponent, tempSignificand) = (leftSign, leftExponent, leftSignificand);
                (leftSign, leftExponent, leftSignificand) = (rightSign, rightExponent, rightSignificand);
                (rightSign, rightExponent, rightSignificand) = (tempSign, tempExponent, tempSignificand);
            }

            if (leftExponent > -EXPONENT_BIAS + 1 && rightExponent <= -EXPONENT_BIAS + 1)
            {
                return new Float128(leftSignificand, leftExponent, leftSign);
            }

            // set tentative exponent
            int sumExponent = leftExponent;

            // align significands
            var exponentDiff = leftExponent - rightExponent;
            var sumSignificand = rightSignificand >> exponentDiff;

            // set sticky bit
            sumSignificand |= UInt128.Min(rightSignificand & ((UInt128.One << exponentDiff) - 1), 1);

            if ((((sumSignificand & 1) |
                 ((sumSignificand >> 2) & 1)) &
                 ((sumSignificand >> 1) & 1)) == 1) // check rounding condition
            {
                sumSignificand++; // increment pth bit from the left
            }

            // call p + 3 bit adder
            bool sumSign;
            if (leftSign == rightSign)
            {
                sumSign = leftSign;
                sumSignificand += leftSignificand;
            }
            else
            {
                sumSign =
                    (rightSign && leftSignificand < sumSignificand) ||
                    (leftSign && sumSignificand < leftSignificand);
                sumSignificand = leftSignificand < sumSignificand ?
                    sumSignificand - leftSignificand :
                    leftSignificand - sumSignificand;
            }

            // normalize output
            if ((sumSignificand >> 3) != 0 && leftExponent > -EXPONENT_BIAS + 1)
            {
                normDist = (short)(UInt128.LeadingZeroCount(sumSignificand >> 3) - 15);
                if (normDist > 0)
                    sumSignificand <<= normDist;
                else if (normDist < 0)
                    sumSignificand >>= -normDist;
                sumExponent -= normDist;
            }
            else
            {
                sumExponent = (short)(-EXPONENT_BIAS + 1);
                normDist = 0;
            }

            // set sticky bit
            sumSignificand &= UInt128.MaxValue << 1;
            sumSignificand |= normDist < 0 ? UInt128.Min(right.Significand & ((UInt128.One << -normDist) - 1), 1) : 0;

            if ((((sumSignificand & 1) |
                 ((sumSignificand >> 2) & 1)) &
                 ((sumSignificand >> 1) & 1)) == 1) // check rounding condition
            {
                sumSignificand++; // increment pth bit from the left
            }

            return new Float128(sumSignificand >> 3, sumExponent, sumSign);
        }

        public static Float128 Ieee754Remainder(Float128 left, Float128 right)
        {
            return left - right * Round(left / right);
        }

        public static Float128 operator +(Float128 left, Float128 right)
        {
            if (IsInfinity(left) && IsInfinity(right))
            {
                return left.RawSignBit == right.RawSignBit ? left : _sNaN;
            }
            else if (IsNaN(left) || IsNaN(right))
            {
                return _qNaN;
            }
            else
            {
                if (left.Exponent < right.Exponent)
                {
                    var temp = left;
                    left = right;
                    right = temp;
                }

                if (IsNormal(left) && IsSubnormal(right))
                {
                    return left;
                }

                // set tentative exponent
                int sumExponent = left.Exponent;

                // align significands
                var exponentDiff = left.Exponent - right.Exponent;
                var sumSignificand = (right.Significand << 3) >> exponentDiff;

                // set sticky bit
                sumSignificand |= UInt128.Min(right.Significand & ((UInt128.One << exponentDiff) - 1), 1);

                if ((((sumSignificand & 1) |
                     ((sumSignificand >> 2) & 1)) &
                     ((sumSignificand >> 1) & 1)) == 1) // check rounding condition
                {
                    sumSignificand++; // increment pth bit from the left
                }

                // call p + 3 bit adder
                bool rawSignBit;
                var leftSignificand = left.Significand << 3;
                if (left.RawSignBit == right.RawSignBit)
                {
                    rawSignBit = left.RawSignBit;
                    sumSignificand += leftSignificand;
                }
                else
                {
                    rawSignBit =
                        (right.RawSignBit && leftSignificand < sumSignificand) ||
                        (left.RawSignBit && sumSignificand < leftSignificand);
                    sumSignificand = leftSignificand < sumSignificand ?
                        sumSignificand - leftSignificand :
                        leftSignificand - sumSignificand;
                }

                // normalize output
                int normDist;
                if ((sumSignificand >> 3) != 0 && IsNormal(left))
                {
                    normDist = (short)(UInt128.LeadingZeroCount(sumSignificand >> 3) - 15);
                    if (normDist > 0)
                        sumSignificand <<= normDist;
                    else if (normDist < 0)
                        sumSignificand >>= -normDist;
                    sumExponent -= normDist;
                }
                else
                {
                    sumExponent = (short)(-EXPONENT_BIAS + 1);
                    normDist = 0;
                }

                // set sticky bit
                sumSignificand &= UInt128.MaxValue << 1;
                sumSignificand |= normDist < 0 ? UInt128.Min(right.Significand & ((UInt128.One << -normDist) - 1), 1) : 0;

                if ((((sumSignificand & 1) |
                     ((sumSignificand >> 2) & 1)) &
                     ((sumSignificand >> 1) & 1)) == 1) // check rounding condition
                {
                    sumSignificand++; // increment pth bit from the left
                }

                return new Float128(sumSignificand >> 3, sumExponent, rawSignBit);
            }
        }

        public static Float128 operator -(Float128 left, Float128 right)
        {
            right.RawSignBit = !right.RawSignBit;
            return left + right;
        }

        public static Float128 operator *(Float128 left, Float128 right)
        {
            if (IsInfinity(left) || IsInfinity(right) || IsNaN(left) || IsNaN(right))
            {
                return _qNaN;
            }
            else
            {
                var prodSign = left.RawSignBit != right.RawSignBit;
                var prodExponent = left.Exponent + right.Exponent;

                var bigSignificand = BigMul256.Multiply(left.Significand, right.Significand);

                var bigLo = bigSignificand._0 | ((UInt128)bigSignificand._1 << 64);
                var bigHi = bigSignificand._2 | ((UInt128)bigSignificand._3 << 64);

                var lowBits = bigLo & (UInt128.MaxValue >> 16);
                var highBits = (bigHi << 19) | (bigLo >> 109);

                // normalize output
                int normDist;
                if ((highBits >> 3) != 0 && IsNormal(left) && IsNormal(right))
                {
                    normDist = (short)(UInt128.LeadingZeroCount(highBits >> 3) - 15);
                    if (normDist > 0)
                        highBits <<= normDist;
                    else if (normDist < 0)
                        highBits >>= -normDist;
                    prodExponent -= normDist;
                }
                else if ((highBits >> 3) == 0)
                {
                    prodExponent = (short)(-EXPONENT_BIAS + 1);
                    normDist = 0;
                }
                else
                {
                    normDist = prodExponent - (-EXPONENT_BIAS + 1);
                    if (normDist > 0)
                        highBits <<= normDist;
                    else if (normDist < 0)
                        highBits >>= normDist;
                    prodExponent -= normDist;
                }

                // set sticky bit
                highBits &= UInt128.MaxValue << 1;
                highBits |= UInt128.Min(lowBits + (normDist < 0 ? highBits & ((UInt128.One << -normDist) - 1) : 0), 1);

                if ((((highBits & 1) |
                     ((highBits >> 2) & 1)) &
                     ((highBits >> 1) & 1)) == 1) // check rounding condition
                {
                    highBits++; // increment pth bit from the left
                }

                return new Float128(highBits >> 3, prodExponent, prodSign);
            }
        }

        public static Float128 operator /(Float128 left, Float128 right)
        {
            if (IsInfinity(left) && IsFinite(right))
            {
                return left;
            }
            else if (IsFinite(left) && IsInfinity(right))
            {
                return Zero;
            }
            else if ((IsInfinity(left) || IsZero(left)) && (IsInfinity(right) || IsZero(right)))
            {
                return _sNaN;
            }
            else if (IsNaN(left) || IsNaN(right))
            {
                return _qNaN;
            }
            else if (IsNormal(left) && IsSubnormal(right))
            {
                return left.RawSignBit ? _nInf : _pInf;
            }
            else
            {
                var leftAdjust = (int)UInt128.LeadingZeroCount(left.Significand);
                var leftSignificand = left.Significand << leftAdjust;
                var leftExponent = left.Exponent - leftAdjust;

                var rightAdjust = (int)UInt128.TrailingZeroCount(right.Significand);
                var rightSignificand = right.Significand >> rightAdjust;
                var rightExponent = right.Exponent + rightAdjust - 115;

                var quotSignificand = Divide(leftSignificand, rightSignificand, out UInt128 remSignificand);
                var quotExponent = leftExponent - rightExponent;
                var quotSign = left.RawSignBit != right.RawSignBit;

                // normalize output
                int normDist;
                if ((quotSignificand >> 3) != 0)
                {
                    normDist = (short)(UInt128.LeadingZeroCount(quotSignificand >> 3) - 15);
                    if (normDist > 0)
                        quotSignificand <<= normDist;
                    else if (normDist < 0)
                        quotSignificand >>= -normDist;
                    quotExponent -= normDist;
                }
                else
                {
                    quotExponent = (short)(-EXPONENT_BIAS + 1);
                    normDist = 0;
                }

                // set sticky bit
                quotSignificand &= UInt128.MaxValue << 1;
                quotSignificand |= UInt128.Min(remSignificand + (normDist < 0 ? quotSignificand & ((UInt128.One << -normDist) - 1) : 0), 1);

                if ((((quotSignificand & 1) |
                     ((quotSignificand >> 2) & 1)) &
                     ((quotSignificand >> 1) & 1)) == 1) // check rounding condition
                {
                    quotSignificand++; // increment pth bit from the left
                }

                return new Float128(quotSignificand >> 3, quotExponent, quotSign);
            }
        }

        public static Float128 operator %(Float128 left, Float128 right)
        {
            return (Abs(left) - (Abs(right) *
            (Floor(Abs(left) / Abs(right))))) *
            Sign(left);
        }

        public static Float128 operator ++(Float128 value)
        {
            return value + One;
        }

        public static Float128 operator --(Float128 value)
        {
            return value - One;
        }

        #endregion

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
            return new Float128(x.RawSignificand - 1, x.Exponent, x.RawSignBit);
        }

        public static Float128 BitIncrement(Float128 x)
        {
            return new Float128(x.RawSignificand + 1, x.Exponent, x.RawSignBit);
        }

        public static Float128 ScaleB(Float128 x, int n)
        {
            return new Float128(x.RawSignificand, x.Exponent + n, x.RawSignBit);
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

        #region Public API (parsing related)

        public static Float128 Parse(string s, IFormatProvider? provider = null)
        {
            NumberFormatInfo formatter = NumberFormatInfo.GetInstance(provider);
            if (s == formatter.NaNSymbol)
            {
                return _qNaN;
            }
            else if (s == formatter.PositiveInfinitySymbol)
            {
                return _pInf;
            }
            else if (s == formatter.NegativeInfinitySymbol)
            {
                return _nInf;
            }

            var match = Regex.Match(s.Trim(), @$"((?:{Regex.Escape(formatter.NegativeSign)} ?)|\()?(\d+)" +
                @$"(?:{Regex.Escape(formatter.NumberDecimalSeparator)}(\d+))?(?:E(\-?\d+))?" +
                @$"((?: ?{Regex.Escape(formatter.NegativeSign)})|\))?");
            if (!match.Success)
            {
                return _sNaN;
            }
            else
            {
                if (match.Groups[4].Success)
                {
                    var qExponent = int.Parse(match.Groups[4].Value);
                    var allDigits = $"{match.Groups[2].Value}{match.Groups[3].Value}";

                    var builder = new StringBuilder();
                    if (qExponent > 0)
                    {
                        if (qExponent > allDigits.Length)
                        {
                            builder.Append(allDigits);
                            builder.Append(new string('0', qExponent - allDigits.Length + 1));
                        }
                        else
                        {
                            builder.Append(allDigits);
                            builder.Insert(allDigits.Length - qExponent, formatter.NumberDecimalSeparator);
                        }
                    }
                    else if (qExponent < 0)
                    {
                        builder.Append("0");
                        builder.Append(formatter.NumberDecimalSeparator);
                        builder.Append(new string('0', -qExponent));
                        builder.Append(allDigits);
                    }

                    return Parse(builder.ToString(), formatter);
                }

                var wholePart = BigInteger.Parse(match.Groups[2].Value);
                var fracPart = match.Groups[3].Success ? BigInteger.Parse(match.Groups[3].Value) : 0;
                var zExponent = match.Groups[3].Value.TakeWhile(ch => ch == '0').Count();

                int resultExponent;
                if (wholePart == 0 && fracPart == 0)
                {
                    resultExponent = -EXPONENT_BIAS + 1;
                }
                else
                {
                    resultExponent = (int)BigInteger.Log2(wholePart);
                    var wholeDiff = (int)wholePart.GetBitLength() - 116;
                    if (wholeDiff > 0)
                    {
                        wholePart >>= wholeDiff;
                    }
                    else if (wholeDiff < 0)
                    {
                        wholePart <<= -wholeDiff;
                    }
                }

                var resultSign =
                    (match.Groups[1].Value.StartsWith(formatter.NegativeSign) ^
                    match.Groups[5].Value.EndsWith(formatter.NegativeSign)) ||
                    (match.Groups[1].Value.StartsWith('(') &&
                    match.Groups[5].Value.EndsWith(')'));
                var resultSignificand = (UInt128)wholePart;
                if (fracPart == 0)
                {
                    return new Float128(resultSignificand >> 3, resultExponent, resultSign);
                }
                else
                {

                    var log10Value = (int)Math.Floor(BigInteger.Log10(fracPart)) + 1;
                    bool isSubNormal = zExponent > 4390;
                    if (wholePart == 0)
                    {
                        resultExponent = -(int)Math.Floor(BigInteger.Log(BigInteger.Pow(10, zExponent), 2)) + 1;
                    }

                    var pow10 = BigInteger.Pow(10, log10Value + zExponent);
                    int binIndex = 114 - resultExponent;
                    while (binIndex > 0 && fracPart != 0)
                    {
                        fracPart <<= 1;
                        if (fracPart / pow10 == 1)
                        {
                            resultSignificand |= UInt128.One << binIndex;
                        }
                        fracPart %= pow10;
                        --binIndex;
                    }

                    // set sticky bit
                    resultSignificand |= (UInt128)BigInteger.Min(fracPart, 1);

                    if ((((resultSignificand & 1) |
                         ((resultSignificand >> 2) & 1)) &
                         ((resultSignificand >> 1) & 1)) == 1) // check rounding condition
                    {
                        resultSignificand++; // increment pth bit from the left
                    }

                    if (isSubNormal)
                    {
                        var expnDiff = -EXPONENT_BIAS + 1 - resultExponent + 3;
                        return new Float128(resultSignificand >> expnDiff, -EXPONENT_BIAS + 1, resultSign);
                    }
                    else
                    {
                        var normDist = (short)(UInt128.LeadingZeroCount(resultSignificand >> 3) - 15);
                        if (normDist > 0)
                            resultSignificand <<= normDist;
                        else if (normDist < 0)
                            resultSignificand >>= -normDist;
                        resultExponent -= normDist;

                        return new Float128(resultSignificand >> 3, resultExponent, resultSign);
                    }
                }
            }
        }

        public static Float128 Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        {
            return Parse(new string(s), style, provider);
        }

        public static Float128 Parse(string s, NumberStyles style, IFormatProvider? provider)
        {
            if (style == NumberStyles.Float)
            {
                return Parse(s);
            }
            else
            {
                throw new ArgumentException($"Parameter must be equal to {nameof(NumberStyles.Float)}.", nameof(style));
            }
        }

        public static Float128 Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            return Parse(new string(s), provider);
        }

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
        {
            return TryParse(new string(s), style, provider, out result);
        }

        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                result = _sNaN;
            }
            else
            {
                result = Parse(s, style, provider);
            }
            return !IsNaN(result);
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
        {
            return TryParse(new string(s), provider, out result);
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                result = _sNaN;
            }
            else
            {
                result = Parse(s, provider);
            }
            return !IsNaN(result);
        }

        #endregion

        #region Public API (string formatting related)

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            NumberFormatInfo formatter = formatProvider is null ?
                new NumberFormatInfo() { NumberDecimalDigits = 38 } :
                NumberFormatInfo.GetInstance(formatProvider);
            if (IsNaN(this))
            {
                return formatter.NaNSymbol;
            }
            else if (IsPositiveInfinity(this))
            {
                return formatter.PositiveInfinitySymbol;
            }
            else if (IsNegativeInfinity(this))
            {
                return formatter.NegativeInfinitySymbol;
            }
            else
            {
                Float128 rounded = Round(this, formatter.NumberDecimalDigits);
                var builder = new StringBuilder();

                BigInteger fracPart;
                int fracDiff;

                BigInteger wholePart = rounded.Significand;
                var wholeDiff = 112 - rounded.Exponent;

                if (wholeDiff > 0)
                {
                    wholePart >>= wholeDiff;
                    if (rounded.Exponent < 0)
                    {
                        var tzc = (int)UInt128.TrailingZeroCount(rounded.Significand);
                        int shift;
                        if (-rounded.Exponent < 112)
                        {
                            shift = -rounded.Exponent;
                            fracDiff = 0;
                        }
                        else
                        {
                            shift = tzc;
                            fracDiff = -rounded.Exponent - shift;
                        }

                        fracPart = (rounded.Significand >> shift) & SIGNIFICAND_MASK;
                    }
                    else
                    {
                        fracDiff = 0;
                        fracPart = (rounded.Significand << rounded.Exponent) & SIGNIFICAND_MASK;
                    }
                }
                else
                {
                    wholePart <<= -wholeDiff;
                    fracPart = 0;
                    fracDiff = 0;
                }

                if (fracPart == 0)
                {
                    string resultStr;
                    var numDigits = (int)Math.Floor(BigInteger.Log10(wholePart));
                    if (numDigits >= 20)
                    {
                        NumberFormatInfo newFormatter = (NumberFormatInfo)formatter.Clone();
                        newFormatter.NumberGroupSizes = [0];

                        builder.Append(wholePart.ToString(newFormatter));
                        resultStr = $"{builder.ToString().Substring(0, 36).TrimEnd('0').Insert(1, formatter.NumberDecimalSeparator)}E{numDigits}";
                    }
                    else
                    {
                        builder.Append(wholePart.ToString(formatter));
                        resultStr = builder.ToString();
                    }

                    switch (formatter.NumberNegativePattern)
                    {
                        case 0:
                            return rounded.RawSignBit ?
                                $"({resultStr})" : resultStr;
                        case 1:
                            return (rounded.RawSignBit ? formatter.NegativeSign : string.Empty)
                                + resultStr;
                        case 2:
                            return (rounded.RawSignBit ? formatter.NegativeSign : string.Empty)
                                + $" {resultStr}";
                        case 3:
                            return resultStr +
                                (rounded.RawSignBit ? formatter.NegativeSign : string.Empty);
                        case 4:
                            return $"{resultStr} " +
                                (rounded.RawSignBit ? formatter.NegativeSign : string.Empty);
                        default:
                            throw new FormatException($"Invalid {nameof(NumberFormatInfo.NumberNegativePattern)} was specified.");
                    }
                }
                else
                {
                    builder.Append(wholePart.ToString(formatProvider));

                    var numDigits = wholePart == 0 ? 0 : builder.Length;
                    var decimalExpn = numDigits;

                    bool nonZeroFlag = numDigits > 0;
                    BigInteger guardDigit = 0;
                    BigInteger lastDigit = 0;

                    while (!nonZeroFlag || ++numDigits < Math.Min(38, formatter.NumberDecimalDigits))
                    {
                        fracPart *= 10;

                        guardDigit = lastDigit;
                        lastDigit = fracPart >> (112 + fracDiff);

                        fracPart &= (BigInteger.One << (112 + fracDiff)) - 1;

                        nonZeroFlag = nonZeroFlag || lastDigit > 0;
                        if (nonZeroFlag) builder.Append(lastDigit);
                        else ++decimalExpn;
                    }

                    if ((fracPart * 10 >> (112 + fracDiff)) >= 5)
                    {
                        if (lastDigit + 1 == 10)
                        {
                            builder.Remove(builder.Length - 2, 2);
                            builder.Append(guardDigit + 1);
                        }
                        else
                        {
                            builder.Remove(builder.Length - 1, 1);
                            builder.Append(lastDigit + 1);
                        }
                    }

                    if (wholePart == 0 && decimalExpn >= 20)
                    {
                        builder.Insert(1, formatter.NumberDecimalSeparator);
                        string resultStr = $"{builder.ToString().TrimEnd('0')}E{-decimalExpn}";
                        switch (formatter.NumberNegativePattern)
                        {
                            case 0:
                                return rounded.RawSignBit ?
                                    $"({resultStr})" : resultStr;
                            case 1:
                                return (rounded.RawSignBit ? formatter.NegativeSign : string.Empty)
                                    + resultStr;
                            case 2:
                                return (rounded.RawSignBit ? formatter.NegativeSign : string.Empty)
                                    + $" {resultStr}";
                            case 3:
                                return resultStr +
                                    (rounded.RawSignBit ? formatter.NegativeSign : string.Empty);
                            case 4:
                                return $"{resultStr} " +
                                    (rounded.RawSignBit ? formatter.NegativeSign : string.Empty);
                            default:
                                throw new FormatException($"Invalid {nameof(NumberFormatInfo.NumberNegativePattern)} was specified.");
                        }
                    }
                    else if (wholePart == 0)
                    {
                        builder.Insert(0, new string('0', decimalExpn));
                        builder.Insert(1, formatter.NumberDecimalSeparator);
                    }
                    else
                    {
                        builder.Insert(decimalExpn, formatter.NumberDecimalSeparator);
                    }

                    {
                        string resultStr = builder.ToString().TrimEnd('0');
                        switch (formatter.NumberNegativePattern)
                        {
                            case 0:
                                return rounded.RawSignBit ?
                                    $"({resultStr})" : resultStr;
                            case 1:
                                return (rounded.RawSignBit ? formatter.NegativeSign : string.Empty)
                                    + resultStr;
                            case 2:
                                return (rounded.RawSignBit ? formatter.NegativeSign : string.Empty)
                                    + $" {resultStr}";
                            case 3:
                                return resultStr +
                                    (rounded.RawSignBit ? formatter.NegativeSign : string.Empty);
                            case 4:
                                return $"{resultStr} " +
                                    (rounded.RawSignBit ? formatter.NegativeSign : string.Empty);
                            default:
                                throw new FormatException($"Invalid {nameof(NumberFormatInfo.NumberNegativePattern)} was specified.");
                        }
                    }
                }
            }
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            string? formatStr = format.Length == 0 ? null : new string(format);
            string resultStr = ToString(formatStr, provider);

            charsWritten = Math.Min(destination.Length, resultStr.Length);
            return resultStr.TryCopyTo(destination);
        }

        #endregion

        #region Public API (conversion methods)

        static bool INumberBase<Float128>.TryConvertFromChecked<TOther>(TOther value, out Float128 result)
        {
            try
            {
                result = (double)(object)value;
                return true;
            }
            catch (InvalidCastException)
            {
                result = _sNaN;
                return false;
            }
        }

        static bool INumberBase<Float128>.TryConvertFromSaturating<TOther>(TOther value, out Float128 result)
        {
            try
            {
                result = (double)(object)value;
                return true;
            }
            catch (InvalidCastException)
            {
                result = _sNaN;
                return false;
            }
        }

        static bool INumberBase<Float128>.TryConvertFromTruncating<TOther>(TOther value, out Float128 result)
        {
            try
            {
                result = (double)(object)value;
                return true;
            }
            catch (InvalidCastException)
            {
                result = _sNaN;
                return false;
            }
        }

        static bool INumberBase<Float128>.TryConvertToChecked<TOther>(Float128 value, out TOther result)
        {
            try
            {
                result = (TOther)(object)(double)value;
                return true;
            }
            catch (InvalidCastException)
            {
                result = default;
                return false;
            }
        }

        static bool INumberBase<Float128>.TryConvertToSaturating<TOther>(Float128 value, out TOther result)
        {
            try
            {
                result = (TOther)(object)(double)value;
                return true;
            }
            catch (InvalidCastException)
            {
                result = default;
                return false;
            }
        }

        static bool INumberBase<Float128>.TryConvertToTruncating<TOther>(Float128 value, out TOther result)
        {
            try
            {
                result = (TOther)(object)(double)value;
                return true;
            }
            catch (InvalidCastException)
            {
                result = default;
                return false;
            }
        }

        #endregion

        #region Public API (conversion operators)

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

        #region Public API: Software-based CoRDiC Routine

        private const int SINCOS_ITER_COUNT = 16;

        private static readonly Float128[] _thetaTable;

        private static readonly Float128 _K_n;

        private static Float128 AtanPow2(int k)
        {
            Float128 x_n = One;
            if (k == 0) return Pi * 0.25;
            for (int n = 0; n < 5; n++)
            {
                x_n = ScaleB(x_n * x_n * (x_n * (x_n * (3 - ScaleB(x_n, k)) + ScaleB(One, k + 3)) - 12) + 12, -k - 2) / 3;
            }
            return x_n;
        }

        private static Float128 ComputeK(int n)
        {
            Float128 K_i = One;
            for (int i = 0; i < n; i++)
            {
                K_i /= Sqrt(One + ScaleB(One, -i << 1));
            }
            return K_i;
        }

        public static Float128 Cos(Float128 x)
        {
            return SinCos(x).Cos;
        }

        public static Float128 CosPi(Float128 x)
        {
            return SinCosPi(x).CosPi;
        }

        public static Float128 Sin(Float128 x)
        {
            return SinCos(x).Sin;
        }

        public static Float128 SinPi(Float128 x)
        {
            return SinCosPi(x).SinPi;
        }

        public static (Float128 Sin, Float128 Cos) SinCos(Float128 alpha)
        {
            Float128 sigma, theta = Zero;
            (Float128 x, Float128 y) = (One, Zero);
            for (int i = 0; i < SINCOS_ITER_COUNT; i++)
            {
                sigma = theta < alpha ? One : NegativeOne;

                (x, y) = (x - ScaleB(sigma * y, -i), ScaleB(sigma * x, -i) + y);
                theta += sigma * _thetaTable[i];
            }
            return (y * _K_n, x * _K_n);
        }

        public static (Float128 SinPi, Float128 CosPi) SinCosPi(Float128 x)
        {
            return SinCos(x * Pi);
        }

        public static Float128 Tan(Float128 alpha)
        {
            (Float128 y, Float128 x) = SinCos(alpha);
            return y / x;
        }

        public static Float128 TanPi(Float128 alpha)
        {
            (Float128 y, Float128 x) = SinCos(alpha * Pi);
            return y / x;
        }

        #endregion

        #region Public API (library functions)

        public static Float128 Log2(Float128 value)
        {
            throw new NotImplementedException();
        }

        public static Float128 Atan2(Float128 y, Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Atan2Pi(Float128 y, Float128 x)
        {
            throw new NotImplementedException();
        }

        public static int ILogB(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Exp(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Exp10(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Exp2(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Acosh(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Asinh(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Atanh(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Cosh(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Sinh(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Tanh(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Log(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Log(Float128 x, Float128 newBase)
        {
            throw new NotImplementedException();
        }

        public static Float128 Log10(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Pow(Float128 x, Float128 y)
        {
            throw new NotImplementedException();
        }

        public static Float128 Cbrt(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Hypot(Float128 x, Float128 y)
        {
            return Sqrt(x * x + y * y);
        }

        public static Float128 RootN(Float128 x, int n)
        {
            throw new NotImplementedException();
        }

        public static Float128 Sqrt(Float128 x)
        {
            Float128 y_n = x * 0.5;
            for (int n = 0; n < 10; n++)
            {
                y_n = 0.5 * (y_n + x / y_n);
            }
            return y_n;
        }

        public static Float128 Acos(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 AcosPi(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Asin(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 AsinPi(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 Atan(Float128 x)
        {
            throw new NotImplementedException();
        }

        public static Float128 AtanPi(Float128 x)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
