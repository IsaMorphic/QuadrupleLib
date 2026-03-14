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

using QuadrupleLib.Tests.Assertions.Types;
using Xunit;

namespace QuadrupleLib.Tests.Math
{
    public class HyperbolicTests
    {
        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsSinhCorrect(double x)
        {
            double y = double.Sinh(x);
            AssertX.NearlyEqual(y, Quad.Sinh(x), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsCoshCorrect(double x)
        {
            double y = double.Cosh(x);
            AssertX.NearlyEqual(y, Quad.Cosh(x), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsTanhCorrect(double x)
        {
            double y = double.Tanh(x);
            AssertX.NearlyEqual(y, Quad.Tanh(x), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsInverseSinhCorrect(double x)
        {
            double y = double.Asinh(x);
            AssertX.NearlyEqual(y, Quad.Asinh(x), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.345)]
        [InlineData(4.5678)]
        public void IsInverseCoshCorrect(double x)
        {
            double y = double.Acosh(x);
            AssertX.NearlyEqual(y, Quad.Acosh(x), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.5678)]
        public void IsInverseCoshNaN(double x)
        {
            Quad y = Quad.Acosh(x);
            Assert.True(Quad.IsNaN(y));
        }

        [Theory]
        [InlineData(0.345)]
        [InlineData(-.5678)]
        public void IsInverseTanhCorrect(double x)
        {
            double y = double.Atanh(x);
            AssertX.NearlyEqual(y, Quad.Atanh(x), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(1.345)]
        [InlineData(-1.0)]
        [InlineData(-1.5678)]
        public void IsInverseTanhNaN(double x)
        {
            Quad y = Quad.Atanh(x);
            Assert.True(Quad.IsNaN(y));
        }
    }
}
