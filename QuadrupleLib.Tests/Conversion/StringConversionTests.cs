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
using System.Numerics;
using System.Reflection;
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

        [Fact]
        public void TryFormatIntoSufficientBufferIsTrue()
        {
            // ISpanFormattable: a destination that holds the text copies it and returns true
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberDecimalDigits = 38 };
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse("123.456", formatter);
            Span<char> destination = stackalloc char[64];
            bool success = value.TryFormat(destination, out int charsWritten, default, formatter);

            Assert.True(success);
            Assert.Equal(123.456, double.Parse(new string(destination[..charsWritten]), NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ParseOfSpecialSymbolsIsCorrect()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Parse("NaN", CultureInfo.InvariantCulture)));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Parse("Infinity", CultureInfo.InvariantCulture)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Parse("-Infinity", CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void ParseOfInvalidStringIsNaN()
        {
            // a string that is not a number fails to match and yields a NaN
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Parse("abc", CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void ParseOfZeroIsZero()
        {
            Assert.True(Float128<TAccelerator>.IsZero(Float128<TAccelerator>.Parse("0", CultureInfo.InvariantCulture)));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(Float128<TAccelerator>.Parse("0", CultureInfo.InvariantCulture)));
            Assert.True(Float128<TAccelerator>.IsZero(Float128<TAccelerator>.Parse("-0", CultureInfo.InvariantCulture)));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(Float128<TAccelerator>.Parse("-0", CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void ParseOfOverflowingValueIsInfinity()
        {
            // 1e4933 exceeds the largest finite binary128 value (~1.18e4932)
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Parse("1e4933", CultureInfo.InvariantCulture)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Parse("-1e4933", CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void ParseWithParenthesesPatternIsNegative()
        {
            // negative pattern 0: negative numbers are wrapped in parentheses
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberNegativePattern = 0 };
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse("(1.5)", formatter);

            Assert.Equal(-1.5, (double)value);
        }

        [Fact]
        public void ParseSpanWithStyleIsCorrect()
        {
            ReadOnlySpan<char> span = "1.5";
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse(span, NumberStyles.Float, CultureInfo.InvariantCulture);

            Assert.Equal(1.5, (double)value);
        }

        [Fact]
        public void ParseWithNonFloatStyleIsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Float128<TAccelerator>.Parse("1.5", NumberStyles.Integer, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ParseSpanIsCorrect()
        {
            ReadOnlySpan<char> span = "2.5";
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse(span, CultureInfo.InvariantCulture);

            Assert.Equal(2.5, (double)value);
        }

        [Fact]
        public void TryParseSpanWithStyleIsCorrect()
        {
            ReadOnlySpan<char> span = "3.5";
            bool parsed = Float128<TAccelerator>.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out Float128<TAccelerator> value);

            Assert.True(parsed);
            Assert.Equal(3.5, (double)value);
        }

        [Fact]
        public void TryParseOfNullOrEmptyIsFalse()
        {
            Assert.False(Float128<TAccelerator>.TryParse(null, NumberStyles.Float, CultureInfo.InvariantCulture, out Float128<TAccelerator> value));
            Assert.True(Float128<TAccelerator>.IsNaN(value));
            Assert.False(Float128<TAccelerator>.TryParse("", NumberStyles.Float, CultureInfo.InvariantCulture, out value));
        }

        [Fact]
        public void TryParseSpanIsCorrect()
        {
            ReadOnlySpan<char> span = "4.5";
            bool parsed = Float128<TAccelerator>.TryParse(span, CultureInfo.InvariantCulture, out Float128<TAccelerator> value);

            Assert.True(parsed);
            Assert.Equal(4.5, (double)value);
        }

        [Fact]
        public void TryParseIsCorrect()
        {
            bool parsed = Float128<TAccelerator>.TryParse("5.5", CultureInfo.InvariantCulture, out Float128<TAccelerator> value);

            Assert.True(parsed);
            Assert.Equal(5.5, (double)value);

            Assert.False(Float128<TAccelerator>.TryParse(null, CultureInfo.InvariantCulture, out value));
        }

        [Fact]
        public void ToStringWithProviderIsCorrect()
        {
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberDecimalDigits = 38 };
            string text = ((Float128<TAccelerator>)1.5).ToString(formatter);

            Assert.Equal(1.5, double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ToStringOfSpecialValuesIsSymbol()
        {
            Assert.Equal("NaN", Float128<TAccelerator>.NaN.ToString());
            Assert.Equal("Infinity", Float128<TAccelerator>.PositiveInfinity.ToString());
            Assert.Equal("-Infinity", Float128<TAccelerator>.NegativeInfinity.ToString());
            Assert.Equal("0", Float128<TAccelerator>.Zero.ToString());
            Assert.Equal("-0", Float128<TAccelerator>.NegativeZero.ToString());
        }

        [Theory]
        [InlineData(0, "(", ")")]
        [InlineData(1, "-", "")]
        [InlineData(2, "- ", "")]
        [InlineData(3, "", "-")]
        [InlineData(4, "", " -")]
        public void ToStringWithNegativePatternPlacesSignCorrectly(int pattern, string prefix, string suffix)
        {
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberNegativePattern = pattern, NumberDecimalDigits = 38 };
            string text = ((Float128<TAccelerator>)(-2.0)).ToString(formatter);

            Assert.StartsWith(prefix, text);
            Assert.EndsWith(suffix, text);
        }

        [Fact]
        public void ToStringWithInvalidNegativePatternIsFormatException()
        {
            // the public setter validates 0-4, so set the backing field directly to reach the Format switch's default branch
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberDecimalDigits = 38 };
            typeof(NumberFormatInfo).GetField("_numberNegativePattern", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(formatter, 5);

            Assert.Throws<FormatException>(() => ((Float128<TAccelerator>)(-2.0)).ToString(formatter));
        }

        [Theory]
        [InlineData("1e99999999999999999999", false)]
        [InlineData("-1e99999999999999999999", true)]
        public void ExponentBeyondLongRangeIsInfinity(string s, bool negative)
        {
            // a 20-digit exponent overflows a long, so it is clamped to half the long range,
            // which is far beyond the largest finite value
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture);

            Assert.True(Float128<TAccelerator>.IsInfinity(value));
            Assert.Equal(negative, Float128<TAccelerator>.IsNegative(value));
        }

        [Fact]
        public void ParseRoundingCarryIntoNewBitIsOne()
        {
            // 1 - 10^-35 lies above the midpoint to 1.0, so it rounds up through a carry
            // into a new leading bit, giving exactly 1.0
            string s = "0." + new string('9', 35);
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000000"), RawStorage<TAccelerator>.Bits(value));
        }

        [Fact]
        public void ParseRoundingCarryOverflowIsInfinity()
        {
            // 2^16384 - 2^16270 is the exact midpoint between the largest finite value
            // (2^16384 - 2^16271) and 2^16384, so it is a tie that rounds up through a
            // carry past the largest finite exponent, overflowing to infinity
            BigInteger x = (BigInteger.One << 16384) - (BigInteger.One << 16270);
            string s = x.ToString(CultureInfo.InvariantCulture);

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Parse("-" + s, CultureInfo.InvariantCulture)));
        }

        // reference: double.ToString("F" + digits), the exact decimal value rounded half to even, with the
        // trailing zeros after the decimal point removed as Float128 does; each input is exact in double and binary128
        [Theory]
        [InlineData(0.0123456, 2)]
        [InlineData(0.123456, 1)]
        [InlineData(1.23456, 3)]
        [InlineData(9.96, 0)]
        [InlineData(99.96, 1)]
        [InlineData(123.456, 0)]
        [InlineData(12345.678, 1)]
        [InlineData(2.5, 0)]
        [InlineData(3.5, 0)]
        [InlineData(0.125, 2)]
        [InlineData(0.375, 2)]
        [InlineData(0.75, 0)]
        [InlineData(0.25, 0)]
        [InlineData(-0.25, 0)]
        [InlineData(0.000123456, 2)]
        [InlineData(-12.3456, 2)]
        public void ToStringWithDecimalDigitsMatchesDoubleFixedFormat(double x, int digits)
        {
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberDecimalDigits = digits };
            Float128<TAccelerator> value = x;

            Assert.Equal(TrimFractionZeros(x.ToString("F" + digits, CultureInfo.InvariantCulture)), value.ToString(formatter));
        }

        // reference: double.ToString("E" + digits) with its exponent written without padding, as Float128 does
        [Theory]
        [InlineData(9.96875E30, 2)]
        [InlineData(9.96875E30, 0)]
        [InlineData(9.96875E30, 1)]
        [InlineData(1.5E30, 0)]
        [InlineData(1.5E30, 38)]
        [InlineData(1.23456E-30, 3)]
        [InlineData(-1.23456E-30, 1)]
        public void ToStringWithDecimalDigitsMatchesDoubleExponentFormat(double x, int digits)
        {
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberDecimalDigits = digits };
            Float128<TAccelerator> value = x;

            string[] parts = x.ToString("E" + digits, CultureInfo.InvariantCulture).Split('E');
            int exponent = int.Parse(parts[1], CultureInfo.InvariantCulture);
            string expected = $"{TrimFractionZeros(parts[0])}E{(exponent < 0 ? "-" : "+")}{System.Math.Abs(exponent)}";

            Assert.Equal(expected, value.ToString(formatter));
        }

        [Fact]
        public void ToStringWithInvariantCultureHasTwoDecimalPlaces()
        {
            // InvariantCulture's NumberDecimalDigits is 2, and 0.125 is a tie that rounds to the even digit
            Float128<TAccelerator> value = 0.125;

            Assert.Equal("0.12", value.ToString(CultureInfo.InvariantCulture));
        }

        // reference: double.ToString("F38"); with no provider, ToString keeps 38 decimal places in plain
        // notation, and each input is exact in double and binary128
        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(2.5)]
        [InlineData(123.456)]
        [InlineData(-123.456)]
        public void ToStringWithNoProviderHas38DecimalPlaces(double x)
        {
            Float128<TAccelerator> value = x;

            Assert.Equal(TrimFractionZeros(x.ToString("F38", CultureInfo.InvariantCulture)), value.ToString());
        }

        [Fact]
        public void ToStringWithNoProviderTrimsExponentFormZeros()
        {
            // 1.5 * 10^30 = 15 * 5^29 * 2^29 with 15 * 5^29 < 2^113, so it is exact in binary128 (unlike in
            // double), and E notation drops the mantissa's trailing zeros
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse("1.5E30", CultureInfo.InvariantCulture);

            Assert.Equal("1.5E+30", value.ToString());
        }

        private static string TrimFractionZeros(string s)
        {
            return s.Contains('.') ? s.TrimEnd('0').TrimEnd('.') : s;
        }
    }

    public class StringConversionTests_DefaultAccelerator :
        StringConversionTests<DefaultAccelerator>
    { }

    public class StringConversionTests_SoftwareAccelerator :
        StringConversionTests<SoftwareAccelerator>
    { }
}