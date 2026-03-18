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
    }
}
