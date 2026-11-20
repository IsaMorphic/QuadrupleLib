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

using System.Numerics;
using QuadrupleLib.Utilities;
using Xunit;

namespace QuadrupleLib.Tests.Utilities
{
    public class UInt256Tests
    {
        [Fact]
        public void IsAddWithoutCarryCorrect()
        {
            UInt256 one = 1UL;
            UInt256 two = 2UL;
            Assert.Equal(3UL, one + two);
        }

        [Fact]
        public void IsAddWithCarryCorrect()
        {
            UInt256 n = UInt256.One + UInt128.MaxValue;
            Assert.Equal((UInt128.Zero, UInt128.One), (n._lo, n._hi));
        }

        [Fact]
        public void IsAddWithOverflowZero()
        {
            UInt256 n = UInt256.MaxValue + UInt256.One;
            Assert.Equal(UInt256.Zero, n);
        }

        [Fact]
        public void IsSubtractWithoutBorrowCorrect()
        {
            UInt256 one = 1UL;
            UInt256 three = 3UL;
            Assert.Equal(2UL, three - one);
        }

        [Fact]
        public void IsSubtractWithBorrowCorrect()
        {
            UInt256 n = UInt256.One + UInt128.MaxValue, m = n - UInt256.One;
            Assert.Equal((UInt128.MaxValue, UInt128.Zero), (m._lo, m._hi));
        }

        [Fact]
        public void IsSubtractWithUnderflowMaximum()
        {
            UInt256 n = UInt256.Zero - UInt256.One;
            Assert.Equal(UInt256.MaxValue, n);
        }

        [Theory]
        [InlineData(0UL, 0UL, 0)]
        [InlineData(0UL, 0UL, 1)]
        [InlineData(0UL, 0UL, 2)]
        [InlineData(1UL, 1UL, 0)]
        [InlineData(1UL, 2UL, 1)]
        [InlineData(1UL, 4UL, 2)]
        [InlineData(2UL, 2UL, 0)]
        [InlineData(2UL, 4UL, 1)]
        [InlineData(2UL, 8UL, 2)]
        public void IsSmallShiftLeftCorrect(ulong n, ulong m, int amt)
        {
            UInt256 N = n; N <<= amt;
            Assert.Equal(m, N);
        }

        [Theory]
        [InlineData(0UL, 0UL, 0)]
        [InlineData(0UL, 0UL, 1)]
        [InlineData(0UL, 0UL, 2)]
        [InlineData(1UL, 1UL, 0)]
        [InlineData(1UL, 2UL, 1)]
        [InlineData(1UL, 4UL, 2)]
        [InlineData(2UL, 2UL, 0)]
        [InlineData(2UL, 4UL, 1)]
        [InlineData(2UL, 8UL, 2)]
        public void IsLargeShiftLeftCorrect(ulong n, ulong m, int amt)
        {
            UInt256 N = n; N <<= 128 + amt;
            UInt256 M = m; M <<= 128;
            Assert.Equal(M, N);
        }

        [Theory]
        [InlineData(0UL, 0)]
        [InlineData(0UL, 1)]
        [InlineData(0UL, 2)]
        [InlineData(1UL, 0)]
        [InlineData(1UL, 1)]
        [InlineData(1UL, 2)]
        [InlineData(2UL, 0)]
        [InlineData(2UL, 1)]
        [InlineData(2UL, 2)]
        public void IsExtraLargeShiftLeftZero(ulong n, int amt)
        {
            UInt256 N = n; N <<= 256 + amt;
            Assert.Equal(UInt256.Zero, N);
        }

        [Theory]
        [InlineData(0UL, 0UL, 0)]
        [InlineData(0UL, 0UL, 1)]
        [InlineData(0UL, 0UL, 2)]
        [InlineData(2UL, 2UL, 0)]
        [InlineData(2UL, 1UL, 1)]
        [InlineData(2UL, 0UL, 2)]
        [InlineData(4UL, 4UL, 0)]
        [InlineData(4UL, 2UL, 1)]
        [InlineData(4UL, 1UL, 2)]
        public void IsSmallShiftRightCorrect(ulong n, ulong m, int amt)
        {
            UInt256 N = n; N >>= amt;
            Assert.Equal(m, N);
        }

        [Theory]
        [InlineData(0UL, 0UL, 0)]
        [InlineData(0UL, 0UL, 1)]
        [InlineData(0UL, 0UL, 2)]
        [InlineData(2UL, 2UL, 0)]
        [InlineData(2UL, 1UL, 1)]
        [InlineData(2UL, 0UL, 2)]
        [InlineData(4UL, 4UL, 0)]
        [InlineData(4UL, 2UL, 1)]
        [InlineData(4UL, 1UL, 2)]
        public void IsLargeShiftRightCorrect(ulong n, ulong m, int amt)
        {
            UInt256 N = (UInt256)n << 128; N >>= 128 + amt;
            Assert.Equal(m, N);
        }

