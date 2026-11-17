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

using System.Globalization;
using QuadrupleLib.Accelerators;
using QuadrupleLib.Tests.Assertions.Types;
using Xunit;

namespace QuadrupleLib.Tests.Conversion
{
    public abstract class StringConversionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.300)]
        [InlineData(-263.0)]
        [InlineData(123.4567)]
        [InlineData(1E-36)]
        [InlineData(1000000.0)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(8.0)]
        [InlineData(1.0E40)]
        public void ConvertToStringRoundtripIsEqual(double x)
        {
            string s = $"{(Float128<TAccelerator>)x}";
            Assert.Equal(x, Float128<TAccelerator>.Parse(s));
        }

        [Theory]
        [InlineData(".5", 0.5)]
        [InlineData("-.5", -0.5)]
        [InlineData("0.5", 0.5)]
        [InlineData("-0.5", -0.5)]
        [InlineData("100", 100.0)]
        [InlineData("100.0", 100.0)]
        [InlineData("-100", -100.0)]
        [InlineData("-100.0", -100.0)]
        [InlineData("1E2", 100.0)]
        [InlineData("1.0E2", 100.0)]
        [InlineData("-1E2", -100.0)]
        [InlineData("-1.0E2", -100.0)]
        [InlineData(".1E3", 100.0)]
        [InlineData("0.1E3", 100.0)]
        [InlineData("-.1E3", -100.0)]
        [InlineData("-0.1E3", -100.0)]
        [InlineData("1E-3", 0.001)]
        [InlineData("-1E-3", -0.001)]
        [InlineData("0.1E-2", 0.001)]
        [InlineData("-0.1E-2", -0.001)]
        [InlineData(".1E-2", 0.001)]
        [InlineData("-.1E-2", -0.001)]
        [InlineData("0.1000000000000000005", 0.1)]
        public void ParseStringIsCorrect(string s, double x)
        {
            AssertX.NearlyEqual((Float128<TAccelerator>)x,
                Float128<TAccelerator>.Parse(s),
                Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData("1e-10", 1e-10)]
        [InlineData("1e10", 1e10)]
        [InlineData("1.5e-10", 1.5e-10)]
        [InlineData("2.5e3", 2.5e3)]
        public void LowercaseExponentParseIsNearestDouble(string s, double expected)
        {
            bool parsed = Float128<TAccelerator>.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out Float128<TAccelerator> value);

            Assert.True(parsed);
            Assert.Equal(expected, (double)value);
        }

        [Theory]
        [InlineData("12.5e1", 125.0)]
        [InlineData("-12.5e1", -125.0)]
        [InlineData("00012.5e-1", 1.25)]
        [InlineData("-0.0125e3", -12.5)]
        [InlineData("125e-2", 1.25)]
        public void ExponentWithMultiDigitWholePartIsExact(string s, double expected)
        {
            // each value is exactly representable, wherever the decimal point falls
            Assert.Equal((Float128<TAccelerator>)expected, Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ExponentWithMultiDigitWholePartIsCorrectlyRounded()
        {
            // reference: 1.23456 rounded to nearest
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse("123.456e-2", CultureInfo.InvariantCulture);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF3C0C1FC8F32378AB0C88A47ECFEA"), RawStorage<TAccelerator>.Bits(value));
        }

        [Theory]
        [InlineData("1e99999999999", false)]
        [InlineData("-1e99999999999", true)]
        public void ExponentBeyondIntRangeIsInfinity(string s, bool negative)
        {
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture);

            Assert.True(Float128<TAccelerator>.IsInfinity(value));
            Assert.Equal(negative, Float128<TAccelerator>.IsNegative(value));
        }

        [Theory]
        [InlineData("1e-99999999999", false)]
        [InlineData("-1e-99999999999", true)]
        public void NegativeExponentBeyondIntRangeIsZero(string s, bool negative)
        {
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture);

            Assert.True(Float128<TAccelerator>.IsZero(value));
            Assert.Equal(negative, RawStorage<TAccelerator>.IsSignSet(value));
        }

        [Fact]
        public void HalfOfSmallestSubnormalOrLessIsZero()
        {
            // 3e-4966 is below half of 2^-16494 (about 3.24e-4966), so it rounds to zero
            Assert.True(Float128<TAccelerator>.IsZero(Float128<TAccelerator>.Parse("3e-4966", CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void TenthToStringLeadingDigitIsSingle()
        {
            // a value below one prints a single digit before the decimal point
            string text = Float128<TAccelerator>.Parse("0.1", CultureInfo.InvariantCulture).ToString();

            Assert.DoesNotMatch(@"^-?0\d", text);
        }

        [Fact]
        public void TenthToStringRoundTripIsNearestDouble()
        {
            string text = Float128<TAccelerator>.Parse("0.1", CultureInfo.InvariantCulture).ToString();

            Assert.Equal(0.1, double.Parse(text, NumberStyles.Float, CultureInfo.CurrentCulture));
        }

        [Fact]
        public void OneEMinusThirtyToStringRoundTripIsNearestDouble()
        {
            string text = Float128<TAccelerator>.Parse("1E-30", CultureInfo.InvariantCulture).ToString();

            Assert.Equal(1e-30, double.Parse(text, NumberStyles.Float, CultureInfo.CurrentCulture));
        }

        [Fact]
        public void LongFractionParseIsNearestDouble()
        {
            // 0.1000000000000000005 rounds to the double 0.1
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse("0.1000000000000000005", CultureInfo.InvariantCulture);

            Assert.Equal(double.Parse("0.1000000000000000005", NumberStyles.Float, CultureInfo.InvariantCulture), (double)value);
        }

        [Fact]
        public void ParseOfTenthIsNearestBinary128()
        {
            // reference: 1/10 rounded to nearest binary128 is 3FFB999999999999999999999999999A
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse("0.1", CultureInfo.InvariantCulture);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFB999999999999999999999999999A"), RawStorage<TAccelerator>.Bits(value));
        }

        [Fact]
        public void ParseOfLongFractionIsNearestBinary128()
        {
            // reference: 1000000000000000005 / 10^19 rounded to nearest binary128 is 3FFB9999999999999A2D2C88282BB6F7
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse("0.1000000000000000005", CultureInfo.InvariantCulture);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFB9999999999999A2D2C88282BB6F7"), RawStorage<TAccelerator>.Bits(value));
        }

        [Fact]
        public void TryFormatIntoTooSmallBufferIsFalse()
        {
            // ISpanFormattable: a destination that cannot hold the text returns false
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse("123.456", CultureInfo.InvariantCulture);
            Span<char> destination = stackalloc char[3];
            Assert.False(value.TryFormat(destination, out int charsWritten, default, CultureInfo.InvariantCulture));
            Assert.Equal(0, charsWritten);
        }
    }

    public class StringConversionTests_DefaultAccelerator :
        StringConversionTests<DefaultAccelerator>
    { }

    public class StringConversionTests_SoftwareAccelerator :
        StringConversionTests<SoftwareAccelerator>
    { }
}