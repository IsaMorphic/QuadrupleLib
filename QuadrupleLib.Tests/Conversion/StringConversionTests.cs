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
                Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture),
                Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData("1E99999")]
        [InlineData("1E99999999999")]
        public void ParseExponentAboveRangeIsPositiveInfinity(string s)
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Parse(s)));
        }

        [Theory]
        [InlineData("-1E99999")]
        [InlineData("-1E99999999999")]
        public void ParseNegativeExponentAboveRangeIsNegativeInfinity(string s)
        {
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Parse(s)));
        }

        [Fact]
        public void ParseExponentBelowRangeIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Parse("1E-99999");
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void ParseExponentAtMaxRangeIsFinite()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Parse("1E4932");
            Assert.True(Float128<TAccelerator>.IsFinite(result));
            Assert.False(Float128<TAccelerator>.IsZero(result));
        }

        [Fact]
        public void ParseExponentJustAboveMaxRangeIsPositiveInfinity()
        {
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Parse("1E4933")));
        }

        [Fact]
        public void ParseSubnormalUnderflowBoundaryIsPositiveZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Parse("3.2E-4966");
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void ParseAboveSubnormalUnderflowBoundaryIsSmallestSubnormal()
        {
            // reference: midpoint 2^-16495 is about 3.2375E-4966
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("00000000000000000000000000000001"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Parse("3.3E-4966")));
        }

        [Fact]
        public void ParseNegativeExponentBelowRangeIsNegativeZero()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Parse("-1E-99999");
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(result));
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
            // each input is exact in binary128, so equality is bitwise
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
            // reference: midpoint of 0 and 2^-16494
            Assert.True(Float128<TAccelerator>.IsZero(Float128<TAccelerator>.Parse("3e-4966", CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void TenthToStringLeadingDigitIsSingle()
        {
            string text = Float128<TAccelerator>.Parse("0.1", CultureInfo.InvariantCulture).ToString();

            Assert.DoesNotMatch(@"^-?0\d", text);
        }

        [Fact]
        public void TenthToStringRoundTripIsNearestDouble()
        {
            string text = Float128<TAccelerator>.Parse("0.1", CultureInfo.InvariantCulture).ToString();

            Assert.Equal(0.1, double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void OneEMinusThirtyToStringRoundTripIsNearestDouble()
        {
            string text = Float128<TAccelerator>.Parse("1E-30", CultureInfo.InvariantCulture).ToString();

            Assert.Equal(1e-30, double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void LongFractionParseIsNearestDouble()
        {
            // reference: double 0.1
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
            // ISpanFormattable: short destination
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse("123.456", CultureInfo.InvariantCulture);
            Span<char> destination = stackalloc char[3];
            Assert.False(value.TryFormat(destination, out int charsWritten, default, CultureInfo.InvariantCulture));
            Assert.Equal(0, charsWritten);
        }

        [Fact]
        public void TryFormatIntoSufficientBufferIsTrue()
        {
            // ISpanFormattable: destination fits
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
            // invalid input gives quiet NaN 7FFF8000..., unlike Float128.NaN, which has no quiet bit (see BREAKING_CHANGES.md §1)
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Parse("abc", CultureInfo.InvariantCulture)));

            UInt128 quietNaNBits = RawStorage<TAccelerator>.ParseBits("7FFF8000000000000000000000000000");
            Assert.Equal(quietNaNBits, RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Parse("abc", CultureInfo.InvariantCulture)));
            Assert.True(Float128.IsNaN(Float128.Parse("abc", CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void NonGenericParseOfInvalidStringIsSignalingNaN()
        {
            // the non-generic Parse returns the sign-set signaling NaN, not the quiet NaN of the generic Parse (see BREAKING_CHANGES.md §1)
            UInt128 signalingNaNBits = RawStorage<TAccelerator>.ParseBits("FFFF0000000000000000000000000001");
            Assert.Equal(signalingNaNBits, RawStorage<TAccelerator>.Bits(Float128.Parse("abc", CultureInfo.InvariantCulture)));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(Float128.Parse("abc", CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void NonGenericTryParseOfInvalidStringIsFalseWithSignalingNaN()
        {
            bool parsed = Float128.TryParse("abc", CultureInfo.InvariantCulture, out Float128 value);

            Assert.False(parsed);
            UInt128 signalingNaNBits = RawStorage<TAccelerator>.ParseBits("FFFF0000000000000000000000000001");
            Assert.Equal(signalingNaNBits, RawStorage<TAccelerator>.Bits(value));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(value));
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
            // reference: largest finite binary128 is about 1.19e4932
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Parse("1e4933", CultureInfo.InvariantCulture)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Parse("-1e4933", CultureInfo.InvariantCulture)));
        }

        [Fact]
        public void ParseWithParenthesesPatternIsNegative()
        {
            // NumberNegativePattern 0 is the parenthesized form
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
        public void ToStringWithNegativePatternIsSignPlacedCorrectly(int pattern, string prefix, string suffix)
        {
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberNegativePattern = pattern, NumberDecimalDigits = 38 };
            string text = ((Float128<TAccelerator>)(-2.0)).ToString(formatter);

            Assert.StartsWith(prefix, text);
            Assert.EndsWith(suffix, text);
        }

        [Fact]
        public void ToStringWithInvalidNegativePatternIsFormatException()
        {
            // the setter rejects 5, so reflection reaches the FormatException branch
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberDecimalDigits = 38 };
            typeof(NumberFormatInfo).GetField("_numberNegativePattern", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(formatter, 5);

            Assert.Throws<FormatException>(() => ((Float128<TAccelerator>)(-2.0)).ToString(formatter));
        }

        [Theory]
        [InlineData("1e99999999999999999999", false)]
        [InlineData("-1e99999999999999999999", true)]
        public void ExponentBeyondLongRangeIsInfinity(string s, bool negative)
        {
            // clamped exponent
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture);

            Assert.True(Float128<TAccelerator>.IsInfinity(value));
            Assert.Equal(negative, Float128<TAccelerator>.IsNegative(value));
        }

        [Fact]
        public void ParseRoundingCarryIntoNewBitIsOne()
        {
            // carry into a new leading bit
            string s = "0." + new string('9', 35);
            Float128<TAccelerator> value = Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture);

            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF0000000000000000000000000000"), RawStorage<TAccelerator>.Bits(value));
        }

        [Fact]
        public void ParseRoundingCarryOverflowIsInfinity()
        {
            // tie past the largest finite value overflows to infinity
            BigInteger x = (BigInteger.One << 16384) - (BigInteger.One << 16270);
            string s = x.ToString(CultureInfo.InvariantCulture);

            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Parse(s, CultureInfo.InvariantCulture)));
            Assert.True(Float128<TAccelerator>.IsNegativeInfinity(Float128<TAccelerator>.Parse("-" + s, CultureInfo.InvariantCulture)));
        }

        // reference: double.ToString("F" + digits) with trailing zeros trimmed; inputs are exact in double and binary128
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
        public void ToStringWithDecimalDigitsIsDoubleFixedFormat(double x, int digits)
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
        public void ToStringWithDecimalDigitsIsDoubleExponentFormat(double x, int digits)
        {
            NumberFormatInfo formatter = new NumberFormatInfo() { NumberDecimalDigits = digits };
            Float128<TAccelerator> value = x;

            string[] parts = x.ToString("E" + digits, CultureInfo.InvariantCulture).Split('E');
            int exponent = int.Parse(parts[1], CultureInfo.InvariantCulture);
            string expected = $"{TrimFractionZeros(parts[0])}E{(exponent < 0 ? "-" : "+")}{System.Math.Abs(exponent)}";

            Assert.Equal(expected, value.ToString(formatter));
        }

        [Fact]
        public void ToStringWithInvariantCultureIsTwoDecimalPlaces()
        {
            // 0.125 is exact in binary128 and a tie at two decimal places; ties round to even, so the result is "0.12"
            Float128<TAccelerator> value = 0.125;

            Assert.Equal("0.12", value.ToString(CultureInfo.InvariantCulture));
        }

        // reference: double.ToString("F38"); inputs are exact in double and binary128
        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(2.5)]
        [InlineData(123.456)]
        [InlineData(-123.456)]
        public void ToStringWithNoProviderIs38DecimalPlaces(double x)
        {
            // precondition: de-DE must use a comma, or this test would not show that the culture is ignored
            Assert.Equal(",", CultureInfo.GetCultureInfo("de-DE").NumberFormat.NumberDecimalSeparator);

            CultureInfo previous = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            try
            {
                Float128<TAccelerator> value = x;

                Assert.Equal(TrimFractionZeros(x.ToString("F38", CultureInfo.InvariantCulture)), value.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Fact]
        public void ToStringWithNoProviderIsExponentFormZerosTrimmed()
        {
            // precondition: de-DE must use a comma, or this test would not show that the culture is ignored
            Assert.Equal(",", CultureInfo.GetCultureInfo("de-DE").NumberFormat.NumberDecimalSeparator);

            CultureInfo previous = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            try
            {
                // reference: 1.5E30 = 15 * 5^29 * 2^29, exact in binary128
                Float128<TAccelerator> value = Float128<TAccelerator>.Parse("1.5E30", CultureInfo.InvariantCulture);

                Assert.Equal("1.5E+30", value.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
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