        [Theory]
        [InlineData(0UL, 0)]
        [InlineData(0UL, 1)]
        [InlineData(0UL, 2)]
        [InlineData(2UL, 0)]
        [InlineData(2UL, 1)]
        [InlineData(2UL, 2)]
        [InlineData(4UL, 0)]
        [InlineData(4UL, 1)]
        [InlineData(4UL, 2)]
        public void IsExtraLargeShiftRightZero(ulong n, int amt)
        {
            UInt256 N = (UInt256)n << 128; N >>= 256 + amt;
            Assert.Equal(UInt256.Zero, N);
        }

        [Theory]
        [InlineData(0UL, 0UL, 0)]
        [InlineData(0UL, 1UL, -1)]
        [InlineData(0UL, 2UL, -1)]
        [InlineData(1UL, 0UL, 1)]
        [InlineData(1UL, 1UL, 0)]
        [InlineData(1UL, 2UL, -1)]
        [InlineData(2UL, 0UL, 1)]
        [InlineData(2UL, 1UL, 1)]
        [InlineData(2UL, 2UL, 0)]
        public void IsSmallCompareToCorrect(ulong n, ulong m, int s)
        {
            UInt256 N = n, M = m;
            int S = System.Math.Sign(N.CompareTo(M));
            Assert.Equal(s, S);
        }

        [Theory]
        [InlineData(0UL, 0UL, 0)]
        [InlineData(0UL, 1UL, -1)]
        [InlineData(0UL, 2UL, -1)]
        [InlineData(1UL, 0UL, 1)]
        [InlineData(1UL, 1UL, 0)]
        [InlineData(1UL, 2UL, -1)]
        [InlineData(2UL, 0UL, 1)]
        [InlineData(2UL, 1UL, 1)]
        [InlineData(2UL, 2UL, 0)]
        public void IsLargeCompareToCorrect(ulong n, ulong m, int s)
        {
            UInt256 N = (UInt256)n << 128, M = (UInt256)m << 128;
            int S = System.Math.Sign(N.CompareTo(M));
            Assert.Equal(s, S);
        }

        [Fact]
        public void IsMinValueZero()
        {
            Assert.Equal(UInt256.Zero, UInt256.MinValue);
        }

        [Fact]
        public void IsBigIntegerConversionCorrect()
        {
            BigInteger n = (BigInteger.One << 128) - 1;
            UInt256 N = (UInt256)n;
            Assert.Equal(UInt128.MaxValue, N._lo);
            Assert.Equal(UInt128.Zero, N._hi);
            Assert.Equal(n, (BigInteger)N);
        }

        [Fact]
        public void IsBigIntegerMaxValueConversionCorrect()
        {
            // 2^256 - 1 is the largest UInt256, with every bit of both halves set
            BigInteger n = (BigInteger.One << 256) - 1;
            UInt256 N = (UInt256)n;
            Assert.Equal(UInt128.MaxValue, N._lo);
            Assert.Equal(UInt128.MaxValue, N._hi);
            Assert.Equal(n, (BigInteger)N);
        }

        [Fact]
        public void IsBigIntegerOverflowException()
        {
            // 2^256 does not fit in 256 bits
            BigInteger n = BigInteger.One << 256;
            Assert.Throws<OverflowException>(() => (UInt256)n);
        }

        [Fact]
        public void IsInt128ConversionCorrect()
        {
            Int128 n = ((Int128)0x1234567890ABCDEFUL << 64) | 0xFEDCBA0987654321UL;
            UInt256 N = (UInt256)n;
            Assert.Equal(new UInt128(0x1234567890ABCDEFUL, 0xFEDCBA0987654321UL), N._lo);
            Assert.Equal(UInt128.Zero, N._hi);
            Assert.Equal(n, (Int128)N);
        }

        [Fact]
        public void IsLongConversionCorrect()
        {
            long n = long.MaxValue;
            UInt256 N = (UInt256)n;
            Assert.Equal((UInt128)(ulong)n, N._lo);
            Assert.Equal(UInt128.Zero, N._hi);
            Assert.Equal(n, (long)N);
        }

        [Fact]
        public void IsIntConversionCorrect()
        {
            int n = int.MaxValue;
            UInt256 N = (UInt256)n;
            Assert.Equal((UInt128)(uint)n, N._lo);
            Assert.Equal(UInt128.Zero, N._hi);
            Assert.Equal(n, (int)N);
        }

        [Fact]
        public void IsIncrementCorrect()
        {
            UInt256 n = 1UL;
            ++n;
            Assert.Equal(2UL, n);
        }

        [Fact]
        public void IsIncrementWithCarryCorrect()
        {
            UInt256 n = UInt128.MaxValue;
            ++n;
            Assert.Equal((UInt128.Zero, UInt128.One), (n._lo, n._hi));
        }

        [Fact]
        public void IsDecrementCorrect()
        {
            UInt256 n = 2UL;
            --n;
            Assert.Equal(1UL, n);
        }

