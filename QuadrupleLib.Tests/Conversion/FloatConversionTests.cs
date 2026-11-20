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

        [Theory]
        [InlineData(1.5)]
        [InlineData(0.5)]
        [InlineData(-2.0)]
        [InlineData(100.0)]
        public void ImplicitConversionToNonGenericIsExact(double x)
        {
            // reference: these values are exactly representable in binary128, so the round trip is exact
            Float128<TAccelerator> value = x;
            Float128 nonGeneric = value;

            Assert.Equal(x, (double)nonGeneric);
        }

        [Theory]
        [InlineData(1.5)]
        [InlineData(0.5)]
        [InlineData(-2.0)]
        [InlineData(100.0)]
        public void ImplicitConversionFromNonGenericIsExact(double x)
        {
            // reference: these values are exactly representable in binary128, so the round trip is exact
            Float128 nonGeneric = x;
            Float128<TAccelerator> value = nonGeneric;

            Assert.Equal(x, (double)value);
        }

        [Theory]
        [InlineData(1.5)]
        [InlineData(0.5)]
        [InlineData(-2.0)]
        [InlineData(100.0)]
        public void StaticWithAcceleratorIsExact(double x)
        {
            // reference: WithAccelerator copies the raw bits, so the value is preserved exactly
            Float128<TAccelerator> value = x;
            Float128<SoftwareAccelerator> converted = Float128<TAccelerator>.WithAccelerator<SoftwareAccelerator>(value);

            Assert.Equal(x, (double)converted);
        }

        [Theory]
        [InlineData(1.5)]
        [InlineData(0.5)]
        [InlineData(-2.0)]
        [InlineData(100.0)]
        public void InstanceWithAcceleratorIsExact(double x)
        {
            // reference: WithAccelerator copies the raw bits, so the value is preserved exactly
            Float128<TAccelerator> value = x;
            Float128<SoftwareAccelerator> converted = value.WithAccelerator<SoftwareAccelerator>();

            Assert.Equal(x, (double)converted);
        }

        [Theory]
        [InlineData(1.5)]
        [InlineData(0.5)]
        [InlineData(-2.0)]
        [InlineData(100.0)]
        public void NonGenericInstanceWithAcceleratorIsExact(double x)
        {
            // reference: WithAccelerator copies the raw bits, so the value is preserved exactly
            Float128 nonGeneric = x;
            Float128<TAccelerator> converted = nonGeneric.WithAccelerator<TAccelerator>();

            Assert.Equal(x, (double)converted);
        }

        [Theory]
        [InlineData(1.5)]
        [InlineData(0.5)]
        [InlineData(-2.0)]
        [InlineData(100.0)]
        public void NonGenericStaticWithAcceleratorIsExact(double x)
        {
            // reference: WithAccelerator copies the raw bits, so the value is preserved exactly
            Float128 nonGeneric = x;
            Float128<TAccelerator> converted = Float128.WithAccelerator<TAccelerator>(nonGeneric);

            Assert.Equal(x, (double)converted);
        }

        [Fact]
        public void HalfNaNConversionIsNaN()
        {
            // IEEE 754: NaN converts to NaN
            Float128<TAccelerator> result = (Float128<TAccelerator>)Half.NaN;

            Assert.True(Float128<TAccelerator>.IsNaN(result));
        }

        [Fact]
        public void HalfPositiveInfinityConversionIsPositiveInfinity()
        {
            Float128<TAccelerator> result = (Float128<TAccelerator>)Half.PositiveInfinity;

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(result));
        }

        [Fact]
        public void HalfNegativeInfinityConversionIsNegativeInfinity()
        {
            Float128<TAccelerator> result = (Float128<TAccelerator>)Half.NegativeInfinity;

            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(result));
        }

        [Fact]
        public void ToHalfOfNaNIsNaN()
        {
            Half result = (Half)Float128<TAccelerator>.NaN;

            Assert.True(Half.IsNaN(result));
        }

        [Fact]
        public void ToHalfOfPositiveInfinityIsPositiveInfinity()
        {
            Half result = (Half)Float128<TAccelerator>.PositiveInfinity;

            Assert.True(Half.IsPositiveInfinity(result));
        }

        [Fact]
        public void ToHalfOfNegativeInfinityIsNegativeInfinity()
        {
            Half result = (Half)Float128<TAccelerator>.NegativeInfinity;

            Assert.True(Half.IsNegativeInfinity(result));
        }

        [Fact]
        public void ToHalfOfBelowRangeIsPositiveZero()
        {
            // 2^-30 is far below the smallest Half subnormal 2^-24, so round-to-nearest gives +0
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -30);

            Assert.Equal(Half.Zero, (Half)value);
        }

        [Fact]
        public void ToHalfOfBelowRangeIsNegativeZero()
        {
            Float128<TAccelerator> value = -Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -30);

            Assert.Equal((ushort)0x8000, BitConverter.HalfToUInt16Bits((Half)value));
        }

        [Fact]
        public void FloatNaNConversionIsNaN()
        {
            Float128<TAccelerator> result = (Float128<TAccelerator>)float.NaN;

            Assert.True(Float128<TAccelerator>.IsNaN(result));
        }

        [Fact]
        public void FloatPositiveInfinityConversionIsPositiveInfinity()
        {
            Float128<TAccelerator> result = (Float128<TAccelerator>)float.PositiveInfinity;

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(result));
        }

        [Fact]
        public void FloatNegativeInfinityConversionIsNegativeInfinity()
        {
            Float128<TAccelerator> result = (Float128<TAccelerator>)float.NegativeInfinity;

            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(result));
        }

        [Fact]
        public void ToFloatOfNaNIsNaN()
        {
            float result = (float)Float128<TAccelerator>.NaN;

            Assert.True(float.IsNaN(result));
        }

        [Fact]
        public void ToFloatOfPositiveInfinityIsPositiveInfinity()
        {
            float result = (float)Float128<TAccelerator>.PositiveInfinity;

            Assert.True(float.IsPositiveInfinity(result));
        }

        [Fact]
        public void ToFloatOfNegativeInfinityIsNegativeInfinity()
        {
            float result = (float)Float128<TAccelerator>.NegativeInfinity;

            Assert.True(float.IsNegativeInfinity(result));
        }

        [Fact]
        public void ToFloatOfSubnormalRoundsUp()
        {
            // reference: 1.5 * 2^-149 is halfway between 2^-149 and 2^-148; ties-to-even gives 2^-148, bit pattern 2
            Float128<TAccelerator> value = RawStorage<TAccelerator>.FromBits((UInt128)16234 << 112 | (UInt128.One << 111));

            Assert.Equal(2, BitConverter.SingleToInt32Bits((float)value));
        }

        [Fact]
        public void ToFloatOfJustBelowTwoIsTwo()
        {
            // reference: 2 - 2^-112 rounds to 2.0f, carrying into a power of two
            Float128<TAccelerator> value = RawStorage<TAccelerator>.FromBits((UInt128)16383 << 112 | (UInt128.MaxValue >> 16));

            Assert.Equal(2.0f, (float)value);
        }

        [Fact]
        public void DoubleNaNConversionIsNaN()
        {
            Float128<TAccelerator> result = (Float128<TAccelerator>)double.NaN;

            Assert.True(Float128<TAccelerator>.IsNaN(result));
        }

        [Fact]
        public void DoublePositiveInfinityConversionIsPositiveInfinity()
        {
            Float128<TAccelerator> result = (Float128<TAccelerator>)double.PositiveInfinity;

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(result));
        }

        [Fact]
        public void DoubleNegativeInfinityConversionIsNegativeInfinity()
        {
            Float128<TAccelerator> result = (Float128<TAccelerator>)double.NegativeInfinity;

            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(result));
        }

        [Fact]
        public void ToDoubleOfNaNIsNaN()
        {
            double result = (double)Float128<TAccelerator>.NaN;

            Assert.True(double.IsNaN(result));
        }

        [Fact]
        public void ToDoubleOfPositiveInfinityIsPositiveInfinity()
        {
            double result = (double)Float128<TAccelerator>.PositiveInfinity;

            Assert.True(double.IsPositiveInfinity(result));
        }

        [Fact]
        public void ToDoubleOfNegativeInfinityIsNegativeInfinity()
        {
            double result = (double)Float128<TAccelerator>.NegativeInfinity;

            Assert.True(double.IsNegativeInfinity(result));
        }

        [Fact]
        public void ToDoubleOfBelowRangeIsPositiveZero()
        {
            // 2^-1076 is below the smallest double subnormal 2^-1074, so round-to-nearest gives +0
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -1076);

            Assert.Equal(0.0, (double)value);
        }

        [Fact]
        public void ToDoubleOfBelowRangeIsNegativeZero()
        {
            Float128<TAccelerator> value = -Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -1076);

            Assert.Equal(long.MinValue, BitConverter.DoubleToInt64Bits((double)value));
        }

        [Fact]
        public void ToDoubleOfSubnormalRoundsUp()
        {
            // reference: 1.5 * 2^-1074 is halfway between 2^-1074 and 2^-1073; ties-to-even gives 2^-1073, bit pattern 2
            Float128<TAccelerator> value = RawStorage<TAccelerator>.FromBits((UInt128)15309 << 112 | (UInt128.One << 111));

            Assert.Equal(2L, BitConverter.DoubleToInt64Bits((double)value));
        }
    }

    public class FloatConversionTests_DefaultAccelerator :
        FloatConversionTests<DefaultAccelerator>
    { }

    public class FloatConversionTests_SoftwareAccelerator :
        FloatConversionTests<SoftwareAccelerator>
    { }
}