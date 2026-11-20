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
    #region Public API (parsing related)

    [MakeNonGeneric]
    public static Float128<TAccelerator> Parse(string s)
    {
        return Parse(s, null);
    }

    [MakeNonGeneric]
    public static Float128<TAccelerator> Parse(string s, IFormatProvider? provider)
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

        Match match = Regex.Match(s, @$"^((?:{Utilities.Regex.Escape(formatter.NegativeSign)} ?)|\()?(\d*)" +
            @$"(?:{Utilities.Regex.Escape(formatter.NumberDecimalSeparator)}(\d+))?" +
            @$"(?:[eE]((?:(?:{Utilities.Regex.Escape(formatter.PositiveSign)})|" +
            @$"(?:{Utilities.Regex.Escape(formatter.NegativeSign)}))?\d+))?" +
            @$"((?: ?{Utilities.Regex.Escape(formatter.NegativeSign)})|\))?$");
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

            BigInteger.TryParse(wholePartSpan, out BigInteger wholePart);
            BigInteger.TryParse(fracPartSpan, out BigInteger fracPart);

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

            // the value is exactly (wholePart * 10^L + fracPart) * 10^(E - L),
            // where L is the number of fraction digits and E is the decimal exponent
            BigInteger numerator = wholePart * Utilities.PowCache.GetPow10(fracPartSpan.Length) + fracPart;
            if (numerator.IsZero)
            {
                return resultSign ? NegativeZero : Zero;
            }

            // an exponent beyond the range of a long is far outside the range of the format
            if (!long.TryParse(exponentSpan, NumberStyles.AllowLeadingSign, formatter, out long decimalExponent) && !exponentSpan.IsEmpty)
            {
                decimalExponent = exponentSpan.StartsWith(formatter.NegativeSign) ? long.MinValue / 2 : long.MaxValue / 2;
            }

            // 1 <= numerator < 10^digitCount, so these bounds put the value beyond the largest
            // finite value, or below half the smallest subnormal value
            long scale = decimalExponent - fracPartSpan.Length;
            long digitCount = wholePartSpan.Length + fracPartSpan.Length;
            if (scale > 4933)
            {
                return resultSign ? _nInf : _pInf;
            }
            else if (scale + digitCount < -4966)
            {
                return resultSign ? NegativeZero : Zero;
            }

            BigInteger denominator = BigInteger.One;
            if (scale >= 0)
            {
                numerator *= Utilities.PowCache.GetPow10((int)scale);
            }
            else
            {
                denominator = Utilities.PowCache.GetPow10((int)-scale);
            }

            // binary exponent of the leading bit, floor(log2(numerator / denominator))
            int resultExponent = (int)(numerator.GetBitLength() - denominator.GetBitLength());
            if (resultExponent >= 0 ? numerator < denominator << resultExponent : numerator << -resultExponent < denominator)
            {
                resultExponent--;
            }

            if (resultExponent > EXPONENT_BIAS) // overflowing result
            {
                return resultSign ? _nInf : _pInf;
            }

            // a subnormal result keeps fewer significant bits
            resultExponent = Math.Max(resultExponent, -EXPONENT_BIAS + 1);

            // scale the quotient to 113 significant bits, then round to nearest, ties to even
            int shift = 112 - resultExponent;
            BigInteger scaledNumerator = shift >= 0 ? numerator << shift : numerator;
            BigInteger scaledDenominator = shift >= 0 ? denominator : denominator << -shift;
            BigInteger quotient = BigInteger.DivRem(scaledNumerator, scaledDenominator, out BigInteger remainder);

            int halfComparison = (remainder << 1).CompareTo(scaledDenominator);
            if (halfComparison > 0 || (halfComparison == 0 && !quotient.IsEven))
            {
                quotient++;
            }

            UInt128 resultSignificand = (UInt128)quotient;
            if ((resultSignificand >> 113) != 0) // rounding carried into a new leading bit
            {
                resultSignificand >>= 1;
                resultExponent++;
            }

            if (resultExponent > EXPONENT_BIAS) // overflowing result
            {
                return resultSign ? _nInf : _pInf;
            }

            return new Float128<TAccelerator>(resultSignificand, resultExponent, resultSign);
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
        if (string.IsNullOrEmpty(s))
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
        if (string.IsNullOrEmpty(s))
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

    private const int MAX_STRING_DIGITS = 38;
    private const int MAX_PADDING_DIGITS = 23;

    [MakeNonGeneric]
    public override string ToString()
    {
        return ToString(null, null);
    }

    [MakeNonGeneric]
    public string ToString(IFormatProvider? provider)
    {
        return ToString(null, provider);
    }

    [MakeNonGeneric]
    public string ToString(string? format, IFormatProvider? provider)
    {
        NumberFormatInfo formatter = provider is null ?
            new NumberFormatInfo() { NumberDecimalDigits = MAX_STRING_DIGITS } :
            NumberFormatInfo.GetInstance(provider);
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
            return RawSignBit ? "-0" : "0";
        }
        else
        {
            int m = (int)UInt128.TrailingZeroCount(Significand);
            int e2 = Exponent + m - 112;

            BigInteger significand;
            switch (e2)
            {
                case >= 0:
                    int adj = e2 - m;
                    significand = adj switch
                    {
                        >= 0 => (BigInteger)Significand << adj,
                        < 0 => Significand >> -adj,
                    };
                    break;
                case < 0:
                    BigInteger pow5 = Utilities.PowCache.GetPow5(-e2);
                    significand = (Significand >> m) * pow5;
                    break;
            }

            int maxDigits = significand == 0 ? 1 :
                (int)Math.Floor(BigInteger.Log10(significand)) + 1;
            Span<char> strBuf = stackalloc char[maxDigits + 1];
            significand.TryFormat(strBuf, out maxDigits);

            StringBuilder result = new StringBuilder(maxDigits);
            result.Append(strBuf[..maxDigits]);

            bool expFlag;
            int offset = e2 < 0 ? maxDigits + --e2 : maxDigits - 1;
            if (offset <= 0 && offset >= -MAX_PADDING_DIGITS)
            {
                for (int i = offset; i < 0; i++)
                {
                    result.Insert(0, '0');
                }

                expFlag = false;
            }
            else
            {
                expFlag = Math.Abs(offset) >= MAX_PADDING_DIGITS;
            }

            int numDigits = offset <= 0 || expFlag ? formatter.NumberDecimalDigits :
                Math.Min(formatter.NumberDecimalDigits + offset + 1, result.Length);
            if (result.Length > numDigits)
            {
                e2 += result.Length - numDigits;

                int lastDigit = result[numDigits - 1] - '0';
                int nextDigit = result[numDigits - 2] - '0';

                result.Remove(numDigits - 1, result.Length - numDigits);
                if (lastDigit >= 5)
                {
                    int i;
                    for (i = result.Length - 2; i >= 0; i--)
                    {
                        nextDigit = result[i] - '0';
                        result[i] = (char)(++nextDigit % 10 + '0');

                        if (nextDigit < 10) break;
                    }

                    if (i < 0 && nextDigit > 9)
                    {
                        // the carry adds a leading digit, which raises the decimal exponent
                        result.Remove(numDigits - 1, 1);
                        result.Insert(0, '1');
                        ++offset;
                    }
                    else
                    {
                        ++e2; result.Remove(numDigits - 1, 1);
                    }
                }
                else
                {
                    ++e2; result.Remove(numDigits - 1, 1);
                }
            }

            if (!expFlag && e2 < 0 && result.Length + e2 + 1 < result.Length)
            {
                result.Insert(result.Length + e2 + 1, formatter.NumberDecimalSeparator);
            }

            if (e2 < 0)
            {
                int trimIdx;
                for (trimIdx = result.Length - 1; trimIdx > 0 && result[trimIdx] == '0'; trimIdx--) ;

                // trimming trailing zeros does not move the leading digit, so the decimal exponent is unchanged
                e2 += result.Length - trimIdx;
                if (trimIdx > 0 && e2 >= 0) --trimIdx;

                result.Remove(trimIdx + 1, result.Length - trimIdx - 1);
            }

            AppendZeroIfEmpty(result);

            if (expFlag)
            {
                if (result.Length > 1)
                {
                    result.Insert(1, formatter.NumberDecimalSeparator);
                }

                result.Append('E');
                result.Append(offset < 0 ? formatter.NegativeSign : formatter.PositiveSign);
                result.Append(Math.Abs(offset));
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

    // The leading digit is always non-zero, so the buffer is never empty here; this guard is
    // unreachable and excluded from coverage.
    [MakeNonGeneric]
    [ExcludeFromCodeCoverage]
    private static void AppendZeroIfEmpty(StringBuilder result)
    {
        if (result.Length == 0)
        {
            result.Append('0');
        }
    }

    [MakeNonGeneric]
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        string? formatStr = format.IsEmpty ? null : new string(format);
        string resultStr = ToString(formatStr, provider);

        if (destination.Length >= resultStr.Length)
        {
            charsWritten = resultStr.Length;
            return resultStr.TryCopyTo(destination);
        }
        else
        {
            charsWritten = 0;
            return false;
        }
    }

    #endregion
}
