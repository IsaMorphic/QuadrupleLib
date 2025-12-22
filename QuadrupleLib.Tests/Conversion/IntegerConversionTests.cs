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

public class IntegerConversionTests
{
    [Theory]
    [InlineData(5L)]
    [InlineData(-1L)]
    [InlineData(-263L)]
    [InlineData(12345L)]
    public void ConvertToInt128IsEqual(long x)
    {
        Assert.Equal(x, (Int128)(Quad)x);
    }

    [Theory]
    [InlineData(5L)]
    [InlineData(-1L)]
    [InlineData(-263L)]
    [InlineData(12345L)]
    public void ConvertToInt64IsEqual(long x)
    {
        Assert.Equal(x, (long)(Quad)x);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(-1)]
    [InlineData(-263)]
    [InlineData(12345)]
    public void ConvertToInt32IsEqual(int x)
    {
        Assert.Equal(x, (int)(Quad)x);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(-1)]
    [InlineData(-263)]
    [InlineData(12345)]
    public void ConvertToInt16IsEqual(short x)
    {
        Assert.Equal(x, (short)(Quad)x);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(-1)]
    [InlineData(-128)]
    [InlineData(127)]
    public void ConvertToSByteIsEqual(sbyte x)
    {
        Assert.Equal(x, (sbyte)(Quad)x);
    }

    [Theory]
    [InlineData(5UL)]
    [InlineData(1UL)]
    [InlineData(263UL)]
    [InlineData(12345UL)]
    public void ConvertToUInt128IsEqual(ulong x)
    {
        Assert.Equal(x, (UInt128)(Quad)x);
    }

    [Theory]
    [InlineData(5UL)]
    [InlineData(1UL)]
    [InlineData(263UL)]
    [InlineData(12345UL)]
    public void ConvertToUInt64IsEqual(ulong x)
    {
        Assert.Equal(x, (ulong)(Quad)x);
    }

    [Theory]
    [InlineData(5U)]
    [InlineData(1U)]
    [InlineData(263U)]
    [InlineData(12345U)]
    public void ConvertToUInt32IsEqual(uint x)
    {
        Assert.Equal(x, (uint)(Quad)x);
    }

    [Theory]
    [InlineData(5U)]
    [InlineData(1U)]
    [InlineData(263U)]
    [InlineData(12345U)]
    public void ConvertToUInt16IsEqual(ushort x)
    {
        Assert.Equal(x, (ushort)(Quad)x);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(3)]
    [InlineData(255)]
    [InlineData(0)]
    public void ConvertToByteIsEqual(byte x)
    {
        Assert.Equal(x, (byte)(Quad)x);
    }
}