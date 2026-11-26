/*
 *  Copyright 2025-2026 Chosen Few Software
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
using QuadrupleLib.Accelerators;
using Xunit;

namespace QuadrupleLib.Tests.Arithmetic
{
    public abstract class DivisionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByNaNIsNaN(double x)
        {
            Assert.True(Float128<TAccelerator>.IsNaN(x / Float128<TAccelerator>.NaN));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByNegativeInfinityIsZero(double x)
        {
            Assert.True(Float128<TAccelerator>.IsZero(x / Float128<TAccelerator>.NegativeInfinity));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByNegativeOneIsNegate(double x)
        {
            Assert.Equal(-x, x / Float128<TAccelerator>.NegativeOne);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByPositiveInfinityIsZero(double x)
        {
            Assert.True(Float128<TAccelerator>.IsZero(x / Float128<TAccelerator>.PositiveInfinity));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByOneIsIdentity(double x)
        {
            Assert.Equal(x, x / Float128<TAccelerator>.One);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByZeroIsInfinity(double x)
        {
            Float128<TAccelerator> result = x / Float128<TAccelerator>.Zero;

            Assert.True(Float128<TAccelerator>.IsInfinity(result));
            Assert.Equal(x < 0, Float128<TAccelerator>.IsNegative(result));
        }

        [Theory]
        [InlineData(1.0, true)]
        [InlineData(0.5, true)]
        [InlineData(-1.0, false)]
        [InlineData(-0.5, false)]
        public void DivideByNegativeZeroIsInfinityWithSign(double x, bool negative)
        {
            Float128<TAccelerator> result = x / Float128<TAccelerator>.NegativeZero;

            Assert.True(Float128<TAccelerator>.IsInfinity(result));
            Assert.Equal(negative, Float128<TAccelerator>.IsNegative(result));
        }

        [Theory]
        [InlineData(-5.5)]
        [InlineData(-10.0)]
        [InlineData(-30.655)]
        [InlineData(-3.33)]
        [InlineData(5.5)]
        [InlineData(10.0)]
        [InlineData(30.655)]
        [InlineData(3.33)]
        public void DivideBySelfIsOne(double x)
        {
            Assert.Equal(Float128<TAccelerator>.One, (Float128<TAccelerator>)x / x);
        }

        [Theory]
        [InlineData([3.0, 1.5, 2.0])]
        [InlineData([-8.0, 2.0, -4.0])]
        [InlineData([3.0, -0.5, -6.0])]
        [InlineData([-5.0, -0.625, 8.0])]
        public void DivideGeneralIsCorrect(double x, double y, double z)
        {
            Assert.Equal(z, (Float128<TAccelerator>)x / y);
        }

        [Theory]
        [InlineData(-5.5)]
        [InlineData(-10.0)]
        [InlineData(-30.655)]
        [InlineData(-3.33)]
        [InlineData(5.5)]
        [InlineData(10.0)]
        [InlineData(30.655)]
        [InlineData(3.33)]
        public void DivideByBigNumberIsZero(double x)
        {
            // the divisor overflows to +Inf, so the quotient is a zero with the sign of x
            Float128<TAccelerator> y = x / Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, short.MaxValue / 2 + 1);
            Assert.True(Float128<TAccelerator>.IsZero(y));
            Assert.Equal(x < 0, Float128<TAccelerator>.IsNegative(y));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideNormalBySubnormalIsInfinity(double x)
        {
            Assert.True(Float128<TAccelerator>.IsInfinity(x / Float128<TAccelerator>.Epsilon));
        }

        [Fact]
        public void DivideSubnormalIsCorrect()
        {
            // 2 * Epsilon has the bit pattern 2; the expected value is written as bits, not taken from the library
            Float128<TAccelerator> twoEps = RawStorage<TAccelerator>.FromBits((UInt128)2);
            Assert.Equal(Float128<TAccelerator>.Epsilon, twoEps / 2.0);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void NegateIsDivideByNegativeOne(double x)
        {
            Assert.Equal(-x, x / Float128<TAccelerator>.NegativeOne);
        }

        [Fact]
        public void PositiveInfinityDividedByPositiveInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity / Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void PositiveInfinityDividedByNegativeInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity / Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void NegativeInfinityDividedByNegativeInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.NegativeInfinity / Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void NegativeInfinityDividedByPositiveInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.NegativeInfinity / Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void PositiveInfinityDividedByZeroIsPositiveInfinity()
        {
            // IEEE 754: an infinity divided by a zero is an infinity, with no invalid operation
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.PositiveInfinity / Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void NegativeInfinityDividedByZeroIsNegativeInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.NegativeInfinity / Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void ZeroDividedByZeroIsPositiveNaN()
        {
            // 0 / 0: invalid operation, returns positive NaN
            Float128<TAccelerator> result = Float128<TAccelerator>.Zero / Float128<TAccelerator>.Zero;
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Theory]
        [InlineData(false, 3.0, false)]
        [InlineData(true, 3.0, true)]
        [InlineData(false, -3.0, true)]
        [InlineData(true, -3.0, false)]
        public void ZeroDividedByFiniteIsZeroWithQuotientSign(bool dividendNegative, double divisor, bool quotientNegative)
        {
            Float128<TAccelerator> dividend = dividendNegative ? Float128<TAccelerator>.NegativeZero : Float128<TAccelerator>.Zero;
            Float128<TAccelerator> result = dividend / divisor;
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.Equal(quotientNegative, RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        public void ZeroDividedByInfinityIsZeroWithQuotientSign(bool dividendNegative, bool divisorNegative, bool quotientNegative)
        {
            Float128<TAccelerator> dividend = dividendNegative ? Float128<TAccelerator>.NegativeZero : Float128<TAccelerator>.Zero;
            Float128<TAccelerator> divisor = divisorNegative ? Float128<TAccelerator>.NegativeInfinity : Float128<TAccelerator>.PositiveInfinity;
            Float128<TAccelerator> result = dividend / divisor;
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.Equal(quotientNegative, RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        public void FiniteDividedByInfinityIsZeroWithQuotientSign(bool dividendNegative, bool divisorNegative, bool quotientNegative)
        {
            Float128<TAccelerator> dividend = dividendNegative ? (Float128<TAccelerator>)(-1.5) : (Float128<TAccelerator>)1.5;
            Float128<TAccelerator> divisor = divisorNegative ? Float128<TAccelerator>.NegativeInfinity : Float128<TAccelerator>.PositiveInfinity;
            Float128<TAccelerator> result = dividend / divisor;
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.Equal(quotientNegative, RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void LargestFiniteDividedBySmallestSubnormalIsInfinityWithQuotientSign()
        {
            // largest finite: exponent field 0x7FFE with an all-ones fraction
            Float128<TAccelerator> largest = RawStorage<TAccelerator>.FromBits(((UInt128)0x7FFE << 112) | ((UInt128.One << 112) - UInt128.One));
            Float128<TAccelerator> epsilon = Float128<TAccelerator>.Epsilon;
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(largest / epsilon));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(-largest / epsilon));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(largest / -epsilon));
        }

        [Fact]
        public void DivisionAtUnderflowBoundaryIsExact()
        {
            // 1 / 2^16382 is the smallest normal, 2^-16382; 1 / 2^16383 is the subnormal 2^111 * 2^-16494
            Float128<TAccelerator> one = Float128<TAccelerator>.One;
            Assert.Equal(UInt128.One << 112, RawStorage<TAccelerator>.Bits(one / Float128<TAccelerator>.ScaleB(one, 16382)));
            Assert.Equal(UInt128.One << 111, RawStorage<TAccelerator>.Bits(one / Float128<TAccelerator>.ScaleB(one, 16383)));
        }

        [Theory]
        [InlineData(1UL, 0UL)]
        [InlineData(3UL, 2UL)]
        [InlineData(5UL, 2UL)]
        [InlineData(7UL, 4UL)]
        public void SmallestSubnormalQuotientIsRoundedHalfToEven(ulong numerator, ulong expectedBits)
        {
            // numerator * 2^-16494 divided by 2 is a tie between two subnormals; the tie goes to the even significand
            Float128<TAccelerator> dividend = RawStorage<TAccelerator>.FromBits((UInt128)numerator);
            Assert.Equal((UInt128)expectedBits, RawStorage<TAccelerator>.Bits(dividend / 2.0));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.3)]
        [InlineData(0.33)]
        public void PositiveInfinityDividedByPositiveIsPositiveInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.PositiveInfinity / x);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.3)]
        [InlineData(0.33)]
        public void NegativeInfinityDividedByPositiveIsNegativeInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.NegativeInfinity / x);
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(-1.0)]
        [InlineData(-2.3)]
        [InlineData(-0.33)]
        public void PositiveInfinityDividedByNegativeIsNegativeInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.PositiveInfinity / x);
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(-1.0)]
        [InlineData(-2.3)]
        [InlineData(-0.33)]
        public void NegativeInfinityDividedByNegativeIsPositiveInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.NegativeInfinity / x);
        }

        [Fact]
        public void OneDividedByNegativeInfinityIsNegativeZero()
        {
            // IEEE 754: a finite nonzero x divided by an infinity is a zero whose sign is the XOR of the operand signs
            Float128<TAccelerator> result = Float128<TAccelerator>.One / Float128<TAccelerator>.NegativeInfinity;

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void NegativeOneDividedByPositiveInfinityIsNegativeZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.NegativeOne / Float128<TAccelerator>.PositiveInfinity;

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void DivideSmallestSubnormalByZeroIsPositiveInfinity()
        {
            // IEEE 754: a finite nonzero dividend divided by a zero is an infinity with the XOR of the signs
            Float128<TAccelerator> smallestSubnormal = RawStorage<TAccelerator>.FromBits(UInt128.One);

            Float128<TAccelerator> result = smallestSubnormal / Float128<TAccelerator>.Zero;

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(result));
        }

        [Theory]
        [InlineData(2.0)]
        [InlineData(0.0)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NaN)]
        public void PositiveInfinityRemainderIsPositiveNaN(double y)
        {
            // invalid operation: NaN
            Float128<TAccelerator> result = Float128<TAccelerator>.PositiveInfinity % y;

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void IeeeRemainderOfTiedQuotientIsRoundedToNearestEven()
        {
            // IEEE 754: n = round-half-even(x / y); 2.5 -> n = 2, 3.5 -> n = 4
            Assert.Equal((Float128<TAccelerator>)0.5, Float128<TAccelerator>.Ieee754Remainder(2.5, 1.0));
            Assert.Equal((Float128<TAccelerator>)(-0.5), Float128<TAccelerator>.Ieee754Remainder(3.5, 1.0));
        }

        [Fact]
        public void IeeeRemainderOfExactMultipleIsZeroWithSignOfDividend()
        {
            Float128<TAccelerator> negative = Float128<TAccelerator>.Ieee754Remainder(-4.0, 2.0);
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(negative));
        }

        [Fact]
        public void IeeeRemainderWithSubnormalResultIsExact()
        {
            // 3u / 2u = 1.5, ties to 2, so r = -u
            Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits((UInt128)3);
            Float128<TAccelerator> y = RawStorage<TAccelerator>.FromBits((UInt128)2);
            Assert.Equal(RawStorage<TAccelerator>.FromBits(UInt128.One | (UInt128.One << 127)), Float128<TAccelerator>.Ieee754Remainder(x, y));
        }

        [Fact]
        public void IeeeRemainderOfHalfOddIntegerAboveTwoToTheHundredIsRoundedToEven()
        {
            // reference: n = 2^100 (even) and n = 2^100 + 2 (even)
            Float128<TAccelerator> large = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 100);
            Assert.Equal((Float128<TAccelerator>)0.5, Float128<TAccelerator>.Ieee754Remainder(large + (Float128<TAccelerator>)0.5, 1.0));
            Assert.Equal((Float128<TAccelerator>)(-0.5), Float128<TAccelerator>.Ieee754Remainder(large + (Float128<TAccelerator>)1.5, 1.0));
        }

        [Fact]
        public void IeeeRemainderAcrossChunksIsExact()
        {
            // reference: 2^200 + 2^88 = -1 (mod 3); |r| <= 1.5 selects -1
            Float128<TAccelerator> x = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 200) + Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 88);
            Assert.Equal((Float128<TAccelerator>)(-1.0), Float128<TAccelerator>.Ieee754Remainder(x, 3.0));
        }

        [Fact]
        public void IeeeRemainderOfOneByTwoIsOne()
        {
            // tie: n = 0, r = 1
            Assert.Equal((Float128<TAccelerator>)1.0, Float128<TAccelerator>.Ieee754Remainder(1.0, 2.0));
        }

        [Fact]
        public void IeeeRemainderRoundsNonTiedQuotientToNearestWithSignedRemainder()
        {
            // reference: 1.5 / 2 = 0.75 rounds to n = 1, so r = 1.5 - 2 = -0.5
            Assert.Equal((Float128<TAccelerator>)(-0.5), Float128<TAccelerator>.Ieee754Remainder(1.5, 2.0));
            Assert.Equal((Float128<TAccelerator>)0.5, Float128<TAccelerator>.Ieee754Remainder(-1.5, 2.0));
            Assert.Equal((Float128<TAccelerator>)(-0.5), Float128<TAccelerator>.Ieee754Remainder(1.5, -2.0));
        }

        [Fact]
        public void IeeeRemainderOfExactMultipleAboveChunkWidthIsZeroWithSignOfDividend()
        {
            // reference: 2^200 = 1 (mod 3), so 3 * 2^200 is an exact multiple of 3
            Float128<TAccelerator> multiple = Float128<TAccelerator>.ScaleB((Float128<TAccelerator>)3.0, 200);

            Float128<TAccelerator> positive = Float128<TAccelerator>.Ieee754Remainder(multiple, 3.0);
            Assert.True(Float128<TAccelerator>.IsZero(positive));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(positive));

            Float128<TAccelerator> negative = Float128<TAccelerator>.Ieee754Remainder(-multiple, 3.0);
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(negative));

            Float128<TAccelerator> negativeDivisor = Float128<TAccelerator>.Ieee754Remainder(-multiple, -3.0);
            Assert.True(Float128<TAccelerator>.IsZero(negativeDivisor));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(negativeDivisor));
        }

        [Fact]
        public void IeeeRemainderOfTieWithOddTruncatedQuotientIsRoundedToEven()
        {
            // reference: 3 = 2 * 2 - 1 (n = 2, even) and -3 = -2 * 2 + 1 (n = -2, even)
            Assert.Equal((Float128<TAccelerator>)(-1.0), Float128<TAccelerator>.Ieee754Remainder((Float128<TAccelerator>)3.0, (Float128<TAccelerator>)2.0));
            Assert.Equal((Float128<TAccelerator>)1.0, Float128<TAccelerator>.Ieee754Remainder((Float128<TAccelerator>)(-3.0), (Float128<TAccelerator>)2.0));
        }

        [Fact]
        public void IeeeRemainderOfTieWithEvenTruncatedQuotientIsRoundedToEven()
        {
            // reference: 5 = 2 * 2 + 1 (n = 2, even) and -5 = -2 * 2 - 1 (n = -2, even)
            Assert.Equal((Float128<TAccelerator>)1.0, Float128<TAccelerator>.Ieee754Remainder((Float128<TAccelerator>)5.0, (Float128<TAccelerator>)2.0));
            Assert.Equal((Float128<TAccelerator>)(-1.0), Float128<TAccelerator>.Ieee754Remainder((Float128<TAccelerator>)(-5.0), (Float128<TAccelerator>)2.0));
        }

        [Fact]
        public void ModuloOfTieWithOddTruncatedQuotientIsTruncated()
        {
            // reference: 3 = 1 * 2 + 1 and -3 = -1 * 2 - 1, with the quotient truncated toward zero
            Assert.Equal((Float128<TAccelerator>)1.0, (Float128<TAccelerator>)3.0 % (Float128<TAccelerator>)2.0);
            Assert.Equal((Float128<TAccelerator>)(-1.0), (Float128<TAccelerator>)(-3.0) % (Float128<TAccelerator>)2.0);
        }

        [Fact]
        public void ModuloOfTieWithEvenTruncatedQuotientIsTruncated()
        {
            // reference: 5 = 2 * 2 + 1 and -5 = -2 * 2 - 1, with the quotient truncated toward zero
            Assert.Equal((Float128<TAccelerator>)1.0, (Float128<TAccelerator>)5.0 % (Float128<TAccelerator>)2.0);
            Assert.Equal((Float128<TAccelerator>)(-1.0), (Float128<TAccelerator>)(-5.0) % (Float128<TAccelerator>)2.0);
        }

        [Fact]
        public void IeeeRemainderOfSpecialOperandsIsIeee754()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Ieee754Remainder(1.0, 0.0)));
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Ieee754Remainder(Float128<TAccelerator>.PositiveInfinity, 2.0)));
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Ieee754Remainder(Float128<TAccelerator>.NaN, 2.0)));
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Ieee754Remainder(2.0, Float128<TAccelerator>.NaN)));
            Assert.Equal((Float128<TAccelerator>)3.0, Float128<TAccelerator>.Ieee754Remainder(3.0, Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void IeeeRemainderOfMinimumNormalAndNextUpIsNegativeSmallestSubnormal()
        {
            // quotient rounds to 1, so r = x - y
            Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits((UInt128)1 << 112);
            Float128<TAccelerator> y = RawStorage<TAccelerator>.FromBits(((UInt128)1 << 112) | UInt128.One);
            Assert.Equal(RawStorage<TAccelerator>.FromBits((UInt128.One << 127) | UInt128.One), Float128<TAccelerator>.Ieee754Remainder(x, y));
        }

        [Fact]
        public void ModuloIsTruncatedRemainderWithSignOfDividend()
        {
            // truncated quotient; sign of x
            Assert.Equal((Float128<TAccelerator>)1.5, (Float128<TAccelerator>)7.5 % (Float128<TAccelerator>)2.0);
            Assert.Equal((Float128<TAccelerator>)(-1.5), (Float128<TAccelerator>)(-7.5) % (Float128<TAccelerator>)2.0);
            Assert.Equal((Float128<TAccelerator>)1.5, (Float128<TAccelerator>)7.5 % (Float128<TAccelerator>)(-2.0));
        }

        [Fact]
        public void ModuloOfLargeOperandsIsExact()
        {
            // 2^100 + 1 is exact in binary128; 2^100 = 1 (mod 3), so 2^100 + 1 = 2 (mod 3)
            Float128<TAccelerator> large = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 100) + Float128<TAccelerator>.One;
            Assert.Equal((Float128<TAccelerator>)2.0, large % (Float128<TAccelerator>)3.0);
        }

        [Fact]
        public void ModuloOfExactMultipleIsZeroWithSignOfDividend()
        {
            Float128<TAccelerator> negative = (Float128<TAccelerator>)(-4.0) % (Float128<TAccelerator>)2.0;
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(negative));
        }

        [Fact]
        public void ModuloByInfinityIsDividend()
        {
            Assert.Equal((Float128<TAccelerator>)3.0, (Float128<TAccelerator>)3.0 % Float128<TAccelerator>.PositiveInfinity);
            Assert.Equal((Float128<TAccelerator>)(-3.0), (Float128<TAccelerator>)(-3.0) % Float128<TAccelerator>.PositiveInfinity);
        }

        [Theory]
        [InlineData(false, 3.0)]
        [InlineData(true, 3.0)]
        [InlineData(false, -3.0)]
        [InlineData(true, -3.0)]
        [InlineData(false, double.PositiveInfinity)]
        [InlineData(true, double.PositiveInfinity)]
        [InlineData(false, double.NegativeInfinity)]
        [InlineData(true, double.NegativeInfinity)]
        public void RemainderOfSignedZeroIsSignOfDividend(bool dividendNegative, double divisor)
        {
            Float128<TAccelerator> dividend = dividendNegative ? Float128<TAccelerator>.NegativeZero : Float128<TAccelerator>.Zero;
            Float128<TAccelerator> modulo = dividend % divisor;
            Float128<TAccelerator> ieee = Float128<TAccelerator>.Ieee754Remainder(dividend, divisor);
            Assert.True(Float128<TAccelerator>.IsZero(modulo));
            Assert.Equal(dividendNegative, RawStorage<TAccelerator>.IsSignSet(modulo));
            Assert.True(Float128<TAccelerator>.IsZero(ieee));
            Assert.Equal(dividendNegative, RawStorage<TAccelerator>.IsSignSet(ieee));
        }

        [Fact]
        public void ModuloWithSubnormalResultIsExact()
        {
            // 3u % 2u = u, with u = 2^-16494
            Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits((UInt128)3);
            Float128<TAccelerator> y = RawStorage<TAccelerator>.FromBits((UInt128)2);
            Assert.Equal(RawStorage<TAccelerator>.FromBits(UInt128.One), x % y);
        }

        [Fact]
        public void SmallestSubnormalRemainderZeroIsNaN()
        {
            // zero divisor: NaN
            Float128<TAccelerator> smallestSubnormal = RawStorage<TAccelerator>.FromBits(UInt128.One);

            Float128<TAccelerator> result = smallestSubnormal % Float128<TAccelerator>.Zero;

            Assert.True(Float128<TAccelerator>.IsNaN(result));
        }

        [Fact]
        public void SubnormalQuotientWithThreeSignificandIsExact()
        {
            // (3 * 2^-16383) / 2 = 3 * 2^-16384, the subnormal with fraction 3 * 2^110
            Float128<TAccelerator> threeTimesTwoToTheMinus16383 = RawStorage<TAccelerator>.FromBits((UInt128.One << 112) | (UInt128.One << 111));

            Float128<TAccelerator> quotient = threeTimesTwoToTheMinus16383 / (Float128<TAccelerator>)2.0;

            Assert.Equal((UInt128)3 << 110, RawStorage<TAccelerator>.Bits(quotient));
        }

        [Fact]
        public void OneDividedByOnePlusThreeUlpsIsRoundedQuotient()
        {
            // reference: 2^112 / (2^112 + 3) rounds to 1 - 6 * 2^-113
            Float128<TAccelerator> divisor = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | (UInt128)3);

            Float128<TAccelerator> quotient = Float128<TAccelerator>.One / divisor;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFEFFFFFFFFFFFFFFFFFFFFFFFFFFFA"), RawStorage<TAccelerator>.Bits(quotient));
        }

        [Fact]
        public void OneDividedByOnePlusTwoToTheMinus100IsRoundedQuotient()
        {
            // reference: 2^100 / (2^100 + 1) rounds to 1 - 2^-100
            Float128<TAccelerator> divisor = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | ((UInt128)1 << 12));

            Float128<TAccelerator> quotient = Float128<TAccelerator>.One / divisor;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFEFFFFFFFFFFFFFFFFFFFFFFFFE000"), RawStorage<TAccelerator>.Bits(quotient));
        }

        [Fact]
        public void FiveDividedByTwoToThe90PlusOneIsRoundedQuotient()
        {
            // reference: 5 / (2^90 + 1) rounds to 1.25 * 2^-88 * (1 - 2^-90)
            Float128<TAccelerator> divisor = RawStorage<TAccelerator>.FromBits(((UInt128)16473 << 112) | ((UInt128)1 << 22));

            Float128<TAccelerator> quotient = (Float128<TAccelerator>)5.0 / divisor;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FA73FFFFFFFFFFFFFFFFFFFFFB00000"), RawStorage<TAccelerator>.Bits(quotient));
        }

        [Fact]
        public void SevenDividedByOnePlusFiveUlpsIsRoundedQuotient()
        {
            // reference: 7 - 9 * 2^-110 (4001BFFF...FFF7)
            Float128<TAccelerator> divisor = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | (UInt128)5);

            Float128<TAccelerator> quotient = (Float128<TAccelerator>)7.0 / divisor;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("4001BFFFFFFFFFFFFFFFFFFFFFFFFFF7"), RawStorage<TAccelerator>.Bits(quotient));
        }

        [Fact]
        public void NormalDividedBySubnormalIsTwo()
        {
            // reference: 2^-16382 / 2^-16383 = 2 exactly
            Float128<TAccelerator> dividend = RawStorage<TAccelerator>.FromBits(UInt128.One << 112);
            Float128<TAccelerator> divisor = RawStorage<TAccelerator>.FromBits(UInt128.One << 111);

            Float128<TAccelerator> quotient = dividend / divisor;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("40000000000000000000000000000000"), RawStorage<TAccelerator>.Bits(quotient));
        }

        [Fact]
        public void NormalDividedBySubnormalIsFour()
        {
            // reference: 2^-16382 / 2^-16384 = 4, with 2^-16384 subnormal
            Float128<TAccelerator> dividend = RawStorage<TAccelerator>.FromBits(UInt128.One << 112);
            Float128<TAccelerator> divisor = RawStorage<TAccelerator>.FromBits(UInt128.One << 110);

            Float128<TAccelerator> quotient = dividend / divisor;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("40010000000000000000000000000000"), RawStorage<TAccelerator>.Bits(quotient));
        }

        [Fact]
        public void NormalDividedBySubnormalIsFourThirds()
        {
            // reference: 2^-16382 / (3 * 2^-16384) = 4/3, rounded to nearest
            Float128<TAccelerator> dividend = RawStorage<TAccelerator>.FromBits(UInt128.One << 112);
            Float128<TAccelerator> divisor = RawStorage<TAccelerator>.FromBits((UInt128)3 << 110);

            Float128<TAccelerator> quotient = dividend / divisor;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF5555555555555555555555555555"), RawStorage<TAccelerator>.Bits(quotient));
        }

        [Fact]
        public void DivideNormalByTinySubnormalIsInfinity()
        {
            // reference: 1.0 / 2^-16491 = 2^16491 overflows to +Inf
            Float128<TAccelerator> dividend = RawStorage<TAccelerator>.FromBits((UInt128)16383 << 112);
            Float128<TAccelerator> divisor = RawStorage<TAccelerator>.FromBits(UInt128.One << 3);

            Float128<TAccelerator> quotient = dividend / divisor;

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(quotient));
        }

        [Fact]
        public void RemainderSeededSweepOfExponentGapsIsExact()
        {
            // seeded BigInteger reference sweep over gaps 0 to 300, plus wide gaps 500 to 32765
            Random random = new Random(20260102);
            int[] wideGaps = { 500, 1000, 4096, 16383, 16384, 32000, 32764, 32765 };
            for (int index = 0; index < 301 + wideGaps.Length; index++)
            {
                int gap = index < 301 ? index : wideGaps[index - 301];
                int trials = index < 301 ? 8 : 2;
                for (int trial = 0; trial < trials; trial++)
                {
                    // both exponents stay in the normal range [-16382, 16383]
                    int low = random.Next(-16382, 16384 - gap);
                    int high = low + gap;
                    int exponentX = random.Next(2) == 0 ? low : high;
                    int exponentY = exponentX == low ? high : low;
                    BigInteger significandX = RawStorage<TAccelerator>.RandomSignificand(random);
                    BigInteger significandY = RawStorage<TAccelerator>.RandomSignificand(random);
                    bool negativeX = random.Next(2) == 0;
                    bool negativeY = random.Next(2) == 0;

                    Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(negativeX, exponentX, significandX));
                    Float128<TAccelerator> y = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(negativeY, exponentY, significandY));

                    int lsbX = exponentX - 112, lsbY = exponentY - 112;
                    Assert.Equal(ReferenceRemainderBits(negativeX, lsbX, significandX, negativeY, lsbY, significandY, false), RawStorage<TAccelerator>.Bits(x % y));
                    Assert.Equal(ReferenceRemainderBits(negativeX, lsbX, significandX, negativeY, lsbY, significandY, true), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Ieee754Remainder(x, y)));
                }
            }
        }

        [Fact]
        public void RemainderSeededSweepOfSubnormalOperandsIsExact()
        {
            // seeded BigInteger reference sweep; trial % 3 selects subnormal x and y, subnormal x only, or subnormal y only
            Random random = new Random(20260104);
            for (int trial = 0; trial < 1000; trial++)
            {
                bool subnormalX = trial % 3 != 2;
                bool subnormalY = trial % 3 != 1;
                bool negativeX = random.Next(2) == 0;
                bool negativeY = random.Next(2) == 0;
                (UInt128 bitsX, BigInteger significandX, int lsbX) = RandomOperand(random, subnormalX, negativeX);
                (UInt128 bitsY, BigInteger significandY, int lsbY) = RandomOperand(random, subnormalY, negativeY);

                Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(bitsX);
                Float128<TAccelerator> y = RawStorage<TAccelerator>.FromBits(bitsY);
                Assert.Equal(ReferenceRemainderBits(negativeX, lsbX, significandX, negativeY, lsbY, significandY, false), RawStorage<TAccelerator>.Bits(x % y));
                Assert.Equal(ReferenceRemainderBits(negativeX, lsbX, significandX, negativeY, lsbY, significandY, true), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Ieee754Remainder(x, y)));
            }
        }

        [Fact]
        public void DivisionSeededSweepOfSignificandPairsIsCorrectlyRounded()
        {
            // seeded BigInteger reference sweep, normal operands and results
            Random random = new Random(20260103);
            for (int trial = 0; trial < 2000; trial++)
            {
                // quotient exponent stays in [-16382, 16383]
                int exponentY = random.Next(-16382, 16384);
                int exponentX = random.Next(System.Math.Max(-16382, exponentY - 16381), System.Math.Min(16383, exponentY + 16382) + 1);
                BigInteger significandX = RawStorage<TAccelerator>.RandomSignificand(random);
                BigInteger significandY = RawStorage<TAccelerator>.RandomSignificand(random);
                if (trial % 4 == 0)
                {
                    // equal significands
                    significandY = significandX;
                }
                else if (trial % 4 == 1)
                {
                    // significands one step apart
                    significandX ^= BigInteger.One;
                }
                else if (trial % 4 == 2)
                {
                    // trailing zeros in the divisor
                    int zeros = random.Next(1, 40);
                    significandY = (significandY >> zeros) << zeros;
                }

                bool negativeX = random.Next(2) == 0;
                bool negativeY = random.Next(2) == 0;

                Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(negativeX, exponentX, significandX));
                Float128<TAccelerator> y = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(negativeY, exponentY, significandY));
                Assert.Equal(ReferenceQuotientBits(negativeX != negativeY, exponentX - exponentY, significandX, significandY), RawStorage<TAccelerator>.Bits(x / y));
            }
        }

        [Fact]
        public void DivisionSeededSweepOfSubnormalQuotientsIsCorrectlyRounded()
        {
            // seeded BigInteger reference sweep; exponent gaps -16400 to -16379 straddle the subnormal boundary near 2^-16382
            Random random = new Random(20260105);
            for (int trial = 0; trial < 1000; trial++)
            {
                int exponentGap = random.Next(-16400, -16378);
                int exponentY = random.Next(-16382 - exponentGap, 16384);
                int exponentX = exponentY + exponentGap;
                BigInteger significandX = RawStorage<TAccelerator>.RandomSignificand(random);
                BigInteger significandY = RawStorage<TAccelerator>.RandomSignificand(random);
                bool negativeX = random.Next(2) == 0;
                bool negativeY = random.Next(2) == 0;

                Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(negativeX, exponentX, significandX));
                Float128<TAccelerator> y = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(negativeY, exponentY, significandY));
                Assert.Equal(ReferenceQuotientBits(negativeX != negativeY, exponentX - exponentY, significandX, significandY), RawStorage<TAccelerator>.Bits(x / y));
            }
        }

        [Fact]
        public void DivisionSeededSweepOfSubnormalOperandsIsCorrectlyRounded()
        {
            Random random = new Random(20260106);
            for (int trial = 0; trial < 1000; trial++)
            {
                bool subnormalX = trial % 3 != 2;
                bool subnormalY = trial % 3 != 1;
                bool negativeX = random.Next(2) == 0;
                bool negativeY = random.Next(2) == 0;
                (UInt128 bitsY, BigInteger significandY, int lsbY) = RandomOperand(random, subnormalY, negativeY);

                // keep the quotient finite
                int maxExponentX = lsbY + (int)significandY.GetBitLength() - 1 + 16382;
                (UInt128 bitsX, BigInteger significandX, int lsbX) = RandomOperand(random, subnormalX, negativeX, maxExponentX);

                Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(bitsX);
                Float128<TAccelerator> y = RawStorage<TAccelerator>.FromBits(bitsY);
                (BigInteger normalizedX, int normalizedLsbX) = NormalizeSignificand(significandX, lsbX);
                (BigInteger normalizedY, int normalizedLsbY) = NormalizeSignificand(significandY, lsbY);
                Assert.Equal(ReferenceQuotientBits(negativeX != negativeY, normalizedLsbX - normalizedLsbY, normalizedX, normalizedY), RawStorage<TAccelerator>.Bits(x / y));
            }
        }

        // shift to 113 bits; value unchanged
        private static (BigInteger Significand, int Lsb) NormalizeSignificand(BigInteger significand, int lsb)
        {
            int shift = 113 - (int)significand.GetBitLength();
            return (significand << shift, lsb - shift);
        }

        // round to nearest even
        private static UInt128 ReferenceQuotientBits(bool negative, int exponentGap, BigInteger significandX, BigInteger significandY)
        {
            // scale and divide
            BigInteger quotient = BigInteger.DivRem(significandX << 116, significandY, out BigInteger remainder);
            int shift = (int)quotient.GetBitLength() - 113;
            int leadingExponent = exponentGap - 116 + shift + 112;

            bool subnormal = leadingExponent < -16382;
            if (subnormal)
            {
                shift = -16494 - (exponentGap - 116);
            }

            BigInteger kept = quotient >> shift;
            BigInteger dropped = quotient & ((BigInteger.One << shift) - 1);
            BigInteger half = BigInteger.One << (shift - 1);
            if (dropped > half || (dropped == half && (!remainder.IsZero || !kept.IsEven)))
            {
                kept += BigInteger.One;
            }

            if (subnormal)
            {
                // subnormal result
                return (negative ? UInt128.One << 127 : UInt128.Zero) | (UInt128)kept;
            }

            if (kept == BigInteger.One << 113)
            {
                kept >>= 1;
                shift++;
            }

            leadingExponent = exponentGap - 116 + shift + 112;
            return RawStorage<TAccelerator>.PackBits(negative, leadingExponent, kept);
        }

        private static (UInt128 Bits, BigInteger Significand, int Lsb) RandomOperand(Random random, bool subnormal, bool negative, int maxExponent = 16383)
        {
            UInt128 sign = negative ? UInt128.One << 127 : UInt128.Zero;
            if (subnormal)
            {
                BigInteger fraction = BigInteger.Max(RawStorage<TAccelerator>.RandomSignificand(random) - (BigInteger.One << 112), BigInteger.One);
                return (sign | (UInt128)fraction, fraction, -16494);
            }

            int exponent = random.Next(-16382, maxExponent + 1);
            BigInteger significand = RawStorage<TAccelerator>.RandomSignificand(random);
            return (RawStorage<TAccelerator>.PackBits(negative, exponent, significand), significand, exponent - 112);
        }

        // exact remainder bits
        private static UInt128 ReferenceRemainderBits(bool negativeX, int lsbX, BigInteger significandX, bool negativeY, int lsbY, BigInteger significandY, bool roundQuotientToNearest)
        {
            int lsb = lsbX < lsbY ? lsbX : lsbY;
            BigInteger x = significandX << (lsbX - lsb);
            BigInteger y = significandY << (lsbY - lsb);
            BigInteger quotient = BigInteger.DivRem(x, y, out BigInteger remainder);
            if (roundQuotientToNearest && (remainder << 1 > y || (remainder << 1 == y && !quotient.IsEven)))
            {
                remainder -= y;
            }

            if (remainder.IsZero)
            {
                return negativeX ? UInt128.One << 127 : UInt128.Zero;
            }

            bool negative = negativeX ^ (remainder.Sign < 0);
            UInt128 sign = negative ? UInt128.One << 127 : UInt128.Zero;
            BigInteger magnitude = BigInteger.Abs(remainder);
            int bitLength = (int)magnitude.GetBitLength();
            Assert.True(bitLength <= 113, "the exact remainder must fit in binary128");
            int leadingExponent = lsb + bitLength - 1;
            if (leadingExponent < -16382)
            {
                return sign | (UInt128)(magnitude << (lsb + 16494));
            }

            return sign | RawStorage<TAccelerator>.PackBits(false, leadingExponent, magnitude << (113 - bitLength));
        }
    }

    public class DivisionTests_DefaultAccelerator :
        DivisionTests<DefaultAccelerator>
    { }

    public class DivisionTests_SoftwareAccelerator :
        DivisionTests<SoftwareAccelerator>
    { }
}
