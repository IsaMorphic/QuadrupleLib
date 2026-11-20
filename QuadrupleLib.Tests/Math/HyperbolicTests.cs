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
        [Theory]
        [InlineData(-0.288)]
        [InlineData(0.5)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(-4.65)]
        public void IsSinhCorrect(double x)
        {
            double y = double.Sinh(x);
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Sinh(x), Precision.NearestTenThousandth);
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
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Cosh(x), Precision.NearestTenThousandth);
        }

        [Fact]
        public void CoshOfZeroIsOne()
        {
            // cosh(0) = (e^0 + e^-0) / 2 = 1 exactly, for both signed zeros
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Cosh(Float128<TAccelerator>.Zero));
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.Cosh(Float128<TAccelerator>.NegativeZero));
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
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Tanh(x), Precision.NearestTenThousandth);
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
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Asinh(x), Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.345)]
        [InlineData(4.5678)]
        public void IsInverseCoshCorrect(double x)
        {
            double y = double.Acosh(x);
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Acosh(x), Precision.NearestTenThousandth);
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
            AssertX.NearlyEqual(y, Float128<TAccelerator>.Atanh(x), Precision.NearestTenThousandth);
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
            // IEEE 754 and C99: atanh(+/-1) is a pole, so the result is +/-Inf
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
            // acosh(2^9000) is ln(2 * 2^9000) = 9001 * ln(2)
            Float128<TAccelerator> x = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 9000);

            Assert.Equal(9001 * double.Log(2.0), (double)Float128<TAccelerator>.Acosh(x), 1e-9);
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
            // asinh(2^9000) is ln(2 * 2^9000) = 9001 * ln(2)
            Float128<TAccelerator> x = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 9000);

            Assert.Equal(9001 * double.Log(2.0), (double)Float128<TAccelerator>.Asinh(x), 1e-9);
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

        [Theory]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        public void AtanhOutsideUnitIntervalIsPositiveNaN(double x)
        {
            // an invalid operation returns Float128.NaN, which has the sign bit clear
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
    }

    public class HyperbolicTests_DefaultAccelerator :
        HyperbolicTests<DefaultAccelerator>
    { }

    public class HyperbolicTests_SoftwareAccelerator :
        HyperbolicTests<SoftwareAccelerator>
    { }
}
