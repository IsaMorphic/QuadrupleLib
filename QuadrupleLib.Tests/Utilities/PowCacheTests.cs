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
using System.Numerics;
using Xunit;

namespace QuadrupleLib.Tests.Utilities
{
    public class PowCacheTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 5)]
        [InlineData(2, 25)]
        [InlineData(3, 125)]
        [InlineData(4, 625)]
        public void GetPow5_ShouldReturnCorrectValue(int exponent, int expected)
        {
            BigInteger result = PowCache.GetPow5(exponent);
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 10)]
        [InlineData(2, 100)]
        [InlineData(3, 1000)]
        [InlineData(4, 10000)]
        public void GetPow10_ShouldReturnCorrectValue(int exponent, int expected)
        {
            BigInteger result = PowCache.GetPow10(exponent);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetPow5_ShouldThrowArgumentOutOfRangeException_ForNegativeExponent()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PowCache.GetPow5(-1));
        }

        [Fact]
        public void GetPow10_ShouldThrowArgumentOutOfRangeException_ForNegativeExponent()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PowCache.GetPow10(-1));
        }

        [Fact]
        public void GetPow5_ShouldCacheValues()
        {
            int exponent = 10;
            BigInteger firstCall = PowCache.GetPow5(exponent);
            BigInteger secondCall = PowCache.GetPow5(exponent);
            Assert.Equal(firstCall, secondCall);
        }

        [Fact]
        public void GetPow10_ShouldCacheValues()
        {
            int exponent = 10;
            BigInteger firstCall = PowCache.GetPow10(exponent);
            BigInteger secondCall = PowCache.GetPow10(exponent);
            Assert.Equal(firstCall, secondCall);
        }

        [Fact]
        public void GetPow5_ShouldNotThrow_WhenEvicted()
        {
            for (int i = 0; i < PowCache.MAX_CACHE_SIZE; i++)
            {
                PowCache.GetPow5(i);
            }

            AssertX.DoesNotThrow<Exception>(() => 
                PowCache.GetPow5(PowCache.MAX_CACHE_SIZE + 1)
                );
        }

        [Fact]
        public void GetPow10_ShouldNotThrow_WhenEvicted()
        {
            for (int i = 0; i < PowCache.MAX_CACHE_SIZE; i++)
            {
                PowCache.GetPow10(i);
            }

            AssertX.DoesNotThrow<Exception>(() =>
                PowCache.GetPow10(PowCache.MAX_CACHE_SIZE + 1)
                );
        }
    }
}
