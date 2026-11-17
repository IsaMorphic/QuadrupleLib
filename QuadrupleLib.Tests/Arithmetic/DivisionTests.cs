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
    public abstract class DivisionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByNaNIsNaN(double x)
        {
            Assert.True(Float128<TAccelerator>.IsNaN(x / Float128<TAccelerator>.NaN));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByNegativeInfinityIsZero(double x)
        {
            Assert.True(Float128<TAccelerator>.IsZero(x / Float128<TAccelerator>.NegativeInfinity));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByNegativeOneIsNegate(double x)
        {
            Assert.Equal(-x, x / Float128<TAccelerator>.NegativeOne);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByPositiveInfinityIsZero(double x)
        {
            Assert.True(Float128<TAccelerator>.IsZero(x / Float128<TAccelerator>.PositiveInfinity));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByOneIsIdentity(double x)
        {
            Assert.Equal(x, x / Float128<TAccelerator>.One);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideByZeroIsInfinity(double x)
        {
            Assert.True(Float128<TAccelerator>.IsInfinity(x / Float128<TAccelerator>.Zero));
        }

        [Theory]
        [InlineData(-5.5)]
        [InlineData(-10.0)]
        [InlineData(-30.655)]
        [InlineData(-3.33)]
        [InlineData(5.5)]
        [InlineData(10.0)]
        [InlineData(30.655)]
        [InlineData(3.33)]
        public void DivideBySelfIsOne(double x)
        {
            Assert.Equal(Float128<TAccelerator>.One, (Float128<TAccelerator>)x / x);
        }

        [Theory]
        [InlineData([3.0, 1.5, 2.0])]
        [InlineData([-8.0, 2.0, -4.0])]
        [InlineData([3.0, -0.5, -6.0])]
        [InlineData([-5.0, -0.625, 8.0])]
        public void DivideGeneralIsCorrect(double x, double y, double z)
        {
            Assert.Equal(z, (Float128<TAccelerator>)x / y);
        }

        [Theory]
        [InlineData(-5.5)]
        [InlineData(-10.0)]
        [InlineData(-30.655)]
        [InlineData(-3.33)]
        [InlineData(5.5)]
        [InlineData(10.0)]
        [InlineData(30.655)]
        [InlineData(3.33)]
        public void DivideByBigNumberIsZero(double x)
        {
            // the divisor overflows to +Inf, so the quotient is a zero with the sign of x
            Float128<TAccelerator> y = x / Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, short.MaxValue / 2 + 1);
            Assert.True(Float128<TAccelerator>.IsZero(y));
            Assert.Equal(x < 0, Float128<TAccelerator>.IsNegative(y));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void DivideNormalBySubnormalIsInfinity(double x)
        {
            Assert.True(Float128<TAccelerator>.IsInfinity(x / Float128<TAccelerator>.Epsilon));
        }

        [Fact]
        public void DivideSubnormalIsCorrect()
        {
            Float128<TAccelerator> twoEps = Float128<TAccelerator>.BitIncrement(Float128<TAccelerator>.Epsilon);
            Assert.Equal(Float128<TAccelerator>.Epsilon, twoEps / 2.0);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void NegateIsDivideByNegativeOne(double x)
        {
            Assert.Equal(x / Float128<TAccelerator>.NegativeOne, -x);
        }

        [Fact]
        public void PositiveInfinityDividedByPositiveInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity / Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void PositiveInfinityDividedByNegativeInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.PositiveInfinity / Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void NegativeInfinityDividedByNegativeInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.NegativeInfinity / Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void NegativeInfinityDividedByPositiveInfinityIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.NegativeInfinity / Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void PositiveInfinityDividedByZeroIsPositiveInfinity()
        {
            // IEEE 754: an infinity divided by a zero is an infinity, with no invalid operation
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.PositiveInfinity / Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void NegativeInfinityDividedByZeroIsNegativeInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.NegativeInfinity / Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void ZeroDividedByZeroIsPositiveNaN()
        {
            // IEEE 754: zero divided by zero is an invalid operation; this library then returns Float128.NaN, whose sign bit is clear
            Float128<TAccelerator> result = Float128<TAccelerator>.Zero / Float128<TAccelerator>.Zero;
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.3)]
        [InlineData(0.33)]
        public void PositiveInfinityDividedByPositiveIsPositiveInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.PositiveInfinity / x);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.3)]
        [InlineData(0.33)]
        public void NegativeInfinityDividedByPositiveIsNegativeInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.NegativeInfinity / x);
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(-1.0)]
        [InlineData(-2.3)]
        [InlineData(-0.33)]
        public void PositiveInfinityDividedByNegativeIsNegativeInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.PositiveInfinity / x);
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(-1.0)]
        [InlineData(-2.3)]
        [InlineData(-0.33)]
        public void NegativeInfinityDividedByNegativeIsPositiveInfinity(double x)
        {
            Assert.Equal(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.NegativeInfinity / x);
        }

        [Fact]
        public void OneDividedByNegativeInfinityIsNegativeZero()
        {
            // IEEE 754: a finite nonzero x divided by an infinity is a zero whose sign is the XOR of the operand signs
            Float128<TAccelerator> result = Float128<TAccelerator>.One / Float128<TAccelerator>.NegativeInfinity;

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void NegativeOneDividedByPositiveInfinityIsNegativeZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.NegativeOne / Float128<TAccelerator>.PositiveInfinity;

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void DivideSmallestSubnormalByZeroIsPositiveInfinity()
        {
            // IEEE 754: a finite nonzero dividend divided by a zero is an infinity with the XOR of the signs
            Float128<TAccelerator> smallestSubnormal = RawStorage<TAccelerator>.FromBits(UInt128.One);

            Float128<TAccelerator> result = smallestSubnormal / Float128<TAccelerator>.Zero;

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(result));
        }

        [Theory]
        [InlineData(2.0)]
        [InlineData(0.0)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NaN)]
        public void PositiveInfinityRemainderIsPositiveNaN(double y)
        {
            // IEEE 754: the remainder of an infinity is an invalid operation; this library then returns Float128.NaN, whose sign bit is clear
            Float128<TAccelerator> result = Float128<TAccelerator>.PositiveInfinity % y;

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void SmallestSubnormalRemainderZeroIsNaN()
        {
            // IEEE 754: the remainder with a zero divisor is an invalid operation, so it returns NaN rather than throwing
            Float128<TAccelerator> smallestSubnormal = RawStorage<TAccelerator>.FromBits(UInt128.One);

            Float128<TAccelerator> result = smallestSubnormal % Float128<TAccelerator>.Zero;

            Assert.True(Float128<TAccelerator>.IsNaN(result));
        }
    }

    public class DivisionTests_DefaultAccelerator :
        DivisionTests<DefaultAccelerator>
    { }

    public class DivisionTests_SoftwareAccelerator :
        DivisionTests<SoftwareAccelerator>
    { }
}
