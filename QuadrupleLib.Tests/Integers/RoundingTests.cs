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

namespace QuadrupleLib.Tests.Integers
{
    public abstract class RoundingTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(1.0)]
        [InlineData(1.1)]
        [InlineData(1.8)]
        [InlineData(2.5)]
        [InlineData(-1.0)]
        [InlineData(-1.1)]
        [InlineData(-1.8)]
        [InlineData(-2.5)]
        [InlineData(3.5)]
        [InlineData(0.5)]
        public void IsFloorCorrect(double x)
        {
            double y0 = double.Floor(x);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Floor(x);
            Assert.Equal(y0, y1);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(1.1)]
        [InlineData(1.8)]
        [InlineData(2.5)]
        [InlineData(-1.0)]
        [InlineData(-1.1)]
        [InlineData(-1.8)]
        [InlineData(-2.5)]
        [InlineData(3.5)]
        [InlineData(0.5)]
        public void IsCeilingCorrect(double x)
        {
            double y0 = double.Ceiling(x);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Ceiling(x);
            Assert.Equal(y0, y1);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(1.1)]
        [InlineData(1.8)]
        [InlineData(2.5)]
        [InlineData(-1.0)]
        [InlineData(-1.1)]
        [InlineData(-1.8)]
        [InlineData(-2.5)]
        [InlineData(3.5)]
        [InlineData(0.5)]
        public void IsRoundCorrect(double x)
        {
            double y0 = double.Round(x);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Round(x);
            Assert.Equal(y0, y1);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(1.1)]
        [InlineData(1.8)]
        [InlineData(2.5)]
        [InlineData(-1.0)]
        [InlineData(-1.1)]
        [InlineData(-1.8)]
        [InlineData(-2.5)]
        [InlineData(3.5)]
        [InlineData(0.5)]
        public void IsTruncateCorrect(double x)
        {
            double y0 = double.Truncate(x);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Truncate(x);
            Assert.Equal(y0, y1);
        }

        [Theory]
        [InlineData(1.12345, 0)]
        [InlineData(1.12345, 1)]
        [InlineData(1.12345, 2)]
        [InlineData(1.12345, 3)]
        [InlineData(-1.12345, 0)]
        [InlineData(-1.12345, 1)]
        [InlineData(-1.12345, 2)]
        [InlineData(-1.12345, 3)]
        public void IsRoundToCorrect(double x, int digits)
        {
            double y0 = double.Round(x, digits);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Round(x, digits);
            AssertX.NearlyEqual(y0, y1, (Precision)digits);
        }

        [Theory]
        [InlineData(2.6, 3.0)]
        [InlineData(2.8, 3.0)]
        [InlineData(4.6, 5.0)]
        [InlineData(-2.6, -3.0)]
        [InlineData(-2.8, -3.0)]
        public void RoundAboveHalfIsNearestInteger(double x, double expected)
        {
            // roundToIntegralTiesToEven: only an exact half is decided by the even rule
            Assert.Equal(expected, (double)Float128<TAccelerator>.Round((Float128<TAccelerator>)x));
        }

        [Fact]
        public void FloorOfNegativeZeroIsNegativeZero()
        {
            // IEEE 754: floor of a zero returns that zero with its sign
            Float128<TAccelerator> result = Float128<TAccelerator>.Floor(-Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void CeilingOfZeroIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Ceiling(Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void RoundBelowOneIsNearestInteger()
        {
            // roundToIntegralTiesToEven: |x| above one half rounds to +/-1, and a smaller |x| rounds to a zero with the sign of x
            Assert.Equal(1.0, (double)Float128<TAccelerator>.Round((Float128<TAccelerator>)0.7));
            Assert.Equal(-1.0, (double)Float128<TAccelerator>.Round((Float128<TAccelerator>)(-0.7)));

            Float128<TAccelerator> positiveZero = Float128<TAccelerator>.Round((Float128<TAccelerator>)0.3);
            Assert.True(Float128<TAccelerator>.IsZero(positiveZero));
            Assert.False(Float128<TAccelerator>.IsNegative(positiveZero));

            Float128<TAccelerator> negativeZero = Float128<TAccelerator>.Round((Float128<TAccelerator>)(-0.3));
            Assert.True(Float128<TAccelerator>.IsZero(negativeZero));
            Assert.True(Float128<TAccelerator>.IsNegative(negativeZero));
        }

        [Fact]
        public void CeilingOfNegativeFractionIsNegativeZero()
        {
            // IEEE 754 roundToIntegralTowardPositive: a negative x above -1 gives -0
            Float128<TAccelerator> result = Float128<TAccelerator>.Ceiling((Float128<TAccelerator>)(-0.5));

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void TruncateOfFractionIsZero()
        {
            // truncation toward zero: a fraction in (0, 1) gives a zero with the sign of the value
            Float128<TAccelerator> positive = Float128<TAccelerator>.Truncate((Float128<TAccelerator>)0.5);
            Assert.True(Float128<TAccelerator>.IsZero(positive));
            Assert.False(Float128<TAccelerator>.IsNegative(positive));

            Float128<TAccelerator> negative = Float128<TAccelerator>.Truncate((Float128<TAccelerator>)(-0.5));
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.True(Float128<TAccelerator>.IsNegative(negative));
        }

        [Fact]
        public void TruncateOfZeroIsUnchanged()
        {
            // a zero is returned as-is, keeping its sign
            Float128<TAccelerator> positive = Float128<TAccelerator>.Truncate(Float128<TAccelerator>.Zero);
            Assert.True(Float128<TAccelerator>.IsZero(positive));
            Assert.False(Float128<TAccelerator>.IsNegative(positive));

            Float128<TAccelerator> negative = Float128<TAccelerator>.Truncate(-Float128<TAccelerator>.Zero);
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.True(Float128<TAccelerator>.IsNegative(negative));
        }

        [Fact]
        public void TruncateOfSpecialValuesIsUnchanged()
        {
            // NaN, infinities and values at or above 2^112 are returned unchanged
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Truncate(Float128<TAccelerator>.NaN)));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Truncate(Float128<TAccelerator>.PositiveInfinity)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Truncate(Float128<TAccelerator>.NegativeInfinity)));

            double large = System.Math.Pow(2, 200);
            Assert.Equal(large, (double)Float128<TAccelerator>.Truncate((Float128<TAccelerator>)large));
        }

        [Fact]
        public void RoundOfSpecialValuesIsUnchanged()
        {
            // NaN, infinities and values at or above 2^112 are returned unchanged
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Round(Float128<TAccelerator>.NaN)));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Round(Float128<TAccelerator>.PositiveInfinity)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Round(Float128<TAccelerator>.NegativeInfinity)));

            double large = System.Math.Pow(2, 200);
            Assert.Equal(large, (double)Float128<TAccelerator>.Round((Float128<TAccelerator>)large));
        }

        [Fact]
        public void FloorOfSpecialValuesIsUnchanged()
        {
            // NaN, infinities and values at or above 2^112 are returned unchanged
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Floor(Float128<TAccelerator>.NaN)));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Floor(Float128<TAccelerator>.PositiveInfinity)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Floor(Float128<TAccelerator>.NegativeInfinity)));

            double large = System.Math.Pow(2, 200);
            Assert.Equal(large, (double)Float128<TAccelerator>.Floor((Float128<TAccelerator>)large));
        }

        [Fact]
        public void CeilingOfSpecialValuesIsUnchanged()
        {
            // NaN, infinities and values at or above 2^112 are returned unchanged
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Ceiling(Float128<TAccelerator>.NaN)));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Ceiling(Float128<TAccelerator>.PositiveInfinity)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Ceiling(Float128<TAccelerator>.NegativeInfinity)));

            double large = System.Math.Pow(2, 200);
            Assert.Equal(large, (double)Float128<TAccelerator>.Ceiling((Float128<TAccelerator>)large));
        }

        [Fact]
        public void RoundWithUnsupportedModeIsArgumentException()
        {
            // only round-to-even is implemented
            Assert.Throws<ArgumentException>(() => Float128<TAccelerator>.Round((Float128<TAccelerator>)1.0, 0, MidpointRounding.AwayFromZero));
        }

        [Fact]
        public void RoundWithNegativeDigitsIsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Float128<TAccelerator>.Round((Float128<TAccelerator>)1.0, -1));
        }

        [Fact]
        public void RoundWithManyDigitsIsUnchanged()
        {
            // 38 digits is the full precision of binary128, so the value is returned unchanged
            Assert.Equal(1.12345, (double)Float128<TAccelerator>.Round((Float128<TAccelerator>)1.12345, 38));
        }

        [Fact]
        public void RoundOfLargeValueIsUnchanged()
        {
            // a value at or above 2^112 is already an integer, so it is returned unchanged
            double large = System.Math.Pow(2, 200);
            Assert.Equal(large, (double)Float128<TAccelerator>.Round((Float128<TAccelerator>)large, 0));
        }
    }

    public class RoundingTests_DefaultAccelerator :
        RoundingTests<DefaultAccelerator>
    { }

    public class RoundingTests_SoftwareAccelerator :
        RoundingTests<SoftwareAccelerator>
    { }
}
