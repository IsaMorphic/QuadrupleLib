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
    public abstract class BoundaryEncodingTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        // Subnormal and smallest-normal inputs use RawStorage (reflection); results are compared as raw bits.

        [Fact]
        public void BitDecrementOfSmallestNormalIsLargestSubnormal()
        {
            // nextDown(2^-16382) is the subnormal with fraction 2^112 - 1 and exponent field 0
            Float128<TAccelerator> smallestNormal = RawStorage<TAccelerator>.FromBits(UInt128.One << 112);

            Float128<TAccelerator> result = Float128<TAccelerator>.BitDecrement(smallestNormal);

            Assert.Equal((UInt128.One << 112) - UInt128.One, RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void BitDecrementOfBitIncrementOfEpsilonIsEpsilon()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.BitDecrement(Float128<TAccelerator>.BitIncrement(Float128<TAccelerator>.Epsilon));

            Assert.Equal(RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Epsilon), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void ScaleBOfOneToTwoToTheMinus16383IsSubnormal()
        {
            // 2^-16383 is the subnormal with fraction 2^111 and exponent field 0
            Float128<TAccelerator> result = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -16383);

            Assert.Equal(UInt128.One << 111, RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void ScaleBOfOneToTwoToTheMinus16382IsSmallestNormal()
        {
            // 2^-16382 has exponent field 1 and fraction 0
            Float128<TAccelerator> result = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -16382);

            Assert.Equal(UInt128.One << 112, RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void RepresentationConstructorIsSmallestNormal()
        {
            // significand 2^112 at exponent -16382 is 2^112 * 2^(-16382 - 112) = 2^-16382
            Float128<TAccelerator> value = RawStorage<TAccelerator>.FromParts(UInt128.One << 112, -16382, false);

            Assert.Equal(UInt128.One << 112, RawStorage<TAccelerator>.Bits(value));
        }

        [Fact]
        public void BitIncrementOfLargestSubnormalIsSmallestNormal()
        {
            Float128<TAccelerator> largestSubnormal = RawStorage<TAccelerator>.FromBits((UInt128.One << 112) - UInt128.One);

            Float128<TAccelerator> result = Float128<TAccelerator>.BitIncrement(largestSubnormal);

            Assert.Equal(UInt128.One << 112, RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void ScaleBToSubnormalWithRemainderIsRoundedUp()
        {
            // reference: (2^112 + 7) / 8 * 2^-16494 = (2^109 + 7/8) * 2^-16494, which rounds to the significand 2^109 + 1
            Float128<TAccelerator> value = RawStorage<TAccelerator>.FromBits((UInt128.One << 112) | (UInt128)7);

            Float128<TAccelerator> result = Float128<TAccelerator>.ScaleB(value, -3);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("00002000000000000000000000000001"), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void ScaleBOddTieRoundsToEven()
        {
            // reference: (2^112 + 12) / 8 * 2^-16494 = (2^109 + 1.5) * 2^-16494, a tie between 2^109 + 1 and 2^109 + 2;
            // ties-to-even chooses the even significand 2^109 + 2
            Float128<TAccelerator> value = RawStorage<TAccelerator>.FromBits((UInt128.One << 112) | (UInt128)12);

            Float128<TAccelerator> result = Float128<TAccelerator>.ScaleB(value, -3);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("00002000000000000000000000000002"), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void ScaleBEvenTieRoundsToEven()
        {
            // reference: (2^112 + 4) / 8 * 2^-16494 = (2^109 + 0.5) * 2^-16494, a tie between 2^109 and 2^109 + 1;
            // ties-to-even chooses the even significand 2^109
            Float128<TAccelerator> value = RawStorage<TAccelerator>.FromBits((UInt128.One << 112) | (UInt128)4);

            Float128<TAccelerator> result = Float128<TAccelerator>.ScaleB(value, -3);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("00002000000000000000000000000000"), RawStorage<TAccelerator>.Bits(result));
        }
    }

    public class BoundaryEncodingTests_DefaultAccelerator :
        BoundaryEncodingTests<DefaultAccelerator> { }

    public class BoundaryEncodingTests_SoftwareAccelerator :
        BoundaryEncodingTests<SoftwareAccelerator> { }
}
