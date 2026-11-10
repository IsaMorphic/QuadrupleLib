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
    }

    public class BitOperationTests_SoftwareAccelerator :
        BitOperationTests<SoftwareAccelerator>
    { }

    public class BitOperationTests_DefaultAccelerator :
        BitOperationTests<DefaultAccelerator>
    { }
}