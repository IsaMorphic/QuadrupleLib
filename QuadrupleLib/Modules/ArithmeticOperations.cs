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

using QuadrupleLib.Utilities;

namespace QuadrupleLib;

public partial struct Float128<TAccelerator>
{
    #region Private API: Exact Multiplication

    private static UInt256 MultiplyExact(Float128<TAccelerator> left, Float128<TAccelerator> right, out int prodExponent)
    {
        // normalize subnormal operands
        var leftAdjust = (int)UInt128.LeadingZeroCount(left.Significand) - 15;
        var rightAdjust = (int)UInt128.LeadingZeroCount(right.Significand) - 15;
        prodExponent = left.Exponent - leftAdjust + right.Exponent - rightAdjust;

        var bigSignificand = BigMul256.Multiply2x2<TAccelerator>(left.Significand << leftAdjust, right.Significand << rightAdjust);

        var bigLo = bigSignificand._0 | ((UInt128)bigSignificand._1 << 64);
        var bigHi = bigSignificand._2 | ((UInt128)bigSignificand._3 << 64);

        return (UInt256)bigLo | ((UInt256)bigHi << 128);
    }

    private static UInt128 MultiplySignificands(Float128<TAccelerator> left, Float128<TAccelerator> right, out int prodExponent)
    {
        var product = MultiplyExact(left, right, out prodExponent);

        var lowBits = product._lo & (UInt128.MaxValue >> 19);
        var highBits = (product._hi << 19) | (product._lo >> 109);

        if ((highBits >> 3) == 0)
        {
            prodExponent = -EXPONENT_BIAS + 1;
            return UInt128.Zero;
        }

        // normalize output
        var normDist = (int)UInt128.LeadingZeroCount(highBits >> 3) - 15;
        if (normDist < 0)
        {
            lowBits |= highBits & ((UInt128.One << -normDist) - 1);
            highBits >>= -normDist;
        }
        prodExponent -= normDist;

        // set sticky bit
        highBits |= UInt128.Min(lowBits, 1);

        // rescale subnormal product
        if (prodExponent < -EXPONENT_BIAS + 1)
        {
            var shiftDist = Math.Min(-EXPONENT_BIAS + 1 - prodExponent, 127);
            var shiftedOut = highBits & ((UInt128.One << shiftDist) - 1);

            highBits >>= shiftDist;
            highBits |= UInt128.Min(shiftedOut, 1);
            prodExponent = -EXPONENT_BIAS + 1;
        }

        return highBits;
    }

    #endregion

    #region Private API: Sticky Shift Routine

    private static UInt256 ShiftWithSticky(UInt256 value, int shiftDist)
    {
        if (shiftDist >= 0)
        {
            return value << shiftDist;
        }

        var shifted = value >> -shiftDist;
        var shiftedOut = value - (shifted << -shiftDist);
        return shifted | (shiftedOut == UInt256.Zero ? UInt256.Zero : UInt256.One);
    }

    #endregion

    #region Private API: Rounded Result Construction

    private static UInt128 RoundToNearestEven(UInt128 significand)
    {
        // guard bit set, and any of the round bit, sticky bit or least significant bit set
        var roundUp = ((significand >> 2) & 1) == 1 && (significand & 0b1011) != 0;
        return (significand >> 3) + (roundUp ? UInt128.One : UInt128.Zero);
    }

    private static Float128<TAccelerator> Compose(UInt128 significand, int exponent, bool rawSignBit)
    {
        if ((significand >> 113) != 0)
        {
            significand >>= 1;
            exponent++;
        }

        if (exponent > EXPONENT_BIAS)
        {
            return rawSignBit ? _nInf : _pInf;
        }
        else
        {
            return new Float128<TAccelerator>(significand, exponent, rawSignBit);
        }
    }

    #endregion

    #region Public API (arithmetic related)

