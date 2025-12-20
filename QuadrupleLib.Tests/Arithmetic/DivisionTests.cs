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

namespace QuadrupleLib.Tests.Arithmetic;

public class DivisionTests
{
    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void DivideByNaNIsNaN(double x)
    {
        Assert.True(Float128.IsNaN(x / Float128.NaN));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void DivideByNegativeInfinityIsZero(double x)
    {
        Assert.True(Float128.IsZero(x / Float128.NegativeInfinity));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void DivideByNegativeOneIsNegate(double x)
    {
        Assert.Equal(-x, x / Float128.NegativeOne);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void DivideByPositiveInfinityIsZero(double x)
    {
        Assert.True(Float128.IsZero(x / Float128.PositiveInfinity));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void DivideByOneIsIdentity(double x)
    {
        Assert.Equal(x, x / Float128.One);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void DivideByZeroIsInfinity(double x)
    {
        Assert.True(Float128.IsInfinity(x / Float128.Zero));
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
        Assert.Equal(Float128.One, (Float128)x / x);
    }

    [Theory]
    [InlineData([3.0, 1.5, 2.0])]
    [InlineData([-8.0, 2.0, -4.0])]
    [InlineData([3.0, -0.5, -6.0])]
    [InlineData([-5.0, -0.625, 8.0])]
    public void DivideGeneralIsCorrect(double x, double y, double z)
    {
        Assert.Equal(z, (Float128)x / y);
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
    public void DivideByBigNumberIsSubnormal(double x)
    {
        Float128 y = x / Float128.ScaleB(Float128.One, short.MaxValue / 2 + 1);
        Assert.True(Float128.IsSubnormal(y));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void DivideNormalBySubnormalIsInfinity(double x)
    {
        Assert.True(Float128.IsInfinity(x / Float128.Epsilon));
    }

    [Fact]
    public void DivideSubnormalIsCorrect()
    {
        Float128 twoEps = Float128.BitIncrement(Float128.Epsilon);
        Assert.Equal(Float128.Epsilon, twoEps / 2.0);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void NegateIsDivideByNegativeOne(double x)
    {
        Assert.Equal(x / Float128.NegativeOne, -x);
    }

    [Fact]
    public void PositiveInfinityDividedByPositiveInfinityIsNaN()
    {
        Assert.True(Float128.IsNaN(Float128.PositiveInfinity / Float128.PositiveInfinity));
    }

    [Fact]
    public void PositiveInfinityDividedByNegativeInfinityIsNaN()
    {
        Assert.True(Float128.IsNaN(Float128.PositiveInfinity / Float128.NegativeInfinity));
    }

    [Fact]
    public void NegativeInfinityDividedByNegativeInfinityIsNaN()
    {
        Assert.True(Float128.IsNaN(Float128.NegativeInfinity / Float128.NegativeInfinity));
    }

    [Fact]
    public void NegativeInfinityDividedByPositiveInfinityIsNaN()
    {
        Assert.True(Float128.IsNaN(Float128.NegativeInfinity / Float128.PositiveInfinity));
    }

    [Fact]
    public void PositiveInfinityDividedByZeroIsNaN()
    {
        Assert.True(Float128.IsNaN(Float128.PositiveInfinity / Float128.Zero));
    }

    [Fact]
    public void NegativeInfinityDividedByZeroIsNaN()
    {
        Assert.True(Float128.IsNaN(Float128.NegativeInfinity / Float128.Zero));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.3)]
    [InlineData(0.33)]
    public void PositiveInfinityDividedByPositiveIsPositiveInfinity(double x)
    {
        Assert.Equal(Float128.PositiveInfinity, Float128.PositiveInfinity / x);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.3)]
    [InlineData(0.33)]
    public void NegativeInfinityDividedByPositiveIsNegativeInfinity(double x)
    {
        Assert.Equal(Float128.NegativeInfinity, Float128.NegativeInfinity / x);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(-1.0)]
    [InlineData(-2.3)]
    [InlineData(-0.33)]
    public void PositiveInfinityDividedByNegativeIsNegativeInfinity(double x)
    {
        Assert.Equal(Float128.NegativeInfinity, Float128.PositiveInfinity / x);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(-1.0)]
    [InlineData(-2.3)]
    [InlineData(-0.33)]
    public void NegativeInfinityDividedByNegativeIsPositiveInfinity(double x)
    {
        Assert.Equal(Float128.PositiveInfinity, Float128.NegativeInfinity / x);
    }
}
