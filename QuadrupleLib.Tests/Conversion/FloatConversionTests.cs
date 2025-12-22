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

namespace QuadrupleLib.Tests.Conversion;

public class FloatConversionTests
{
    [Theory]
    [InlineData(0.5)]
    [InlineData(1.300)]
    [InlineData(-263.0)]
    [InlineData(123.4567)]
    public void ConvertToDoubleIsEqual(double x)
    {
        Assert.Equal(x, (double)(Quad)x);
    }

    [Theory]
    [InlineData(0.5f)]
    [InlineData(1.300f)]
    [InlineData(-263.0f)]
    [InlineData(123.4567f)]
    public void ConvertToSingleIsEqual(float x)
    {
        Assert.Equal(x, (float)(Quad)x);
    }

    [Theory]
    [InlineData(0.5f)]
    [InlineData(1.300f)]
    [InlineData(-263.0f)]
    [InlineData(123.4567f)]
    public void ConvertToHalfIsEqual(float x)
    {
        Assert.Equal((Half)x, (Half)(Quad)x);
    }
}
