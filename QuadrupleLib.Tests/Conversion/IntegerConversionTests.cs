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
    public abstract class IntegerConversionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(5L)]
        [InlineData(-1L)]
        [InlineData(-263L)]
        [InlineData(12345L)]
        public void ConvertToInt128IsEqual(long x)
        {
            Assert.Equal(x, (Int128)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5L)]
        [InlineData(-1L)]
        [InlineData(-263L)]
        [InlineData(12345L)]
        public void ConvertToInt64IsEqual(long x)
        {
            Assert.Equal(x, (long)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-263)]
        [InlineData(12345)]
        public void ConvertToInt32IsEqual(int x)
        {
            Assert.Equal(x, (int)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-263)]
        [InlineData(12345)]
        public void ConvertToInt16IsEqual(short x)
        {
            Assert.Equal(x, (short)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-128)]
        [InlineData(127)]
        public void ConvertToSByteIsEqual(sbyte x)
        {
            Assert.Equal(x, (sbyte)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5UL)]
        [InlineData(1UL)]
        [InlineData(263UL)]
        [InlineData(12345UL)]
        public void ConvertToUInt128IsEqual(ulong x)
        {
            Assert.Equal(x, (UInt128)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5UL)]
        [InlineData(1UL)]
        [InlineData(263UL)]
        [InlineData(12345UL)]
        public void ConvertToUInt64IsEqual(ulong x)
        {
            Assert.Equal(x, (ulong)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5U)]
        [InlineData(1U)]
        [InlineData(263U)]
        [InlineData(12345U)]
        public void ConvertToUInt32IsEqual(uint x)
        {
            Assert.Equal(x, (uint)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5U)]
        [InlineData(1U)]
        [InlineData(263U)]
        [InlineData(12345U)]
        public void ConvertToUInt16IsEqual(ushort x)
        {
            Assert.Equal(x, (ushort)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(3)]
        [InlineData(255)]
        [InlineData(0)]
        public void ConvertToByteIsEqual(byte x)
        {
            Assert.Equal(x, (byte)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-263.5)]
        [InlineData(12345.0)]
        public void ConvertToInt128IsTruncated(double x)
        {
            Assert.Equal((Int128)x, (Int128)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-263.5)]
        [InlineData(12345.0)]
        public void ConvertToInt64IsTruncated(double x)
        {
            Assert.Equal((long)x, (long)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-263.5)]
        [InlineData(12345.0)]
        public void ConvertToInt32IsTruncated(double x)
        {
            Assert.Equal((int)x, (int)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-263.5)]
        [InlineData(12345.0)]
        public void ConvertToInt16IsTruncated(double x)
        {
            Assert.Equal((short)x, (short)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.3)]
        [InlineData(-1.12)]
        [InlineData(-127.5)]
        [InlineData(123.0)]
        public void ConvertToSByteIsTruncated(double x)
        {
            Assert.Equal((sbyte)x, (sbyte)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.54)]
        [InlineData(1.0)]
        [InlineData(263.8677)]
        [InlineData(12345.9)]
        public void ConvertToUInt128IsTruncated(double x)
        {
            Assert.Equal((UInt128)x, (UInt128)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.54)]
        [InlineData(1.0)]
        [InlineData(263.8677)]
        [InlineData(12345.9)]
        public void ConvertToUInt64IsTruncated(double x)
        {
            Assert.Equal((ulong)x, (ulong)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.54)]
        [InlineData(1.0)]
        [InlineData(263.8677)]
        [InlineData(12345.9)]
        public void ConvertToUInt32IsTruncated(double x)
        {
            Assert.Equal((uint)x, (uint)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.54)]
        [InlineData(1.0)]
        [InlineData(263.8677)]
        [InlineData(12345.9)]
        public void ConvertToUInt16IsTruncated(double x)
        {
            Assert.Equal((ushort)x, (ushort)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData(5.1)]
        [InlineData(3.456)]
        [InlineData(254.5)]
        [InlineData(0.5)]
        public void ConvertToByteIsTruncated(double x)
        {
            Assert.Equal((byte)x, (byte)(Float128<TAccelerator>)x);
        }

        [Theory]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("Int128")]
        [InlineData("byte")]
        [InlineData("ushort")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("UInt128")]
        public void PositiveInfinityToIntegerConversionIsOverflowException(string target)
        {
            Assert.Throws<OverflowException>(() => ConvertToIntegerType(target, Float128<TAccelerator>.PositiveInfinity));
        }

        [Theory]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("Int128")]
        [InlineData("byte")]
        [InlineData("ushort")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("UInt128")]
        public void NaNToIntegerConversionIsOverflowException(string target)
        {
            Assert.Throws<OverflowException>(() => ConvertToIntegerType(target, Float128<TAccelerator>.NaN));
        }

        [Theory]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("Int128")]
        [InlineData("byte")]
        [InlineData("ushort")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("UInt128")]
        public void OutOfRangeCheckedToIntegerConversionIsOverflowException(string target)
        {
            Float128<TAccelerator> value = 1e300;

            Assert.Throws<OverflowException>(() => ConvertToIntegerTypeChecked(target, value));
        }

        [Theory]
        [InlineData("byte")]
        [InlineData("ushort")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("UInt128")]
        public void NegativeCheckedToUnsignedConversionIsOverflowException(string target)
        {
            Float128<TAccelerator> value = -1.0;

            Assert.Throws<OverflowException>(() => ConvertToIntegerTypeChecked(target, value));
        }

        [Theory]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("Int128")]
        [InlineData("byte")]
        [InlineData("ushort")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("UInt128")]
        public void NaNCheckedToIntegerConversionIsOverflowException(string target)
        {
            Assert.Throws<OverflowException>(() => ConvertToIntegerTypeChecked(target, Float128<TAccelerator>.NaN));
        }

        [Theory]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("Int128")]
        [InlineData("byte")]
        [InlineData("ushort")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("UInt128")]
        public void PositiveInfinityCheckedToIntegerConversionIsOverflowException(string target)
        {
            Assert.Throws<OverflowException>(() => ConvertToIntegerTypeChecked(target, Float128<TAccelerator>.PositiveInfinity));
        }

        [Theory]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("Int128")]
        [InlineData("byte")]
        [InlineData("ushort")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("UInt128")]
        public void NegativeInfinityCheckedToIntegerConversionIsOverflowException(string target)
        {
            Assert.Throws<OverflowException>(() => ConvertToIntegerTypeChecked(target, Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void NegativeCheckedToSignedConversionIsExact()
        {
            Float128<TAccelerator> value = -1.0;

            Assert.Equal((sbyte)-1, checked((sbyte)value));
            Assert.Equal((short)-1, checked((short)value));
            Assert.Equal(-1, checked((int)value));
            Assert.Equal(-1L, checked((long)value));
            Assert.Equal(Int128.NegativeOne, checked((Int128)value));
        }

        [Fact]
        public void NegativeUncheckedToUnsignedConversionWraps()
        {
            // an unchecked conversion keeps the low bits of the two's complement value, as (uint)(-1L) does
            Float128<TAccelerator> negativeOne = -1.0;
            Float128<TAccelerator> negativeThree = -3.0;

            Assert.Equal(byte.MaxValue, (byte)negativeOne);
            Assert.Equal(ushort.MaxValue, (ushort)negativeOne);
            Assert.Equal(uint.MaxValue, (uint)negativeOne);
            Assert.Equal(ulong.MaxValue - 2, (ulong)negativeThree);
            Assert.Equal(UInt128.MaxValue, (UInt128)negativeOne);
        }

        [Fact]
        public void UncheckedConversionAtLeastTwoToThe128Wraps()
        {
            // 2^128 + 2^20 is exact in binary128; mod 2^128 it is 2^20
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 128) +
                Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 20);

            Assert.Equal(UInt128.One << 20, (UInt128)value);
            Assert.Equal(-(Int128.One << 20), (Int128)(-value));
            Assert.Equal(1 << 20, (int)value);
            Assert.Equal((short)0, (short)value);
        }

        [Fact]
        public void UncheckedConversionOfLargeMultipleOfTwoToThe128IsZero()
        {
            // 1e300 and the largest finite value are multiples of 2^128, so their low 128 bits are zero
            Float128<TAccelerator> largest = RawStorage<TAccelerator>.LargestFinite();

            Assert.Equal(UInt128.Zero, (UInt128)(Float128<TAccelerator>)1e300);
            Assert.Equal(Int128.Zero, (Int128)(-largest));
            Assert.Equal(0L, (long)largest);
        }

        [Fact]
        public void UncheckedConversionInRangeWraps()
        {
            // 3e9 is above int.MaxValue; as (int)3000000000L does, the conversion keeps the low 32 bits
            Float128<TAccelerator> value = 3e9;

            Assert.Equal(unchecked((int)3000000000L), (int)value);
            Assert.Throws<OverflowException>(() => checked((int)value));
        }

        private static object ConvertToIntegerType(string target, Float128<TAccelerator> value)
        {
            return target switch
            {
                "sbyte" => (object)(sbyte)value,
                "short" => (object)(short)value,
                "int" => (object)(int)value,
                "long" => (object)(long)value,
                "Int128" => (object)(Int128)value,
                "byte" => (object)(byte)value,
                "ushort" => (object)(ushort)value,
                "uint" => (object)(uint)value,
                "ulong" => (object)(ulong)value,
                "UInt128" => (object)(UInt128)value,
                _ => throw new ArgumentException(target, nameof(target))
            };
        }

        private static object ConvertToIntegerTypeChecked(string target, Float128<TAccelerator> value)
        {
            return target switch
            {
                "sbyte" => (object)checked((sbyte)value),
                "short" => (object)checked((short)value),
                "int" => (object)checked((int)value),
                "long" => (object)checked((long)value),
                "Int128" => (object)checked((Int128)value),
                "byte" => (object)checked((byte)value),
                "ushort" => (object)checked((ushort)value),
                "uint" => (object)checked((uint)value),
                "ulong" => (object)checked((ulong)value),
                "UInt128" => (object)checked((UInt128)value),
                _ => throw new ArgumentException(target, nameof(target))
            };
        }

        [Fact]
        public void MinValueConversionRoundTripIsExact()
        {
            // the most negative value of each signed type is a power of two, so binary128 holds it exactly
            Assert.Equal(sbyte.MinValue, (sbyte)(Float128<TAccelerator>)sbyte.MinValue);
            Assert.Equal(short.MinValue, (short)(Float128<TAccelerator>)short.MinValue);
            Assert.Equal(int.MinValue, (int)(Float128<TAccelerator>)int.MinValue);
            Assert.Equal(long.MinValue, (long)(Float128<TAccelerator>)long.MinValue);
            Assert.Equal(Int128.MinValue, (Int128)(Float128<TAccelerator>)Int128.MinValue);
        }

        [Fact]
        public void MaxValueCheckedConversionRoundTripIsExact()
        {
            // MaxValue of each type up to 64 bits is exact in binary128, and is the largest value the checked conversion accepts
            Assert.Equal(sbyte.MaxValue, checked((sbyte)(Float128<TAccelerator>)sbyte.MaxValue));
            Assert.Equal(short.MaxValue, checked((short)(Float128<TAccelerator>)short.MaxValue));
            Assert.Equal(int.MaxValue, checked((int)(Float128<TAccelerator>)int.MaxValue));
            Assert.Equal(long.MaxValue, checked((long)(Float128<TAccelerator>)long.MaxValue));
            Assert.Equal(byte.MaxValue, checked((byte)(Float128<TAccelerator>)byte.MaxValue));
            Assert.Equal(ushort.MaxValue, checked((ushort)(Float128<TAccelerator>)ushort.MaxValue));
            Assert.Equal(uint.MaxValue, checked((uint)(Float128<TAccelerator>)uint.MaxValue));
            Assert.Equal(ulong.MaxValue, checked((ulong)(Float128<TAccelerator>)ulong.MaxValue));
        }

        [Fact]
        public void JustAboveMaxValueCheckedConversionIsOverflowException()
        {
            // MaxValue + 1 is exact in binary128 for each type up to 64 bits, and is the smallest value out of range
            Float128<TAccelerator> one = Float128<TAccelerator>.One;

            Assert.Throws<OverflowException>(() => checked((sbyte)((Float128<TAccelerator>)sbyte.MaxValue + one)));
            Assert.Throws<OverflowException>(() => checked((short)((Float128<TAccelerator>)short.MaxValue + one)));
            Assert.Throws<OverflowException>(() => checked((int)((Float128<TAccelerator>)int.MaxValue + one)));
            Assert.Throws<OverflowException>(() => checked((long)((Float128<TAccelerator>)long.MaxValue + one)));
            Assert.Throws<OverflowException>(() => checked((byte)((Float128<TAccelerator>)byte.MaxValue + one)));
            Assert.Throws<OverflowException>(() => checked((ushort)((Float128<TAccelerator>)ushort.MaxValue + one)));
            Assert.Throws<OverflowException>(() => checked((uint)((Float128<TAccelerator>)uint.MaxValue + one)));
            Assert.Throws<OverflowException>(() => checked((ulong)((Float128<TAccelerator>)ulong.MaxValue + one)));

            // the 128-bit MaxValues are not exact in binary128, but MaxValue + 1 is a power of two, so it is
            Float128<TAccelerator> twoTo127 = Float128<TAccelerator>.ScaleB(one, 127);
            Float128<TAccelerator> twoTo128 = Float128<TAccelerator>.ScaleB(one, 128);
            Assert.Throws<OverflowException>(() => checked((Int128)twoTo127));
            Assert.Throws<OverflowException>(() => checked((UInt128)twoTo128));
        }

        [Fact]
        public void JustBelowMinValueCheckedConversionIsOverflowException()
        {
            // MinValue - 1 is exact in binary128 for each signed type up to 64 bits
            Float128<TAccelerator> one = Float128<TAccelerator>.One;

            Assert.Throws<OverflowException>(() => checked((sbyte)((Float128<TAccelerator>)sbyte.MinValue - one)));
            Assert.Throws<OverflowException>(() => checked((short)((Float128<TAccelerator>)short.MinValue - one)));
            Assert.Throws<OverflowException>(() => checked((int)((Float128<TAccelerator>)int.MinValue - one)));
            Assert.Throws<OverflowException>(() => checked((long)((Float128<TAccelerator>)long.MinValue - one)));
        }

        [Fact]
        public void MinValueCheckedConversionRoundTripIsExact()
        {
            // the magnitude of MinValue is one more than MaxValue, and the checked conversion accepts it
            Assert.Equal(sbyte.MinValue, checked((sbyte)(Float128<TAccelerator>)sbyte.MinValue));
            Assert.Equal(short.MinValue, checked((short)(Float128<TAccelerator>)short.MinValue));
            Assert.Equal(int.MinValue, checked((int)(Float128<TAccelerator>)int.MinValue));
            Assert.Equal(long.MinValue, checked((long)(Float128<TAccelerator>)long.MinValue));
            Assert.Equal(Int128.MinValue, checked((Int128)(Float128<TAccelerator>)Int128.MinValue));
        }

        [Fact]
        public void PositiveInRangeCheckedConversionIsExact()
        {
            Float128<TAccelerator> value = 1e8;

            Assert.Equal(100_000_000, checked((int)value));
            Assert.Equal(100_000_000L, checked((long)value));
            Assert.Equal(100_000_000U, checked((uint)value));
            Assert.Equal(100_000_000UL, checked((ulong)value));
            Assert.Equal((Int128)100_000_000, checked((Int128)value));
            Assert.Equal((UInt128)100_000_000, checked((UInt128)value));
        }

        [Fact]
        public void ZeroConversionIsExact()
        {
            // Zero is exact in both checked and unchecked conversions for all integer types
            Assert.Equal(0, (int)(Float128<TAccelerator>)0.0);
            Assert.Equal(0, checked((int)(Float128<TAccelerator>)0.0));
            Assert.Equal(0L, (long)(Float128<TAccelerator>)0.0);
            Assert.Equal(0L, checked((long)(Float128<TAccelerator>)0.0));
            Assert.Equal(Int128.Zero, (Int128)(Float128<TAccelerator>)0.0);
            Assert.Equal(Int128.Zero, checked((Int128)(Float128<TAccelerator>)0.0));
            Assert.Equal(0U, (uint)(Float128<TAccelerator>)0.0);
            Assert.Equal(0U, checked((uint)(Float128<TAccelerator>)0.0));
            Assert.Equal(0UL, (ulong)(Float128<TAccelerator>)0.0);
            Assert.Equal(0UL, checked((ulong)(Float128<TAccelerator>)0.0));
            Assert.Equal(UInt128.Zero, (UInt128)(Float128<TAccelerator>)0.0);
            Assert.Equal(UInt128.Zero, checked((UInt128)(Float128<TAccelerator>)0.0));
        }
    }

    public class IntegerConversionTests_DefaultAccelerator :
        IntegerConversionTests<DefaultAccelerator>
    { }

    public class IntegerConversionTests_SoftwareAccelerator :
        IntegerConversionTests<SoftwareAccelerator>
    { }
}