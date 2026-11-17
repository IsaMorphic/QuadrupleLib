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
            // IEEE 754: the square root of a negative operand is an invalid operation; this library then returns Float128.NaN, whose sign bit is clear
            Float128<TAccelerator> result = Float128<TAccelerator>.Sqrt(Float128<TAccelerator>.NegativeInfinity);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
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
        [InlineData(1.0, 2)]
        [InlineData(2.1, 2)]
        [InlineData(3.676, 2)]
        [InlineData(1.0, 3)]
        [InlineData(2.1, 3)]
        [InlineData(3.676, 3)]
        [InlineData(-1.0, 3)]
        [InlineData(-2.1, 3)]
        [InlineData(-3.676, 3)]
        [InlineData(1.0, 4)]
        [InlineData(2.1, 4)]
        [InlineData(3.676, 4)]
        public void IsRootNEqualToPow(double x, int n)
        {
            Float128<TAccelerator> y0 = Float128<TAccelerator>.RootN(x, n);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Pow(
                Float128<TAccelerator>.Abs(x), Float128<TAccelerator>.One / n
                );
            if (x < 0.0) y1 = -y1;
            AssertX.NearlyEqual(y0, y1, Precision.NearestTenThousandth);
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

        [Theory]
        [InlineData(-8.0, -2.0)]
        [InlineData(-27.0, -3.0)]
        public void RootNOfNegativeCubeIsExact(double x, double expected)
        {
            // an odd root of a negative value uses the same exact path as its positive counterpart
            Assert.Equal((Float128<TAccelerator>)expected, Float128<TAccelerator>.RootN(x, 3));
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
        public void SqrtOfLargeValueIsNearestSquareRoot()
        {
            // reference: double.Sqrt(1e100) = 1e50; tolerance is a few double ulps
            Assert.Equal(double.Sqrt(1e100), (double)Float128<TAccelerator>.Sqrt(1e100), 1e35);
        }
    }

    public class PowerAndRootTests_DefaultAccelerator :
        PowerAndRootTests<DefaultAccelerator>
    { }

    public class PowerAndRootTests_SoftwareAccelerator :
        PowerAndRootTests<SoftwareAccelerator>
    { }
}
