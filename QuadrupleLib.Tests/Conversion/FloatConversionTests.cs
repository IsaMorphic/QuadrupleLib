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
    public abstract class FloatConversionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(1.5)]
        [InlineData(0.5)]
        [InlineData(1.300)]
        [InlineData(-263.0)]
        [InlineData(123.4567)]
        [InlineData(double.Epsilon)]
        public void ConvertToDoubleIsEqual(double x)
        {
            Assert.Equal(x, (double)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(1.5f)]
        [InlineData(0.5f)]
        [InlineData(1.300f)]
        [InlineData(-263.0f)]
        [InlineData(123.4567f)]
        [InlineData(float.Epsilon)]
        public void ConvertToSingleIsEqual(float x)
        {
            Assert.Equal(x, (float)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(1.5f)]
        [InlineData(0.5f)]
        [InlineData(1.300f)]
        [InlineData(-263.0f)]
        [InlineData(123.4567f)]
        [InlineData(5.9604645E-08f)]
        public void ConvertToHalfIsEqual(float x)
        {
            Half _x = (Half)x;
            Assert.Equal(_x, (Half)(Float128<TAccelerator>)_x);
        }
    }

    public class FloatConversionTests_DefaultAccelerator :
        FloatConversionTests<DefaultAccelerator>
    { }

    public class FloatConversionTests_SoftwareAccelerator :
        FloatConversionTests<SoftwareAccelerator>
    { }
}