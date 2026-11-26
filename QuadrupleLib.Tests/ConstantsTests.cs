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

namespace QuadrupleLib.Tests
{
    public abstract class ConstantsTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Fact]
        public void RadixIsTwo()
        {
            // IEEE 754 binary128 is a base-2 format
            Assert.Equal(2, Float128<TAccelerator>.Radix);
        }

        [Fact]
        public void AdditiveIdentityIsZero()
        {
            Assert.Equal(Float128<TAccelerator>.Zero, Float128<TAccelerator>.AdditiveIdentity);
        }

        [Fact]
        public void MultiplicativeIdentityIsOne()
        {
            Assert.Equal(Float128<TAccelerator>.One, Float128<TAccelerator>.MultiplicativeIdentity);
        }

        [Fact]
        public void EIsReferenceValue()
        {
            // reference: e to 300 digits, rounded to binary128 (Python decimal, exact series)
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("40005BF0A8B1457695355FB8AC404E7A"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.E));
        }

        [Fact]
        public void TauIsReferenceValue()
        {
            // reference: 2 * pi to 300 digits, rounded to binary128 (Python decimal, Machin's formula); equals Pi with exponent + 1
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("4001921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Tau));
        }
    }

    public class ConstantsTests_DefaultAccelerator :
        ConstantsTests<DefaultAccelerator> { }

    public class ConstantsTests_SoftwareAccelerator :
        ConstantsTests<SoftwareAccelerator> { }
}
