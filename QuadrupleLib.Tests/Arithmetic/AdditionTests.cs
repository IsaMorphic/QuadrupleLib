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

using QuadrupleLib.Accelerators;
using System.Numerics;
using Xunit;

namespace QuadrupleLib.Tests.Arithmetic
{
    public abstract class AdditionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        [InlineData(double.NaN)]
        public void AddNaNIsNaN(double x)
        {
            Assert.True(Float128<TAccelerator>.IsNaN(x + Float128<TAccelerator>.NaN));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void AddNegativeInfinityIsNegativeInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, x + Float128<TAccelerator>.NegativeInfinity);
        }

        [Theory]
        [InlineData([0.5, 1.5])]
        [InlineData([1.0, 2.0])]
        [InlineData([-1.0, 0.0])]
        public void AddOneIsCorrect(double x, double y)
        {
            Assert.Equal(y, x + Float128<TAccelerator>.One);
        }

        [Theory]
        [InlineData([0.5, -0.5])]
        [InlineData([1.0, 0.0])]
        [InlineData([-1.0, -2.0])]
        public void AddNegativeOneIsCorrect(double x, double y)
        {
            Assert.Equal(y, x + Float128<TAccelerator>.NegativeOne);
        }

        [Theory]
        [InlineData([0.5, 1.5, 2.0])]
        [InlineData([1.0, 2.0, 3.0])]
        [InlineData([-1.0, 3.5, 2.5])]
        public void AddNormalIsCorrect(double x, double y, double z)
        {
            Assert.Equal(z, (Float128<TAccelerator>)x + y);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void AddPositiveInfinityIsPositiveInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, x + Float128<TAccelerator>.PositiveInfinity);
        }

        [Fact]
        public void AddSubnormalIsSubnormal()
        {
            Assert.True(Float128<TAccelerator>.IsSubnormal(Float128<TAccelerator>.Epsilon + Float128<TAccelerator>.Epsilon));
        }

        [Fact]
        public void AddSubnormalIsCorrect()
        {
            Float128<TAccelerator> sum = -Float128<TAccelerator>.Epsilon + Float128<TAccelerator>.Epsilon;

            Assert.Equal(Float128<TAccelerator>.Zero, sum);
            Assert.False(Float128<TAccelerator>.IsNegative(sum));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void AddSubnormalIsIdentity(double x)
        {
            Assert.Equal(x, x + Float128<TAccelerator>.Epsilon);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void AddZeroIsIdentity(double x)
        {
            Assert.Equal(x, x + Float128<TAccelerator>.Zero);
        }

        [Theory]
        [InlineData([0.5, 1.5])]
        [InlineData([1.0, 2.0])]
        [InlineData([-1.0, 0.0])]
        public void IncrementIsCorrect(double x, double y)
        {
            Float128<TAccelerator> x_inc = (Float128<TAccelerator>)x;
            Assert.Equal(y, ++x_inc);
        }

        [Fact]
        public void NegativeInfinityPlusNegativeInfinityIsNegativeInfinity()
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.NegativeInfinity + Float128<TAccelerator>.NegativeInfinity);
        }

        [Fact]
        public void PositiveInfinityPlusNegativeInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity + Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void PositiveInfinityPlusPositiveInfinityIsPositiveInfinity()
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.PositiveInfinity + Float128<TAccelerator>.PositiveInfinity);
        }

        [Fact]
        public void ZeroPlusZeroIsZero()
        {
            Float128<TAccelerator> sum = Float128<TAccelerator>.Zero + Float128<TAccelerator>.Zero;

            Assert.Equal(Float128<TAccelerator>.Zero, sum);
            Assert.False(Float128<TAccelerator>.IsNegative(sum));
        }

        [Fact]
        public void NegativeZeroPlusZeroIsPositiveZero()
        {
            Float128<TAccelerator> sum = Float128<TAccelerator>.NegativeZero + Float128<TAccelerator>.Zero;

            Assert.Equal(Float128<TAccelerator>.Zero, sum);
            Assert.False(Float128<TAccelerator>.IsNegative(sum));
        }

        [Fact]
        public void OverflowingSumIsInfinity()
        {
            Float128<TAccelerator> max = RawStorage<TAccelerator>.LargestFinite();
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, max + max);
        }

        // reference: 2^-16382 - 2^-16383 = 2^-16383 is subnormal
        [Fact]
        public void UnderflowingSumIsSubnormal()
        {
            Float128<TAccelerator> min = Float128<TAccelerator>.ScaleB(1.0, -(short.MaxValue >> 1) + 1);
            Float128<TAccelerator> half = Float128<TAccelerator>.ScaleB(-0.5, -(short.MaxValue >> 1) + 1);
            Assert.True(Float128<TAccelerator>.IsSubnormal(min + half));
        }

        [Fact]
        public void PositiveInfinityPlusNaNIsNaN()
        {
            // IEEE 754: NaN operand in either position
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity + Float128<TAccelerator>.NaN));
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.NaN + Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void SubnormalSumIsExact()
        {
            // 2^-16382 - 2^-16383 = 2^-16383, the subnormal with fraction 2^111
            Float128<TAccelerator> smallestNormal = RawStorage<TAccelerator>.FromBits(UInt128.One << 112);
            Float128<TAccelerator> negativeHalf = RawStorage<TAccelerator>.FromBits((UInt128.One << 127) | (UInt128.One << 111));

            Float128<TAccelerator> sum = smallestNormal + negativeHalf;

            Assert.Equal(UInt128.One << 111, RawStorage<TAccelerator>.Bits(sum));
        }

        [Fact]
        public void OverflowingSumIsPositiveInfinity()
        {
            Float128<TAccelerator> largest = RawStorage<TAccelerator>.LargestFinite();

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(largest + largest));
        }

        [Fact]
        public void NegativeOverflowingSumIsNegativeInfinity()
        {
            Float128<TAccelerator> largest = RawStorage<TAccelerator>.LargestFinite();

            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(-largest + -largest));
        }

        [Fact]
        public void TieSumIsRoundedToEven()
        {
            // reference: 1 + 2^-112 + 2^-113 is halfway between 1 + 2^-112 and 1 + 2 * 2^-112; the even one is chosen
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | UInt128.One);
            Float128<TAccelerator> halfUlp = RawStorage<TAccelerator>.FromBits((UInt128)16270 << 112);

            Float128<TAccelerator> sum = one + halfUlp;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000002"), RawStorage<TAccelerator>.Bits(sum));
        }

        [Fact]
        public void JustAboveTieSumIsRoundedUp()
        {
            // reference: 1 + 1.5 * 2^-113 = 1 + 3 * 2^-114 lies above the tie, so the result is 1 + 2^-112
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits((UInt128)16383 << 112);
            Float128<TAccelerator> addend = RawStorage<TAccelerator>.FromBits(((UInt128)16270 << 112) | (UInt128.One << 111));

            Float128<TAccelerator> sum = one + addend;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000001"), RawStorage<TAccelerator>.Bits(sum));
        }

        [Fact]
        public void SumJustAboveHalfUlpIsRoundedUp()
        {
            // reference: 1 + 1.75 * 2^-113 = 1 + 7 * 2^-115 is above the half ulp, so the result is 1 + 2^-112
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits((UInt128)16383 << 112);
            Float128<TAccelerator> addend = RawStorage<TAccelerator>.FromBits(((UInt128)16270 << 112) | ((UInt128)3 << 110));

            Float128<TAccelerator> sum = one + addend;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000001"), RawStorage<TAccelerator>.Bits(sum));
        }

        [Fact]
        public void SumAboveTieWithTinyTailIsRoundedUp()
        {
            // reference: 1 + (2^-113 + 2^-200) is above the half ulp, so the result is 1 + 2^-112
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits((UInt128)16383 << 112);
            Float128<TAccelerator> addend = RawStorage<TAccelerator>.FromBits(((UInt128)16270 << 112) | (UInt128.One << 25));

            Float128<TAccelerator> sum = one + addend;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000001"), RawStorage<TAccelerator>.Bits(sum));
        }

        [Fact]
        public void OnePlusTinyTailIsOne()
        {
            // tail below half ulp
            Float128<TAccelerator> tiny = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -200);

            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.One + tiny);
        }

        [Fact]
        public void OneMinusTinyTailIsOne()
        {
            // tail below half ulp
            Float128<TAccelerator> tiny = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -200);

            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.One - tiny);
        }

        [Fact]
        public void OneMinusHalfUlpBelowOneIsOne()
        {
            // reference: 1 - 2^-114 is a tie, rounded to even (1)
            Float128<TAccelerator> halfUlp = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -114);

            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.One - halfUlp);
        }

        [Fact]
        public void TwoPlusNegatedTwoMinusUlpIsExact()
        {
            // reference: 2 - 2^-112 is representable, so 2 + -(2 - 2^-112) = 2^-112 exactly
            Float128<TAccelerator> two = RawStorage<TAccelerator>.FromBits((UInt128)16384 << 112);
            Float128<TAccelerator> twoMinusUlp = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | ((UInt128.One << 112) - 1));

            Float128<TAccelerator> difference = two + -twoMinusUlp;

            Assert.Equal(RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.ParseBits("3F8F0000000000000000000000000000")), difference);
        }

        [Fact]
        public void OnePlusNegatedPredecessorOfOneIsExact()
        {
            // reference: 1 - 2^-113 is representable, so 1 + -(1 - 2^-113) = 2^-113 exactly
            Float128<TAccelerator> one = Float128<TAccelerator>.One;
            Float128<TAccelerator> negatedPredecessor = RawStorage<TAccelerator>.FromBits(((UInt128)0xBFFE << 112) | ((UInt128.One << 112) - UInt128.One));

            Float128<TAccelerator> difference = one + negatedPredecessor;

            Assert.Equal(RawStorage<TAccelerator>.FromBits((UInt128)0x3F8E << 112), difference);
        }

        [Fact]
        public void AddSeededSweepOfExponentGapsIsCorrectlyRounded()
        {
            // seeded sweep, gaps 0 to 120, BigInteger reference
            Random random = new Random(20260101);
            for (int gap = 0; gap <= 120; gap++)
            {
                for (int trial = 0; trial < 32; trial++)
                {
                    bool nearCancel = trial % 4 == 0;
                    int exponentX = random.Next(-1500, 1500);
                    int exponentY = nearCancel ? exponentX : (random.Next(2) == 0 ? exponentX - gap : exponentX + gap);
                    BigInteger significandX = RawStorage<TAccelerator>.RandomSignificand(random);
                    BigInteger significandY = nearCancel ? significandX ^ random.Next(1 << 20) : RawStorage<TAccelerator>.RandomSignificand(random);
                    bool negativeX = random.Next(2) == 0;
                    bool negativeY = nearCancel ? !negativeX : random.Next(2) == 0;

                    UInt128 expected = ReferenceSumBits(negativeX, exponentX, significandX, negativeY, exponentY, significandY);
                    Float128<TAccelerator> sum = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(negativeX, exponentX, significandX))
                        + RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(negativeY, exponentY, significandY));

                    Assert.Equal(expected, RawStorage<TAccelerator>.Bits(sum));
                }
            }
        }

        [Fact]
        public void AddSeededSweepOfSubnormalResultsIsExact()
        {
            // seeded sweep, exponents at the normal limit, cancelling to subnormal results (exact)
            Random random = new Random(20260102);
            for (int trial = 0; trial < 256; trial++)
            {
                int exponent = random.Next(-16382, -16380);
                BigInteger significandX = RawStorage<TAccelerator>.RandomSignificand(random);
                BigInteger significandY = significandX ^ random.Next(1 << 20);
                bool negativeX = random.Next(2) == 0;

                // exact difference in ulps
                BigInteger difference = significandX - significandY;
                bool negative = difference.Sign < 0 ? !negativeX : negativeX;
                UInt128 expected = UInt128.Zero;
                if (!difference.IsZero)
                {
                    // one subnormal unit is 2^-16494
                    expected = (UInt128)(BigInteger.Abs(difference) << (exponent + 16382)) | (negative ? UInt128.One << 127 : UInt128.Zero);
                }

                Float128<TAccelerator> sum = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(negativeX, exponent, significandX))
                    + RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(!negativeX, exponent, significandY));

                Assert.Equal(expected, RawStorage<TAccelerator>.Bits(sum));
            }
        }

        private static UInt128 ReferenceSumBits(bool negativeX, int exponentX, BigInteger significandX, bool negativeY, int exponentY, BigInteger significandY)
        {
            // exact integer sum at 2^lsb
            int lsb = (exponentX < exponentY ? exponentX : exponentY) - 112;
            BigInteger x = significandX << (exponentX - 112 - lsb);
            BigInteger y = significandY << (exponentY - 112 - lsb);
            BigInteger sum = (negativeX ? -x : x) + (negativeY ? -y : y);
            if (sum.IsZero)
            {
                return UInt128.Zero;
            }

            BigInteger magnitude = BigInteger.Abs(sum);
            int exponent = lsb;
            int bitLength = (int)magnitude.GetBitLength();
            if (bitLength > 113)
            {
                int shift = bitLength - 113;
                BigInteger quotient = magnitude >> shift;
                BigInteger remainder = magnitude - (quotient << shift);
                BigInteger half = BigInteger.One << (shift - 1);
                if (remainder > half || (remainder == half && !quotient.IsEven))
                {
                    quotient += BigInteger.One;
                }
                if (quotient == BigInteger.One << 113)
                {
                    quotient >>= 1;
                    shift++;
                }
                magnitude = quotient;
                exponent += shift;
            }
            else
            {
                int shift = 113 - bitLength;
                magnitude <<= shift;
                exponent -= shift;
            }

            return RawStorage<TAccelerator>.PackBits(sum.Sign < 0, exponent + 112, magnitude);
        }
    }

    public class AdditionTests_DefaultAccelerator :
        AdditionTests<DefaultAccelerator>
    { }

    public class AdditionTests_SoftwareAccelerator :
        AdditionTests<SoftwareAccelerator>
    { }
}