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
using Xunit;

namespace QuadrupleLib.Tests.Arithmetic
{
    public abstract class SubtractionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData([0.5, -0.5])]
        [InlineData([1.0, 0.0])]
        [InlineData([-1.0, -2.0])]
        public void DecrementIsCorrect(double x, double y)
        {
            Float128<TAccelerator> x_dec = (Float128<TAccelerator>)x;
            Assert.Equal(y, --x_dec);
        }

        [Fact]
        public void NegativeInfinityMinusNegativeInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.NegativeInfinity - Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void NegativeInfinityMinusPositiveInfinityIsNegativeInfinity()
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.NegativeInfinity - Float128<TAccelerator>.PositiveInfinity);
        }

        [Fact]
        public void PositiveInfinityMinusNegativeInfinityIsPositiveInfinity()
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.PositiveInfinity - Float128<TAccelerator>.NegativeInfinity);
        }

        [Fact]
        public void PositiveInfinityMinusPositiveInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity - Float128<TAccelerator>.PositiveInfinity));
        }

        [Theory]
        [InlineData([0.5, 1.5, -1.0])]
        [InlineData([1.0, -2.0, 3.0])]
        [InlineData([-1.0, 3.5, -4.5])]
        public void SubtractGeneralIsCorrect(double x, double y, double z)
        {
            Assert.Equal(z, (Float128<TAccelerator>)x - y);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        [InlineData(double.NaN)]
        public void SubtractNaNIsNaN(double x)
        {
            Assert.True(Float128<TAccelerator>.IsNaN(x - Float128<TAccelerator>.NaN));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void SubtractNegativeInfinityIsPositiveInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, x - Float128<TAccelerator>.NegativeInfinity);
        }

        [Theory]
        [InlineData([0.5, -0.5])]
        [InlineData([1.0, 0.0])]
        [InlineData([-1.0, -2.0])]
        public void SubtractOneIsCorrect(double x, double y)
        {
            Assert.Equal(y, x - Float128<TAccelerator>.One);
        }

        [Theory]
        [InlineData([0.5, 1.5])]
        [InlineData([1.0, 2.0])]
        [InlineData([-1.0, 0.0])]
        public void SubtractNegativeOneIsCorrect(double x, double y)
        {
            Assert.Equal(y, x - Float128<TAccelerator>.NegativeOne);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void SubtractPositiveInfinityIsNegativeInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, x - Float128<TAccelerator>.PositiveInfinity);
        }

        [Fact]
        public void SubtractSubnormalIsSubnormal()
        {
            Assert.True(Float128<TAccelerator>.IsSubnormal(Float128<TAccelerator>.Epsilon - -Float128<TAccelerator>.Epsilon));
        }

        [Fact]
        public void SubtractSubnormalIsCorrect()
        {
            Assert.Equal(Float128<TAccelerator>.Zero, Float128<TAccelerator>.Epsilon - Float128<TAccelerator>.Epsilon);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void SubtractSubnormalIsIdentity(double x)
        {
            Assert.Equal(x, x - Float128<TAccelerator>.Epsilon);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void SubtractZeroIsIdentity(double x)
        {
            Assert.Equal(x, x - Float128<TAccelerator>.Zero);
        }
    }

    public class SubtractionTests_DefaultAccelerator :
        SubtractionTests<DefaultAccelerator>
    { }

    public class SubtractionTests_SoftwareAccelerator :
        SubtractionTests<SoftwareAccelerator>
    { }
}