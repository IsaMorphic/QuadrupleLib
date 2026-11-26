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
    public abstract class HyperbolicTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        private static readonly Float128<TAccelerator> _relativeTolerance = Float128<TAccelerator>.Parse("1E-32");
        private static readonly Float128<TAccelerator> _doubleTolerance = Float128<TAccelerator>.Parse("1E-15");

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsSinhCorrect(double x)
        {
            double y = double.Sinh(x);
            AssertX.RelativelyEqual(y, Float128<TAccelerator>.Sinh(x), _doubleTolerance);
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
            AssertX.RelativelyEqual(y, Float128<TAccelerator>.Cosh(x), _doubleTolerance);
        }

        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void SinhOfAsinhIsInputAfterRoundTripTo30Digits(double x)
        {
            // sinh(asinh x) = x
            Float128<TAccelerator> input = (Float128<TAccelerator>)x;
            AssertX.RelativelyEqual(input, Float128<TAccelerator>.Sinh(Float128<TAccelerator>.Asinh(input)), _relativeTolerance);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.345)]
        [InlineData(4.5678)]
        public void CoshOfAcoshIsInputAfterRoundTripTo30Digits(double x)
        {
            // cosh(acosh x) = x for x >= 1
            Float128<TAccelerator> input = (Float128<TAccelerator>)x;
            AssertX.RelativelyEqual(input, Float128<TAccelerator>.Cosh(Float128<TAccelerator>.Acosh(input)), _relativeTolerance);
        }

        [Theory]
        [InlineData(0.345)]
        [InlineData(-0.5678)]
        public void TanhOfAtanhIsInputAfterRoundTripTo30Digits(double x)
        {
            // tanh(atanh x) = x for |x| < 1
            Float128<TAccelerator> input = (Float128<TAccelerator>)x;
            AssertX.RelativelyEqual(input, Float128<TAccelerator>.Tanh(Float128<TAccelerator>.Atanh(input)), _relativeTolerance);
        }

        [Fact]
        public void CoshOfZeroIsOne()
        {
            // cosh(+/-0) = 1
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Cosh(Float128<TAccelerator>.Zero));
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Cosh(Float128<TAccelerator>.NegativeZero));
        }

        [Fact]
        public void SinhOfHalfIsReferenceValueWithinTwoUlps()
        {
            // reference: sinh(0.5), 60 significant digits (checked with Python mpmath)
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("0.521095305493747361622425626411491559105928982611480527946094");
            AssertX.WithinUlps(expected, Float128<TAccelerator>.Sinh(Float128<TAccelerator>.Parse("0.5")), 2);
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
            AssertX.RelativelyEqual(y, Float128<TAccelerator>.Tanh(x), _doubleTolerance);
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
            AssertX.RelativelyEqual(y, Float128<TAccelerator>.Asinh(x), _doubleTolerance);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.345)]
        [InlineData(4.5678)]
        public void IsInverseCoshCorrect(double x)
        {
            double y = double.Acosh(x);
            AssertX.RelativelyEqual(y, Float128<TAccelerator>.Acosh(x), _doubleTolerance);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.5678)]
        public void IsInverseCoshNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Acosh(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
        }

        [Theory]
        [InlineData(0.345)]
        [InlineData(-.5678)]
        public void IsInverseTanhCorrect(double x)
        {
            double y = double.Atanh(x);
            AssertX.RelativelyEqual(y, Float128<TAccelerator>.Atanh(x), _doubleTolerance);
        }

        [Theory]
        [InlineData(1.345)]
        [InlineData(-1.5678)]
        public void IsInverseTanhNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Atanh(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        public void InverseTanhPoleIsInfinity(double x)
        {
            // IEEE 754 and C99: atanh(+/-1) is +/-Inf
            Float128<TAccelerator> y = Float128<TAccelerator>.Atanh(x);

            Assert.True(Float128<TAccelerator>.IsInfinity(y));
            Assert.Equal(x < 0, Float128<TAccelerator>.IsNegative(y));
        }

        [Fact]
        public void AcoshOfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Acosh(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void AcoshOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Acosh(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void AcoshOfLargeValueIsCorrect()
        {
            // reference: 9001 ln 2, ln 2 = 0.6931471805599453094172321214581765680755... (OEIS A002162)
            Float128<TAccelerator> x = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 9000);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("6239.0177722200677300645063252450472892475767");

            AssertX.WithinUlps(expected, Float128<TAccelerator>.Acosh(x), 2);
        }

        [Fact]
        public void AcoshOfPowerOfTwoIsReferenceValue()
        {
            // reference: 101 ln 2, ln 2 = 0.6931471805599453094172321214581765680755...
            Float128<TAccelerator> x = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 100);

            AssertX.WithinUlps(Float128<TAccelerator>.Parse("70.0078652365544762511404442672758333756255"), Float128<TAccelerator>.Acosh(x), 2);
        }

        [Fact]
        public void AsinhOfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Asinh(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void AsinhOfNegativeInfinityIsNegativeInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Asinh(Float128<TAccelerator>.NegativeInfinity)));
        }

        [Fact]
        public void AsinhOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Asinh(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void AsinhOfLargeValueIsCorrect()
        {
            // reference: 9001 ln 2, ln 2 = 0.6931471805599453094172321214581765680755... (OEIS A002162)
            Float128<TAccelerator> x = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 9000);
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("6239.0177722200677300645063252450472892475767");

            AssertX.WithinUlps(expected, Float128<TAccelerator>.Asinh(x), 2);
        }

        [Fact]
        public void AsinhOfPowerOfTwoIsReferenceValue()
        {
            // reference: 101 ln 2, ln 2 = 0.6931471805599453094172321214581765680755...
            Float128<TAccelerator> x = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 100);

            AssertX.WithinUlps(Float128<TAccelerator>.Parse("70.0078652365544762511404442672758333756255"), Float128<TAccelerator>.Asinh(x), 2);
        }

        [Fact]
        public void AtanhOfOneIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Atanh(Float128<TAccelerator>.One)));
        }

        [Fact]
        public void AtanhOfNegativeOneIsNegativeInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Atanh(Float128<TAccelerator>.NegativeOne)));
        }

        [Fact]
        public void AtanhOfZeroIsZero()
        {
            // IEEE 754: atanh(0) is 0
            Float128<TAccelerator> result = Float128<TAccelerator>.Atanh(Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void AtanhOfSmallArgumentIsArgument()
        {
            // atanh(x) rounds to x below the small-argument cutoff
            Float128<TAccelerator> tiny = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -57);
            Float128<TAccelerator> subnormal = RawStorage<TAccelerator>.FromBits((UInt128)1);
            Assert.Equal(RawStorage<TAccelerator>.Bits(tiny), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atanh(tiny)));
            Assert.Equal(RawStorage<TAccelerator>.Bits(subnormal), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atanh(subnormal)));
        }

        [Theory]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        public void AtanhOutsideUnitIntervalIsPositiveNaN(double x)
        {
            // invalid operation: NaN with sign bit clear
            Float128<TAccelerator> result = Float128<TAccelerator>.Atanh((Float128<TAccelerator>)x);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void AtanhOfPositiveInfinityIsPositiveNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Atanh(Float128<TAccelerator>.PositiveInfinity);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void CoshOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Cosh(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void SinhOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Sinh(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void TanhOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Tanh(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void AtanhOfNaNIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Atanh(Float128<TAccelerator>.NaN);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void SinhOfNegativeZeroIsNegativeZero()
        {
            // IEEE 754 and C99: sinh(-0) is -0
            Float128<TAccelerator> result = Float128<TAccelerator>.Sinh(-Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void TanhOfNegativeZeroIsNegativeZero()
        {
            // IEEE 754 and C99: tanh(-0) is -0
            Float128<TAccelerator> result = Float128<TAccelerator>.Tanh(-Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void AsinhOfNegativeZeroIsNegativeZero()
        {
            // IEEE 754 and C99: asinh(-0) is -0
            Float128<TAccelerator> result = Float128<TAccelerator>.Asinh(-Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void AsinhOfSmallArgumentIsArgument()
        {
            // asinh(x) rounds to x below the small-argument cutoff
            Float128<TAccelerator> tiny = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -57);
            Float128<TAccelerator> subnormal = RawStorage<TAccelerator>.FromBits((UInt128)1);
            Assert.Equal(RawStorage<TAccelerator>.Bits(tiny), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Asinh(tiny)));
            Assert.Equal(RawStorage<TAccelerator>.Bits(subnormal), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Asinh(subnormal)));
        }

        [Fact]
        public void CoshOfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Cosh(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void CoshOfNegativeInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Cosh(Float128<TAccelerator>.NegativeInfinity)));
        }

        [Fact]
        public void SinhOfPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Sinh(Float128<TAccelerator>.PositiveInfinity)));
        }

        [Fact]
        public void SinhOfNegativeInfinityIsNegativeInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Sinh(Float128<TAccelerator>.NegativeInfinity)));
        }

        [Fact]
        public void TanhOfPositiveInfinityIsOne()
        {
            // tanh(+Inf) is exactly 1
            Assert.Equal(1.0, (double)Float128<TAccelerator>.Tanh(Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void TanhOfNegativeInfinityIsNegativeOne()
        {
            Assert.Equal(-1.0, (double)Float128<TAccelerator>.Tanh(Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void SinhAndCoshOfHugeValueIsInfinite()
        {
            Float128<TAccelerator> x = Float128<TAccelerator>.Parse("8.94E+4931");
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Sinh(x)));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Cosh(x)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Sinh(-x)));
        }

        [Fact]
        public void SinhAndCoshAboveExponentOverflowIsFinite()
        {
            // reference: ln(cosh x) = ln(sinh x) = x - ln 2 for large x
            Float128<TAccelerator> x = Float128<TAccelerator>.Parse("11356.75");
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("11356.05685281944005469058276787854182343192449986563974");
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(Float128<TAccelerator>.Cosh(x)), _relativeTolerance);
            AssertX.RelativelyEqual(expected, Float128<TAccelerator>.Log(Float128<TAccelerator>.Sinh(x)), _relativeTolerance);
        }

        [Fact]
        public void SinhAndCoshAtHalfExponentialThresholdIsReferenceValue()
        {
            // threshold 45
            Float128<TAccelerator> at = Float128<TAccelerator>.Parse("45");
            // reference: 45 - ln 2, ln 2 = 0.6931471805599453094172321214581765680755...
            Float128<TAccelerator> expected = Float128<TAccelerator>.Parse("44.30685281944005469058276787854182343192449986563974");
            AssertX.WithinUlps(expected, Float128<TAccelerator>.Log(Float128<TAccelerator>.Cosh(at)), 2);
            AssertX.WithinUlps(expected, Float128<TAccelerator>.Log(Float128<TAccelerator>.Sinh(at)), 2);
        }

        [Fact]
        public void SinhAndTanhAtSmallArgumentThresholdIsWithinOneUlp()
        {
            // reference: sinh and tanh round to x at 2^-57
            Float128<TAccelerator> at = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -57);
            Float128<TAccelerator> below = at - Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -170);
            AssertX.WithinUlps(below, Float128<TAccelerator>.Sinh(below), 1);
            AssertX.WithinUlps(at, Float128<TAccelerator>.Sinh(at), 1);
            AssertX.WithinUlps(below, Float128<TAccelerator>.Tanh(below), 1);
            AssertX.WithinUlps(at, Float128<TAccelerator>.Tanh(at), 1);
        }

        [Fact]
        public void TanhOfPowerOfTwoIsRoundedToPredecessor()
        {
            // reference: tanh(2^-56) = 2^-56 - 0.667 * 2^-169, rounds to 2^-56 - 2^-169
            Float128<TAccelerator> x = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -56);
            Float128<TAccelerator> expected = x - Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -169);
            Assert.Equal(expected, Float128<TAccelerator>.Tanh(x));
        }

        [Theory]
        [InlineData("1", "0.881373587019543025232609324979792309028")]
        [InlineData("-1", "-0.881373587019543025232609324979792309028")]
        [InlineData("0.5", "0.48121182505960344749775891342436842313518")]
        public void AsinhOfReferenceInputIsReferenceValue(string x, string expected)
        {
            // reference: asinh(1) = ln(1 + sqrt 2), asinh(0.5) = ln(golden ratio)
            AssertX.WithinUlps(Float128<TAccelerator>.Parse(expected), Float128<TAccelerator>.Asinh(Float128<TAccelerator>.Parse(x)), 2);
        }

        [Fact]
        public void AcoshOfOneIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Acosh(Float128<TAccelerator>.One);

            Assert.Equal(Float128<TAccelerator>.Zero, result);
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void AcoshOfTwoIsReferenceValue()
        {
            // reference: acosh(2) = ln(2 + sqrt 3)
            AssertX.WithinUlps(Float128<TAccelerator>.Parse("1.31695789692481670862504634730796844402698197146751"), Float128<TAccelerator>.Acosh((Float128<TAccelerator>)2.0), 2);
        }

        [Fact]
        public void AtanhOfHalfIsReferenceValue()
        {
            // reference: atanh(1/2) = ln(3) / 2
            AssertX.WithinUlps(Float128<TAccelerator>.Parse("0.54930614433405484569762261846126285232374527891137"), Float128<TAccelerator>.Atanh((Float128<TAccelerator>)0.5), 2);
        }

        [Fact]
        public void TanhOfOneIsReferenceValue()
        {
            // reference: tanh(1)
            AssertX.WithinUlps(Float128<TAccelerator>.Parse("0.7615941559557648881194582826047935904127"), Float128<TAccelerator>.Tanh(Float128<TAccelerator>.One), 2);
        }

        [Fact]
        public void TanhOfLargeArgumentIsOne()
        {
            // reference: 1 - 2e^(-2x) at 44.5, below half the spacing under 1
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Tanh((Float128<TAccelerator>)44.5));
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Tanh((Float128<TAccelerator>)45));
        }

        [Fact]
        public void TanhOfNegativeLargeArgumentIsNegativeOne()
        {
            Assert.Equal(Float128<TAccelerator>.NegativeOne, Float128<TAccelerator>.Tanh((Float128<TAccelerator>)(-44.5)));
            Assert.Equal(Float128<TAccelerator>.NegativeOne, Float128<TAccelerator>.Tanh((Float128<TAccelerator>)(-45)));
        }
    }

    public class HyperbolicTests_DefaultAccelerator :
        HyperbolicTests<DefaultAccelerator>
    { }

    public class HyperbolicTests_SoftwareAccelerator :
        HyperbolicTests<SoftwareAccelerator>
    { }
}
