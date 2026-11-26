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
        private static readonly Float128<TAccelerator> _relativeTolerance = Float128<TAccelerator>.Parse("1E-32");

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsILogBCorrect(double x)
        {
            double y = double.Exp2(x);

            int n = (int)System.Math.Floor(x);
            Assert.Equal(n, Float128<TAccelerator>.ILogB(y));
        }

        [Theory]
        [InlineData(-8.0, 3)]
        [InlineData(-0.125, -3)]
        public void ILogBOfNegativeIsExponentOfMagnitude(double x, int n)
        {
            Assert.Equal(n, Float128<TAccelerator>.ILogB(x));
        }

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsLogCorrect(double x)
        {
            double y = double.Exp(x);
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Log(y), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(-1.0)]
        public void IsLogNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Log(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
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
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Log2(y), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(-1.0)]
        public void IsLog2NaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Log2(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
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
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Log10(y), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(-1.0)]
        public void IsLog10NaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Log10(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
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
            AssertX.NearlyEqual(x, Float128<TAccelerator>.Log(y, b), Precision.NearestTenThousandth);
        }

        // Log(y, newBase) domain: see BREAKING_CHANGES.md §1
        [Theory]
        [InlineData(-1.0, 2.0)]
        [InlineData(-1.0, 0.5)]
        [InlineData(2.0, 0.0)]
        [InlineData(3.0, 0.0)]
        [InlineData(1.0, -1.0)]
        [InlineData(2.0, -1.0)]
        [InlineData(3.0, -1.0)]
        [InlineData(0.5, 1.0)]
        [InlineData(2.0, 1.0)]
        [InlineData(3.0, 1.0)]
        public void IsLogBNaN(double x, double b)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Log(x, b);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Fact]
        public void LogOfZeroIsNegativeInfinity()
        {
            // zero maps to -inf for every base
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log(0.0));
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log2(0.0));
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log10(0.0));
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log(0.0, 2.0));
        }

        [Fact]
        public void LogOfNegativeZeroIsNegativeInfinity()
        {
            // log(-0) = -inf, as for +0 (IEEE 754 log(+-0) = -inf)
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log(Float128<TAccelerator>.NegativeZero));
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log2(Float128<TAccelerator>.NegativeZero));
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log10(Float128<TAccelerator>.NegativeZero));
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log(Float128<TAccelerator>.NegativeZero, 2.0));
        }

        [Fact]
        public void LogOfZeroWithBaseBelowOneIsPositiveInfinity()
        {
            // ln(0) / ln(b) with ln(b) < 0
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.Log(0.0, 0.5));
        }

        [Fact]
        public void LogOfOneWithBaseOneIsNaN()
        {
            // 0 / 0 (ln 1 / ln 1); System.Math.Log(1, 1) is NaN
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Log(1.0, 1.0)));
        }

        [Fact]
        public void LogOfPositiveInfinityWithPositiveInfinityBaseIsNaN()
        {
            // ln(+inf) / ln(+inf) is inf / inf
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Log(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void LogOfZeroWithPositiveInfinityBaseIsNaN()
        {
            // ln(0) / ln(+inf) is -inf / inf
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Log(0.0, Float128<TAccelerator>.PositiveInfinity)));
        }

        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(0.0, -2.0)]
        [InlineData(-0.0, -0.5)]
        public void LogOfZeroWithNonPositiveBaseIsNaN(double y, double newBase)
        {
            // a base of zero or below is a domain error, even when y is zero
            Float128<TAccelerator> result = Float128<TAccelerator>.Log(y, newBase);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void LogWithPositiveInfinityBaseIsNaNExceptForOne()
        {
            // System.Math.Log(a, +inf) is NaN for a != 1
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Log(3.0, Float128<TAccelerator>.PositiveInfinity)));
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Log(0.5, Float128<TAccelerator>.PositiveInfinity)));

            // ln(1) = +0, divided by +inf
            Float128<TAccelerator> one = Float128<TAccelerator>.Log(1.0, Float128<TAccelerator>.PositiveInfinity);
            Assert.True(Float128<TAccelerator>.IsZero(one));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(one));
        }

        [Fact]
        public void Log2OfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Log2(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void LogOfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Log(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void Log10OfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Log10(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void Log2OfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Log2(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void LogOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Log(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void LogWithNaNOperandIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Log(Float128<TAccelerator>.NaN, 2.0)));
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Log(2.0, Float128<TAccelerator>.NaN)));
        }

        [Fact]
        public void LogOfOneIsZero()
        {
            // IEEE 754: log_b(1) is 0 for any valid base
            Float128<TAccelerator> result = Float128<TAccelerator>.Log(1.0, 2.0);

            Assert.True(Float128<TAccelerator>.IsZero(result));
        }

        [Fact]
        public void LogOfPositiveInfinityWithBaseTwoIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Log(Float128<TAccelerator>.PositiveInfinity, 2.0)));
        }

        [Fact]
        public void LogOfPositiveInfinityWithBaseHalfIsNegativeInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Log(Float128<TAccelerator>.PositiveInfinity, 0.5)));
        }

        [Fact]
        public void Log10OfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Log10(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void Log10OfOneIsZero()
        {
            // IEEE 754: log10(1) is 0
            Float128<TAccelerator> result = Float128<TAccelerator>.Log10(1.0);

            Assert.True(Float128<TAccelerator>.IsZero(result));
        }

        [Fact]
        public void LogOfTwoIsReferenceValueTo30Digits()
        {
            // reference: ln(2), known constant
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("0.693147180559945309417232121458176568075500134");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(Float128<TAccelerator>.Parse("2")), _relativeTolerance);
        }

        [Fact]
        public void LogOfTenIsReferenceValueTo30Digits()
        {
            // reference: ln(10), known constant
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("2.302585092994045684017991454684364207601101488");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(Float128<TAccelerator>.Parse("10")), _relativeTolerance);
        }

        [Fact]
        public void LogOfHalfIsNegativeLogOfTwoTo30Digits()
        {
            // reference: -ln(2), known constant
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("-0.693147180559945309417232121458176568075500134");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(Float128<TAccelerator>.Parse("0.5")), _relativeTolerance);
        }

        [Fact]
        public void LogOfEulerNumberIsOneTo30Digits()
        {
            // reference: e (Euler's number) to 45 digits (mathematical constant)
            Float128<TAccelerator> e = Float128<TAccelerator>.Parse("2.718281828459045235360287471352662497757247093");
            AssertX.RelativelyEqual(Float128<TAccelerator>.One, Float128<TAccelerator>.Log(e), _relativeTolerance);
        }

        [Theory]
        [InlineData("3", "1.09861228866810969139524523692252570464749E+0")]
        [InlineData("1.5", "4.05465108108164381978013115464349136571990E-1")]
        public void LogOfReferenceValuesIsWithinTwoUlpsOfReference(string x, string expected)
        {
            // reference: 42 significant digits (checked with Python mpmath)
            AssertX.WithinUlps(Float128<TAccelerator>.Parse(expected), Float128<TAccelerator>.Log(Float128<TAccelerator>.Parse(x)), 2);
        }

        [Fact]
        public void LogOfOneWithBaseBelowOneIsNegativeZero()
        {
            // +0 divided by a negative ln(0.5) gives -0
            Float128<TAccelerator> result = Float128<TAccelerator>.Log(Float128<TAccelerator>.One, Float128<TAccelerator>.Parse("0.5"));
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void LogOfOneWithZeroBaseIsNegativeZero()
        {
            // ln(1) = +0 divided by ln(+-0) = -inf gives -0, as System.Math.Log(1, +-0)
            Float128<TAccelerator> one = Float128<TAccelerator>.One;

            Float128<TAccelerator> positiveZeroBase = Float128<TAccelerator>.Log(one, Float128<TAccelerator>.Zero);
            Assert.True(Float128<TAccelerator>.IsZero(positiveZeroBase));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(positiveZeroBase));

            Float128<TAccelerator> negativeZeroBase = Float128<TAccelerator>.Log(one, Float128<TAccelerator>.NegativeZero);
            Assert.True(Float128<TAccelerator>.IsZero(negativeZeroBase));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(negativeZeroBase));
        }

        [Fact]
        public void LogOfOneIsPositiveZero()
        {
            // IEEE 754: ln(1) is +0
            Float128<TAccelerator> result = Float128<TAccelerator>.Log(Float128<TAccelerator>.One);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Theory]
        [InlineData("0.5")]
        [InlineData("1.5")]
        [InlineData("2.0")]
        [InlineData("10.0")]
        [InlineData("0.1")]
        [InlineData("100.0")]
        public void ExpOfLogIsReturnedAfterRoundTripTo30Digits(string x)
        {
            Float128<TAccelerator> input = Float128<TAccelerator>.Parse(x);
            Float128<TAccelerator> roundTrip = Float128<TAccelerator>.Exp(Float128<TAccelerator>.Log(input));
            AssertX.RelativelyEqual(input, roundTrip, _relativeTolerance);
        }

        [Fact]
        public void LogOfOnePlusTinyIsTinyTo30Digits()
        {
            // reference: ln(1 + 2^-60), 42 significant digits
            Float128<TAccelerator> input = Float128<TAccelerator>.One + Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -60);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("8.67361737988403546829804048432821366808139E-19");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Fact]
        public void LogOfOneMinusTinyIsTinyTo30Digits()
        {
            // reference: ln(1 - 2^-60), 42 significant digits (Python mpmath)
            Float128<TAccelerator> input = Float128<TAccelerator>.One - Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -60);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("-8.67361737988403547582120432959085371908131E-19");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Fact]
        public void LogOfTwoToMinusOneThousandIsReferenceValueTo30Digits()
        {
            // reference: ln(2^-1000) = -1000*ln(2), 42 significant digits (Python mpmath)
            Float128<TAccelerator> input = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -1000);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("-6.93147180559945309417232121458176568075500E+2");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Fact]
        public void LogOfTwoToOneThousandIsWithinOneUlpOfReference()
        {
            // reference: 1000 ln 2 (42 significant digits, as above), within 1 ulp
            Float128<TAccelerator> input = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 1000);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("6.93147180559945309417232121458176568075500E+2");
            AssertX.WithinUlps(expected, Float128<TAccelerator>.Log(input), 1);
        }

        [Fact]
        public void LogOfSmallestSubnormalIsReferenceValueTo30Digits()
        {
            // 2^-16494 is the smallest positive subnormal; reference: ln(2^-16494) = -16494*ln(2), 42 significant digits (Python mpmath)
            Float128<TAccelerator> input = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -16494);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("-1.14327695961557379335278266113311643138373E+4");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Fact]
        public void LogOfMaxValueIsReferenceValueTo30Digits()
        {
            // MaxValue = (2 - 2^-112) * 2^16383, built exactly from representable parts; reference: 42 significant digits (Python decimal, 110-digit arithmetic)
            Float128<TAccelerator> mantissa = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 1) - Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -112);
            Float128<TAccelerator> input = Float128<TAccelerator>.ScaleB(mantissa, 16383);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("1.13565234062941439494919310779707648912527E+4");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Theory]
        [InlineData("2", "8", "2.772588722239781237668928485832706272302")]
        [InlineData("0.5", "4", "0.6931471805599453094172321214581765680755")]
        [InlineData("1E-200", "1E+100", "-230.2585092994045684017991454684364207601")]
        public void LogOfProductIsSumOfLogs(string a, string b, string expected)
        {
            // reference: ln(a * b) = ln(a) + ln(b) to 40 significant digits; ln 2 = 0.6931471805599453094172321214581765680755, ln 10 = 2.302585092994045684017991454684364207601
            Float128<TAccelerator> pa = Float128<TAccelerator>.Parse(a);
            Float128<TAccelerator> pb = Float128<TAccelerator>.Parse(b);
            Float128<TAccelerator> reference = Float128<TAccelerator>.Parse(expected);
            AssertX.RelativelyEqual(reference, Float128<TAccelerator>.Log(pa) + Float128<TAccelerator>.Log(pb), _relativeTolerance);
            AssertX.RelativelyEqual(reference, Float128<TAccelerator>.Log(pa * pb), _relativeTolerance);
        }
    }

    public class LogarithmTests_DefaultAccelerator :
        LogarithmTests<DefaultAccelerator>
    { }

    public class LogarithmTests_SoftwareAccelerator :
        LogarithmTests<SoftwareAccelerator>
    { }
}
