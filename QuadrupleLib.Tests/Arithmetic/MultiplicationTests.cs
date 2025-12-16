/*
 *  Copyright 2025 Chosen Few Software
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

namespace QuadrupleLib.Tests.Arithmetic;

public class MultiplicationTests
{
    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void MultiplyByNaNIsNaN(double x)
    {
        Assert.True(Float128.IsNaN(x * Float128.NaN));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void MultiplyByNegativeInfinityIsNaN(double x)
    {
        Assert.True(Float128.IsNaN(x * Float128.NegativeInfinity));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void MultiplyByNegativeOneIsNegate(double x)
    {
        Assert.Equal(-x, x * Float128.NegativeOne);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void MultiplyByPositiveInfinityIsNaN(double x)
    {
        Assert.True(Float128.IsNaN(x * Float128.PositiveInfinity));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void MultiplyByOneIsIdentity(double x)
    {
        Assert.Equal(x, x * Float128.One);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void MultiplyByZeroIsZero(double x)
    {
        Assert.Equal(Float128.Zero, x * Float128.Zero);
    }

    [Theory]
    [InlineData([0.25, 1.5, 0.375])]
    [InlineData([-8.0, 2.0, -16.0])]
    [InlineData([3.0, -0.5, -1.5])]
    [InlineData([-5.0, -0.625, 3.125])]
    public void MultiplyGeneralIsCorrect(double x, double y, double z)
    {
        Assert.Equal(z, (Float128)x * y);
    }

    [Theory]
    [InlineData(-5.5)]
    [InlineData(-10.0)]
    [InlineData(-30.655)]
    [InlineData(-3.33)]
    public void MultiplyNegativeByBigNumberIsNegativeInfinity(double x)
    {
        Assert.Equal(Float128.NegativeInfinity, x * Float128.ScaleB(Float128.One, short.MaxValue / 2));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void MultiplyNormalBySubnormalIsSubnormal(double x)
    {
        Assert.True(Float128.IsSubnormal(x * Float128.Epsilon));
    }

    [Theory]
    [InlineData(5.5)]
    [InlineData(10.0)]
    [InlineData(30.655)]
    [InlineData(3.33)]
    public void MultiplyPositiveByBigNumberIsPositiveInfinity(double x)
    {
        Assert.Equal(Float128.PositiveInfinity, x * Float128.ScaleB(Float128.One, short.MaxValue / 2));
    }

    [Fact]
    public void MultiplySubnormalIsCorrect()
    {
        Float128 twoEps = Float128.BitIncrement(Float128.Epsilon);
        Assert.Equal(twoEps, Float128.Epsilon * 2.0);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void NegateIsMultiplyByNegativeOne(double x)
    {
        Assert.Equal(x * Float128.NegativeOne, -x);
    }
}