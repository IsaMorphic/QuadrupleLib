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
        private static readonly Float128<TAccelerator> _relativeTolerance = Float128<TAccelerator>.Parse("1E-32");
        private static readonly Float128<TAccelerator> _doubleTolerance = Float128<TAccelerator>.Parse("1E-15");

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
            AssertX.RelativelyEqual(y, Float128<TAccelerator>.Exp(x), _doubleTolerance);
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
            AssertX.RelativelyEqual(y, Float128<TAccelerator>.Exp2(x), _doubleTolerance);
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
            AssertX.RelativelyEqual(y, Float128<TAccelerator>.Exp10(x), _doubleTolerance);
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
        public void Exp10OfHugeValueIsPositiveInfinity()
        {
            // 10^8.94e4931 exceeds the exponent range of binary128
            Float128<TAccelerator> x = Float128<TAccelerator>.Parse("8.94E+4931");
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp10(x)));
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
        public void LogOfExpRoundTripsToInputTo30Digits(string x)
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
        [InlineData("11356.52", "1.18568581423346597820797443929279097079028677E+4932")]
        [InlineData("11356.5234", "1.18972400703950504195530621222447780862599612E+4932")]
        public void ExpOfReferenceValuesIsWithinTwoUlpsOfReference(string x, string expected)
        {
            // reference: exp of the binary128-rounded argument, 42 to 45 significant digits (checked with Python mpmath)
            AssertX.WithinUlps(Float128<TAccelerator>.Parse(expected), Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse(x)), 2);
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
        [InlineData("0.3", "0.7", "2.718281828459045235360287471352662497757247093")]
        [InlineData("-5.25", "5.25", "1")]
        [InlineData("10.5", "-10.5", "1")]
        public void ExpOfSumIsProductOfExps(string a, string b, string expected)
        {
            // reference: exp(0) = 1 when a + b = 0; exp(1) = e (known constant) when a + b = 1
            Float128<TAccelerator> pa = Float128<TAccelerator>.Parse(a);
            Float128<TAccelerator> pb = Float128<TAccelerator>.Parse(b);
            Float128<TAccelerator> reference = Float128<TAccelerator>.Parse(expected);
            AssertX.RelativelyEqual(reference, Float128<TAccelerator>.Exp(pa) * Float128<TAccelerator>.Exp(pb), _relativeTolerance);
            AssertX.RelativelyEqual(reference, Float128<TAccelerator>.Exp(pa + pb), _relativeTolerance);
        }

        [Theory]
        [InlineData("-5.25", "2.125", "4.3936933623407417326747681701462713798517631625253E-2")]
        [InlineData("-400", "250", "7.1750959731644104198326929072089881884086882742441E-66")]
        [InlineData("10.5", "-3.25", "1.4081048482046955750200863270136378224200718445264E3")]
        public void ProductOfExpsOfMixedSignArgumentsIsReferenceOfSum(string a, string b, string expected)
        {
            // reference: exp of the exact sum (-3.125, -150, 7.25), 50 significant digits (checked with Python mpmath)
            Float128<TAccelerator> pa = Float128<TAccelerator>.Parse(a);
            Float128<TAccelerator> pb = Float128<TAccelerator>.Parse(b);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse(expected), Float128<TAccelerator>.Exp(pa) * Float128<TAccelerator>.Exp(pb), _relativeTolerance);
        }

        [Fact]
        public void ExpOfConsecutiveLargeArgumentsRatioIsEulerNumber()
        {
            // exp(11356) / exp(11355) = e; the exp values themselves have no reference here
            Float128<TAccelerator> ratio = Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("11356")) / Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("11355"));
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("2.718281828459045235360287471352662497757247093"), ratio, _relativeTolerance);
        }

        [Theory]
        [InlineData("-11370")]
        [InlineData("-11380")]
        public void ExpOfSubnormalResultRatioIsEulerNumber(string x)
        {
            // subnormal results: reduced precision
            Float128<TAccelerator> start = Float128<TAccelerator>.Parse(x);
            Float128<TAccelerator> ratio = Float128<TAccelerator>.Exp(start + Float128<TAccelerator>.One) / Float128<TAccelerator>.Exp(start);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("2.718281828459045235360287471352662497757247093"), ratio, Float128<TAccelerator>.Parse("1E-22"));
        }

        [Fact]
        public void ExpJustBelowOverflowIsFinite()
        {
            // overflow bound: ln(2^16384) = 16384 ln 2 = 11356.5234...
            Float128<TAccelerator> high = Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("11356.5"));
            Assert.True(Float128<TAccelerator>.IsFinite(high));
            Float128<TAccelerator> ratio = high / Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("11355.5"));
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("2.718281828459045235360287471352662497757247093"), ratio, _relativeTolerance);
        }

        [Theory]
        [InlineData(-16494.0)]
        [InlineData(-16382.0)]
        [InlineData(-1074.0)]
        [InlineData(-1.0)]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(100.0)]
        [InlineData(16383.0)]
        public void Exp2OfIntegerIsExactPowerOfTwo(double y)
        {
            // reference: 2^y is exact for integer y in range
            int exponent = (int)y;
            UInt128 bits = exponent >= -16382 ? (UInt128)(exponent + 16383) << 112 : UInt128.One << (exponent + 16494);
            Assert.Equal(RawStorage<TAccelerator>.FromBits(bits), Float128<TAccelerator>.Exp2(y));
        }

        [Fact]
        public void Exp2OfSubnormalResultIsCorrectlyRounded()
        {
            // reference: 2^43 * sqrt(2) rounds to 12439554047902
            Float128<TAccelerator> expected = RawStorage<TAccelerator>.FromBits((UInt128)12439554047902UL);
            Assert.Equal(expected, Float128<TAccelerator>.Exp2(Float128<TAccelerator>.Parse("-16450.5")));
        }

        [Fact]
        public void ExpOfArgumentRoundingToSmallestSubnormalIsSmallestSubnormal()
        {
            // reference: exp(-11433.3) = 0.588 * 2^-16494 rounds to 2^-16494
            Float128<TAccelerator> smallest = RawStorage<TAccelerator>.FromBits(UInt128.One);
            Assert.Equal(smallest, Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("-11433.3")));
        }

        [Fact]
        public void ExpOfArgumentBelowHalfSmallestSubnormalIsZero()
        {
            // exp(-11433.6) = 0.436 * 2^-16494, which rounds to zero
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("-11433.6"));
            Assert.Equal(Float128<TAccelerator>.Zero, result);
            Assert.False(Float128<TAccelerator>.IsNegative(result));

            Float128<TAccelerator> underflow = Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("-11434"));
            Assert.Equal(Float128<TAccelerator>.Zero, underflow);
            Assert.False(Float128<TAccelerator>.IsNegative(underflow));
        }

        [Fact]
        public void ExpJustAboveOverflowBoundaryIsPositiveInfinity()
        {
            // overflow bound: 16384 ln 2 = 11356.5234...
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("11356.6"))));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp(Float128<TAccelerator>.Parse("11356.53"))));
        }

        [Fact]
        public void ExpOfVeryLargeNegativeValueIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp((Float128<TAccelerator>)(-1e300));

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void Exp2OfVeryLargeValueIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp2((Float128<TAccelerator>)1e300)));
        }

        [Fact]
        public void Exp2OfVeryLargeNegativeValueIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp2((Float128<TAccelerator>)(-1e300));

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void Exp10OfVeryLargeValueIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Exp10((Float128<TAccelerator>)1e300)));
        }

        [Fact]
        public void Exp10OfVeryLargeNegativeValueIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Exp10((Float128<TAccelerator>)(-1e300));

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
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
