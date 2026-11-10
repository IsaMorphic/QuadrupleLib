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

namespace QuadrupleLib.Tests.Math
{
    public abstract class SignMagnitudeTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(-1.0)]
        [InlineData(0.0)]
        [InlineData(1.0)]
        public void SignReturnsCorrectValue(double x)
        {
            Assert.Equal(System.Math.Sign(x), Float128<TAccelerator>.Sign(x));
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(0.0)]
        [InlineData(1.0)]
        public void AbsReturnsCorrectValue(double x)
        {
            Assert.Equal(System.Math.Abs(x), Float128<TAccelerator>.Abs(x));
        }

        [Theory]
        [InlineData(2.0, 3.0)]
        [InlineData(-2.0, 3.0)]
        [InlineData(2.0, 1.0)]
        [InlineData(-3.0, 4.0)]
        [InlineData(-1.0, 2.0)]
        public void MaxReturnsCorrectValue(double x, double y)
        {
            Assert.Equal(System.Math.Max(x, y), Float128<TAccelerator>.Max(x, y));
        }

        [Theory]
        [InlineData(2.0, 3.0)]
        [InlineData(-2.0, 3.0)]
        [InlineData(2.0, 1.0)]
        [InlineData(-3.0, 4.0)]
        [InlineData(-1.0, 2.0)]
        public void MinReturnsCorrectValue(double x, double y)
        {
            Assert.Equal(System.Math.Min(x, y), Float128<TAccelerator>.Min(x, y));
        }

        [Theory]
        [InlineData(2.0, 3.0)]
        [InlineData(-2.0, 3.0)]
        [InlineData(2.0, 1.0)]
        [InlineData(-3.0, 4.0)]
        [InlineData(-1.0, 2.0)]
        public void MaxMagnitudeReturnsCorrectValue(double x, double y)
        {
            Assert.Equal(System.Math.MaxMagnitude(x, y), Float128<TAccelerator>.MaxMagnitude(x, y));
        }

        [Theory]
        [InlineData(2.0, 3.0)]
        [InlineData(-2.0, 3.0)]
        [InlineData(2.0, 1.0)]
        [InlineData(-3.0, 4.0)]
        [InlineData(-1.0, 2.0)]
        public void MinMagnitudeReturnsCorrectValue(double x, double y)
        {
            Assert.Equal(System.Math.MinMagnitude(x, y), Float128<TAccelerator>.MinMagnitude(x, y));
        }
    }

    public class SignMagnitudeTests_DefaultAccelerator :
        SignMagnitudeTests<DefaultAccelerator>
    { }

    public class SignMagnitudeTests_SoftwareAccelerator :
        SignMagnitudeTests<SoftwareAccelerator>
    { }
}
