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

        [Fact]
        public void ExpOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void Exp2OfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp2(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void Exp10OfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp10(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void Exp10OfLargeValueIsPositiveInfinity()
        {
            // 10^(1e30) overflows binary128
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp10((Float128<TAccelerator>)1e30)));
        }

        [Fact]
        public void Exp10OfLargeNegativeValueIsPositiveZero()
        {
            // 10^(-1e30) underflows binary128
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp10((Float128<TAccelerator>)(-1e30));

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        private static readonly Float128<TAccelerator> _relativeTolerance = Float128<TAccelerator>.Parse("1E-32");

        [Fact]
        public void ExpOfOneIsEulerNumberTo30Digits()
        {
            // reference: e (Euler's number), known constant
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("2.718281828459045235360287471352662497757247093");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Exp(Float128<TAccelerator>.One), _relativeTolerance);
        }

        [Fact]
        public void ExpOfNegativeOneIsInverseEulerNumberTo30Digits()
        {
            // reference: 1/e, known constant
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("0.367879441171442321595523770161460867445811131");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("-1")), _relativeTolerance);
        }

        [Fact]
        public void ExpOfTenIsReferenceValueTo30Digits()
        {
            // reference: e^10, known constant
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("22026.46579480671651695790064528424");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("10")), _relativeTolerance);
        }

        [Theory]
        [InlineData("-16.98046875")]
        [InlineData("-0.5")]
        [InlineData("0.25")]
        [InlineData("2.5")]
        [InlineData("11.75")]
        public void ExpOfLogReturnsInputAfterRoundTripTo30Digits(string x)
        {
            Float128<TAccelerator> input = Float128<TAccelerator>.Parse(x);
            Float128<TAccelerator> roundTrip = Float128<TAccelerator>.Log(Float128<TAccelerator>.Exp(input));
            AssertX.RelativelyEqual(input, roundTrip, _relativeTolerance);
        }

        [Fact]
        public void ExpOfOverflowBoundaryIsPositiveInfinity()
        {
            // exp(11358) overflows binary128
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp((Float128<TAccelerator>)11358)));
        }

        [Fact]
        public void ExpJustBelowOverflowBoundaryIsFinite()
        {
            // exp(11356) is finite
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp((Float128<TAccelerator>)11356);
            Assert.True(Float128<TAccelerator>.IsFinite(result));
        }

        [Fact]
        public void ExpOfUnderflowBoundaryIsPositiveZero()
        {
            // exp(-11435) underflows binary128
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp((Float128<TAccelerator>)(-11435));
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void ExpJustAboveUnderflowBoundaryIsNonzero()
        {
            // exp(-11433) is nonzero
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp((Float128<TAccelerator>)(-11433));
            Assert.False(Float128<TAccelerator>.IsZero(result));
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
