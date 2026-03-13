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
    public class PowerAndRootTests
    {
        [Theory]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        public void IsSqrtCorrect(double x) 
        {
            double y = double.Sqrt(x);
            AssertX.NearlyEqual(y, Quad.Sqrt(x), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        public void IsCbrtCorrect(double x)
        {
            double y = double.Cbrt(x);
            AssertX.NearlyEqual(y, Quad.Cbrt(x), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(1.0, 2)]
        [InlineData(2.1, 2)]
        [InlineData(3.676, 2)]
        [InlineData(1.0, 3)]
        [InlineData(2.1, 3)]
        [InlineData(3.676, 3)]
        [InlineData(1.0, 4)]
        [InlineData(2.1, 4)]
        [InlineData(3.676, 4)]
        public void IsRootNCorrect(double x, int n)
        {
            double y = double.RootN(x, n);
            AssertX.NearlyEqual(y, Quad.RootN(x, n), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(1.0, 2)]
        [InlineData(2.1, 2)]
        [InlineData(3.676, 2)]
        [InlineData(1.0, 3)]
        [InlineData(2.1, 3)]
        [InlineData(3.676, 3)]
        [InlineData(1.0, 4)]
        [InlineData(2.1, 4)]
        [InlineData(3.676, 4)]
        public void IsRootNEqualToPow(double x, int n)
        {
            Quad y0 = Quad.RootN(x, n);
            Quad y1 = Quad.Pow(x, Quad.One / n);
            AssertX.NearlyEqual(y0, y1, Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(1.0, 2.0)]
        [InlineData(2.1, 2.0)]
        [InlineData(3.676, 2.0)]
        [InlineData(1.0, 3.111)]
        [InlineData(2.1, 3.111)]
        [InlineData(3.676, 3.111)]
        [InlineData(1.0, 0.4)]
        [InlineData(2.1, 0.4)]
        [InlineData(3.676, 0.4)]
        [InlineData(1.0, -1.0)]
        [InlineData(2.1, -1.0)]
        [InlineData(3.676, -1.0)]
        public void IsPowCorrect(double x, double y)
        {
            double z = double.Pow(x, y);
            AssertX.NearlyEqual(z, Quad.Pow(x, y), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        public void IsPowZeroEqualToOne(double x)
        {
            AssertX.NearlyEqual(Quad.One, Quad.Pow(x, 0), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-2.1)]
        [InlineData(-0.1)]
        public void IsPowZeroEqualToNaN(double x)
        {
            Quad y = Quad.Pow(x, 0);
            Assert.True(Quad.IsNaN(y));
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        public void IsPowOneIdentity(double x)
        {
            AssertX.NearlyEqual(x, Quad.Pow(x, Quad.One), Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.1)]
        [InlineData(3.676)]
        public void IsPowNegativeOneReciprocal(double x)
        {
            AssertX.NearlyEqual(Quad.One / x, Quad.Pow(x, Quad.NegativeOne), Precision.NearestThousandth);
        }
    }
}
