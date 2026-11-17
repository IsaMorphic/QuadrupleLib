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
using Xunit;

namespace QuadrupleLib.Tests.Storage
{
    public abstract class ClassificationTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Fact]
        public void ZeroIsNotPowerOfTwo()
        {
            // zero is not 2^k for any integer k
            Assert.False(Float128<TAccelerator>.IsPow2(Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void PositiveInfinityIsNotPowerOfTwo()
        {
            // infinity is not a finite power of two
            Assert.False(Float128<TAccelerator>.IsPow2(Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void NegativeInfinityIsNotPowerOfTwo()
        {
            // infinity is not a finite power of two
            Assert.False(Float128<TAccelerator>.IsPow2(Float128<TAccelerator>.NegativeInfinity));
        }
        [Fact]
        public void NaNIsNotPowerOfTwo()
        {
            Assert.False(Float128<TAccelerator>.IsPow2(Float128<TAccelerator>.NaN));
        }

        [Fact]
        public void PositiveInfinityIsNotNormal()
        {
            // IEEE 754: a normal number has an exponent field that is neither all zeros nor all ones
            Assert.False(Float128<TAccelerator>.IsNormal(Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void NaNIsNotNormal()
        {
            Assert.False(Float128<TAccelerator>.IsNormal(Float128<TAccelerator>.NaN));
        }

        [Fact]
        public void ZeroIsNotSubnormal()
        {
            // IEEE 754: a subnormal number has a zero exponent field and a nonzero fraction
            Assert.False(Float128<TAccelerator>.IsSubnormal(Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void NegativeZeroIsNotSubnormal()
        {
            Assert.False(Float128<TAccelerator>.IsSubnormal(-Float128<TAccelerator>.Zero));
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(1.5)]
        [InlineData(-2.0)]
        public void FiniteValueIsCanonical(double x)
        {
            // every binary interchange format encoding is canonical (IEEE 754-2019)
            Assert.True(Float128<TAccelerator>.IsCanonical((Float128<TAccelerator>)x));
        }

        [Fact]
        public void ZeroAndNegativeZeroHashCodeIsEqual()
        {
            // +0 and -0 compare equal, so equal values must hash equally (.NET Equals and GetHashCode contract)
            Assert.True(Float128<TAccelerator>.Zero == -Float128<TAccelerator>.Zero);
            Assert.Equal(Float128<TAccelerator>.Zero.GetHashCode(), (-Float128<TAccelerator>.Zero).GetHashCode());
        }
    }

    public class ClassificationTests_DefaultAccelerator :
        ClassificationTests<DefaultAccelerator> { }

    public class ClassificationTests_SoftwareAccelerator :
        ClassificationTests<SoftwareAccelerator> { }
}
