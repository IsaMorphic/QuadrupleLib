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
        public void ExpOfZeroIsOne()
        {
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Exp(Float128<TAccelerator>.Zero));
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Exp(Float128<TAccelerator>.NegativeZero));
        }

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
        [InlineData("-100")]
        [InlineData("700")]
        public void LogOfExpReturnsInputAfterRoundTripTo30Digits(string x)
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

        [Theory]
        [InlineData("3", "2.00855369231876677409285296545817178969879E+1")]
        [InlineData("-3.5", "3.01973834223185007397862923636198450716605E-2")]
        [InlineData("-20", "2.06115362243855782796594038015582097637580E-9")]
        [InlineData("-100", "3.72007597602083596295969580386311833735889E-44")]
        [InlineData("700", "1.01423205473500450945532959523126761520467E+304")]
        public void ExpMatchesReferenceValueTo30Digits(string x, string expected)
        {
            // reference: Python decimal module at 70 significant digits
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse(expected), Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse(x)), _relativeTolerance);
        }

        [Fact]
        public void ExpOfTinyArgumentIsOnePlusArgumentTo30Digits()
        {
            // exp(2^-100) = 1 + 2^-100 + O(2^-200); reference: Taylor series expansion
            Float128<TAccelerator> tiny = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -100);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("1.00000000000000000000000000000078886090522");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Exp(tiny), _relativeTolerance);
        }

        [Theory]
        [InlineData("-5.25", "2.125")]
        [InlineData("0.3", "0.7")]
        [InlineData("-400", "250")]
        [InlineData("10.5", "-3.25")]
        public void ExpOfSumMatchesProductOfExps(string a, string b)
        {
            Float128<TAccelerator> pa = Float128<TAccelerator>.Parse(a);
            Float128<TAccelerator> pb = Float128<TAccelerator>.Parse(b);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Exp(pa) * Float128<TAccelerator>.Exp(pb), Float128<TAccelerator>.Exp(pa + pb), _relativeTolerance);
        }

        [Fact]
        public void ExpOfLargeArgumentMatchesProductOfParts()
        {
            // exp(11356) = exp(11355) * exp(1); all three values are normal
            Float128<TAccelerator> expected = Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("11355")) * Float128<TAccelerator>.Exp(Float128<TAccelerator>.One);
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("11356")), _relativeTolerance);
        }

        [Theory]
        [InlineData("-11370")]
        [InlineData("-11380")]
        public void ExpOfSubnormalResultRatioIsEulerNumber(string x)
        {
            // results below 2^-16382 are subnormal and carry fewer significant bits, so this uses a looser tolerance
            Float128<TAccelerator> start = Float128<TAccelerator>.Parse(x);
            Float128<TAccelerator> ratio = Float128<TAccelerator>.Exp(start + Float128<TAccelerator>.One) / Float128<TAccelerator>.Exp(start);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("2.718281828459045235360287471352662497757247093"), ratio, Float128<TAccelerator>.Parse("1E-20"));
        }

        [Fact(Timeout = 10000)]
        public void ExpOfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact(Timeout = 10000)]
        public void ExpOfNegativeInfinityIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp(Float128<TAccelerator>.NegativeInfinity);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact(Timeout = 10000)]
        public void ExpOfLargeFiniteIsPositiveInfinity()
        {
            // exp(1e30) overflows binary128
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp((Float128<TAccelerator>)1e30)));
        }

        [Fact(Timeout = 10000)]
        public void Exp10OfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp10(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact(Timeout = 10000)]
        public void Exp10OfNegativeInfinityIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp10(Float128<TAccelerator>.NegativeInfinity);

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
}
