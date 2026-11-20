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
using Xunit;

namespace QuadrupleLib.Tests
{
    public abstract class FloatOperationsTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        #region Classification

        [Fact]
        public void IsRealNumberIsTrue()
        {
            Assert.True(Float128<TAccelerator>.IsRealNumber(Float128<TAccelerator>.One));
        }

        [Fact]
        public void IsImaginaryNumberIsFalse()
        {
            Assert.False(Float128<TAccelerator>.IsImaginaryNumber(Float128<TAccelerator>.One));
        }

        [Fact]
        public void IsComplexNumberIsFalse()
        {
            Assert.False(Float128<TAccelerator>.IsComplexNumber(Float128<TAccelerator>.One));
        }

        #endregion

        #region Sign-magnitude

        [Fact]
        public void UnaryPlusIsIdentity()
        {
            Assert.Equal(Float128<TAccelerator>.One, +Float128<TAccelerator>.One);
        }

        [Fact]
        public void SignOfZeroIsZero()
        {
            Assert.Equal(Float128<TAccelerator>.Zero, Float128<TAccelerator>.Sign(Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void SignOfPositiveIsOne()
        {
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Sign((Float128<TAccelerator>)1.0));
        }

        [Fact]
        public void SignOfNegativeIsNegativeOne()
        {
            Assert.Equal(Float128<TAccelerator>.NegativeOne, Float128<TAccelerator>.Sign((Float128<TAccelerator>)(-1.0)));
        }

        [Fact]
        public void SignOfNaNIsNaN()
        {
            // NaN is neither zero, positive, nor negative, so Sign returns the value itself
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Sign(Float128<TAccelerator>.NaN)));
        }

        [Theory]
        [InlineData(1.0, 2.0, 2.0)]
        [InlineData(2.0, 1.0, 2.0)]
        public void MaxMagnitudeNumberReturnsLargerMagnitude(double x, double y, double expected)
        {
            Assert.Equal(expected, (double)Float128<TAccelerator>.MaxMagnitudeNumber((Float128<TAccelerator>)x, (Float128<TAccelerator>)y));
        }

        [Fact]
        public void MaxMagnitudeNumberWithNaNFirstReturnsSecond()
        {
            Assert.Equal(1.0, (double)Float128<TAccelerator>.MaxMagnitudeNumber(Float128<TAccelerator>.NaN, (Float128<TAccelerator>)1.0));
        }

        [Fact]
        public void MaxMagnitudeNumberWithNaNSecondReturnsFirst()
        {
            Assert.Equal(1.0, (double)Float128<TAccelerator>.MaxMagnitudeNumber((Float128<TAccelerator>)1.0, Float128<TAccelerator>.NaN));
        }

        [Theory]
        [InlineData(1.0, 2.0, 1.0)]
        [InlineData(2.0, 1.0, 1.0)]
        public void MinMagnitudeNumberReturnsSmallerMagnitude(double x, double y, double expected)
        {
            Assert.Equal(expected, (double)Float128<TAccelerator>.MinMagnitudeNumber((Float128<TAccelerator>)x, (Float128<TAccelerator>)y));
        }

        [Fact]
        public void MinMagnitudeNumberWithNaNFirstReturnsSecond()
        {
            Assert.Equal(1.0, (double)Float128<TAccelerator>.MinMagnitudeNumber(Float128<TAccelerator>.NaN, (Float128<TAccelerator>)1.0));
        }

        [Fact]
        public void MinMagnitudeNumberWithNaNSecondReturnsFirst()
        {
            Assert.Equal(1.0, (double)Float128<TAccelerator>.MinMagnitudeNumber((Float128<TAccelerator>)1.0, Float128<TAccelerator>.NaN));
        }

        #endregion
    }

    public class FloatOperationsTests_DefaultAccelerator :
        FloatOperationsTests<DefaultAccelerator>
    { }

    public class FloatOperationsTests_SoftwareAccelerator :
        FloatOperationsTests<SoftwareAccelerator>
    { }
}