        [Fact]
        public void IsDecrementWithBorrowCorrect()
        {
            UInt256 n = UInt256.One << 128;
            --n;
            Assert.Equal((UInt128.MaxValue, UInt128.Zero), (n._lo, n._hi));
        }

        [Fact]
        public void IsXorCorrect()
        {
            UInt256 a = new UInt128(0xFF00UL, 0x00FFUL);
            UInt256 b = new UInt128(0xFF0FUL, 0x0F0FUL);
            UInt256 c = a ^ b;
            Assert.Equal(new UInt128(0x000FUL, 0x0FF0UL), c);
        }

        [Theory]
        [InlineData(1UL, 0UL, 0)]
        [InlineData(8UL, 0UL, 3)]
        [InlineData(0UL, 1UL, 64)]
        public void IsTrailingZeroCountInLowHalfCorrect(ulong lo, ulong hi, int expected)
        {
            UInt256 n = new UInt128(hi, lo);
            Assert.Equal((UInt256)expected, UInt256.TrailingZeroCount(n));
        }

        [Theory]
        [InlineData(0UL, 256)]
        [InlineData(1UL, 128)]
        [InlineData(4UL, 130)]
        public void IsTrailingZeroCountInHighHalfCorrect(ulong hi, int expected)
        {
            UInt256 n = ((UInt256)hi) << 128;
            Assert.Equal((UInt256)expected, UInt256.TrailingZeroCount(n));
        }

        [Theory]
        [InlineData(0UL, 0UL, 0)]
        [InlineData(1UL, 0UL, 1)]
        [InlineData(0xFFUL, 0UL, 8)]
        [InlineData(0UL, 1UL, 1)]
        [InlineData(0xFFUL, 0xFFUL, 16)]
        public void IsPopCountCorrect(ulong lo, ulong hi, int expected)
        {
            UInt256 n = new UInt128(hi, lo);
            Assert.Equal((UInt256)expected, UInt256.PopCount(n));
        }

        [Fact]
        public void IsPopCountOfMaxValueCorrect()
        {
            Assert.Equal((UInt256)256, UInt256.PopCount(UInt256.MaxValue));
        }

        [Fact]
        public void IsMinCorrect()
        {
            Assert.Equal(UInt256.One, UInt256.Min(UInt256.One, UInt256.MaxValue));
            Assert.Equal(UInt256.One, UInt256.Min(UInt256.MaxValue, UInt256.One));
        }

        [Fact]
        public void IsMaxCorrect()
        {
            Assert.Equal(UInt256.MaxValue, UInt256.Max(UInt256.One, UInt256.MaxValue));
            Assert.Equal(UInt256.MaxValue, UInt256.Max(UInt256.MaxValue, UInt256.One));
        }

        [Fact]
        public void IsGreaterThanCorrect()
        {
            Assert.True(UInt256.MaxValue > UInt256.One);
            Assert.True(!(UInt256.One > UInt256.MaxValue));
            Assert.True(!(UInt256.One > UInt256.One));
        }

        [Fact]
        public void IsGreaterThanOrEqualCorrect()
        {
            Assert.True(UInt256.MaxValue >= UInt256.One);
            Assert.True(UInt256.One >= UInt256.One);
            Assert.True(!(UInt256.One >= UInt256.MaxValue));
        }

        [Fact]
        public void IsLessThanOrEqualCorrect()
        {
            Assert.True(UInt256.One <= UInt256.MaxValue);
            Assert.True(UInt256.One <= UInt256.One);
            Assert.True(!(UInt256.MaxValue <= UInt256.One));
        }

        [Fact]
        public void IsCompareToObjectCorrect()
        {
            UInt256 n = UInt256.One;
            Assert.Equal(0, n.CompareTo((object)n));
            Assert.Equal(1, System.Math.Sign(n.CompareTo((object)UInt256.Zero)));
        }

        [Fact]
        public void IsCompareToInvalidObjectException()
        {
            Assert.Throws<InvalidOperationException>(() => UInt256.One.CompareTo((object)1));
        }

        [Fact]
        public void IsEqualsObjectCorrect()
        {
            UInt256 n = UInt256.One;
            Assert.True(n.Equals((object)n));
            Assert.True(!n.Equals((object)UInt256.Zero));
            Assert.True(!n.Equals((object)1));
            Assert.True(!n.Equals(null));
        }

        [Fact]
        public void IsGetHashCodeConsistent()
        {
            UInt256 a = new UInt128(1UL, 2UL);
            UInt256 b = new UInt128(1UL, 2UL);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void IsToStringCorrect()
        {
            Assert.Equal("0", UInt256.Zero.ToString());
            Assert.Equal("1", UInt256.One.ToString());
            Assert.Equal("340282366920938463463374607431768211456", (UInt256.One << 128).ToString());
            Assert.Equal("115792089237316195423570985008687907853269984665640564039457584007913129639935", UInt256.MaxValue.ToString());
        }
    }
}
