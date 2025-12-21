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

public class SubtractionTests
{
    [Theory]
    [InlineData([0.5, -0.5])]
    [InlineData([1.0, 0.0])]
    [InlineData([-1.0, -2.0])]
    public void DecrementIsCorrect(double x, double y)
    {
        Quad x_dec = (Quad)x;
        Assert.Equal(y, --x_dec);
    }

    [Fact]
    public void NegativeInfinityMinusNegativeInfinityIsNaN()
    {
        Assert.True(Quad.IsNaN(Quad.NegativeInfinity - Quad.NegativeInfinity));
    }

    [Fact]
    public void NegativeInfinityMinusPositiveInfinityIsNegativeInfinity()
    {
        Assert.Equal(Quad.NegativeInfinity, Quad.NegativeInfinity - Quad.PositiveInfinity);
    }

    [Fact]
    public void PositiveInfinityMinusNegativeInfinityIsPositiveInfinity()
    {
        Assert.Equal(Quad.PositiveInfinity, Quad.PositiveInfinity - Quad.NegativeInfinity);
    }

    [Fact]
    public void PositiveInfinityMinusPositiveInfinityIsNaN()
    {
        Assert.True(Quad.IsNaN(Quad.PositiveInfinity - Quad.PositiveInfinity));
    }

    [Theory]
    [InlineData([0.5, 1.5, -1.0])]
    [InlineData([1.0, -2.0, 3.0])]
    [InlineData([-1.0, 3.5, -4.5])]
    public void SubtractGeneralIsCorrect(double x, double y, double z)
    {
        Assert.Equal(z, (Quad)x - y);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    [InlineData(double.NaN)]
    public void SubtractNaNIsNaN(double x)
    {
        Assert.True(Quad.IsNaN(x - Quad.NaN));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void SubtractNegativeInfinityIsPositiveInfinity(double x)
    {
        Assert.Equal(Quad.PositiveInfinity, x - Quad.NegativeInfinity);
    }

    [Theory]
    [InlineData([0.5, -0.5])]
    [InlineData([1.0, 0.0])]
    [InlineData([-1.0, -2.0])]
    public void SubtractOneIsCorrect(double x, double y)
    {
        Assert.Equal(y, x - Quad.One);
    }

    [Theory]
    [InlineData([0.5, 1.5])]
    [InlineData([1.0, 2.0])]
    [InlineData([-1.0, 0.0])]
    public void SubtractNegativeOneIsCorrect(double x, double y)
    {
        Assert.Equal(y, x - Quad.NegativeOne);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void SubtractPositiveInfinityIsNegativeInfinity(double x)
    {
        Assert.Equal(Quad.NegativeInfinity, x - Quad.PositiveInfinity);
    }

    [Fact]
    public void SubtractSubnormalIsSubnormal()
    {
        Assert.True(Quad.IsSubnormal(Quad.Epsilon - -Quad.Epsilon));
    }

    [Fact]
    public void SubtractSubnormalIsCorrect()
    {
        Assert.Equal(Quad.Zero, Quad.Epsilon - Quad.Epsilon);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void SubtractSubnormalIsIdentity(double x)
    {
        Assert.Equal(x, x - Quad.Epsilon);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    [InlineData(0.33)]
    public void SubtractZeroIsIdentity(double x)
    {
        Assert.Equal(x, x - Quad.Zero);
    }
}