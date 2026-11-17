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
    }

    public class RoundingTests_DefaultAccelerator :
        RoundingTests<DefaultAccelerator>
    { }

    public class RoundingTests_SoftwareAccelerator :
        RoundingTests<SoftwareAccelerator>
    { }
}
