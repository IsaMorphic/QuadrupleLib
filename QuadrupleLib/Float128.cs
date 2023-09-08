/*
 *  Copyright 2023 Chosen Few Software
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace QuadrupleLib
{
    public struct Float128 : ISignedNumber<Float128>
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

        #region Public API (constants)

        private static readonly Float128 _qNaN = new Float128(UInt128.One, short.MaxValue, false);
        private static readonly Float128 _sNaN = new Float128(UInt128.One, short.MaxValue, true);

        private static readonly Float128 _pInf = new Float128(UInt128.Zero, short.MaxValue, false);
        private static readonly Float128 _nInf = new Float128(UInt128.Zero, short.MaxValue, true);

        public static Float128 PositiveInfinity => _pInf;
        public static Float128 NegativeInfinity => _nInf;
        public static Float128 NaN => _qNaN;

        private static readonly Float128 _pi = Parse("3.1415926535897932384626433832795028");
        public static Float128 PI => _pi;

        private static readonly Float128 _epsilon = new Float128(UInt128.One, -EXPONENT_BIAS + 1, false);
        public static Float128 Epsilon => _epsilon;

        public static Float128 One => 1.0;
        public static Float128 Zero => new();
        public static Float128 NegativeOne => -1.0;

        public static int Radix => 2;
        public static Float128 AdditiveIdentity => Zero;
        public static Float128 MultiplicativeIdentity => One;

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

        #region Private API: Full-width 128-bit Multiplication Utility

        private struct BigMul
        {
            private readonly UInt128 _lo;
            private readonly UInt128 _hi;

            private Span<ulong> GetUInt64Bits()
            {
                var thisSpan = MemoryMarshal.CreateSpan(ref this, 1);
                return MemoryMarshal.Cast<BigMul, ulong>(thisSpan);
            }

            public ReadOnlySpan<UInt128> GetUInt128Bits()
            {
                var thisSpan = MemoryMarshal.CreateSpan(ref this, 1);
                return MemoryMarshal.Cast<BigMul, UInt128>(thisSpan);
            }

            private static BigMul Add(BigMul left, BigMul right)
            {
                var leftBits = left.GetUInt64Bits();
                var rightBits = right.GetUInt64Bits();

                BigMul result = new();
                Span<ulong> newBits = result.GetUInt64Bits();

                int i;
                for (i = 0; i < newBits.Length - 1; i++)
                {
                    newBits[i] = leftBits[i] + rightBits[i];
                    var carry = (ulong)Math.Max(0, leftBits[i].CompareTo(newBits[i]));
                    newBits[i + 1] = leftBits[i + 1] + rightBits[i + 1] + carry;
                }

                return result;
            }

            private static BigMul Multiply(UInt128 left, ulong right)
            {
                Span<UInt128> leftVal = stackalloc[] { left };
                Span<ulong> leftBits = MemoryMarshal.Cast<UInt128, ulong>(leftVal);

                BigMul result = new();
                Span<ulong> newBits = result.GetUInt64Bits();

                int i;
                ulong oldHigh = Math.BigMul(leftBits[0], right, out ulong low1);
                newBits[0] = low1;
                for (i = 1; i < leftBits.Length; i++)
                {
                    ulong newHigh = Math.BigMul(leftBits[i], right, out ulong low2);
                    newBits[i] += low2 + oldHigh;
                    oldHigh = newHigh + (ulong)Math.Max(0, low2.CompareTo(low2 + oldHigh));
                }
                newBits[i] = oldHigh;

                return result;
            }

            public static BigMul Multiply(UInt128 left, UInt128 right)
            {
                Span<UInt128> rightVal = stackalloc[] { right };
                Span<ulong> rightBits = MemoryMarshal.Cast<UInt128, ulong>(rightVal);

                var leftProd = Multiply(left, rightBits[0]);
                var rightProd = Multiply(left, rightBits[1]);
                var rightShift = new BigMul();

                var rightProdBits = rightProd.GetUInt64Bits();
                var rightShiftBits = rightShift.GetUInt64Bits();
                rightProdBits.Slice(0, 3).CopyTo(rightShiftBits.Slice(1));

                return Add(leftProd, rightShift);
            }
        }

        #endregion

        #region Public API (arithmetic related)

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
            else if (left.Exponent >= right.Exponent)
            {
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
            else
            {
                return right + left;
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

                var bigSignificand = BigMul.Multiply(left.Significand, right.Significand).GetUInt128Bits();
                var lowBits = bigSignificand[0] & (UInt128.MaxValue >> 16);
                var highBits = (bigSignificand[1] << 19) | (bigSignificand[0] >> 109);

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

                var quotSignificand = leftSignificand / rightSignificand;
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
                quotSignificand |= UInt128.Min(leftSignificand % rightSignificand + (normDist < 0 ? quotSignificand & ((UInt128.One << -normDist) - 1) : 0), 1);

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

        public static Float128 Parse(string s)
        {
            switch (s)
            {
                case "NaN":
                    return _qNaN;
                case "Infinity":
                    return _pInf;
                case "-Infinity":
                    return _nInf;
            }

            var match = Regex.Match(s, @"(\-)?(\d+)(?:\.(\d+))?(?:E(\-?\d+))?");
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
                            builder.Insert(allDigits.Length - qExponent, '.');
                        }
                    }
                    else if (qExponent < 0)
                    {
                        builder.Append("0.");
                        builder.Append(new string('0', -qExponent));
                        builder.Append(allDigits);
                    }

                    return Parse(builder.ToString());
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

                var resultSign = match.Groups[1].Value.StartsWith('-');
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

        public static bool TryParse(string s, out Float128 result)
        {
            result = Parse(s);
            return !IsNaN(result);
        }

        // TODO: These methods >(

        public static Float128 Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static Float128 Parse(string s, NumberStyles style, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static Float128 Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static Float128 Parse(string s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public API (string formatting related)

        public override string ToString()
        {
            if (IsNaN(this))
            {
                return "NaN";
            }
            else if (IsPositiveInfinity(this))
            {
                return "Infinity";
            }
            else if (IsNegativeInfinity(this))
            {
                return "-Infinity";
            }
            else
            {
                var builder = new StringBuilder();

                BigInteger fracPart;
                int fracDiff;

                BigInteger wholePart = Significand;
                var wholeDiff = 112 - Exponent;

                if (wholeDiff > 0)
                {
                    wholePart >>= wholeDiff;
                    if (Exponent < 0)
                    {
                        var tzc = (int)UInt128.TrailingZeroCount(Significand);
                        int shift;
                        if (-Exponent < 112)
                        {
                            shift = -Exponent;
                            fracDiff = 0;
                        }
                        else
                        {
                            shift = tzc;
                            fracDiff = -Exponent - shift;
                        }

                        fracPart = (Significand >> shift) & SIGNIFICAND_MASK;
                    }
                    else
                    {
                        fracDiff = 0;
                        fracPart = (Significand << Exponent) & SIGNIFICAND_MASK;
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
                    builder.Append(wholePart);
                    var numDigits = (int)Math.Floor(BigInteger.Log10(wholePart));
                    if (numDigits >= 20)
                    {
                        return $"{builder.ToString().Substring(0, 36).TrimEnd('0').Insert(1, ".")}E{numDigits}";
                    }
                    else
                    {
                        return builder.ToString();
                    }
                }
                else
                {
                    builder.Append(wholePart);

                    var numDigits = wholePart == 0 ? 0 : builder.Length;
                    var decimalExpn = numDigits;

                    bool nonZeroFlag = numDigits > 0;
                    BigInteger guardDigit = 0;
                    BigInteger lastDigit = 0;

                    while (!nonZeroFlag || ++numDigits < 38)
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
                        builder.Insert(1, '.');
                        return $"{(RawSignBit ? "-" : "")}{builder.ToString().TrimEnd('0')}E{-decimalExpn}";
                    }
                    else if (wholePart == 0)
                    {
                        builder.Insert(0, new string('0', decimalExpn));
                        builder.Insert(1, '.');
                    }
                    else
                    {
                        builder.Insert(decimalExpn, '.');
                    }

                    return $"{(RawSignBit ? "-" : "")}{builder.ToString().TrimEnd('0')}";
                }
            }
        }

        // TODO: IFormatProvider
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            var formatter = NumberFormatInfo.GetInstance(formatProvider);
            return ToString();
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var formatter = NumberFormatInfo.GetInstance(provider);
            throw new NotImplementedException();
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
                ulong bits = BitConverter.DoubleToUInt64Bits(x);
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

                if (BitOperations.TrailingZeroCount(smallMantissa >> 3) == 52)
                {
                    return BitConverter.UInt64BitsToDouble(
                        ((ulong)((x.Exponent + 0x400) << 52) & 0x7ff) |
                        (x.RawSignBit ? 1UL << 63 : 0));
                }
                else
                {
                    return BitConverter.UInt64BitsToDouble(
                        ((smallMantissa >> 3) & 0xfffffffffffffUL) |
                        ((ulong)((x.Exponent + 0x3ff) << 52) & 0x7ff) |
                        (x.RawSignBit ? 1UL << 63 : 0));
                }
            }
        }

        #endregion
    }
}
