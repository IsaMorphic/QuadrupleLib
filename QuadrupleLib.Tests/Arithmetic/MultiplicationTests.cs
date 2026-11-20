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
    public abstract class MultiplicationTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void MultiplyByNaNIsNaN(double x)
        {
            Assert.True(Float128<TAccelerator>.IsNaN(x * Float128<TAccelerator>.NaN));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(0.33)]
        public void MultiplyPositiveByNegativeInfinityIsNegativeInfinity(double x)
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(x * Float128<TAccelerator>.NegativeInfinity));
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(-1.0)]
        [InlineData(-0.33)]
        public void MultiplyNegativeByNegativeInfinityIsPositiveInfinity(double x)
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(x * Float128<TAccelerator>.NegativeInfinity));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(0.33)]
        public void MultiplyPositiveByPositiveInfinityIsPositiveInfinity(double x)
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(x * Float128<TAccelerator>.PositiveInfinity));
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(-1.0)]
        [InlineData(-0.33)]
        public void MultiplyNegativeByPositiveInfinityIsNegativeInfinity(double x)
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(x * Float128<TAccelerator>.PositiveInfinity));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void MultiplyByNegativeOneIsNegate(double x)
        {
            Assert.Equal(-x, x * Float128<TAccelerator>.NegativeOne);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void MultiplyByPositiveInfinityIsInfinity(double x)
        {
            Assert.True(Float128<TAccelerator>.IsInfinity(x * Float128<TAccelerator>.PositiveInfinity));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void MultiplyByOneIsIdentity(double x)
        {
            Assert.Equal(x, x * Float128<TAccelerator>.One);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void MultiplyByZeroIsZero(double x)
        {
            Assert.Equal(Float128<TAccelerator>.Zero, x * Float128<TAccelerator>.Zero);
        }

        [Theory]
        [InlineData([0.25, 1.5, 0.375])]
        [InlineData([-8.0, 2.0, -16.0])]
        [InlineData([3.0, -0.5, -1.5])]
        [InlineData([-5.0, -0.625, 3.125])]
        public void MultiplyGeneralIsCorrect(double x, double y, double z)
        {
            Assert.Equal(z, (Float128<TAccelerator>)x * y);
        }

        [Theory]
        [InlineData(-5.5)]
        [InlineData(-10.0)]
        [InlineData(-30.655)]
        [InlineData(-3.33)]
        public void MultiplyNegativeByBigNumberIsNegativeInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, x * Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, short.MaxValue / 2));
        }

        [Theory]
        [InlineData(1.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(1.33)]
        [InlineData(0.75)]
        public void MultiplyNormalBySubnormalIsSubnormal(double x)
        {
            Assert.True(Float128<TAccelerator>.IsSubnormal(x * Float128<TAccelerator>.Epsilon));
        }

        [Theory]
        [InlineData(5.5)]
        [InlineData(10.0)]
        [InlineData(30.655)]
        [InlineData(3.33)]
        public void MultiplyPositiveByBigNumberIsPositiveInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, x * Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, short.MaxValue / 2));
        }

        [Fact]
        public void MultiplySubnormalIsCorrect()
        {
            Float128<TAccelerator> twoEps = Float128<TAccelerator>.BitIncrement(Float128<TAccelerator>.Epsilon);
            Assert.Equal(twoEps, Float128<TAccelerator>.Epsilon * 2.0);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void NegateIsMultiplyByNegativeOne(double x)
        {
            Assert.Equal(x * Float128<TAccelerator>.NegativeOne, -x);
        }

        [Fact]
        public void MultiplyPositiveInfinityByPositiveInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.PositiveInfinity * Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void MultiplyNegativeInfinityByNegativeInfinityIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.NegativeInfinity * Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void MultiplyNegativeInfinityByPositiveInfinityIsNegativeInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.NegativeInfinity * Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void PositiveInfinityTimesZeroIsNaN()
        {
            // IEEE 754: infinity times zero is an invalid operation and returns NaN
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity * Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void ZeroTimesNegativeInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Zero * Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void FusedMultiplyAddWithInfiniteProductIsPositiveInfinity()
        {
            // the product +Inf * 2 is +Inf, and +Inf + 1 is +Inf
            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd(Float128<TAccelerator>.PositiveInfinity, (Float128<TAccelerator>)2.0, Float128<TAccelerator>.One);

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(result));
        }

        [Fact]
        public void FusedMultiplyAddWithNegativeInfiniteProductIsNegativeInfinity()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd(Float128<TAccelerator>.NegativeInfinity, (Float128<TAccelerator>)2.0, Float128<TAccelerator>.One);

            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(result));
        }

        [Fact]
        public void FusedMultiplyAddWithZeroTimesInfinityIsPositiveNaN()
        {
            // IEEE 754: inf * 0 is an invalid operation; this library then returns Float128.NaN, whose sign bit is clear
            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.Zero, Float128<TAccelerator>.One);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void FusedMultiplyAddWithCancellingInfiniteProductIsPositiveNaN()
        {
            // IEEE 754: (+Inf) + (-Inf) is an invalid operation; this library then returns Float128.NaN, whose sign bit is clear
            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd(Float128<TAccelerator>.PositiveInfinity, (Float128<TAccelerator>)2.0, Float128<TAccelerator>.NegativeInfinity);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void FusedMultiplyAddWithNaNOperandIsNaN()
        {
            // IEEE 754: any NaN operand makes the result NaN
            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd(Float128<TAccelerator>.NaN, (Float128<TAccelerator>)1.0, (Float128<TAccelerator>)1.0);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FusedMultiplyAddWithInfiniteAddendIsAddend(bool positive)
        {
            // IEEE 754: a finite product plus an infinite addend is the addend
            Float128<TAccelerator> addend = positive ? Float128<TAccelerator>.PositiveInfinity : Float128<TAccelerator>.NegativeInfinity;

            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd((Float128<TAccelerator>)1.0, (Float128<TAccelerator>)2.0, addend);

            Assert.True(positive ? Float128<TAccelerator>.IsPositiveInfinity(result) : Float128<TAccelerator>.IsNegativeInfinity(result));
        }

        [Fact]
        public void UnderflowingProductByHalfIsSubnormal()
        {
            // 2^-16382 * 0.5 = 2^-16383, the subnormal with fraction 2^111
            Float128<TAccelerator> smallestNormal = RawStorage<TAccelerator>.FromBits(UInt128.One << 112);

            Float128<TAccelerator> product = smallestNormal * (Float128<TAccelerator>)0.5;

            Assert.Equal(UInt128.One << 111, RawStorage<TAccelerator>.Bits(product));
        }

        [Fact]
        public void UnderflowingProductByQuarterIsSubnormal()
        {
            // 2^-16382 * 0.25 = 2^-16384, the subnormal with fraction 2^110
            Float128<TAccelerator> smallestNormal = RawStorage<TAccelerator>.FromBits(UInt128.One << 112);

            Float128<TAccelerator> product = smallestNormal * (Float128<TAccelerator>)0.25;

            Assert.Equal(UInt128.One << 110, RawStorage<TAccelerator>.Bits(product));
        }

        [Fact]
        public void OverflowingProductIsPositiveInfinity()
        {
            Float128<TAccelerator> largest = RawStorage<TAccelerator>.LargestFinite();

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(largest * (Float128<TAccelerator>)1.5));
        }

        [Fact]
        public void NegativeOverflowingProductIsNegativeInfinity()
        {
            Float128<TAccelerator> largest = RawStorage<TAccelerator>.LargestFinite();

            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(-largest * (Float128<TAccelerator>)1.5));
        }

        [Theory]
        [InlineData(2.0, 3.0, "40018000000000000000000000000000")]
        [InlineData(3.0, 5.0, "4002E000000000000000000000000000")]
        [InlineData(-2.0, 3.0, "C0018000000000000000000000000000")]
        [InlineData(0.5, 0.25, "3FFC0000000000000000000000000000")]
        public void FusedMultiplyAddZeroAddendIsExactProduct(double x, double y, string expected)
        {
            // reference: the exact products 6, 15, -6 and 1/8 are representable, so the result is the product itself
            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd(x, y, Float128<TAccelerator>.Zero);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits(expected), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void FusedMultiplyAddInexactProductWithZeroAddendIsRoundedProduct()
        {
            // reference: 15 * 2^-16498 is 15/16 of the smallest subnormal 2^-16494, so round-to-nearest gives 2^-16494
            Float128<TAccelerator> product = RawStorage<TAccelerator>.FromBits(15);

            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd(product, (Float128<TAccelerator>)0.0625, Float128<TAccelerator>.Zero);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("00000000000000000000000000000001"), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void TieProductIsRoundedToEven()
        {
            // reference: 1.5 * (1 + 2^-112) = 1.5 + 1.5 * 2^-112 is halfway between two binary128 values,
            // and ties-to-even chooses the one with the even significand
            Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | UInt128.One);

            Float128<TAccelerator> result = x * (Float128<TAccelerator>)1.5;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF8000000000000000000000000002"), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void FusedMultiplyAddCancelledProductIsTwoToTheMinus224()
        {
            // reference: (1 + 2^-112)^2 - (1 + 2^-111) = 2^-224 exactly, so the single rounding is exact
            Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | UInt128.One);
            Float128<TAccelerator> addend = RawStorage<TAccelerator>.FromBits((UInt128.One << 127) | ((UInt128)16383 << 112) | (UInt128)2);

            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd(x, x, addend);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3F1F0000000000000000000000000000"), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void FusedMultiplyAddCancelledProductIsThreeTimesTwoToTheMinus224()
        {
            // reference: (1 + 2^-112)(1 + 3 * 2^-112) - (1 + 2^-110) = 3 * 2^-224 exactly
            Float128<TAccelerator> x = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | UInt128.One);
            Float128<TAccelerator> y = RawStorage<TAccelerator>.FromBits(((UInt128)16383 << 112) | (UInt128)3);
            Float128<TAccelerator> addend = RawStorage<TAccelerator>.FromBits((UInt128.One << 127) | ((UInt128)16383 << 112) | (UInt128)4);

            Float128<TAccelerator> result = Float128<TAccelerator>.FusedMultiplyAdd(x, y, addend);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3F208000000000000000000000000000"), RawStorage<TAccelerator>.Bits(result));
        }
    }

    public class MultiplicationTests_DefaultAccelerator :
        MultiplicationTests<DefaultAccelerator>
    { }

    public class MultiplicationTests_SoftwareAccelerator :
        MultiplicationTests<SoftwareAccelerator>
    { }
}