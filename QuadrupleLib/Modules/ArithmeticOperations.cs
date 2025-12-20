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

using System.Runtime.InteropServices;

namespace QuadrupleLib;

public partial struct Float128
{

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

    #region Private API: Software-based 128-bit Division Routine

    private static UInt128 Divide(UInt128 n, ulong d, out ulong r) 
    {
        uint n_0 = (uint)n;
        uint n_1 = (uint)(n >> 32);
        ulong n_2 = (ulong)(n >> 64);

        (UInt128 q_2, ulong r_2) = Math.DivRem(n_2, d);
        (UInt128 q_1, ulong r_1) = Math.DivRem((r_2 << 32) | n_1, d);
        (UInt128 q_0, r) = Math.DivRem((r_1 << 32) | n_0, d);

        return q_0 | (q_1 << 32) | (q_2 << 64);
    }

    private static UInt128 Divide(UInt128 n, UInt128 d, out UInt128 r)
    {
        var dLoBits = (ulong)d;
        var dHiBits = (ulong)(d >> 64);
        if (d != 0 && d > n)
        {
            r = n; return UInt128.Zero;
        }
        else if (d == n)
        {
            r = UInt128.Zero; return UInt128.One;
        }
        else if (dHiBits > 0)
        {
            UInt128 q = UInt128.Zero; r = n;
            while (r >= d)
            {
                var p = Divide(r, dHiBits, out ulong _);
                var p_hi = (ulong)(p >> 64);

                var prod = BigMul256.Multiply(d, p_hi);
                UInt128 s = prod._0 | ((UInt128)prod._1 << 64);
                while (p_hi > 0 && r < s)
                {
                    prod = BigMul256.Multiply(d, p_hi >>= 1);
                    s = prod._0 | ((UInt128)prod._1 << 64);
                }

                if (p_hi == 0)
                {
                    ++q; r -= d;
                    break;
                }
                else
                {
                    q += p_hi; r -= s;
                }
            }

            return q;
        }
        else
        {
            var q = Divide(n, dLoBits, out ulong _r);
            r = _r; return q;
        }
    }

    #endregion

    #region Private API: Software-based 256-bit Division Routine

    private static UInt256 Divide(UInt256 n, UInt128 d, out UInt128 r)
    {
        ulong n_0 = (ulong)n;
        ulong n_1 = (ulong)(n >> 64);
        UInt128 n_2 = (UInt128)(n >> 128);

        UInt256 q_2 = Divide(n_2, d, out UInt128 r_2);
        UInt256 q_1 = Divide((r_2 << 64) | n_1, d, out UInt128 r_1);
        UInt256 q_0 = Divide((r_1 << 64) | n_0, d, out r);

        return q_0 | (q_1 << 64) | (q_2 << 128);
    }

    #endregion

    #region Public API (arithmetic related)

    public static Float128 FusedMultiplyAdd(Float128 left, Float128 right, Float128 addend)
    {
        if (IsInfinity(left) || IsInfinity(right) || IsNaN(left) || IsNaN(right) || IsNaN(addend))
        {
            return _qNaN;
        }

        var prodSign = left.RawSignBit != right.RawSignBit;
        var prodExponent = left.Exponent + right.Exponent;

        BigMul256 bigSignificand;
        if (prodExponent > EXPONENT_BIAS)
        {
            return (prodSign ? _nInf : _pInf) + addend;
        }
        else if (IsInfinity(addend))
        {
            return addend;
        }
        else
        {
            bigSignificand = BigMul256.Multiply(left.Significand, right.Significand);
        }

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
                highBits >>= -normDist;
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
        sumSignificand |= normDist < 0 ? UInt128.Min(rightSignificand & ((UInt128.One << -normDist) - 1), 1) : 0;

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
        else if (IsInfinity(left) && !IsInfinity(right))
        {
            return left;
        }
        else if (IsInfinity(right) && !IsInfinity(left))
        {
            return right;
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

            BigMul256 bigSignificand;
            if (prodExponent > EXPONENT_BIAS)
            {
                return prodSign ? _nInf : _pInf;
            }
            else
            {
                bigSignificand = BigMul256.Multiply(left.Significand, right.Significand);
            }

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
                    highBits >>= -normDist;
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
        if (IsFinite(left) && IsInfinity(right))
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
        else if ((IsInfinity(left) && IsFinite(right)) || (IsNormal(left) && IsSubnormal(right)))
        {
            return left.RawSignBit != right.RawSignBit ? _nInf : _pInf;
        }
        else
        {
            var leftAdjust = (int)UInt256.LeadingZeroCount(left.Significand);
            var leftSignificand = (UInt256)left.Significand << leftAdjust;
            var leftExponent = left.Exponent - leftAdjust + 115;

            var rightAdjust = (int)UInt128.TrailingZeroCount(right.Significand);
            var rightSignificand = right.Significand >> rightAdjust;
            var rightExponent = right.Exponent + rightAdjust;

            var quotExponent = leftExponent - rightExponent;
            var quotSign = left.RawSignBit != right.RawSignBit;

            UInt256 fullSignificand;
            UInt128 quotSignificand, remSignificand;
            if (quotExponent > EXPONENT_BIAS)
            {
                return quotSign ? _nInf : _pInf;
            }
            else
            {
                fullSignificand = Divide(leftSignificand, rightSignificand, out remSignificand);
            }

            // normalize output
            int normDist;
            if ((fullSignificand >> 3) != 0)
            {
                normDist = 143 - (int)UInt256.LeadingZeroCount(fullSignificand >> 3);
                quotSignificand = (UInt128)(fullSignificand >> normDist);
                quotExponent += normDist;
            }
            else
            {
                quotExponent = (short)(-EXPONENT_BIAS + 1);
                quotSignificand = UInt128.Zero;
                normDist = 0;
            }

            // set sticky bit
            quotSignificand &= UInt128.MaxValue << 1;
            quotSignificand |= (UInt128)UInt256.Min(remSignificand + fullSignificand & ((UInt256.One << normDist) - 1), 1);

            if ((((quotSignificand & 1) |
                 ((quotSignificand >> 2) & 1)) &
                 ((quotSignificand >> 1) & 1)) == 1) // check rounding condition
            {
                quotSignificand++; // increment pth bit from the left
            }

            if (quotExponent < -EXPONENT_BIAS + 1)
            {
                var finalAdjust = (int)UInt128.TrailingZeroCount(quotSignificand) - quotExponent - EXPONENT_BIAS - 111;
                return new Float128(quotSignificand >> finalAdjust, -EXPONENT_BIAS + 1, quotSign);
            }
            else
            {
                return new Float128(quotSignificand >> 3, quotExponent, quotSign);
            }
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
}
