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

namespace QuadrupleLib.Tests.Math
{
    public abstract class LogarithmTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsLogCorrect(double x)
        {
            double y = double.Exp(x);
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Log(y), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void IsLogNaN(double x) 
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Log(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsLog2Correct(double x)
        {
            double y = double.Exp2(x);
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Log2(y), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void IsLog2NaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Log2(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsLog10Correct(double x)
        {
            double y = double.Exp10(x);
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Log10(y), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void IsLog10NaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Log10(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(-0.288, 2.0)]
        [InlineData(0.5, 2.0)]
        [InlineData(2.0, 2.0)]
        [InlineData(-2.0, 2.0)]
        [InlineData(-4.65, 2.0)]
        [InlineData(-0.288, 0.5)]
        [InlineData(0.5, 0.5)]
        [InlineData(2.0, 0.5)]
        [InlineData(-2.0, 0.5)]
        [InlineData(-4.65, 0.5)]
        public void IsLogBCorrect(double x, double b)
        {
            double y = double.Pow(b, x);
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Log(y, b), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(0.0, 2.0)]
        [InlineData(-1.0, 2.0)]
        [InlineData(0.0, 0.5)]
        [InlineData(-1.0, 0.5)]
        [InlineData(1.0, 0.0)]
        [InlineData(2.0, 0.0)]
        [InlineData(3.0, 0.0)]
        [InlineData(1.0, -1.0)]
        [InlineData(2.0, -1.0)]
        [InlineData(3.0, -1.0)]
        public void IsLogBNaN(double x, double b)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Log(x, b);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }
    }

    public class LogarithmTests_DefaultAccelerator :
        LogarithmTests<DefaultAccelerator>
    { }

    public class LogarithmTests_SoftwareAccelerator :
        LogarithmTests<SoftwareAccelerator>
    { }
}
