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
using QuadrupleLib.Tests.Assertions.Types;
using Xunit;

namespace QuadrupleLib.Tests.Integers
{
    public abstract class RoundingTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(1.0)]
        [InlineData(1.1)]
        [InlineData(1.8)]
        [InlineData(2.5)]
        [InlineData(-1.0)]
        [InlineData(-1.1)]
        [InlineData(-1.8)]
        [InlineData(-2.5)]
        public void IsFloorCorrect(double x) 
        {
            double y0 = double.Floor(x);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Floor(x);
            Assert.Equal(y0, y1);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(1.1)]
        [InlineData(1.8)]
        [InlineData(2.5)]
        [InlineData(-1.0)]
        [InlineData(-1.1)]
        [InlineData(-1.8)]
        [InlineData(-2.5)]
        public void IsCeilingCorrect(double x)
        {
            double y0 = double.Ceiling(x);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Ceiling(x);
            Assert.Equal(y0, y1);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(1.1)]
        [InlineData(1.8)]
        [InlineData(2.5)]
        [InlineData(-1.0)]
        [InlineData(-1.1)]
        [InlineData(-1.8)]
        [InlineData(-2.5)]
        public void IsRoundCorrect(double x)
        {
            double y0 = double.Round(x);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Round(x);
            Assert.Equal(y0, y1);
        }

        [Theory]
        [InlineData(1.12345, 0)]
        [InlineData(1.12345, 1)]
        [InlineData(1.12345, 2)]
        [InlineData(1.12345, 3)]
        [InlineData(-1.12345, 0)]
        [InlineData(-1.12345, 1)]
        [InlineData(-1.12345, 2)]
        [InlineData(-1.12345, 3)]
        public void IsRoundToCorrect(double x, int digits)
        {
            double y0 = double.Round(x, digits);
            Float128<TAccelerator> y1 = Float128<TAccelerator>.Round(x, digits);
            AssertX.NearlyEqual(y0, y1, (Precision)digits);
        }
    }

    public class RoundingTests_DefaultAccelerator :
        RoundingTests<DefaultAccelerator>
    { }

    public class RoundingTests_SoftwareAccelerator :
        RoundingTests<SoftwareAccelerator>
    { }
}
