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

namespace QuadrupleLib.Tests.Arithmetic
{
    public abstract class BitOperationTests<TAccelerator>
            where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(0.33)]
        public void BitIncrementRoundtripIsExactlyEqual(double x)
        {
            Assert.Equal(x, Float128<TAccelerator>.BitIncrement(Float128<TAccelerator>.BitDecrement(x)));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(0.33)]
        public void BitDecrementRoundtripIsExactlyEqual(double x)
        {
            Assert.Equal(x, Float128<TAccelerator>.BitDecrement(Float128<TAccelerator>.BitIncrement(x)));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(0.33)]
        public void BitIncrementIsGreaterThanOriginal(double x)
        {
            Assert.True(Float128<TAccelerator>.BitIncrement(x) > x);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(0.33)]
        public void BitDecrementIsLessThanOriginal(double x)
        {
            Assert.True(Float128<TAccelerator>.BitDecrement(x) < x);
        }

        [Theory]
        [InlineData(0.5, 0)]
        [InlineData(1.0, 0)]
        [InlineData(0.33, 0)]
        [InlineData(-0.288, 0)]
        [InlineData(2.0, 0)]
        [InlineData(-2.0, 0)]
        [InlineData(-4.65, 0)]
        [InlineData(0.5, 1)]
        [InlineData(1.0, 1)]
        [InlineData(0.33, 1)]
        [InlineData(-0.288, 1)]
        [InlineData(2.0, 1)]
        [InlineData(-2.0, 1)]
        [InlineData(-4.65, 1)]
        [InlineData(0.5, 2)]
        [InlineData(1.0, 2)]
        [InlineData(0.33, 2)]
        [InlineData(-0.288, 2)]
        [InlineData(2.0, 2)]
        [InlineData(-2.0, 2)]
        [InlineData(-4.65, 2)]
        [InlineData(0.5, -1)]
        [InlineData(1.0, -1)]
        [InlineData(0.33, -1)]
        [InlineData(-0.288, -1)]
        [InlineData(2.0, -1)]
        [InlineData(-2.0, -1)]
        [InlineData(-4.65, -1)]
        [InlineData(0.5, -2)]
        [InlineData(1.0, -2)]
        [InlineData(0.33, -2)]
        [InlineData(-0.288, -2)]
        [InlineData(2.0, -2)]
        [InlineData(-2.0, -2)]
        [InlineData(-4.65, -2)]
        public void ScaleBIsCorrect(double x, int n)
        {
            double y = double.ScaleB(x, n);
            Assert.Equal(y, Float128<TAccelerator>.ScaleB(x, n));
        }

        [Fact]
        public void GetExponentByteCountIsTwo()
        {
            Assert.Equal(2, Float128<TAccelerator>.One.GetExponentByteCount());
        }

        [Theory]
        [InlineData(2.0, 0)]
        [InlineData(4.0, 1)]
        [InlineData(16.0, 2)]
        [InlineData(256.0, 3)]
        public void GetExponentShortestBitLengthIsCorrect(double x, int expected)
        {
            Float128<TAccelerator> value = x;
            Assert.Equal(expected, value.GetExponentShortestBitLength());
        }

        [Fact]
        public void GetSignificandBitLengthIs113()
        {
            Assert.Equal(113, Float128<TAccelerator>.One.GetSignificandBitLength());
        }

        [Fact]
        public void GetSignificandByteCountIs16()
        {
            Assert.Equal(16, Float128<TAccelerator>.One.GetSignificandByteCount());
        }

        [Theory]
        [InlineData(2.0, 0x00, 0x01)]
        [InlineData(256.0, 0x00, 0x08)]
        public void TryWriteExponentBigEndianIsCorrect(double x, byte high, byte low)
        {
            Float128<TAccelerator> value = x;
            byte[] destination = new byte[16];
            bool result = value.TryWriteExponentBigEndian(destination.AsSpan(), out int bytesWritten);
            Assert.True(result);
            Assert.Equal(2, bytesWritten);
            Assert.Equal(high, destination[0]);
            Assert.Equal(low, destination[1]);
        }

        [Theory]
        [InlineData(2.0, 0x01, 0x00)]
        [InlineData(256.0, 0x08, 0x00)]
        public void TryWriteExponentLittleEndianIsCorrect(double x, byte low, byte high)
        {
            Float128<TAccelerator> value = x;
            byte[] destination = new byte[16];
            bool result = value.TryWriteExponentLittleEndian(destination.AsSpan(), out int bytesWritten);
            Assert.True(result);
            Assert.Equal(2, bytesWritten);
            Assert.Equal(low, destination[0]);
            Assert.Equal(high, destination[1]);
        }

        // binary128 stores 112 fraction bits plus the implicit leading 1 at bit 112, so the 16-byte
        // significand of 2.0 (1.0 x 2^1) is 1 << 112, and of 3.0 (1.1b x 2^1) is (1 << 112) | (1 << 111)
        [Theory]
        [InlineData(2.0, 0x01, 0x00)]
        [InlineData(3.0, 0x01, 0x80)]
        public void TryWriteSignificandBigEndianIsCorrect(double x, byte second, byte third)
        {
            Float128<TAccelerator> value = x;
            byte[] destination = new byte[16];
            bool result = value.TryWriteSignificandBigEndian(destination.AsSpan(), out int bytesWritten);
            Assert.True(result);
            Assert.Equal(16, bytesWritten);
            byte[] expected = new byte[16];
            expected[1] = second;
            expected[2] = third;
            Assert.Equal(expected, destination);
        }

        [Theory]
        [InlineData(2.0, 0x01, 0x00)]
        [InlineData(3.0, 0x01, 0x80)]
        public void TryWriteSignificandLittleEndianIsCorrect(double x, byte second, byte third)
        {
            Float128<TAccelerator> value = x;
            byte[] destination = new byte[16];
            bool result = value.TryWriteSignificandLittleEndian(destination.AsSpan(), out int bytesWritten);
            Assert.True(result);
            Assert.Equal(16, bytesWritten);
            byte[] expected = new byte[16];
            expected[14] = second;
            expected[13] = third;
            Assert.Equal(expected, destination);
        }

        [Fact]
        public void TryWriteExponentToShortSpanIsFalse()
        {
            byte[] destination = new byte[1];
            Assert.False(Float128<TAccelerator>.One.TryWriteExponentBigEndian(destination.AsSpan(), out int bigWritten));
            Assert.Equal(0, bigWritten);
            Assert.False(Float128<TAccelerator>.One.TryWriteExponentLittleEndian(destination.AsSpan(), out int littleWritten));
            Assert.Equal(0, littleWritten);
        }

        [Fact]
        public void TryWriteSignificandToShortSpanIsFalse()
        {
            byte[] destination = new byte[15];
            Assert.False(Float128<TAccelerator>.One.TryWriteSignificandBigEndian(destination.AsSpan(), out int bigWritten));
            Assert.Equal(0, bigWritten);
            Assert.False(Float128<TAccelerator>.One.TryWriteSignificandLittleEndian(destination.AsSpan(), out int littleWritten));
            Assert.Equal(0, littleWritten);
        }

        // The bitwise operators act on the raw IEEE 754 bit patterns, so the expected values are the
        // floats whose encodings result from combining the operands' bits.
        [Fact]
        public void BitwiseAndOfEqualValuesIsThatValue()
        {
            Float128<TAccelerator> a = 1.0;
            Assert.Equal(Float128<TAccelerator>.One, a & a);
        }

        [Fact]
        public void BitwiseAndOfOneAndTwoIsZero()
        {
            Float128<TAccelerator> a = 1.0;
            Float128<TAccelerator> b = 2.0;
            Assert.Equal(Float128<TAccelerator>.Zero, a & b);
        }

        [Fact]
        public void BitwiseOrOfOneAndTwoIsPositiveInfinity()
        {
            Float128<TAccelerator> a = 1.0;
            Float128<TAccelerator> b = 2.0;
            Float128<TAccelerator> expected = double.PositiveInfinity;
            Assert.Equal(expected, a | b);
        }

        [Fact]
        public void BitwiseXorOfEqualValuesIsZero()
        {
            Float128<TAccelerator> a = 1.0;
            Assert.Equal(Float128<TAccelerator>.Zero, a ^ a);
        }

        [Fact]
        public void BitwiseXorOfOneAndTwoIsPositiveInfinity()
        {
            Float128<TAccelerator> a = 1.0;
            Float128<TAccelerator> b = 2.0;
            Float128<TAccelerator> expected = double.PositiveInfinity;
            Assert.Equal(expected, a ^ b);
        }

        [Fact]
        public void BitwiseNotOfZeroIsNaN()
        {
            Float128<TAccelerator> a = Float128<TAccelerator>.Zero;
            Assert.True(Float128<TAccelerator>.IsNaN(~a));
        }
    }

    public class BitOperationTests_SoftwareAccelerator :
        BitOperationTests<SoftwareAccelerator>
    { }

    public class BitOperationTests_DefaultAccelerator :
        BitOperationTests<DefaultAccelerator>
    { }
}