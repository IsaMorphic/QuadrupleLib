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

namespace QuadrupleLib.Tests.Conversion
{
    public abstract class IntegerConversionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(5L)]
        [InlineData(-1L)]
        [InlineData(-263L)]
        [InlineData(12345L)]
        public void ConvertToInt128IsEqual(long x)
        {
            Assert.Equal(x, (Int128)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5L)]
        [InlineData(-1L)]
        [InlineData(-263L)]
        [InlineData(12345L)]
        public void ConvertToInt64IsEqual(long x)
        {
            Assert.Equal(x, (long)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-263)]
        [InlineData(12345)]
        public void ConvertToInt32IsEqual(int x)
        {
            Assert.Equal(x, (int)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-263)]
        [InlineData(12345)]
        public void ConvertToInt16IsEqual(short x)
        {
            Assert.Equal(x, (short)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-128)]
        [InlineData(127)]
        public void ConvertToSByteIsEqual(sbyte x)
        {
            Assert.Equal(x, (sbyte)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5UL)]
        [InlineData(1UL)]
        [InlineData(263UL)]
        [InlineData(12345UL)]
        public void ConvertToUInt128IsEqual(ulong x)
        {
            Assert.Equal(x, (UInt128)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5UL)]
        [InlineData(1UL)]
        [InlineData(263UL)]
        [InlineData(12345UL)]
        public void ConvertToUInt64IsEqual(ulong x)
        {
            Assert.Equal(x, (ulong)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5U)]
        [InlineData(1U)]
        [InlineData(263U)]
        [InlineData(12345U)]
        public void ConvertToUInt32IsEqual(uint x)
        {
            Assert.Equal(x, (uint)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5U)]
        [InlineData(1U)]
        [InlineData(263U)]
        [InlineData(12345U)]
        public void ConvertToUInt16IsEqual(ushort x)
        {
            Assert.Equal(x, (ushort)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(3)]
        [InlineData(255)]
        [InlineData(0)]
        public void ConvertToByteIsEqual(byte x)
        {
            Assert.Equal(x, (byte)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-263.5)]
        [InlineData(12345.0)]
        public void ConvertToInt128IsTruncated(double x)
        {
            Assert.Equal((Int128)x, (Int128)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-263.5)]
        [InlineData(12345.0)]
        public void ConvertToInt64IsTruncated(double x)
        {
            Assert.Equal((long)x, (long)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-263.5)]
        [InlineData(12345.0)]
        public void ConvertToInt32IsTruncated(double x)
        {
            Assert.Equal((int)x, (int)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-263.5)]
        [InlineData(12345.0)]
        public void ConvertToInt16IsTruncated(double x)
        {
            Assert.Equal((short)x, (short)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-128.5)]
        [InlineData(123.0)]
        public void ConvertToSByteIsTruncated(double x)
        {
            Assert.Equal((sbyte)x, (sbyte)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.54)]
        [InlineData(1.0)]
        [InlineData(263.8677)]
        [InlineData(12345.9)]
        public void ConvertToUInt128IsTruncated(double x)
        {
            Assert.Equal((UInt128)x, (UInt128)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.54)]
        [InlineData(1.0)]
        [InlineData(263.8677)]
        [InlineData(12345.9)]
        public void ConvertToUInt64IsTruncated(double x)
        {
            Assert.Equal((ulong)x, (ulong)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.54)]
        [InlineData(1.0)]
        [InlineData(263.8677)]
        [InlineData(12345.9)]
        public void ConvertToUInt32IsTruncated(double x)
        {
            Assert.Equal((uint)x, (uint)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.54)]
        [InlineData(1.0)]
        [InlineData(263.8677)]
        [InlineData(12345.9)]
        public void ConvertToUInt16IsTruncated(double x)
        {
            Assert.Equal((ushort)x, (ushort)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.1)]
        [InlineData(3.456)]
        [InlineData(255.5)]
        [InlineData(0.5)]
        public void ConvertToByteIsTruncated(double x)
        {
            Assert.Equal((byte)x, (byte)(Float128<TAccelerator>)x);
        }
    }

    public class IntegerConversionTests_DefaultAccelerator :
        IntegerConversionTests<DefaultAccelerator>
    { }

    public class IntegerConversionTests_SoftwareAccelerator :
        IntegerConversionTests<SoftwareAccelerator>
    { }
}