    public static Float128<TAccelerator> FusedMultiplyAdd(Float128<TAccelerator> left, Float128<TAccelerator> right, Float128<TAccelerator> addend)
    {
        if (IsNaN(left) || IsNaN(right) || IsNaN(addend))
        {
            return _qNaN;
        }

        var prodSign = left.RawSignBit != right.RawSignBit;
        if (IsInfinity(left) || IsInfinity(right))
        {
            if (IsZero(left) || IsZero(right))
            {
                return _qNaN;
            }
            return (prodSign ? _nInf : _pInf) + addend;
        }
        else if (IsInfinity(addend))
        {
            return addend;
        }
        else if (IsZero(left) || IsZero(right))
        {
            return IsZero(addend) ? (prodSign && addend.RawSignBit ? NegativeZero : Zero) : addend;
        }
        else if (IsZero(addend))
        {
            return left * right;
        }

        // the product stays exact until the single final rounding
        var product = MultiplyExact(left, right, out int prodExponent);

        var productUnit = prodExponent - 224;
        var addendUnit = addend.Exponent - 112;

        var productTop = productUnit + 255 - (int)UInt256.LeadingZeroCount(product);
        var addendTop = addendUnit + 127 - (int)UInt128.LeadingZeroCount(addend.Significand);

        // align significands, with the larger operand's leading bit at bit 250
        var baseExponent = Math.Max(productTop, addendTop) - 250;
        var productAligned = ShiftWithSticky(product, productUnit - baseExponent);
        var addendAligned = ShiftWithSticky(addend.Significand, addendUnit - baseExponent);

        bool sumSign;
        UInt256 sum;
        if (prodSign == addend.RawSignBit)
        {
            sumSign = prodSign;
            sum = productAligned + addendAligned;
        }
        else if (productAligned < addendAligned)
        {
            sumSign = addend.RawSignBit;
            sum = addendAligned - productAligned;
        }
        else
        {
            sumSign = prodSign;
            sum = productAligned - addendAligned;
        }

        if (sum == UInt256.Zero)
        {
            return Zero;
        }

        // normalize output
        var sumExponent = Math.Max(baseExponent + 255 - (int)UInt256.LeadingZeroCount(sum), -EXPONENT_BIAS + 1);
        var shiftDist = sumExponent - 115 - baseExponent;

        // set sticky bit
        UInt128 sumSignificand = shiftDist > 0 ?
            (UInt128)ShiftWithSticky(sum, -shiftDist) :
            (UInt128)(sum << -shiftDist);

        return Compose(RoundToNearestEven(sumSignificand), sumExponent, sumSign);
    }

    public static Float128<TAccelerator> Ieee754Remainder(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        return left - right * Round(left / right);
    }

    public static Float128<TAccelerator> operator +(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        if (IsNaN(left) || IsNaN(right))
        {
            return _qNaN;
        }
        else if (IsInfinity(left) && IsInfinity(right))
        {
            return left.RawSignBit == right.RawSignBit ? left : _qNaN;
        }
        else if (IsInfinity(left) && !IsInfinity(right))
        {
            return left;
        }
        else if (IsInfinity(right) && !IsInfinity(left))
        {
            return right;
        }
        else
        {
            if (left.Exponent < right.Exponent)
            {
                var temp = left;
                left = right;
                right = temp;
            }

            // From @GreatCoder1000 (https://github.com/GreatCoder1000):
            // when exponent difference is beyond rounding precision, 
            // do not do any adding (leave left addend untouched). 
            if (left.Exponent - right.Exponent > 115)
            {
                return left;
            }

            // set tentative exponent
            int sumExponent = left.Exponent;

            // align significands
            var exponentDiff = left.Exponent - right.Exponent;
            var sumSignificand = (right.Significand << 3) >> exponentDiff;

            // set sticky bit
            sumSignificand |= UInt128.Min((right.Significand << 3) & ((UInt128.One << exponentDiff) - 1), 1);

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
            if ((sumSignificand >> 3) != 0)
            {
                normDist = (int)UInt128.LeadingZeroCount(sumSignificand >> 3) - 15;
                if (normDist > 0)
                {
                    normDist = Math.Min(normDist, sumExponent - (-EXPONENT_BIAS + 1));
                    sumSignificand <<= normDist;
                }
                else if (normDist < 0)
                {
                    // set sticky bit
                    var shiftedOut = sumSignificand & ((UInt128.One << -normDist) - 1);
                    sumSignificand = (sumSignificand >> -normDist) | UInt128.Min(shiftedOut, 1);
                }
                sumExponent -= normDist;
            }
            else
            {
                sumExponent = -EXPONENT_BIAS + 1;
            }

            return Compose(RoundToNearestEven(sumSignificand), sumExponent, rawSignBit);
        }
    }

