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
    public abstract class ExponentialTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsExpCorrect(double x)
        {
            double y = double.Exp(x);
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Exp(x), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsExp2Correct(double x) 
        {
            double y = double.Exp2(x);
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Exp2(x), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsExp10Correct(double x)
        {
            double y = double.Exp10(x);
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Exp10(x), Precision.NearestTenThousandth);
        }
    }

    public class ExponentialTests_DefaultAccelerator :
        ExponentialTests<DefaultAccelerator>
    { }

    public class ExponentialTests_SoftwareAccelerator :
        ExponentialTests<SoftwareAccelerator>
    { }
}
