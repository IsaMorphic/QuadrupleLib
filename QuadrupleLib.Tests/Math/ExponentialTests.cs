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

        [Fact]
        public void Exp2OfPositiveInfinityIsPositiveInfinity()
        {
            // IEEE 754: exp2(+Inf) is +Inf
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp2(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void Exp2OfNegativeInfinityIsPositiveZero()
        {
            // IEEE 754: exp2(-Inf) is +0
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp2(Float128<TAccelerator>.NegativeInfinity);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }
    }

    public class ExponentialTests_DefaultAccelerator :
        ExponentialTests<DefaultAccelerator>
    { }

    public class ExponentialTests_SoftwareAccelerator :
        ExponentialTests<SoftwareAccelerator>
    { }

    // Every test in this class can loop without end on the current code. The timeout stops the test
    // but not its thread, so the class runs under the Hang category.
    public abstract class ExponentialHangTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Trait("Category", "Hang")]
        [Fact(Timeout = 10000)]
        public void ExpOfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Trait("Category", "Hang")]
        [Fact(Timeout = 10000)]
        public void ExpOfNegativeInfinityIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp(Float128<TAccelerator>.NegativeInfinity);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Trait("Category", "Hang")]
        [Fact(Timeout = 10000)]
        public void ExpOfLargeFiniteIsPositiveInfinity()
        {
            // exp(1e30) overflows binary128
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp((Float128<TAccelerator>)1e30)));
        }

        [Trait("Category", "Hang")]
        [Fact(Timeout = 10000)]
        public void Exp10OfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp10(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Trait("Category", "Hang")]
        [Fact(Timeout = 10000)]
        public void Exp10OfNegativeInfinityIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp10(Float128<TAccelerator>.NegativeInfinity);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }
    }

    public class ExponentialHangTests_DefaultAccelerator :
        ExponentialHangTests<DefaultAccelerator>
    { }

    public class ExponentialHangTests_SoftwareAccelerator :
        ExponentialHangTests<SoftwareAccelerator>
    { }
}
