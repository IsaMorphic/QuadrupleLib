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
using QuadrupleLib.Tests.Assertions.Types;
using Xunit;

namespace QuadrupleLib.Tests.Conversion
{
    public abstract class StringConversionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.300)]
        [InlineData(-263.0)]
        [InlineData(123.4567)]
        [InlineData(1E-36)]
        [InlineData(1000000.0)]
        public void ConvertToStringRoundtripIsEqual(double x)
        {
            string s = $"{(Float128<TAccelerator>)x}";
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Parse(s), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(".5", 0.5)]
        [InlineData("-.5", -0.5)]
        [InlineData("0.5", 0.5)]
        [InlineData("-0.5", -0.5)]
        [InlineData("100", 100.0)]
        [InlineData("100.0", 100.0)]
        [InlineData("-100", -100.0)]
        [InlineData("-100.0", -100.0)]
        [InlineData("1E2", 100.0)]
        [InlineData("1.0E2", 100.0)]
        [InlineData("-1E2", -100.0)]
        [InlineData("-1.0E2", -100.0)]
        [InlineData(".1E3", 100.0)]
        [InlineData("0.1E3", 100.0)]
        [InlineData("-.1E3", -100.0)]
        [InlineData("-0.1E3", -100.0)]
        public void ParseStringIsCorrect(string s, double x) 
        {
            Assert.Equal((Float128<TAccelerator>)x, Float128<TAccelerator>.Parse(s));
        }
    }

    public class StringConversionTests_DefaultAccelerator :
        StringConversionTests<DefaultAccelerator>
    { }

    public class StringConversionTests_SoftwareAccelerator :
        StringConversionTests<SoftwareAccelerator>
    { }
}