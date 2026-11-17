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

namespace QuadrupleLib.Tests.Conversion
{
    public abstract class FloatConversionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(1.5)]
        [InlineData(0.5)]
        [InlineData(1.300)]
        [InlineData(-263.0)]
        [InlineData(123.4567)]
        [InlineData(double.Epsilon)]
        public void ConvertToDoubleIsEqual(double x)
        {
            Assert.Equal(x, (double)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(1.5f)]
        [InlineData(0.5f)]
        [InlineData(1.300f)]
        [InlineData(-263.0f)]
        [InlineData(123.4567f)]
        [InlineData(float.Epsilon)]
        public void ConvertToSingleIsEqual(float x)
        {
            Assert.Equal(x, (float)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(1.5f)]
        [InlineData(0.5f)]
        [InlineData(1.300f)]
        [InlineData(-263.0f)]
        [InlineData(123.4567f)]
        [InlineData(5.9604645E-08f)]
        public void ConvertToHalfIsEqual(float x)
        {
            Half _x = (Half)x;
            Assert.Equal(_x, (Half)(Float128<TAccelerator>)_x);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(0.5)]
        [InlineData(4.0)]
        [InlineData(1.5)]
        [InlineData(100.0)]
        public void HalfConversionIsExact(double x)
        {
            // every binary16 value is exactly representable in binary128
            Half value = (Half)x;

            Assert.Equal(x, (double)(Float128<TAccelerator>)value);
        }

        [Fact]
        public void SmallestFloatSubnormalConversionIsExact()
        {
            // 2^-149 has biased exponent 16383 - 149 = 16234 and zero fraction
            Float128<TAccelerator> value = float.Epsilon;

            Assert.Equal((UInt128)16234 << 112, RawStorage<TAccelerator>.Bits(value));
        }

        [Fact]
        public void ToFloatOfTwoToTheOneTwentySevenIsExact()
        {
            Float128<TAccelerator> value = double.ScaleB(1.0, 127);

            Assert.Equal(MathF.ScaleB(1.0f, 127), (float)value);
        }

        [Fact]
        public void ToFloatBelowRangeIsPositiveZero()
        {
            // 1e-60 is far below the smallest float subnormal 2^-149, so round-to-nearest gives +0
            Float128<TAccelerator> value = 1e-60;

            float result = (float)value;

            Assert.Equal(0, BitConverter.SingleToInt32Bits(result));
        }

        [Fact]
        public void SmallestDoubleSubnormalConversionIsExact()
        {
            // 2^-1074 has biased exponent 16383 - 1074 = 15309 and zero fraction
            Float128<TAccelerator> value = double.Epsilon;

            Assert.Equal((UInt128)15309 << 112, RawStorage<TAccelerator>.Bits(value));
        }

        [Fact]
        public void ToDoubleOfTwoToTheOneThousandTwentyThreeIsExact()
        {
            Float128<TAccelerator> value = double.ScaleB(1.0, 1023);

            Assert.Equal(double.ScaleB(1.0, 1023), (double)value);
        }

        [Fact]
        public void ToDoubleOfSmallestNormalIsExact()
        {
            // 2^-1022 is the smallest normal double
            Float128<TAccelerator> value = double.ScaleB(1.0, -1022);

            Assert.Equal(double.ScaleB(1.0, -1022), (double)value);
        }

        [Fact]
        public void NegativeZeroDoubleConversionIsNegativeZero()
        {
            // IEEE 754: the conversion preserves the sign of zero
            Float128<TAccelerator> value = BitConverter.Int64BitsToDouble(long.MinValue);

            Assert.True(Float128<TAccelerator>.IsZero(value));
            Assert.True(Float128<TAccelerator>.IsNegative(value));
        }

        [Fact]
        public void HalfSubnormalConversionIsExact()
        {
            // reference: the binary16 subnormal 0x0001 is 2^-14 * 2^-10 = 2^-24, which is exact in binary128
            Half value = BitConverter.UInt16BitsToHalf(0x0001);

            Float128<TAccelerator> result = (Float128<TAccelerator>)value;

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FE70000000000000000000000000000"), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void HalfNegativeZeroConversionIsNegativeZero()
        {
            // IEEE 754: the conversion preserves the sign of zero; the exact result is the sign bit alone
            Half value = BitConverter.UInt16BitsToHalf(0x8000);

            Float128<TAccelerator> result = (Float128<TAccelerator>)value;

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("80000000000000000000000000000000"), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void FloatNegativeZeroConversionIsNegativeZero()
        {
            // IEEE 754: the conversion preserves the sign of zero; the exact result is the sign bit alone
            float value = BitConverter.Int32BitsToSingle(int.MinValue);

            Float128<TAccelerator> result = value;

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("80000000000000000000000000000000"), RawStorage<TAccelerator>.Bits(result));
        }

        [Fact]
        public void TenthNearestBinary128ConvertsToNearestDouble()
        {
            // reference: 3FFB999999999999999999999999999A is the binary128 value nearest 0.1; rounded to the nearest double it is 0x3FB999999999999A
            Float128<TAccelerator> value = RawStorage<TAccelerator>.FromBits(RawStorage<TAccelerator>.ParseBits("3FFB999999999999999999999999999A"));

            Assert.Equal(0x3FB999999999999AL, BitConverter.DoubleToInt64Bits((double)value));
        }

        [Fact]
        public void ToDoubleOfSmallestDoubleSubnormalIsExact()
        {
            // 2^-1074 is double.Epsilon, the smallest positive double subnormal
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -1074);
            Assert.Equal(double.Epsilon, (double)value);
        }

        [Fact]
        public void ToFloatOfSmallestFloatSubnormalIsExact()
        {
            // 2^-149 is float.Epsilon, the smallest positive float subnormal
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -149);
            Assert.Equal(float.Epsilon, (float)value);
        }

        [Fact]
        public void ToHalfOfSmallestHalfSubnormalIsExact()
        {
            // 2^-24 is Half.Epsilon, the smallest positive Half subnormal
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -24);
            Assert.Equal((ushort)0x0001, BitConverter.HalfToUInt16Bits((Half)value));
        }

        [Fact]
        public void ToHalfOfLargestHalfSubnormalIsExact()
        {
            // 1023 * 2^-24 is the largest Half subnormal, bit pattern 0x03FF
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB((Float128<TAccelerator>)1023, -24);
            Assert.Equal((ushort)0x03FF, BitConverter.HalfToUInt16Bits((Half)value));
        }

        [Fact]
        public void ToHalfOfHalfwayBelowSmallestSubnormalIsPositiveZero()
        {
            // 2^-25 is halfway between 0 and 2^-24; ties-to-even gives +0
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -25);
            Assert.Equal((ushort)0x0000, BitConverter.HalfToUInt16Bits((Half)value));
        }

        [Fact]
        public void ToHalfOfRoundedLargestSubnormalIsSmallestNormal()
        {
            // 1023.5 * 2^-24 is a tie between 0x03FF and 0x0400; ties-to-even carries into the smallest normal
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB((Float128<TAccelerator>)1023.5, -24);
            Assert.Equal((ushort)0x0400, BitConverter.HalfToUInt16Bits((Half)value));
        }
    }

    public class FloatConversionTests_DefaultAccelerator :
        FloatConversionTests<DefaultAccelerator>
    { }

    public class FloatConversionTests_SoftwareAccelerator :
        FloatConversionTests<SoftwareAccelerator>
    { }
}