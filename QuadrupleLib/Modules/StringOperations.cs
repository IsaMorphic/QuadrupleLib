/*
 *  Copyright 2024-2026 Chosen Few Software
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

public partial struct Float128<TAccelerator>
{
    #region Public API (parsing related)

    public static Float128<TAccelerator> Parse(string s, IFormatProvider? provider = null)
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

        var match = Regex.Match(s.Trim(), @$"((?:{Regex.Escape(formatter.NegativeSign)} ?)|\()?(\d*)" +
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
                    if (qExponent >= allDigits.Length)
                    {
                        builder.Append(allDigits);
                        builder.Append(new string('0', qExponent - allDigits.Length + 1));
                    }
                    else
                    {
                        builder.Append(allDigits);
                        builder.Insert(qExponent + 1, formatter.NumberDecimalSeparator);
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

            var wholePart = match.Groups[2].Length > 0 ? BigInteger.Parse(match.Groups[2].Value) : 0;
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
                return new Float128<TAccelerator>(resultSignificand >> 3, resultExponent, resultSign);
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
                    return new Float128<TAccelerator>(resultSignificand >> expnDiff, -EXPONENT_BIAS + 1, resultSign);
                }
                else
                {
                    var normDist = (short)(UInt128.LeadingZeroCount(resultSignificand >> 3) - 15);
                    if (normDist > 0)
                        resultSignificand <<= normDist;
                    else if (normDist < 0)
                        resultSignificand >>= -normDist;
                    resultExponent -= normDist;

                    return new Float128<TAccelerator>(resultSignificand >> 3, resultExponent, resultSign);
                }
            }
        }
    }

    public static Float128<TAccelerator> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        return Parse(new string(s), style, provider);
    }

    public static Float128<TAccelerator> Parse(string s, NumberStyles style, IFormatProvider? provider)
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

    public static Float128<TAccelerator> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Parse(new string(s), provider);
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128<TAccelerator> result)
    {
        return TryParse(new string(s), style, provider, out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128<TAccelerator> result)
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

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128<TAccelerator> result)
    {
        return TryParse(new string(s), provider, out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128<TAccelerator> result)
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

    private const int MAX_STRING_DIGITS = 34;

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
            Float128<TAccelerator> rounded = Round(this, formatter.NumberDecimalDigits);

            int m = (int)UInt128.TrailingZeroCount(rounded.Significand);
            int e2 = rounded.Exponent - Math.Abs(m - 112);

            BigInteger significand = e2 switch
            {
                >= 0 => rounded.Significand >> m,
                < 0 => (rounded.Significand >> m) * BigInteger.Pow(5, -e2)
            };

            (BigInteger n, BigInteger r) = BigInteger.DivRem(significand, 10);
            while (n > 0 && r == 0)
            {
                (n, r) = BigInteger.DivRem(n, 10);
            }

            StringBuilder result = new();
            if (n == 0)
            {
                result.Append(r); 
                --e2;
            }
            else if (r == 0)
            {
                result.Append(n);
            }
            else
            {
                result.Append(n);
                result.Append(r);
                --e2;
            }

            if (result.Length > MAX_STRING_DIGITS)
            {
                e2 += result.Length - MAX_STRING_DIGITS;
                int lastDigit = result[MAX_STRING_DIGITS] - '0';
                int nextDigit = result[MAX_STRING_DIGITS - 1] - '0';
                result.Remove(MAX_STRING_DIGITS - 1, result.Length - MAX_STRING_DIGITS);
                if (lastDigit < 5)
                {
                    result.Append(nextDigit);
                    --e2;
                }
                else
                {
                    for (int i = result.Length - 1; i >= 0 && ++nextDigit > 9; i--)
                    {
                        result[i] = (char)(nextDigit % 10 + '0');
                        if (i > 0) nextDigit = result[i - 1] - '0';
                    }

                    if (++nextDigit > 9)
                    {
                        result.Insert(0, '1');
                    }
                }
            }

            bool expFlag;
            int offset = result.Length + e2;
            if (offset < 0 && result.Length - offset <= MAX_STRING_DIGITS)
            {
                for (int i = offset; i < 0; i++)
                {
                    result.Insert(0, '0');
                }
                expFlag = false;
            }
            else if (offset >= 0 && result.Length + offset <= MAX_STRING_DIGITS)
            {
                for (int i = 0; i < offset; i++)
                {
                    result.Append('0');
                }
                expFlag = false;
            }
            else
            {
                expFlag = true;
            }

            int trimIdx;
            for (trimIdx = result.Length - 1; trimIdx > 0 && result[trimIdx] == '0'; trimIdx--) ;
            result.Remove(trimIdx + 1, result.Length - trimIdx - 1);
            e2 += result.Length - trimIdx - 1;

            if (expFlag)
            {
                if (result.Length > 1)
                {
                    result.Insert(1, '.');
                }

                result.Append('E');
                result.Append(offset);
            }
            else if (e2 < 0 && result.Length + e2 + 1 < result.Length)
            {
                result.Insert(result.Length + e2 + 1, '.');
            }

            string resultStr = result.ToString();
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
