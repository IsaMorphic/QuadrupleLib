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
            Assert.Equal(Float128<TAccelerator>.Zero, -Float128<TAccelerator>.Epsilon + Float128<TAccelerator>.Epsilon);
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
    }

    public class AdditionTests_DefaultAccelerator :
        AdditionTests<DefaultAccelerator>
    { }

    public class AdditionTests_SoftwareAccelerator :
        AdditionTests<SoftwareAccelerator>
    { }
}