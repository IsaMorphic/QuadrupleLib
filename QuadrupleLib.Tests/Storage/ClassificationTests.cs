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
    }

    public class ClassificationTests_DefaultAccelerator :
        ClassificationTests<DefaultAccelerator> { }

    public class ClassificationTests_SoftwareAccelerator :
        ClassificationTests<SoftwareAccelerator> { }
}