    public static Float128<TAccelerator> operator -(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        right.RawSignBit = !right.RawSignBit;
        return left + right;
    }

    public static Float128<TAccelerator> operator *(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        if (IsNaN(left) || IsNaN(right))
        {
            return _qNaN;
        }
        else if (IsInfinity(left) || IsInfinity(right))
        {
            if (IsZero(left) || IsZero(right))
            {
                return _qNaN;
            }
            return left.RawSignBit != right.RawSignBit ? _nInf : _pInf;
        }
        else
        {
            var prodSign = left.RawSignBit != right.RawSignBit;
            if (left.Exponent + right.Exponent > EXPONENT_BIAS)
            {
                return prodSign ? _nInf : _pInf;
            }

            var highBits = MultiplySignificands(left, right, out int prodExponent);

            return Compose(RoundToNearestEven(highBits), prodExponent, prodSign);
        }
    }

    public static Float128<TAccelerator> operator /(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        if (IsNaN(left) || IsNaN(right))
        {
            return _qNaN;
        }
        else if ((IsInfinity(left) && IsInfinity(right)) || (IsZero(left) && IsZero(right)))
        {
            return _qNaN;
        }
        else if (IsInfinity(left) || IsZero(right))
        {
            return left.RawSignBit != right.RawSignBit ? _nInf : _pInf;
        }
        else if (IsFinite(left) && IsInfinity(right))
        {
            return left.RawSignBit != right.RawSignBit ? NegativeZero : Zero;
        }
        else
        {
            var leftAdjust = (int)UInt256.LeadingZeroCount(left.Significand) - 32;
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
                (fullSignificand, remSignificand) = TAccelerator.DivRem(leftSignificand, rightSignificand);
            }

            // normalize output
            int normDist;
            if ((fullSignificand >> 3) != 0)
            {
                normDist = (int)UInt256.LeadingZeroCount(fullSignificand >> 3) - 143;
                if (quotExponent - normDist > EXPONENT_BIAS) // overflowing quotient
                {
                    return quotSign ? _nInf : _pInf;
                }
                else if (quotExponent - normDist <= -EXPONENT_BIAS + 1) // underflowing quotient
                {
                    normDist = quotExponent + EXPONENT_BIAS - 1;
                }

                // divide again at the normalized scale so the guard, round and sticky bits are exact
                UInt256 scaled = normDist >= 0 ? leftSignificand << normDist : leftSignificand >> -normDist;
                (fullSignificand, remSignificand) = TAccelerator.DivRem(scaled, rightSignificand);
                quotSignificand = (UInt128)fullSignificand;
                quotExponent -= normDist;
            }
            else
            {
                quotExponent = -EXPONENT_BIAS + 1;
                quotSignificand = UInt128.Zero;
                normDist = 0;
            }

            // set sticky bit: the remainder, or any bits of the dividend shifted out below the guard bits
            quotSignificand |= remSignificand != UInt128.Zero || (normDist < 0 && (leftSignificand & ((UInt256.One << -normDist) - 1)) != UInt256.Zero) ?
                UInt128.One : UInt128.Zero;

            // rescale subnormal quotient
            if (quotExponent < -EXPONENT_BIAS + 1)
            {
                var shiftDist = Math.Min(-EXPONENT_BIAS + 1 - quotExponent, 127);
                var shiftedOut = quotSignificand & ((UInt128.One << shiftDist) - 1);

                quotSignificand >>= shiftDist;
                quotSignificand |= UInt128.Min(shiftedOut, 1);
                quotExponent = -EXPONENT_BIAS + 1;
            }

            return Compose(RoundToNearestEven(quotSignificand), quotExponent, quotSign);
        }
    }

    public static Float128<TAccelerator> operator %(Float128<TAccelerator> left, Float128<TAccelerator> right)
    {
        return (Abs(left) - (Abs(right) *
        (Floor(Abs(left) / Abs(right))))) *
        Sign(left);
    }

    public static Float128<TAccelerator> operator ++(Float128<TAccelerator> value)
    {
        return value + One;
    }

    public static Float128<TAccelerator> operator --(Float128<TAccelerator> value)
    {
        return value - One;
    }

    #endregion
}
