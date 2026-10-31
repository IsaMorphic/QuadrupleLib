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

using QuadrupleLib.Annotations;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace QuadrupleLib;

public partial struct Float128<TAccelerator>
{
    #region Private API (regex pattern related)

    [MakeNonGeneric]
    private static string RegexEscape(string s)
    {
        return Regex.Replace(s, @"([\\\*\+\?\|\{\[\(\)\^\$\.\#\-\s])", @"\$1");
    }

    #endregion

    #region Public API (parsing related)

    [MakeNonGeneric]
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

        Match match = Regex.Match(s, @$"^((?:{RegexEscape(formatter.NegativeSign)} ?)|\()?(\d*)" +
            @$"(?:{RegexEscape(formatter.NumberDecimalSeparator)}(\d+))?(?:E(\-?\d+))?" +
            @$"((?: ?{RegexEscape(formatter.NegativeSign)})|\))?$");
        if (!match.Success)
        {
            return _sNaN;
        }
        else
        {
            ReadOnlySpan<char> leftSignSpan = match.Groups[1].ValueSpan;
            ReadOnlySpan<char> wholePartSpan = match.Groups[2].ValueSpan;
            ReadOnlySpan<char> fracPartSpan = match.Groups[3].ValueSpan;
            ReadOnlySpan<char> exponentSpan = match.Groups[4].ValueSpan;
            ReadOnlySpan<char> rightSignSpan = match.Groups[5].ValueSpan;
            if (int.TryParse(exponentSpan, out int qExponent))
            {
                int numDigits = Math.Max(wholePartSpan.Length, 1) + fracPartSpan.Length;

                StringBuilder builder = new();
                builder.Append(leftSignSpan);

                if (qExponent > 0)
                {
                    if (qExponent >= numDigits)
                    {
                        builder.Append(wholePartSpan);
                        builder.Append(fracPartSpan);
                        for (int i = 0; i < qExponent - numDigits + 1; i++)
                        {
                            builder.Append('0');
                        }
                    }
                    else
                    {
                        builder.Append(wholePartSpan);
                        builder.Append(fracPartSpan);
                        builder.Insert(qExponent + 1, formatter.NumberDecimalSeparator);
                    }
                }
                else if (qExponent < 0)
                {
                    builder.Append('0');
                    builder.Append(formatter.NumberDecimalSeparator);
                    for (int i = 0; i < -qExponent; i++)
                    {
                        builder.Append('0');
                    }
                    builder.Append(wholePartSpan);
                    builder.Append(fracPartSpan);
                }

                builder.Append(rightSignSpan);
                return Parse(builder.ToString(), formatter);
            }

            BigInteger.TryParse(wholePartSpan, out BigInteger wholePart);
            BigInteger.TryParse(fracPartSpan, out BigInteger fracPart);

            int zExponent;
            for (zExponent = 0;
                zExponent < fracPartSpan.Length &&
                fracPartSpan[zExponent] == '0';
                zExponent++) ;

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

            bool resultSign;
            switch (formatter.NumberNegativePattern)
            {
                case 0:
                    resultSign =
                        leftSignSpan.StartsWith('(') &&
                        rightSignSpan.EndsWith(')');
                    break;
                default:
                    resultSign = 
                        leftSignSpan.StartsWith(formatter.NegativeSign) ^
                        rightSignSpan.EndsWith(formatter.NegativeSign);
                    break;
            }

            UInt128 resultSignificand = (UInt128)wholePart;
            if (fracPart == 0)
            {
                return new Float128<TAccelerator>(resultSignificand >> 3, resultExponent, resultSign);
            }
            else
            {

                int log10Value = (int)Math.Floor(BigInteger.Log10(fracPart)) + 1;
                bool isSubNormal = zExponent > 4390;
                if (wholePart == 0)
                {
                    resultExponent = -(int)Math.Floor(BigInteger.Log(BigInteger.Pow(10, zExponent), 2)) + 1;
                }

                BigInteger pow10 = BigInteger.Pow(10, log10Value + zExponent);
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
                    int expnDiff = -EXPONENT_BIAS + 1 - resultExponent + 3;
                    return new Float128<TAccelerator>(resultSignificand >> expnDiff, -EXPONENT_BIAS + 1, resultSign);
                }
                else
                {
                    int normDist = (int)UInt128.LeadingZeroCount(resultSignificand >> 3) - 15;
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

    [MakeNonGeneric]
    public static Float128<TAccelerator> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        return Parse(new string(s), style, provider);
    }

    [MakeNonGeneric]
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

    [MakeNonGeneric]
    public static Float128<TAccelerator> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Parse(new string(s), provider);
    }

