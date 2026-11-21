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
        public void IsILogBCorrect(double x)
        {
            double y = double.Exp2(x);

            int n = (int)System.Math.Floor(x);
            Assert.Equal(n, Float128<TAccelerator>.ILogB(y));
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

        [Theory]
        [InlineData(-1.0, 2.0)]
        [InlineData(-1.0, 0.5)]
        [InlineData(1.0, 0.0)]
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
            // IEEE 754: the logarithm of zero is negative infinity, for every base above 1 (and the base-2 and base-10 forms)
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log(0.0));
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log2(0.0));
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log10(0.0));
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.Log(0.0, 2.0));
        }

        [Fact]
        public void LogOfZeroWithBaseBelowOneIsPositiveInfinity()
        {
            // log_b(0) = ln(0) / ln(b), and ln(b) is negative for 0 < b < 1
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.Log(0.0, 0.5));
        }

        [Fact]
        public void LogOfOneWithBaseOneIsNaN()
        {
            // ln(1) / ln(1) is 0 / 0; matches System.Math.Log(1, 1), which is NaN
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

        [Fact]
        public void LogWithPositiveInfinityBaseIsNaNExceptForOne()
        {
            // matches System.Math.Log(a, +Infinity): NaN for a != 1, and 0 for a = 1
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Log(3.0, Float128<TAccelerator>.PositiveInfinity)));
            Assert.Equal(Float128<TAccelerator>.Zero, Float128<TAccelerator>.Log(1.0, Float128<TAccelerator>.PositiveInfinity));
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

        private static readonly Float128<TAccelerator> _relativeTolerance = Float128<TAccelerator>.Parse("1E-32");

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
        public void LogMatchesReferenceValueTo30Digits(string x, string expected)
        {
            // reference: Python decimal module at 70 significant digits; 3 and 1.5 have mantissas above sqrt(2), so the normalisation step recentres them
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse(expected), Float128<TAccelerator>.Log(Float128<TAccelerator>.Parse(x)), _relativeTolerance);
        }

        [Theory]
        [InlineData("0.5")]
        [InlineData("1.5")]
        [InlineData("2.0")]
        [InlineData("10.0")]
        [InlineData("0.1")]
        [InlineData("100.0")]
        public void ExpOfLogReturnsInputAfterRoundTripTo30Digits(string x)
        {
            Float128<TAccelerator> input = Float128<TAccelerator>.Parse(x);
            Float128<TAccelerator> roundTrip = Float128<TAccelerator>.Exp(Float128<TAccelerator>.Log(input));
            AssertX.RelativelyEqual(input, roundTrip, _relativeTolerance);
        }

        [Fact]
        public void LogOfOnePlusTinyIsTinyTo30Digits()
        {
            // ln(1 + 2^-60): reference: Python decimal module at 70 significant digits; relative accuracy must hold near 1
            Float128<TAccelerator> input = Float128<TAccelerator>.One + Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -60);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("8.67361737988403546829804048432821366808139E-19");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Fact]
        public void LogOfOneMinusTinyIsTinyTo30Digits()
        {
            // ln(1 - 2^-60): reference: Python decimal module at 70 significant digits; relative accuracy must hold on the other side of 1
            Float128<TAccelerator> input = Float128<TAccelerator>.One - Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -60);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("-8.67361737988403547582120432959085371908130E-19");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Fact]
        public void LogOfTwoToMinusOneThousandMatchesReferenceValueTo30Digits()
        {
            // reference: ln(2^-1000) = -1000*ln(2); checked with Python decimal module at 70 significant digits
            Float128<TAccelerator> input = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -1000);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("-6.93147180559945309417232121458176568075500E+2");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Fact]
        public void LogOfSmallestSubnormalMatchesReferenceValueTo30Digits()
        {
            // 2^-16494 is the smallest positive subnormal; reference: ln(2^-16494) = -16494*ln(2); checked with Python decimal module at 70 significant digits
            Float128<TAccelerator> input = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -16494);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("-1.14327695961557379335278266113311643138372E+4");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Fact]
        public void LogOfMaxValueMatchesReferenceValueTo30Digits()
        {
            // MaxValue = (2 - 2^-112) * 2^16383, built exactly from representable parts; reference: Python decimal module at 70 significant digits
            Float128<TAccelerator> mantissa = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 1) - Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -112);
            Float128<TAccelerator> input = Float128<TAccelerator>.ScaleB(mantissa, 16383);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("1.13565234062941439494919310779707648912526E+4");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(input), _relativeTolerance);
        }

        [Theory]
        [InlineData("3", "7")]
        [InlineData("0.3", "0.7")]
        [InlineData("1E-200", "1E+100")]
        public void LogOfProductMatchesSumOfLogs(string a, string b)
        {
            Float128<TAccelerator> pa = Float128<TAccelerator>.Parse(a);
            Float128<TAccelerator> pb = Float128<TAccelerator>.Parse(b);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Log(pa) + Float128<TAccelerator>.Log(pb), Float128<TAccelerator>.Log(pa * pb), _relativeTolerance);
        }
    }

    public class LogarithmTests_DefaultAccelerator :
        LogarithmTests<DefaultAccelerator>
    { }

    public class LogarithmTests_SoftwareAccelerator :
        LogarithmTests<SoftwareAccelerator>
    { }
}
