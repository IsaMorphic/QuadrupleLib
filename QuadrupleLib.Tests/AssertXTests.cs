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
using QuadrupleLib.Tests.Assertions.Exceptions;
using QuadrupleLib.Tests.Assertions.Types;
using Xunit;

namespace QuadrupleLib.Tests
{
    public abstract class AssertXTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        private static readonly UInt128 _oneBits = RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000000");
        private static readonly UInt128 _twoBits = RawStorage<TAccelerator>.ParseBits("40000000000000000000000000000000");
        private static readonly UInt128 _signBit = UInt128.One << 127;
        private static readonly UInt128 _infinityBits = RawStorage<TAccelerator>.ParseBits("7FFF0000000000000000000000000000");
        private static readonly UInt128 _maxValueBits = RawStorage<TAccelerator>.ParseBits("7FFEFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

        [Fact]
        public void WithinUlpsOfIdenticalValuesIsAccepted()
        {
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits(_oneBits);
            AssertX.WithinUlps(one, one, 0);
        }

        [Fact]
        public void WithinUlpsOfNeighbourAtToleranceIsAccepted()
        {
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits(_oneBits);
            AssertX.WithinUlps(one, RawStorage<TAccelerator>.FromBits(_oneBits + 1), 1);
        }

        [Fact]
        public void WithinUlpsOfNeighbourBeyondToleranceIsRejected()
        {
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits(_oneBits);
            Float128<TAccelerator> twoUlpsAway = RawStorage<TAccelerator>.FromBits(_oneBits + 2);
            Assert.Throws<NearlyEqualException>(() => AssertX.WithinUlps(one, twoUlpsAway, 1));
        }

        [Fact]
        public void WithinUlpsOfSameSignNegativeNeighbourAtToleranceIsAccepted()
        {
            Float128<TAccelerator> negativeOne = RawStorage<TAccelerator>.FromBits(_oneBits | _signBit);
            AssertX.WithinUlps(negativeOne, RawStorage<TAccelerator>.FromBits((_oneBits + 1) | _signBit), 1);
        }

        [Fact]
        public void WithinUlpsOfSameSignNegativeNeighbourBeyondToleranceIsRejected()
        {
            Float128<TAccelerator> negativeOne = RawStorage<TAccelerator>.FromBits(_oneBits | _signBit);
            Float128<TAccelerator> negativeTwoUlpsAway = RawStorage<TAccelerator>.FromBits((_oneBits + 2) | _signBit);
            Assert.Throws<NearlyEqualException>(() => AssertX.WithinUlps(negativeOne, negativeTwoUlpsAway, 1));
        }

        [Fact]
        public void WithinUlpsAcrossBinadeBoundaryIsAccepted()
        {
            // binade boundary
            Float128<TAccelerator> two = RawStorage<TAccelerator>.FromBits(_twoBits);
            AssertX.WithinUlps(two, RawStorage<TAccelerator>.FromBits(_twoBits - 1), 1);
        }

        [Fact]
        public void WithinUlpsOfPositiveAndNegativeZeroIsRejected()
        {
            Float128<TAccelerator> positiveZero = RawStorage<TAccelerator>.FromBits(UInt128.Zero);
            Float128<TAccelerator> negativeZero = RawStorage<TAccelerator>.FromBits(_signBit);
            Assert.Throws<NearlyEqualException>(() => AssertX.WithinUlps(positiveZero, negativeZero, UInt128.MaxValue));
        }

        [Fact]
        public void WithinUlpsOfOppositeSignsIsRejected()
        {
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits(_oneBits);
            Float128<TAccelerator> negativeOne = RawStorage<TAccelerator>.FromBits(_oneBits | _signBit);
            Assert.Throws<NearlyEqualException>(() => AssertX.WithinUlps(one, negativeOne, UInt128.MaxValue));
        }

        [Fact]
        public void WithinUlpsOfNaNIsRejected()
        {
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits(_oneBits);
            Assert.Throws<NearlyEqualException>(() => AssertX.WithinUlps(one, Float128<TAccelerator>.NaN, 0));
        }

        [Fact]
        public void WithinUlpsOfNaNAtMaxToleranceIsRejected()
        {
            Float128<TAccelerator> one = RawStorage<TAccelerator>.FromBits(_oneBits);
            Assert.Throws<NearlyEqualException>(() => AssertX.WithinUlps(one, Float128<TAccelerator>.NaN, UInt128.MaxValue));
        }

        [Fact]
        public void WithinUlpsOfSameInfinityIsAccepted()
        {
            Float128<TAccelerator> positiveInfinity = RawStorage<TAccelerator>.FromBits(_infinityBits);
            AssertX.WithinUlps(positiveInfinity, positiveInfinity, 0);
        }

        [Fact]
        public void WithinUlpsOfInfinityAndMaxValueIsRejected()
        {
            Float128<TAccelerator> positiveInfinity = RawStorage<TAccelerator>.FromBits(_infinityBits);
            Float128<TAccelerator> maxValue = RawStorage<TAccelerator>.FromBits(_maxValueBits);
            Assert.Throws<NearlyEqualException>(() => AssertX.WithinUlps(positiveInfinity, maxValue, 1));
        }

        [Fact]
        public void RelativelyEqualOfNaNActualIsRejected()
        {
            Float128<TAccelerator> one = Float128<TAccelerator>.One;
            Assert.Throws<NearlyEqualException>(() => AssertX.RelativelyEqual(one, Float128<TAccelerator>.NaN, Float128<TAccelerator>.Parse("0.1")));
        }

        [Fact]
        public void RelativelyEqualOfNaNExpectedIsRejected()
        {
            Float128<TAccelerator> one = Float128<TAccelerator>.One;
            Assert.Throws<NearlyEqualException>(() => AssertX.RelativelyEqual(Float128<TAccelerator>.NaN, one, Float128<TAccelerator>.Parse("0.1")));
        }

        [Fact]
        public void RelativelyEqualOfTwoNaNsIsRejected()
        {
            Assert.Throws<NearlyEqualException>(() => AssertX.RelativelyEqual(Float128<TAccelerator>.NaN, Float128<TAccelerator>.NaN, Float128<TAccelerator>.Parse("0.1")));
        }

        [Fact]
        public void RelativelyEqualOutOfBoundIsRejected()
        {
            Assert.Throws<NearlyEqualException>(() => AssertX.RelativelyEqual(Float128<TAccelerator>.One, Float128<TAccelerator>.Parse("1.5"), Float128<TAccelerator>.Parse("0.1")));
        }

        [Fact]
        public void RelativelyEqualInBoundIsAccepted()
        {
            AssertX.RelativelyEqual(Float128<TAccelerator>.One, Float128<TAccelerator>.Parse("1.05"), Float128<TAccelerator>.Parse("0.1"));
        }

        [Fact]
        public void NearlyEqualWithinPrecisionIsAccepted()
        {
            AssertX.NearlyEqual(Float128<TAccelerator>.One, Float128<TAccelerator>.Parse("1.000001"), Precision.NearestThousandth);
        }

        [Fact]
        public void NearlyEqualOutsidePrecisionIsRejected()
        {
            Assert.Throws<NearlyEqualException>(() => AssertX.NearlyEqual(Float128<TAccelerator>.One, Float128<TAccelerator>.Parse("1.5"), Precision.NearestTenth));
        }

        [Fact]
        public void DoesNotThrowWhenActionCompletesIsAccepted()
        {
            AssertX.DoesNotThrow<InvalidOperationException>(() => { });
        }

        [Fact]
        public void DoesNotThrowWhenMatchingExceptionIsThrownIsRejected()
        {
            Assert.Throws<DoesNotThrowException>(() => AssertX.DoesNotThrow<InvalidOperationException>(() => throw new InvalidOperationException()));
        }

        [Fact]
        public void DoesNotThrowWithOtherExceptionIsPropagated()
        {
            Assert.Throws<ArgumentException>(() => AssertX.DoesNotThrow<InvalidOperationException>(() => throw new ArgumentException()));
        }
    }

    public class AssertXTests_DefaultAccelerator :
        AssertXTests<DefaultAccelerator>
    { }

    public class AssertXTests_SoftwareAccelerator :
        AssertXTests<SoftwareAccelerator>
    { }
}
