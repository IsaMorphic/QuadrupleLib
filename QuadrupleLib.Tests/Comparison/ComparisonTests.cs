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

namespace QuadrupleLib.Tests.Comparison
{
    public abstract class ComparisonTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        #region CompareTo(object)

        [Fact]
        public void CompareToSameTypeIsZero()
        {
            object other = Float128<TAccelerator>.One;

            int result = Float128<TAccelerator>.One.CompareTo(other);

            Assert.Equal(0, result);
        }

        [Fact]
        public void CompareToOtherTypeThrows()
        {
            object other = "not a Float128";

            Assert.Throws<ArgumentException>(() => Float128<TAccelerator>.One.CompareTo(other));
        }

        #endregion

        #region CompareTo(Float128)

        [Theory]
        [InlineData(1.0, 2.0)]
        [InlineData(-2.0, -1.0)]
        [InlineData(-1.0, 1.0)]
        [InlineData(0.5, 0.75)]
        [InlineData(double.NegativeInfinity, double.MinValue)]
        [InlineData(double.MaxValue, double.PositiveInfinity)]
        [InlineData(double.NaN, double.NegativeInfinity)]
        [InlineData(double.NaN, 0.0)]
        public void CompareToOrdersLikeDouble(double smaller, double larger)
        {
            // reference: double.CompareTo, which sorts NaN below every other value
            Float128<TAccelerator> a = smaller;
            Float128<TAccelerator> b = larger;

            Assert.Equal(System.Math.Sign(smaller.CompareTo(larger)), System.Math.Sign(a.CompareTo(b)));
            Assert.Equal(System.Math.Sign(larger.CompareTo(smaller)), System.Math.Sign(b.CompareTo(a)));
        }

        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(0.0, -0.0)]
        [InlineData(1.5, 1.5)]
        public void CompareToOfEquivalentValuesIsZero(double x, double y)
        {
            // reference: double.CompareTo treats NaN as equal to itself, and +0 as equal to -0
            Float128<TAccelerator> a = x;
            Float128<TAccelerator> b = y;

            Assert.Equal(x.CompareTo(y), a.CompareTo(b));
        }

        #endregion

        #region Equals(object)

        [Fact]
        public void EqualsSameTypeIsTrue()
        {
            object other = Float128<TAccelerator>.One;

            Assert.True(Float128<TAccelerator>.One.Equals(other));
        }

        [Fact]
        public void EqualsOtherTypeIsFalse()
        {
            object other = "not a Float128";

            Assert.False(Float128<TAccelerator>.One.Equals(other));
        }

        #endregion
    }

    public class ComparisonTests_DefaultAccelerator :
        ComparisonTests<DefaultAccelerator> { }

    public class ComparisonTests_SoftwareAccelerator :
        ComparisonTests<SoftwareAccelerator> { }
}
