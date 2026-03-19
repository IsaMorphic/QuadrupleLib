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

namespace QuadrupleLib.Tests.Utilities
{
    public abstract class AcceleratorTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(1UL, 0UL, 0UL, 0UL)]
        [InlineData(1UL, 1UL, 1UL, 0UL)]
        [InlineData(1UL, 2UL, 2UL, 0UL)]
        [InlineData(4UL, 1UL << 63, 0UL, 2UL)]
        public void IsBigMulCorrect(ulong a, ulong b, ulong expectedLo, ulong expectedHi) 
        {
            ulong actualHi = TAccelerator.BigMul(a, b, out ulong actualLo);
            Assert.Equal((expectedLo, expectedHi), (actualLo, actualHi));
        }

        [Theory]
        [InlineData(2UL, 0UL, 1UL, 0UL, 2UL, 0UL)]
        [InlineData(2UL, 0UL, 2UL, 0UL, 1UL, 0UL)]
        [InlineData(0UL, 4UL, 2UL, 0UL, 0UL, 2UL)]
        [InlineData(0UL, 4UL, 3UL, 0UL, 6148914691236517205UL, 1UL)]
        public void IsDivRemQuotientCorrect(ulong aLo, ulong aHi, ulong bLo, ulong bHi, ulong expectedLo, ulong expectedHi) 
        {
            UInt128 a = aLo | ((UInt128)aHi << 64);
            UInt128 b = bLo | ((UInt128)bHi << 64);
            UInt128 expected = expectedLo | ((UInt128)expectedHi << 64);

            (UInt128 actual, UInt128 _) = TAccelerator.DivRem(a, b);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(2UL, 0UL, 1UL, 0UL, 0UL, 0UL)]
        [InlineData(2UL, 0UL, 2UL, 0UL, 0UL, 0UL)]
        [InlineData(0UL, 4UL, 2UL, 0UL, 0UL, 0UL)]
        [InlineData(0UL, 4UL, 3UL, 0UL, 1UL, 0UL)]
        public void IsDivRemRemainderCorrect(ulong aLo, ulong aHi, ulong bLo, ulong bHi, ulong expectedLo, ulong expectedHi)
        {
            UInt128 a = aLo | ((UInt128)aHi << 64);
            UInt128 b = bLo | ((UInt128)bHi << 64);
            UInt128 expected = expectedLo | ((UInt128)expectedHi << 64);

            (UInt128 _, UInt128 actual) = TAccelerator.DivRem(a, b);
            Assert.Equal(expected, actual);
        }
    }

    public class AcceleratorTests_DefaultAccelerator : 
        AcceleratorTests<DefaultAccelerator> { }

    public class AcceleratorTests_SoftwareAccelerator :
        AcceleratorTests<SoftwareAccelerator>
    { }
}
