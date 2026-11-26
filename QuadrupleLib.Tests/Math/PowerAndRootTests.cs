/*
 *  Copyright 2026 Chosen Few Software
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
using QuadrupleLib.Tests.Assertions.Types;
using Xunit;

namespace QuadrupleLib.Tests.Math
{
    public abstract class PowerAndRootTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        public void IsSqrtCorrect(double x)
        {
            double y = double.Sqrt(x);
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Sqrt(x), Precision.NearestTenThousandth);
        }

        [Fact]
        public void SqrtOfPositiveInfinityIsPositiveInfinity()
        {
            // IEEE 754: sqrt(+Inf) is +Inf
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Sqrt(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void SqrtOfNegativeInfinityIsNaN()
        {
            // IEEE 754 section 5.4.1: invalid operation, quiet NaN
            Float128<TAccelerator> result = Float128<TAccelerator>.Sqrt(Float128<TAccelerator>.NegativeInfinity);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void SqrtOfNegativeZeroIsNegativeZero()
        {
            // IEEE 754: sqrt(-0) is -0
            Float128<TAccelerator> result = Float128<TAccelerator>.Sqrt(-Float128<TAccelerator>.Zero);
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        [InlineData(-1.0)]
        [InlineData(-2.1)]
        [InlineData(-3.676)]
        public void IsCbrtCorrect(double x)
        {
            double y = double.Cbrt(x);
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Cbrt(x), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(0.0, 2)]
        [InlineData(1.0, 2)]
        [InlineData(2.1, 2)]
        [InlineData(3.676, 2)]
        [InlineData(0.0, 3)]
        [InlineData(1.0, 3)]
        [InlineData(2.1, 3)]
        [InlineData(3.676, 3)]
        [InlineData(-1.0, 3)]
        [InlineData(-2.1, 3)]
        [InlineData(-3.676, 3)]
        [InlineData(0.0, 4)]
        [InlineData(1.0, 4)]
        [InlineData(2.1, 4)]
        [InlineData(3.676, 4)]
        public void IsRootNCorrect(double x, int n)
        {
            double y = double.RootN(x, n);
            AssertX.NearlyEqual(y, Float128<TAccelerator>.RootN(x, n), Precision.NearestTenThousandth);
        }

        [Theory]
        // reference: |x|^(1/n) to 60 digits (Python mpmath, exact binary value of the double |x|)
        [InlineData(-2.1, 3, "1.28057916498749427330010543933982273672153920177681554427077")]
        [InlineData(-3.676, 5, "1.29740360561177652070565531944591174169696791977446026023603")]
        public void RootNOfNegativeOddDegreeIsNegatedRootOfMagnitude(double x, int n, string magnitudeRoot)
        {
            // odd root of a negative value is the negated root of its magnitude, checked against fixed references
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse(magnitudeRoot);
            AssertX.WithinUlps(expected, Float128<TAccelerator>.RootN(-x, n), 2);
            AssertX.WithinUlps(-expected, Float128<TAccelerator>.RootN(x, n), 2);
        }

        [Fact]
        public void CbrtOfTwoIsWithinUlpsOfReference()
        {
            // reference: 2^(1/3) to 38 digits
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("1.2599210498948731647672106072782283505");
            AssertX.WithinUlps(expected, Float128<TAccelerator>.Cbrt(2.0), 2);
        }

        [Fact]
        public void RootNOfTwoFourthIsWithinUlpsOfReference()
        {
            // reference: 2^(1/4) to 38 digits
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("1.1892071150027210667174999705604759152");
            AssertX.WithinUlps(expected, Float128<TAccelerator>.RootN(2.0, 4), 2);
        }

        [Theory]
        [InlineData(1.0, 2.0)]
        [InlineData(2.1, 2.0)]
        [InlineData(3.676, 2.0)]
        [InlineData(1.0, 3.111)]
        [InlineData(2.1, 3.111)]
        [InlineData(3.676, 3.111)]
        [InlineData(1.0, 0.4)]
        [InlineData(2.1, 0.4)]
        [InlineData(3.676, 0.4)]
        [InlineData(1.0, -1.0)]
        [InlineData(2.1, -1.0)]
        [InlineData(3.676, -1.0)]
        [InlineData(2.0, -1.0)]
        public void IsPowCorrect(double x, double y)
        {
            double z = double.Pow(x, y);
            AssertX.NearlyEqual(z, Float128<TAccelerator>.Pow(x, y), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        [InlineData(2.0)]
        [InlineData(-2.1)]
        [InlineData(-0.1)]
        public void IsPowZeroEqualToOne(double x)
        {
            AssertX.NearlyEqual(Float128<TAccelerator>.One, Float128<TAccelerator>.Pow(x, 0), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-2.1)]
        [InlineData(-0.1)]
        public void PowZeroExponentIsOne(double x)
        {
            // IEEE 754 and C99: pow(x, +/-0) is 1 for any x, including zero and NaN
            Float128<TAccelerator> y = Float128<TAccelerator>.Pow(x, 0);
            Assert.Equal(Float128<TAccelerator>.One, y);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        public void IsPowOneIdentity(double x)
        {
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Pow(x, Float128<TAccelerator>.One), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        public void IsPowNegativeOneReciprocal(double x)
        {
            AssertX.NearlyEqual(Float128<TAccelerator>.One / x, Float128<TAccelerator>.Pow(x, Float128<TAccelerator>.NegativeOne), Precision.NearestTenThousandth);
        }

        [Fact]
        public void PowOfPositiveZeroToNegativeFiveIsPositiveInfinity()
        {
            // IEEE 754: pow(+0, y) is +Inf for an odd negative integer y
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow(Float128<TAccelerator>.Zero, (Float128<TAccelerator>)(-5.0));

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(result));
        }

        [Fact]
        public void PowOfNegativeZeroToNegativeOddIntegerIsNegativeInfinity()
        {
            // IEEE 754 and C99: pow(-0, y) is -Inf when y is an odd negative integer
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow(-Float128<TAccelerator>.Zero, (Float128<TAccelerator>)(-5.0));

            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(result));
        }

        [Fact]
        public void RootNOfPositiveZeroWithNegativeDegreeIsPositiveInfinity()
        {
            // IEEE 754 and C99: the root is the reciprocal of the power, so (+0)^(-1) is +Inf
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.RootN(Float128<TAccelerator>.Zero, -1)));
        }

        [Fact]
        public void RootNOfNegativeZeroWithNegativeOddDegreeIsNegativeInfinity()
        {
            // IEEE 754 and C99: (-0)^(-1) is -Inf, since the degree is odd
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.RootN(-Float128<TAccelerator>.Zero, -1)));
        }

        [Fact]
        public void RootNOfZeroWithNegativeEvenDegreeIsPositiveInfinity()
        {
            // IEEE 754 and C99: the sign of a zero base matters only for odd degrees, so (+-0)^(-2) is +Inf
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.RootN(Float128<TAccelerator>.Zero, -2)));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.RootN(-Float128<TAccelerator>.Zero, -2)));
        }

        [Theory]
        [InlineData(-8.0, -2.0)]
        [InlineData(-27.0, -3.0)]
        public void RootNOfNegativeCubeIsExact(double x, double expected)
        {
            // odd root of a negative value
            Assert.Equal((Float128<TAccelerator>)expected, Float128<TAccelerator>.RootN(x, 3));
        }

        [Fact]
        public void RootNOfNegativeFifthPowerIsExact()
        {
            Assert.Equal((Float128<TAccelerator>)(-2.0), Float128<TAccelerator>.RootN(-32.0, 5));
        }

        [Theory]
        [InlineData(-2.1, 4)]
        [InlineData(-2.0, 2)]
        public void RootNOfNegativeEvenDegreeIsPositiveNaN(double x, int n)
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.RootN(x, n);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void CbrtOfInfinityIsSameInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Cbrt(Float128<TAccelerator>.PositiveInfinity)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Cbrt(Float128<TAccelerator>.NegativeInfinity)));
        }

        [Fact]
        public void CbrtOfNegativeZeroIsNegativeZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Cbrt(Float128<TAccelerator>.NegativeZero);
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void RootNOfNegativeInfinityWithOddDegreeIsNegativeInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.RootN(Float128<TAccelerator>.NegativeInfinity, 3)));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void RootNOfPositiveInfinityWithPositiveDegreeIsPositiveInfinity(int n)
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.RootN(Float128<TAccelerator>.PositiveInfinity, n)));
        }

        [Fact]
        public void RootNOfPositiveInfinityWithNegativeDegreeIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.RootN(Float128<TAccelerator>.PositiveInfinity, -3);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void PowOfOneToNaNIsOne()
        {
            // IEEE 754 and C99: pow(1, y) is 1 for any y, including NaN
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Pow(Float128<TAccelerator>.One, Float128<TAccelerator>.NaN));
        }

        [Fact]
        public void PowOfNaNToZeroIsOne()
        {
            // IEEE 754 and C99: pow(x, 0) is 1 for any x, including NaN
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Pow(Float128<TAccelerator>.NaN, Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void RootNOfNegativeZeroWithPositiveOddDegreeIsNegativeZero()
        {
            // IEEE 754 rootn(-0, n) is -0 for odd n > 0
            Float128<TAccelerator> result = Float128<TAccelerator>.RootN(Float128<TAccelerator>.NegativeZero, 3);
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void RootNOfNegativeZeroWithPositiveEvenDegreeIsPositiveZero()
        {
            // IEEE 754 rootn(-0, n) is +0 for even n > 0
            Float128<TAccelerator> result = Float128<TAccelerator>.RootN(Float128<TAccelerator>.NegativeZero, 2);
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void PowOfNegativeBaseWithNonIntegerExponentIsPositiveNaN()
        {
            // IEEE 754: pow(x, y) for x < 0 and non-integer y is an invalid operation
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow((Float128<TAccelerator>)(-2.0), (Float128<TAccelerator>)0.5);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void PowOfNegativeInfinityToNonIntegerPowerIsPositiveInfinity()
        {
            // IEEE 754-2019: (-Inf)^y is +Inf for y > 0 when y is not an odd integer
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Pow(Float128<TAccelerator>.NegativeInfinity, (Float128<TAccelerator>)2.5)));
        }

        [Fact]
        public void PowOfNegativeInfinityToNegativeNonIntegerPowerIsPositiveZero()
        {
            // IEEE 754-2019: (-Inf)^y is +0 for y < 0 when y is not an odd integer
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow(Float128<TAccelerator>.NegativeInfinity, (Float128<TAccelerator>)(-2.5));

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void PowOfNegativeInfinityToOddIntegerPowerIsNegativeInfinity()
        {
            // IEEE 754-2019: (-Inf)^y is -Inf for y > 0 when y is an odd integer
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Pow(Float128<TAccelerator>.NegativeInfinity, (Float128<TAccelerator>)3.0)));
        }

        [Fact]
        public void PowOfNegativeHalfToPositiveInfinityIsPositiveZero()
        {
            // IEEE 754-2019: x^+Inf is +0 for |x| < 1, regardless of the sign of x
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow((Float128<TAccelerator>)(-0.5), Float128<TAccelerator>.PositiveInfinity);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void PowOfNegativeOneToPositiveInfinityIsOne()
        {
            // IEEE 754-2019: (-1)^+Inf is 1
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Pow((Float128<TAccelerator>)(-1.0), Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void PowOfFourToPositiveInfinityIsPositiveInfinity()
        {
            // IEEE 754: x^+Inf is +Inf for |x| > 1
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Pow((Float128<TAccelerator>)4.0, Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void PowOfNegativeThreeToPositiveInfinityIsPositiveInfinity()
        {
            // IEEE 754-2019: x^+Inf is +Inf for |x| > 1, including negative x
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Pow((Float128<TAccelerator>)(-3.0), Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void PowOfHalfToPositiveInfinityIsPositiveZero()
        {
            // IEEE 754: x^+Inf is +0 for 0 < x < 1
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow((Float128<TAccelerator>)0.5, Float128<TAccelerator>.PositiveInfinity);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void PowOfSignedZeroToSignedZeroIsOne()
        {
            // IEEE 754 and C99: pow(±0, ±0) is 1
            Float128<TAccelerator> positive = Float128<TAccelerator>.Pow(Float128<TAccelerator>.Zero, -Float128<TAccelerator>.Zero);
            Float128<TAccelerator> negative = Float128<TAccelerator>.Pow(-Float128<TAccelerator>.Zero, Float128<TAccelerator>.Zero);

            Assert.Equal(Float128<TAccelerator>.One, positive);
            Assert.False(Float128<TAccelerator>.IsNegative(positive));
            Assert.Equal(Float128<TAccelerator>.One, negative);
            Assert.False(Float128<TAccelerator>.IsNegative(negative));
        }

        [Fact]
        public void PowOfSignedZeroToPositiveInfinityIsPositiveZero()
        {
            // IEEE 754 and C99: pow(±0, +Inf) is +0
            Float128<TAccelerator> positive = Float128<TAccelerator>.Pow(Float128<TAccelerator>.Zero, Float128<TAccelerator>.PositiveInfinity);
            Float128<TAccelerator> negative = Float128<TAccelerator>.Pow(-Float128<TAccelerator>.Zero, Float128<TAccelerator>.PositiveInfinity);

            Assert.True(Float128<TAccelerator>.IsZero(positive));
            Assert.False(Float128<TAccelerator>.IsNegative(positive));
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.False(Float128<TAccelerator>.IsNegative(negative));
        }

        [Fact]
        public void PowOfSignedZeroToPositiveNonIntegerIsPositiveZero()
        {
            // IEEE 754 and C99: pow(±0, y) is +0 for y > 0 that is not an odd integer
            Float128<TAccelerator> positive = Float128<TAccelerator>.Pow(Float128<TAccelerator>.Zero, (Float128<TAccelerator>)0.5);
            Float128<TAccelerator> negative = Float128<TAccelerator>.Pow(-Float128<TAccelerator>.Zero, (Float128<TAccelerator>)0.5);

            Assert.True(Float128<TAccelerator>.IsZero(positive));
            Assert.False(Float128<TAccelerator>.IsNegative(positive));
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.False(Float128<TAccelerator>.IsNegative(negative));
        }

        [Fact]
        public void PowOfSubnormalToPositiveInfinityIsPositiveZero()
        {
            // IEEE 754 and C99: pow(x, +Inf) is +0 for |x| < 1, whatever the sign of x
            Float128<TAccelerator> positive = Float128<TAccelerator>.Pow(Float128<TAccelerator>.Epsilon, Float128<TAccelerator>.PositiveInfinity);
            Float128<TAccelerator> negative = Float128<TAccelerator>.Pow(-Float128<TAccelerator>.Epsilon, Float128<TAccelerator>.PositiveInfinity);

            Assert.True(Float128<TAccelerator>.IsZero(positive));
            Assert.False(Float128<TAccelerator>.IsNegative(positive));
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.False(Float128<TAccelerator>.IsNegative(negative));
        }

        [Fact]
        public void PowOfSubnormalToNegativeInfinityIsPositiveInfinity()
        {
            // IEEE 754 and C99: pow(x, -Inf) is +Inf for |x| < 1, whatever the sign of x
            Float128<TAccelerator> positive = Float128<TAccelerator>.Pow(Float128<TAccelerator>.Epsilon, Float128<TAccelerator>.NegativeInfinity);
            Float128<TAccelerator> negative = Float128<TAccelerator>.Pow(-Float128<TAccelerator>.Epsilon, Float128<TAccelerator>.NegativeInfinity);

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(positive));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(negative));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(-3)]
        public void RootNOfNaNIsNaN(int n)
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.RootN(Float128<TAccelerator>.NaN, n);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void RootNOfFourWithZeroDegreeIsNaN()
        {
            // the zeroth root is a domain error
            Float128<TAccelerator> result = Float128<TAccelerator>.RootN((Float128<TAccelerator>)4.0, 0);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void PowOfNegativeTwoToNegativeThreeIsNegativeEighth()
        {
            // exact result: (-2)^-3 = -0.125
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow((Float128<TAccelerator>)(-2.0), (Float128<TAccelerator>)(-3.0));

            Assert.Equal(-0.125, (double)result);
        }

        [Fact]
        public void SqrtOfLargeValueIsCorrectlyRounded()
        {
            // input: the exact integer 10^100 rounded to binary128, as literal bits (computed with Python, ties to even)
            // reference: exact integer square root of that input, rounded half to even
            UInt128 bits = RawStorage<TAccelerator>.ParseBits("414B249AD2594C37CEB0B2784C4CE0BF");
            BigInteger significand = (BigInteger)((bits & ((UInt128.One << 112) - 1)) | (UInt128.One << 112));
            int exponent = (int)((bits >> 112) & (UInt128)0x7FFF) - 16383;
            Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(bits);
            Assert.Equal(CorrectlyRoundedSqrtBits(significand, exponent), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Sqrt(x)));
        }

        [Fact]
        public void SqrtOfRandomNormalIsCorrectlyRounded()
        {
            // reference: exact integer square root of the input, rounded half to even
            Random random = new(20260111);
            for (int i = 0; i < 1000; i++)
            {
                int exponent = random.Next(-8000, 8000);
                BigInteger significand = RawStorage<TAccelerator>.RandomSignificand(random);
                Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.PackBits(false, exponent, significand));
                Assert.Equal(CorrectlyRoundedSqrtBits(significand, exponent), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Sqrt(x)));
            }
        }

        [Fact]
        public void SqrtOfEpsilonIsExactPowerOfTwo()
        {
            // sqrt(2^-16494) = 2^-8247 exactly
            Assert.Equal(RawStorage<TAccelerator>.PackBits(false, -8247, BigInteger.One << 112), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Sqrt(Float128<TAccelerator>.Epsilon)));
        }

        [Fact]
        public void SqrtOfOddPowerSubnormalIsCorrectlyRounded()
        {
            // reference: exact integer square root of 2^-16493 (subnormal significand 2), rounded half to even
            Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits((UInt128)2);
            Assert.Equal(CorrectlyRoundedSqrtBits(BigInteger.One << 112, -16493), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Sqrt(x)));
        }

        // bits of sqrt(significand * 2^(exponent - 112)), rounded half to even
        private static UInt128 CorrectlyRoundedSqrtBits(BigInteger significand, int exponent)
        {
            // x = significand * 2^power with power even
            int power = exponent - 112;
            if ((power & 1) != 0)
            {
                significand <<= 1;
                power--;
            }

            // scale by 2^230 before the integer square root
            BigInteger scaled = significand << 230;
            BigInteger root = IntegerSquareRoot(scaled);
            bool inexact = scaled != root * root;
            int bitLength = (int)root.GetBitLength();
            int drop = bitLength - 113;
            BigInteger quotient = root >> drop;
            bool roundBit = ((root >> (drop - 1)) & BigInteger.One) == BigInteger.One;
            bool sticky = inexact || (root & ((BigInteger.One << (drop - 1)) - 1)) != BigInteger.Zero;
            if (roundBit && (sticky || !quotient.IsEven))
            {
                quotient += BigInteger.One;
            }

            int resultExponent = (power - 230) / 2 + bitLength - 1;
            if (quotient == BigInteger.One << 113)
            {
                quotient >>= 1;
                resultExponent++;
            }

            return RawStorage<TAccelerator>.PackBits(false, resultExponent, quotient);
        }

        private static BigInteger IntegerSquareRoot(BigInteger n)
        {
            BigInteger x = BigInteger.One << (int)((n.GetBitLength() + 1) / 2);
            while (true)
            {
                BigInteger y = (x + n / x) >> 1;
                if (y >= x)
                {
                    return x;
                }
                x = y;
            }
        }

        [Theory]
        [InlineData(3.0, 5.0, 243.0)]
        [InlineData(-3.0, 5.0, -243.0)]
        [InlineData(10.0, 20.0, 1e20)]
        [InlineData(2.0, 100.0, 1.2676506002282294e30)]
        public void IntegerPowerWithExactResultIsExact(double x, double y, double expected)
        {
            // each result is exactly representable, so an integer power must return it exactly
            Assert.Equal((Float128<TAccelerator>)expected, Float128<TAccelerator>.Pow(x, y));
        }

        [Fact]
        public void LargeIntegerPowerIsExact()
        {
            // 3^40 = 12157665459056928801 has 64 significant bits, so it is exactly representable
            Assert.Equal((Float128<TAccelerator>)12157665459056928801UL, Float128<TAccelerator>.Pow(3.0, 40.0));
        }

        [Fact]
        public void IntegerPowerIsCorrectlyRounded()
        {
            // reference: the double 1.1 is 4953959590107546 / 2^52, so 1.1^10 = m^10 / 2^520 exactly; round to 113 bits, ties to even
            BigInteger power = BigInteger.Pow(4953959590107546, 10);
            int drop = (int)power.GetBitLength() - 113;
            BigInteger quotient = power >> drop;
            BigInteger dropped = power - (quotient << drop);
            BigInteger half = BigInteger.One << (drop - 1);
            if (dropped > half || (dropped == half && !quotient.IsEven))
            {
                quotient += BigInteger.One;
            }

            int exponent = (int)power.GetBitLength() - 1 - 520;
            if (quotient == BigInteger.One << 113)
            {
                quotient >>= 1;
                exponent++;
            }

            UInt128 expected = ((UInt128)(exponent + 16383) << 112) | (UInt128)(quotient - (BigInteger.One << 112));
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow(1.1, 10.0);

            Assert.Equal(expected, RawStorage<TAccelerator>.Bits(result));
        }

        [Theory]
        [InlineData(2.0, -16494.0)]
        [InlineData(0.5, 16494.0)]
        public void IntegerPowerToSmallestSubnormalIsExact(double x, double y)
        {
            // 2^-16494 is the smallest subnormal value
            Assert.Equal(Float128<TAccelerator>.Epsilon, Float128<TAccelerator>.Pow(x, y));
        }

        [Theory]
        [InlineData(32.0, 5, 2.0)]
        [InlineData(1024.0, 10, 2.0)]
        [InlineData(1e10, 10, 10.0)]
        [InlineData(32.0, -5, 0.5)]
        [InlineData(-32.0, 5, -2.0)]
        public void RootNWithExactResultIsExact(double x, int n, double expected)
        {
            Assert.Equal((Float128<TAccelerator>)expected, Float128<TAccelerator>.RootN(x, n));
        }

        [Fact]
        public void PowOfTwoToLargeIntegerIsPowerOfTwo()
        {
            // reference: 2^9000 has biased exponent 9000 + 16383 = 25383 (0x6327) and a zero fraction
            Float128<TAccelerator> expected = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.ParseBits("63270000000000000000000000000000"));
            Assert.Equal(expected, Float128<TAccelerator>.Pow(2.0, 9000.0));
        }

        [Fact]
        public void PowOfTwoAtOverflowAndUnderflowThresholdsIsIeee754Result()
        {
            // 2^16383 is the largest finite power of two (biased exponent 16383 + 16383 = 32766 = 0x7FFE); 2^16384 overflows
            Assert.Equal(RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.ParseBits("7FFE0000000000000000000000000000")), Float128<TAccelerator>.Pow(2.0, 16383.0));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Pow(2.0, 16384.0)));

            // 2^-16494 is the smallest subnormal (bits 1); 2^-16495 is half of it and rounds to even (zero)
            Assert.Equal(RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.ParseBits("00000000000000000000000000000001")), Float128<TAccelerator>.Pow(2.0, -16494.0));
            Float128<TAccelerator> underflow = Float128<TAccelerator>.Pow(2.0, -16495.0);
            Assert.Equal(Float128<TAccelerator>.Zero, underflow);
            Assert.False(Float128<TAccelerator>.IsNegative(underflow));
        }

        [Fact]
        public void FifthRootIsCorrectlyRounded()
        {
            // reference: 2^(1/5) * 2^112 = 5964382860055229082216465774895329.96..., rounded up to 5964382860055229082216465774895330.
            // The integer part is the floor of the 5th root of 2^561, computed by exact integer arithmetic.
            Float128<TAccelerator> result = Float128<TAccelerator>.RootN(2.0, 5);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF2611186BAE67496A521E420760E2"), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void RootNWithMinimumDegreeIsJustBelowOne()
        {
            // int.MinValue has no positive counterpart; 2^(-1/2^31) is about 1 - 3.2e-10
            Float128<TAccelerator> result = Float128<TAccelerator>.RootN(2.0, int.MinValue);

            Assert.True(result < Float128<TAccelerator>.One);
            Assert.True(result > (Float128<TAccelerator>)0.9999999996);
        }

        [Fact]
        public void SqrtOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Sqrt(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void CbrtOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Cbrt(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void PowWithNaNBaseIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow(Float128<TAccelerator>.NaN, 2.0);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void PowWithNaNExponentIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Pow(2.0, Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void PowWithLargeEvenIntegerExponentIsPositiveInfinity()
        {
            // 2^(2^65) overflows binary128; the exponent exceeds ulong.MaxValue, so the exponential path is used
            Float128<TAccelerator> y = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 65);

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Pow(2.0, y)));
        }

        [Fact]
        public void PowWithLargeOddIntegerExponentIsNegativeInfinity()
        {
            // (-2)^(2^65 + 1) is -Inf; the exponent exceeds ulong.MaxValue, so the exponential path is used
            Float128<TAccelerator> y = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 65) + 1.0;

            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Pow(-2.0, y)));
        }

        [Fact]
        public void RootNWithDegreeOneIsIdentity()
        {
            // IEEE 754 rootn(x, 1) is x
            Assert.Equal((Float128<TAccelerator>)2.1, Float128<TAccelerator>.RootN(2.1, 1));
        }

        [Fact]
        public void RootNWithNegativeDegreeOneIsReciprocal()
        {
            // IEEE 754 rootn(x, -1) is 1/x; 10/21 = 0.(476190) repeating, and 2.1 in binary128 is within half an ulp of 2.1
            AssertX.WithinUlps(Float128<TAccelerator>.Parse("0.4761904761904761904761904761904761904762"), Float128<TAccelerator>.RootN(Float128<TAccelerator>.Parse("2.1"), -1), 2);
        }

        [Fact]
        public void RootNWithNegativeDegreeTwoIsReciprocalSquareRoot()
        {
            // IEEE 754 rootn(4, -2) is 1/sqrt(4) = 0.5
            Assert.Equal(Float128<TAccelerator>.Parse("0.5"), Float128<TAccelerator>.RootN(4.0, -2));
        }

        [Fact]
        public void RootNWithNegativeDegreeThreeIsReciprocalCubeRoot()
        {
            // IEEE 754 rootn(8, -3) is 1/cbrt(8) = 0.5
            Assert.Equal(Float128<TAccelerator>.Parse("0.5"), Float128<TAccelerator>.RootN(8.0, -3));
        }

        [Fact]
        public void RootNWithNegativeDegreeOneOfThreeIsCorrectlyRounded()
        {
            // reference: 1/3 to 40 digits
            Assert.Equal(Float128<TAccelerator>.Parse("0.3333333333333333333333333333333333333333"), Float128<TAccelerator>.RootN(3.0, -1));
        }

        [Theory]
        [InlineData(8.0, "2")]
        [InlineData(-27.0, "-3")]
        [InlineData(0.125, "0.5")]
        [InlineData(4096.0, "16")]
        public void CbrtOfExactCubeIsCubeRoot(double x, string expected)
        {
            Assert.Equal(Float128<TAccelerator>.Parse(expected), Float128<TAccelerator>.Cbrt(x));
        }

        [Fact]
        public void CbrtOfSmallestSubnormalIsPowerOfTwo()
        {
            // 2^-16494 / 3 = 2^-5498 exactly, since 16494 = 3 * 5498
            Assert.Equal(RawStorage<TAccelerator>.PackBits(false, -5498, BigInteger.One << 112), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Cbrt(Float128<TAccelerator>.Epsilon)));
        }

        [Fact]
        public void PowOfTwoToHalfIsSqrtOfTwo()
        {
            // reference: sqrt(2) = 1.41421356237309504880168872420969807856967187537694...
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("1.41421356237309504880168872420969807856967"), Float128<TAccelerator>.Pow(2.0, 0.5), Float128<TAccelerator>.Parse("1E-33"));
        }
    }

    public class PowerAndRootTests_DefaultAccelerator :
        PowerAndRootTests<DefaultAccelerator>
    { }

    public class PowerAndRootTests_SoftwareAccelerator :
        PowerAndRootTests<SoftwareAccelerator>
    { }
}
