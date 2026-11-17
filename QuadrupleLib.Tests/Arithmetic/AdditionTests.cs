/*
 *  Copyright 2025-2026 Chosen Few Software
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

namespace QuadrupleLib.Tests.Arithmetic
{
    public abstract class AdditionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        [InlineData(double.NaN)]
        public void AddNaNIsNaN(double x)
        {
            Assert.True(Float128<TAccelerator>.IsNaN(x + Float128<TAccelerator>.NaN));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void AddNegativeInfinityIsNegativeInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, x + Float128<TAccelerator>.NegativeInfinity);
        }

        [Theory]
        [InlineData([0.5, 1.5])]
        [InlineData([1.0, 2.0])]
        [InlineData([-1.0, 0.0])]
        public void AddOneIsCorrect(double x, double y)
        {
            Assert.Equal(y, x + Float128<TAccelerator>.One);
        }

        [Theory]
        [InlineData([0.5, -0.5])]
        [InlineData([1.0, 0.0])]
        [InlineData([-1.0, -2.0])]
        public void AddNegativeOneIsCorrect(double x, double y)
        {
            Assert.Equal(y, x + Float128<TAccelerator>.NegativeOne);
        }

        [Theory]
        [InlineData([0.5, 1.5, 2.0])]
        [InlineData([1.0, 2.0, 3.0])]
        [InlineData([-1.0, 3.5, 2.5])]
        public void AddNormalIsCorrect(double x, double y, double z)
        {
            Assert.Equal(z, (Float128<TAccelerator>)x + y);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void AddPositiveInfinityIsPositiveInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, x + Float128<TAccelerator>.PositiveInfinity);
        }

        [Fact]
        public void AddSubnormalIsSubnormal()
        {
            Assert.True(Float128<TAccelerator>.IsSubnormal(Float128<TAccelerator>.Epsilon + Float128<TAccelerator>.Epsilon));
        }

        [Fact]
        public void AddSubnormalIsCorrect()
        {
            Assert.Equal(Float128<TAccelerator>.Zero, -Float128<TAccelerator>.Epsilon + Float128<TAccelerator>.Epsilon);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void AddSubnormalIsIdentity(double x)
        {
            Assert.Equal(x, x + Float128<TAccelerator>.Epsilon);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void AddZeroIsIdentity(double x)
        {
            Assert.Equal(x, x + Float128<TAccelerator>.Zero);
        }

        [Theory]
        [InlineData([0.5, 1.5])]
        [InlineData([1.0, 2.0])]
        [InlineData([-1.0, 0.0])]
        public void IncrementIsCorrect(double x, double y)
        {
            Float128<TAccelerator> x_inc = (Float128<TAccelerator>)x;
            Assert.Equal(y, ++x_inc);
        }

        [Fact]
        public void NegativeInfinityPlusNegativeInfinityIsNegativeInfinity()
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.NegativeInfinity + Float128<TAccelerator>.NegativeInfinity);
        }

        [Fact]
        public void PositiveInfinityPlusNegativeInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity + Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void PositiveInfinityPlusPositiveInfinityIsPositiveInfinity()
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.PositiveInfinity + Float128<TAccelerator>.PositiveInfinity);
        }

        [Fact]
        public void ZeroPlusZeroIsZero()
        {
            Assert.Equal(Float128<TAccelerator>.Zero, Float128<TAccelerator>.Zero + Float128<TAccelerator>.Zero);
        }

        [Fact]
        public void NegativeZeroPlusZeroIsNegativeZero()
        {
            Assert.Equal(Float128<TAccelerator>.NegativeZero, Float128<TAccelerator>.NegativeZero + Float128<TAccelerator>.Zero);
        }

        [Fact]
        public void OverflowingSumIsInfinity()
        {
            Float128<TAccelerator> max = Float128<TAccelerator>.ScaleB((Float128<TAccelerator>)(UInt128.MaxValue >> 15), short.MaxValue >> 1);
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, max + max);
        }

        [Fact]
        public void UnderflowingSumIsSubnormal()
        {
            Float128<TAccelerator> min = Float128<TAccelerator>.ScaleB(1.0, -(short.MaxValue >> 1) + 1);
            Float128<TAccelerator> half = Float128<TAccelerator>.ScaleB(-0.5, -(short.MaxValue >> 1) + 1);
            Assert.True(Float128<TAccelerator>.IsSubnormal(min + half));
        }

        [Fact]
        public void PositiveInfinityPlusNaNIsNaN()
        {
            // IEEE 754: an operation with a NaN operand returns NaN, whichever operand is the NaN
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity + Float128<TAccelerator>.NaN));
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.NaN + Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void SubnormalSumIsExact()
        {
            // 2^-16382 - 2^-16383 = 2^-16383, the subnormal with fraction 2^111
            Float128<TAccelerator> smallestNormal = RawStorage<TAccelerator>.FromBits(UInt128.One << 112);
            Float128<TAccelerator> negativeHalf = RawStorage<TAccelerator>.FromBits((UInt128.One << 127) | (UInt128.One << 111));

            Float128<TAccelerator> sum = smallestNormal + negativeHalf;

            Assert.Equal(UInt128.One << 111, RawStorage<TAccelerator>.Bits(sum));
        }

        [Fact]
        public void OverflowingSumIsPositiveInfinity()
        {
            Float128<TAccelerator> largest = RawStorage<TAccelerator>.LargestFinite();

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(largest + largest));
        }

        [Fact]
        public void NegativeOverflowingSumIsNegativeInfinity()
        {
            Float128<TAccelerator> largest = RawStorage<TAccelerator>.LargestFinite();

            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(-largest + -largest));
        }

        [Fact]
        public void TieSumIsRoundedToEven()
        {
            // reference: 1 + 2^-112 + 2^-113 is halfway between 1 + 2^-112 and 1 + 2 * 2^-112; the even one is chosen
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | UInt128.One);
            Float128<TAccelerator> halfUlp = RawStorage<TAccelerator>.FromBits((UInt128)16270 << 112);

            Float128<TAccelerator> sum = one + halfUlp;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000002"), RawStorage<TAccelerator>.Bits(sum));
        }

        [Fact]
        public void JustAboveTieSumIsRoundedUp()
        {
            // reference: 1 + 1.5 * 2^-113 = 1 + 3 * 2^-114 lies above the tie, so the result is 1 + 2^-112
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits((UInt128)16383 << 112);
            Float128<TAccelerator> addend = RawStorage<TAccelerator>.FromBits(((UInt128)16270 << 112) | (UInt128.One << 111));

            Float128<TAccelerator> sum = one + addend;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000001"), RawStorage<TAccelerator>.Bits(sum));
        }

        [Fact]
        public void SumJustAboveHalfUlpIsRoundedUp()
        {
            // reference: 1 + 1.75 * 2^-113 = 1 + 7 * 2^-115 is above the half ulp, so the result is 1 + 2^-112
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits((UInt128)16383 << 112);
            Float128<TAccelerator> addend = RawStorage<TAccelerator>.FromBits(((UInt128)16270 << 112) | ((UInt128)3 << 110));

            Float128<TAccelerator> sum = one + addend;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000001"), RawStorage<TAccelerator>.Bits(sum));
        }

        [Fact]
        public void SumAboveTieWithTinyTailIsRoundedUp()
        {
            // reference: 1 + (2^-113 + 2^-200) is above the half ulp, so the result is 1 + 2^-112
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits((UInt128)16383 << 112);
            Float128<TAccelerator> addend = RawStorage<TAccelerator>.FromBits(((UInt128)16270 << 112) | (UInt128.One << 25));

            Float128<TAccelerator> sum = one + addend;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000001"), RawStorage<TAccelerator>.Bits(sum));
        }
    }

    public class AdditionTests_DefaultAccelerator :
        AdditionTests<DefaultAccelerator>
    { }

    public class AdditionTests_SoftwareAccelerator :
        AdditionTests<SoftwareAccelerator>
    { }
}