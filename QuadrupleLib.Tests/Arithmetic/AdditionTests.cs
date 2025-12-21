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

public class AdditionTests
{
    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    [InlineData(double.NaN)]
    public void AddNaNIsNaN(double x)
    {
        Assert.True(Quad.IsNaN(x + Quad.NaN));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void AddNegativeInfinityIsNegativeInfinity(double x)
    {
        Assert.Equal(Quad.NegativeInfinity, x + Quad.NegativeInfinity);
    }

    [Theory]
    [InlineData([0.5, 1.5])]
    [InlineData([1.0, 2.0])]
    [InlineData([-1.0, 0.0])]
    public void AddOneIsCorrect(double x, double y)
    {
        Assert.Equal(y, x + Quad.One);
    }

    [Theory]
    [InlineData([0.5, -0.5])]
    [InlineData([1.0, 0.0])]
    [InlineData([-1.0, -2.0])]
    public void AddNegativeOneIsCorrect(double x, double y)
    {
        Assert.Equal(y, x + Quad.NegativeOne);
    }

    [Theory]
    [InlineData([0.5, 1.5, 2.0])]
    [InlineData([1.0, 2.0, 3.0])]
    [InlineData([-1.0, 3.5, 2.5])]
    public void AddNormalIsCorrect(double x, double y, double z)
    {
        Assert.Equal(z, (Quad)x + y);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void AddPositiveInfinityIsPositiveInfinity(double x)
    {
        Assert.Equal(Quad.PositiveInfinity, x + Quad.PositiveInfinity);
    }

    [Fact]
    public void AddSubnormalIsSubnormal()
    {
        Assert.True(Quad.IsSubnormal(Quad.Epsilon + Quad.Epsilon));
    }

    [Fact]
    public void AddSubnormalIsCorrect()
    {
        Assert.Equal(Quad.Zero, -Quad.Epsilon + Quad.Epsilon);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void AddSubnormalIsIdentity(double x)
    {
        Assert.Equal(x, x + Quad.Epsilon);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void AddZeroIsIdentity(double x)
    {
        Assert.Equal(x, x + Quad.Zero);
    }

    [Theory]
    [InlineData([0.5, 1.5])]
    [InlineData([1.0, 2.0])]
    [InlineData([-1.0, 0.0])]
    public void IncrementIsCorrect(double x, double y)
    {
        Quad x_inc = (Quad)x;
        Assert.Equal(y, ++x_inc);
    }

    [Fact]
    public void NegativeInfinityPlusNegativeInfinityIsNegativeInfinity()
    {
        Assert.Equal(Quad.NegativeInfinity, Quad.NegativeInfinity + Quad.NegativeInfinity);
    }

    [Fact]
    public void PositiveInfinityPlusNegativeInfinityIsNaN()
    {
        Assert.True(Quad.IsNaN(Quad.PositiveInfinity + Quad.NegativeInfinity));
    }

    [Fact]
    public void PositiveInfinityPlusPositiveInfinityIsPositiveInfinity()
    {
        Assert.Equal(Quad.PositiveInfinity, Quad.PositiveInfinity + Quad.PositiveInfinity);
    }
}