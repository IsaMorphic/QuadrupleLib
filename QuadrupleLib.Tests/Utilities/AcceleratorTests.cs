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
using QuadrupleLib.Utilities;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
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

            (UInt256 actual, UInt128 _) = TAccelerator.DivRem(a, b);
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

            (UInt256 _, UInt128 actual) = TAccelerator.DivRem(a, b);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(32)]
        [InlineData(64)]
        [InlineData(96)]
        public void IsDivRemRandomCorrect(int shiftAmt)
        {
            Random rng = new(0);
            Span<UInt128> operands = stackalloc UInt128[3];
            for (int i = 0; i < 100000; i++)
            {
                rng.NextBytes(MemoryMarshal.AsBytes(operands));
                (BigInteger eQ, BigInteger eR) = BigInteger.DivRem(
                    (((BigInteger)operands[0] << 128) | operands[1]) >> 32,
                    operands[2] >> shiftAmt);
                (BigInteger aQ, BigInteger aR) = TAccelerator.DivRem(
                    (((UInt256)operands[0] << 128) | operands[1]) >> 32,
                    operands[2] >> shiftAmt);
                Assert.Equal((eQ, eR), (aQ, aR));
            }
        }

        [Theory]
        [InlineData("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "1000000000000000000000003")]
        [InlineData("8000000000000000000000000000000000000000000000000000000000000000", "1000000000000000000000001")]
        [InlineData("1000000000000000000000000000000000000000000000000000000001234", "200000000000000000000000")]
        [InlineData("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", "10000000000000000000000000000000")]
        [InlineData("3000000000000000000000000000000000000000000000000000000000000000", "1000000000000000000000000000001")]
        public void IsDivRemWideDividendCorrect(string dividendHex, string divisorHex)
        {
            BigInteger dividend = BigInteger.Parse("0" + dividendHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            BigInteger divisor = BigInteger.Parse("0" + divisorHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            (BigInteger expectedQuotient, BigInteger expectedRemainder) = BigInteger.DivRem(dividend, divisor);

            (UInt256 quotient, UInt128 remainder) = TAccelerator.DivRem((UInt256)dividend, (UInt128)divisor);

            Assert.Equal((expectedQuotient, expectedRemainder), ((BigInteger)quotient, (BigInteger)remainder));
        }

        // Reference: UInt128.DivRem on these operands gives quotient 1 and remainder REMAINDER,
        // and 1 * DIVISOR + REMAINDER equals DIVIDEND.
        private static readonly UInt128 DIVIDEND = UInt128.Parse("283838025373803181643844235218533528156", CultureInfo.InvariantCulture);
        private static readonly UInt128 DIVISOR = UInt128.Parse("217316732702658059057889766104822010038", CultureInfo.InvariantCulture);
        private static readonly UInt128 REMAINDER = UInt128.Parse("66521292671145122585954469113711518118", CultureInfo.InvariantCulture);

        [Fact]
        public void WideDivisorQuotientIsOne()
        {
            (UInt256 quotient, UInt128 _) = TAccelerator.DivRem(DIVIDEND, DIVISOR);

            Assert.Equal(UInt256.One, quotient);
        }

        [Fact]
        public void WideDivisorRemainderIsCorrect()
        {
            (UInt256 _, UInt128 remainder) = TAccelerator.DivRem(DIVIDEND, DIVISOR);

            Assert.Equal(REMAINDER, remainder);
        }

        [Fact]
        public void IsDivRemFullWidthRandomCorrect()
        {
            Random rng = new(0);
            byte[] dividendBytes = new byte[32];
            byte[] divisorBytes = new byte[16];
            for (int i = 0; i < 100000; i++)
            {
                rng.NextBytes(dividendBytes);
                rng.NextBytes(divisorBytes);
                // vary the divisor width from 1 to 128 bits
                divisorBytes[rng.Next(16)] = 0;
                divisorBytes[15] &= (byte)(0xFF >> rng.Next(8));
                BigInteger dividend = new BigInteger(dividendBytes, isUnsigned: true);
                BigInteger divisor = new BigInteger(divisorBytes, isUnsigned: true);
                if (divisor.IsZero)
                {
                    continue;
                }
                (BigInteger eQ, BigInteger eR) = BigInteger.DivRem(dividend, divisor);
                (UInt256 aQ, UInt128 aR) = TAccelerator.DivRem((UInt256)dividend, (UInt128)divisor);
                Assert.Equal((eQ, eR), ((BigInteger)aQ, (BigInteger)aR));
            }
        }
    }

    public class AcceleratorTests_DefaultAccelerator :
        AcceleratorTests<DefaultAccelerator>
    { }

    public class AcceleratorTests_SoftwareAccelerator :
        AcceleratorTests<SoftwareAccelerator>
    { }
}
