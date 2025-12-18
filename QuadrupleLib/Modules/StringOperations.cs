/*
 *  Copyright 2025 Chosen Few Software
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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace QuadrupleLib;

public partial struct Float128
{
    #region Public API (parsing related)

    public static Float128 Parse(string s, IFormatProvider? provider = null)
    {
        NumberFormatInfo formatter = NumberFormatInfo.GetInstance(provider);
        if (s == formatter.NaNSymbol)
        {
            return _qNaN;
        }
        else if (s == formatter.PositiveInfinitySymbol)
        {
            return _pInf;
        }
        else if (s == formatter.NegativeInfinitySymbol)
        {
            return _nInf;
        }

        var match = Regex.Match(s.Trim(), @$"((?:{Regex.Escape(formatter.NegativeSign)} ?)|\()?(\d+)" +
            @$"(?:{Regex.Escape(formatter.NumberDecimalSeparator)}(\d+))?(?:E(\-?\d+))?" +
            @$"((?: ?{Regex.Escape(formatter.NegativeSign)})|\))?");
        if (!match.Success)
        {
            return _sNaN;
        }
        else
        {
            if (match.Groups[4].Success)
            {
                var qExponent = int.Parse(match.Groups[4].Value);
                var allDigits = $"{match.Groups[2].Value}{match.Groups[3].Value}";

                var builder = new StringBuilder();
                if (qExponent > 0)
                {
                    if (qExponent > allDigits.Length)
                    {
                        builder.Append(allDigits);
                        builder.Append(new string('0', qExponent - allDigits.Length + 1));
                    }
                    else
                    {
                        builder.Append(allDigits);
                        builder.Insert(allDigits.Length - qExponent, formatter.NumberDecimalSeparator);
                    }
                }
                else if (qExponent < 0)
                {
                    builder.Append("0");
                    builder.Append(formatter.NumberDecimalSeparator);
                    builder.Append(new string('0', -qExponent));
                    builder.Append(allDigits);
                }

                return Parse(builder.ToString(), formatter);
            }

            var wholePart = BigInteger.Parse(match.Groups[2].Value);
            var fracPart = match.Groups[3].Success ? BigInteger.Parse(match.Groups[3].Value) : 0;
            var zExponent = match.Groups[3].Value.TakeWhile(ch => ch == '0').Count();

            int resultExponent;
            if (wholePart == 0 && fracPart == 0)
            {
                resultExponent = -EXPONENT_BIAS + 1;
            }
            else
            {
                resultExponent = (int)BigInteger.Log2(wholePart);
                var wholeDiff = (int)wholePart.GetBitLength() - 116;
                if (wholeDiff > 0)
                {
                    wholePart >>= wholeDiff;
                }
                else if (wholeDiff < 0)
                {
                    wholePart <<= -wholeDiff;
                }
            }

            var resultSign =
                (match.Groups[1].Value.StartsWith(formatter.NegativeSign) ^
                match.Groups[5].Value.EndsWith(formatter.NegativeSign)) ||
                (match.Groups[1].Value.StartsWith('(') &&
                match.Groups[5].Value.EndsWith(')'));
            var resultSignificand = (UInt128)wholePart;
            if (fracPart == 0)
            {
                return new Float128(resultSignificand >> 3, resultExponent, resultSign);
            }
            else
            {

                var log10Value = (int)Math.Floor(BigInteger.Log10(fracPart)) + 1;
                bool isSubNormal = zExponent > 4390;
                if (wholePart == 0)
                {
                    resultExponent = -(int)Math.Floor(BigInteger.Log(BigInteger.Pow(10, zExponent), 2)) + 1;
                }

                var pow10 = BigInteger.Pow(10, log10Value + zExponent);
                int binIndex = 114 - resultExponent;
                while (binIndex > 0 && fracPart != 0)
                {
                    fracPart <<= 1;
                    if (fracPart / pow10 == 1)
                    {
                        resultSignificand |= UInt128.One << binIndex;
                    }
                    fracPart %= pow10;
                    --binIndex;
                }

                // set sticky bit
                resultSignificand |= (UInt128)BigInteger.Min(fracPart, 1);

                if ((((resultSignificand & 1) |
                     ((resultSignificand >> 2) & 1)) &
                     ((resultSignificand >> 1) & 1)) == 1) // check rounding condition
                {
                    resultSignificand++; // increment pth bit from the left
                }

                if (isSubNormal)
                {
                    var expnDiff = -EXPONENT_BIAS + 1 - resultExponent + 3;
                    return new Float128(resultSignificand >> expnDiff, -EXPONENT_BIAS + 1, resultSign);
                }
                else
                {
                    var normDist = (short)(UInt128.LeadingZeroCount(resultSignificand >> 3) - 15);
                    if (normDist > 0)
                        resultSignificand <<= normDist;
                    else if (normDist < 0)
                        resultSignificand >>= -normDist;
                    resultExponent -= normDist;

                    return new Float128(resultSignificand >> 3, resultExponent, resultSign);
                }
            }
        }
    }

    public static Float128 Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        return Parse(new string(s), style, provider);
    }

    public static Float128 Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        if (style == NumberStyles.Float)
        {
            return Parse(s);
        }
        else
        {
            throw new ArgumentException($"Parameter must be equal to {nameof(NumberStyles.Float)}.", nameof(style));
        }
    }

    public static Float128 Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Parse(new string(s), provider);
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
    {
        return TryParse(new string(s), style, provider, out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = _sNaN;
        }
        else
        {
            result = Parse(s, style, provider);
        }
        return !IsNaN(result);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
    {
        return TryParse(new string(s), provider, out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128 result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = _sNaN;
        }
        else
        {
            result = Parse(s, provider);
        }
        return !IsNaN(result);
    }

    #endregion

    #region Public API (string formatting related)

    public override string ToString()
    {
        return ToString(null, null);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        NumberFormatInfo formatter = formatProvider is null ?
            new NumberFormatInfo() { NumberDecimalDigits = 38 } :
            NumberFormatInfo.GetInstance(formatProvider);
        if (IsNaN(this))
        {
            return formatter.NaNSymbol;
        }
        else if (IsPositiveInfinity(this))
        {
            return formatter.PositiveInfinitySymbol;
        }
        else if (IsNegativeInfinity(this))
        {
            return formatter.NegativeInfinitySymbol;
        }
        else
        {
            Float128 rounded = Round(this, formatter.NumberDecimalDigits);
            var builder = new StringBuilder();

            BigInteger fracPart;
            int fracDiff;

            BigInteger wholePart = rounded.Significand;
            var wholeDiff = 112 - rounded.Exponent;

            if (wholeDiff > 0)
            {
                wholePart >>= wholeDiff;
                if (rounded.Exponent < 0)
                {
                    var tzc = (int)UInt128.TrailingZeroCount(rounded.Significand);
                    int shift;
                    if (-rounded.Exponent < 112)
                    {
                        shift = -rounded.Exponent;
                        fracDiff = 0;
                    }
                    else
                    {
                        shift = tzc;
                        fracDiff = -rounded.Exponent - shift;
                    }

                    fracPart = (rounded.Significand >> shift) & SIGNIFICAND_MASK;
                }
                else
                {
                    fracDiff = 0;
                    fracPart = (rounded.Significand << rounded.Exponent) & SIGNIFICAND_MASK;
                }
            }
            else
            {
                wholePart <<= -wholeDiff;
                fracPart = 0;
                fracDiff = 0;
            }

            string resultStr;
            if (fracPart == 0)
            {
                var numDigits = (int)Math.Floor(BigInteger.Log10(wholePart));
                if (numDigits >= 20)
                {
                    NumberFormatInfo newFormatter = (NumberFormatInfo)formatter.Clone();
                    newFormatter.NumberGroupSizes = [0];

                    builder.Append(wholePart.ToString(newFormatter));
                    resultStr = $"{builder.ToString().Substring(0, 36).TrimEnd('0').Insert(1, formatter.NumberDecimalSeparator)}E{numDigits}";
                }
                else
                {
                    builder.Append(wholePart.ToString(formatter));
                    resultStr = builder.ToString();
                }
            }
            else
            {
                builder.Append(wholePart.ToString(formatProvider));

                var numDigits = wholePart == 0 ? 0 : builder.Length;
                var decimalExpn = numDigits;

                bool nonZeroFlag = numDigits > 0;
                BigInteger guardDigit = 0;
                BigInteger lastDigit = 0;

                while (!nonZeroFlag)
                {
                    fracPart *= 10;

                    guardDigit = lastDigit;
                    lastDigit = fracPart >> (112 + fracDiff);

                    fracPart &= (BigInteger.One << (112 + fracDiff)) - 1;

                    nonZeroFlag = nonZeroFlag || lastDigit > 0;
                    if (nonZeroFlag) builder.Append(lastDigit);
                    else ++decimalExpn;
                }

                int addDigits = decimalExpn < 20 ? decimalExpn : 0;
                int maxDigits = Math.Min(38, formatter.NumberDecimalDigits);
                while (++numDigits + addDigits < maxDigits)
                {
                    fracPart *= 10;

                    guardDigit = lastDigit;
                    lastDigit = fracPart >> (112 + fracDiff);

                    fracPart &= (BigInteger.One << (112 + fracDiff)) - 1;

                    nonZeroFlag = nonZeroFlag || lastDigit > 0;
                    if (nonZeroFlag) builder.Append(lastDigit);
                    else ++decimalExpn;
                }

                if ((fracPart * 10 >> (112 + fracDiff)) >= 5)
                {
                    if (lastDigit + 1 == 10)
                    {
                        builder.Remove(builder.Length - 2, 2);
                        builder.Append(guardDigit + 1);
                    }
                    else
                    {
                        builder.Remove(builder.Length - 1, 1);
                        builder.Append(lastDigit + 1);
                    }
                }

                if (wholePart == 0 && decimalExpn >= 20)
                {
                    builder.Remove(0, 1);
                    builder.Insert(1, formatter.NumberDecimalSeparator);
                    resultStr = $"{builder.ToString().TrimEnd('0')}E{1 - decimalExpn}";
                }
                else if (wholePart == 0)
                {
                    builder.Insert(0, new string('0', decimalExpn));
                    builder.Insert(1, formatter.NumberDecimalSeparator);
                    resultStr = builder.ToString().TrimEnd('0');
                }
                else
                {
                    builder.Insert(decimalExpn, formatter.NumberDecimalSeparator);
                    resultStr = builder.ToString().TrimEnd('0');
                }
            }

            switch (formatter.NumberNegativePattern)
            {
                case 0:
                    return rounded.RawSignBit ?
                        $"({resultStr})" : resultStr;
                case 1:
                    return (rounded.RawSignBit ? formatter.NegativeSign : string.Empty)
                        + resultStr;
                case 2:
                    return (rounded.RawSignBit ? formatter.NegativeSign : string.Empty)
                        + $" {resultStr}";
                case 3:
                    return resultStr +
                        (rounded.RawSignBit ? formatter.NegativeSign : string.Empty);
                case 4:
                    return $"{resultStr} " +
                        (rounded.RawSignBit ? formatter.NegativeSign : string.Empty);
                default:
                    throw new FormatException($"Invalid {nameof(NumberFormatInfo.NumberNegativePattern)} was specified.");
            }
        }
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        string? formatStr = format.Length == 0 ? null : new string(format);
        string resultStr = ToString(formatStr, provider);

        charsWritten = Math.Min(destination.Length, resultStr.Length);
        return resultStr.TryCopyTo(destination);
    }

    #endregion
}