    [MakeNonGeneric]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128<TAccelerator> result)
    {
        return TryParse(new string(s), style, provider, out result);
    }

    [MakeNonGeneric]
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

    [MakeNonGeneric]
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Float128<TAccelerator> result)
    {
        return TryParse(new string(s), provider, out result);
    }

    [MakeNonGeneric]
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
    private const int MAX_PADDING_DIGITS = 23;

    [MakeNonGeneric]
    public override string ToString()
    {
        return ToString(null, null);
    }

    [MakeNonGeneric]
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        NumberFormatInfo formatter = formatProvider is null ?
            new NumberFormatInfo() { NumberDecimalDigits = MAX_STRING_DIGITS } :
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
        else if (IsZero(this))
        {
            return "0";
        }
        else
        {
            int m = (int)UInt128.TrailingZeroCount(Significand);
            int e2 = Exponent - Math.Abs(m - 112);

            BigInteger significand = e2 switch
            {
                >= 0 => (Significand >> m) / BigInteger.Pow(5, e2),
                < 0 => (Significand >> m) * BigInteger.Pow(5, -e2)
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

            bool expFlag;
            int offset = result.Length + e2;
            int numDigits = formatter.NumberDecimalDigits;
            if (offset < 0 && offset >= -MAX_PADDING_DIGITS)
            {
                for (int i = offset; i < 0; i++)
                {
                    result.Insert(0, '0');
                }

                expFlag = false;
            }
            else if (offset >= 0 && offset <= MAX_PADDING_DIGITS)
            {
                for (int i = 0; i <= offset; i++)
                {
                    result.Append('0');
                }

                e2 -= offset + 1;
                expFlag = false;
            }
            else
            {
                expFlag = true;
            }

            if (result.Length > numDigits - 1)
            {
                e2 += result.Length - numDigits;

                int lastDigit = result[numDigits - 1] - '0';
                int nextDigit = result[numDigits - 2] - '0';

                result.Remove(numDigits - 1, result.Length - numDigits);
                if (lastDigit >= 5)
                {
                    int i;
                    for (i = result.Length - 1; i >= 0 && ++nextDigit > 9; i--)
                    {
                        result[i] = (char)(nextDigit % 10 + '0');
                        if (i > 0) nextDigit = result[i - 1] - '0';
                    }

                    if (i < 0 && nextDigit > 9)
                    {
                        result.Remove(numDigits - 1, 1);
                        result.Insert(0, '1');
                    }
                }
            }

            if (offset < 0)
            {
                int trimIdx;
                for (trimIdx = result.Length - 1; trimIdx > 0 && result[trimIdx] == '0'; trimIdx--) ;

                e2 += result.Length - trimIdx - 1;
                result.Remove(trimIdx + 1, result.Length - trimIdx - 1);
            }

            if (expFlag)
            {
                if (result.Length > 1)
                {
                    result.Insert(1, formatter.NumberDecimalSeparator);
                }

                result.Append('E');
                result.Append(offset);
            }
            else if (e2 < 0 && result.Length + e2 + 1 < result.Length)
            {
                result.Insert(result.Length + e2 + 1, formatter.NumberDecimalSeparator);
            }

            switch (formatter.NumberNegativePattern)
            {
                case 0:
                    if (RawSignBit)
                    {
                        result.Insert(0, '(');
                        result.Append(')');
                    }
                    break;
                case 1:
                    if (RawSignBit)
                    {
                        result.Insert(0, formatter.NegativeSign);
                    }
                    break;
                case 2:
                    if (RawSignBit)
                    {
                        result.Insert(0, ' ');
                        result.Insert(0, formatter.NegativeSign);
                    }
                    break;
                case 3:
                    if (RawSignBit)
                    {
                        result.Append(formatter.NegativeSign);
                    }
                    break;
                case 4:
                    if (RawSignBit)
                    {
                        result.Append(' ');
                        result.Append(formatter.NegativeSign);
                    }
                    break;
                default:
                    throw new FormatException($"Invalid {nameof(NumberFormatInfo.NumberNegativePattern)} was specified.");
            }

            return result.ToString();
        }
    }

    [MakeNonGeneric]
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        string? formatStr = format.Length == 0 ? null : new string(format);
        string resultStr = ToString(formatStr, provider);

        charsWritten = Math.Min(destination.Length, resultStr.Length);
        return resultStr.TryCopyTo(destination);
    }

    #endregion
}
