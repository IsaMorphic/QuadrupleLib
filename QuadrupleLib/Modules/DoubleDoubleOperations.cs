/*
 *  Copyright 2026 Chosen Few Software
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

namespace QuadrupleLib;

// double-double: unevaluated sum hi + lo
public partial struct Float128<TAccelerator>
{
    #region Constants

    // log2(10) = Hi + Lo, relative error below 2^-230 against a 110-digit reference. Used by Exp10.
    private static readonly Float128<TAccelerator> _log210Hi = new(new UInt128(0x4000A934F0979A37UL, 0x15FC9257EDFE9B60UL));
    private static readonly Float128<TAccelerator> _log210Lo = new(new UInt128(0xBF8E2599349D500EUL, 0x4262958B724A9BFCUL));
    // 1/ln 2 = Hi + Lo, relative error below 2^-227 against a 110-digit reference. Used by Exp, DDExpOf, HalfExp, and DDLog2.
    private static readonly Float128<TAccelerator> _invLn2Hi = new(new UInt128(0x3FFF71547652B82FUL, 0xE1777D0FFDA0D23AUL));
    private static readonly Float128<TAccelerator> _invLn2Lo = new(new UInt128(0x3F8DF4475ABBD546UL, 0xEB4AD2C45928B367UL));
    // 1/ln 10 = Hi + Lo, relative error below 2^-228 against a 110-digit reference. Used by Log10.
    private static readonly Float128<TAccelerator> _invLn10Hi = new(new UInt128(0x3FFDBCB7B1526E50UL, 0xE32A6AB7555F5A68UL));
    private static readonly Float128<TAccelerator> _invLn10Lo = new(new UInt128(0xBF8B1E6E08E5CFEDUL, 0xD1B2EFEE2E0695D8UL));
    // pi = Hi + Lo, relative error below 2^-229 against a 110-digit reference. Used by DDAtan, Atan2, and PiTimes. Hi is pi rounded to binary128.
    private static readonly Float128<TAccelerator> _piHi = new(new UInt128(0x4000921FB54442D1UL, 0x8469898CC51701B8UL));
    private static readonly Float128<TAccelerator> _piLo = new(new UInt128(0x3F8DCD129024E088UL, 0xA67CC74020BBEA64UL));

    #endregion

    #region Extended-precision primitives

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDTwoSum(Float128<TAccelerator> a, Float128<TAccelerator> b)
    {
        Float128<TAccelerator> s = a + b;
        Float128<TAccelerator> bb = s - a;
        return (s, (a - (s - bb)) + (b - bb));
    }

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDQuickTwoSum(Float128<TAccelerator> a, Float128<TAccelerator> b)
    {
        Float128<TAccelerator> s = a + b;
        return (s, b - (s - a));
    }

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDTwoProd(Float128<TAccelerator> a, Float128<TAccelerator> b)
    {
        Float128<TAccelerator> p = a * b;
        return (p, FusedMultiplyAdd(a, b, -p));
    }

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDAdd(
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a,
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) b)
    {
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) s = DDTwoSum(a.Hi, b.Hi);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) t = DDTwoSum(a.Lo, b.Lo);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) u = DDQuickTwoSum(s.Hi, s.Lo + t.Hi);
        return DDQuickTwoSum(u.Hi, u.Lo + t.Lo);
    }

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDNegate((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a)
    {
        return (-a.Hi, -a.Lo);
    }

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDMul(
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a,
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) b)
    {
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) p = DDTwoProd(a.Hi, b.Hi);
        // keep infinity
        if (IsInfinity(p.Hi))
        {
            return (p.Hi, Zero);
        }
        return DDQuickTwoSum(p.Hi, p.Lo + (a.Hi * b.Lo + a.Lo * b.Hi));
    }

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDDiv(
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a,
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) b)
    {
        Float128<TAccelerator> q1 = a.Hi / b.Hi;
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) r = DDAdd(a, DDNegate(DDMul((q1, Zero), b)));
        Float128<TAccelerator> q2 = r.Hi / b.Hi;
        r = DDAdd(r, DDNegate(DDMul((q2, Zero), b)));
        Float128<TAccelerator> q3 = r.Hi / b.Hi;
        return DDAdd(DDQuickTwoSum(q1, q2), (q3, Zero));
    }

    // Requires a.Hi > 0: a zero argument gives NaN from the 0/0 seed. Callers must handle zero first.
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDSqrt((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a)
    {
        // double seed, one Newton step
        int half = a.Hi.Exponent / 2;
        Float128<TAccelerator> seed = ScaleB((Float128<TAccelerator>)Math.Sqrt((double)ScaleB(a.Hi, -2 * half)), half);
        Float128<TAccelerator> x = 0.5 * (seed + a.Hi / seed);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) residual = DDAdd(a, DDNegate(DDTwoProd(x, x)));
        return DDQuickTwoSum(x, residual.Hi / (2 * x));
    }

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDScale((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a, int n)
    {
        return (ScaleB(a.Hi, n), ScaleB(a.Lo, n));
    }

    #endregion

    #region Extended-precision elementary functions

    // ln(1 + z) = 2 atanh(z / (2 + z)). Callers keep |s| < 0.1716 (see DDLog). The series is cut after 26 odd terms;
    // the omitted terms are below 2^-130 relative.
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDLog1pSmall((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) z)
    {
        if (z.Hi == Zero)
        {
            return (Zero, Zero);
        }

        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) s = DDDiv(z, DDAdd((2, Zero), z));
        Float128<TAccelerator> s2 = s.Hi * s.Hi;

        // tail = s2 / 3 + s2^2 / 5 + ...
        Float128<TAccelerator>[] odd = OddReciprocals;
        Float128<TAccelerator> p = odd[ODD_SERIES_TERMS];
        for (int n = ODD_SERIES_TERMS - 1; n >= 1; n--)
        {
            p = p * s2 + odd[n];
        }
        Float128<TAccelerator> tail = s2 * p;

        return DDQuickTwoSum(2 * s.Hi, 2 * s.Lo + 2 * s.Hi * tail);
    }

    // ln(y), finite y > 0
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDLog(Float128<TAccelerator> y)
    {
        // reduce: y = m * 2^k, m in (sqrt(1/2), 1.4142135623730951], so |s| < 0.1716 in DDLog1pSmall.
        // k * _ln2Hi is exact for |k| <= 2^20 (see Constants.cs).
        int k = ILogB(y);
        Float128<TAccelerator> m = ScaleB(y, -k);
        if (m > 1.4142135623730951)
        {
            m = ScaleB(m, -1);
            k++;
        }

        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) lnM = DDLog1pSmall((m - One, Zero));

        // k * ln 2
        Float128<TAccelerator> kf = k;
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) kLn2 = DDMul((kf, Zero), (_ln2Hi, _ln2Lo));
        return DDAdd(kLn2, lnM);
    }

    // ln(u) for an extended-precision u > 0
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDLogOf((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) u)
    {
        return DDAdd(DDLog(u.Hi), DDLog1pSmall((u.Lo / u.Hi, Zero)));
    }

    private const int EXP_SERIES_TERMS = 27;

    private const int ODD_SERIES_TERMS = 26;

    // 1/(2n+1) for n = 0 .. ODD_SERIES_TERMS, built on first use
    private static readonly Lazy<Float128<TAccelerator>[]> _oddReciprocals =
        new Lazy<Float128<TAccelerator>[]>(BuildOddReciprocals);

    private static Float128<TAccelerator>[] OddReciprocals => _oddReciprocals.Value;

    private static Float128<TAccelerator>[] BuildOddReciprocals()
    {
        Float128<TAccelerator>[] table = new Float128<TAccelerator>[ODD_SERIES_TERMS + 1];
        for (int n = 0; n <= ODD_SERIES_TERMS; n++)
        {
            table[n] = One / (2 * n + 1);
        }
        return table;
    }

    // exp(r), Horner form over 1/k!. Callers pass |r| <= ln 2 / 2 (see DDExp2). The series is cut after k = 27;
    // the omitted terms are below 2^-140.
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDExpSmall((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) r)
    {
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo)[] inverse = InverseFactorials;
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) s = inverse[EXP_SERIES_TERMS];
        for (int n = EXP_SERIES_TERMS - 1; n >= 0; n--)
        {
            s = DDAdd(inverse[n], DDMul(r, s));
        }
        return s;
    }

    // 1/k!, for k = 0 .. EXP_SERIES_TERMS, built on first use.
    private static readonly Lazy<(Float128<TAccelerator> Hi, Float128<TAccelerator> Lo)[]> _inverseFactorials =
        new Lazy<(Float128<TAccelerator> Hi, Float128<TAccelerator> Lo)[]>(BuildInverseFactorials);

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo)[] InverseFactorials => _inverseFactorials.Value;

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo)[] BuildInverseFactorials()
    {
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo)[] table = new (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo)[EXP_SERIES_TERMS + 1];
        table[0] = (One, Zero);
        for (int n = 1; n < table.Length; n++)
        {
            table[n] = DDDiv(table[n - 1], (n, Zero));
        }
        return table;
    }

    // 2^w, unchecked. |w - round(w)| <= 1/2, so r = f ln 2 satisfies |r| <= ln 2 / 2 for DDExpSmall.
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDExp2((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) w)
    {
        // w = k + f
        Float128<TAccelerator> kf = Round(w.Hi);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) f = DDQuickTwoSum(w.Hi - kf, w.Lo);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) r = DDMul(f, (_ln2Hi, _ln2Lo));
        return DDScale(DDExpSmall(r), (int)kf);
    }

    // 2^w rounded to binary128, for a plain or extended-precision exponent
    private static Float128<TAccelerator> Exp2FromDD((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) w)
    {
        if (w.Hi >= EXPONENT_BIAS + 1)
        {
            return _pInf;
        }
        else if (w.Hi < -EXPONENT_BIAS - 120)
        {
            return Zero;
        }

        // integer exponent: exact power of two
        if (w.Lo == Zero && IsInteger(w.Hi))
        {
            return ScaleB(One, (int)w.Hi);
        }

        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) e = DDExp2(w);
        return e.Hi + e.Lo;
    }

    // exp(a), extended precision
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDExpOf(Float128<TAccelerator> a)
    {
        return DDExp2(DDMul((a, Zero), (_invLn2Hi, _invLn2Lo)));
    }

    // exp(a) / 2 = 2^(a / ln 2 - 1)
    private static Float128<TAccelerator> HalfExp(Float128<TAccelerator> a)
    {
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) w = DDMul((a, Zero), (_invLn2Hi, _invLn2Lo));
        if (IsInfinity(w.Hi))
        {
            return _pInf;
        }
        return Exp2FromDD(DDAdd(w, (-One, Zero)));
    }

    // thresholds, as raw binary128 bits.
    // |x| < 2^-57: Asin, Sinh, Tanh, Asinh, Atan, Atanh return x. The omitted cubic term is below 2^-115 times |x|, under half an ulp of x.
    private static readonly Float128<TAccelerator> _smallArgument = new(new UInt128(0x3FC6000000000000UL, 0x0UL));
    // |x| >= 45: Sinh, Cosh use HalfExp, with an omitted relative term below e^-90 < 2^-129. Tanh returns +-1. The omitted Tanh tail, about 2*e^(-2|x|), is below 2*e^-90 < 2^-128, far under half an ulp of 1.
    private static readonly Float128<TAccelerator> _hyperbolicLargeArgument = new(new UInt128(0x4004680000000000UL, 0x0UL));
    // |x| > 2^60: Acosh, Asinh use ln(2|x|). The dropped 1/(4x^2) is below 2^-122, under half an ulp of a result above 42 (2^-108).
    private static readonly Float128<TAccelerator> _logLargeArgument = new(new UInt128(0x403B000000000000UL, 0x0UL));

    // b^n for n >= 1, by repeated squaring
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDPowInt((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) b, ulong n)
    {
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) result = (Zero, Zero);
        bool hasResult = false;
        while (true)
        {
            if ((n & 1) != 0)
            {
                result = hasResult ? DDMul(result, b) : b;
                hasResult = true;
            }
            n >>= 1;
            if (n == 0)
            {
                return result;
            }
            b = DDMul(b, b);
        }
    }

    // ln(y) / ln(2) as an extended-precision value
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDLog2(Float128<TAccelerator> y)
    {
        return DDMul(DDLog(y), (_invLn2Hi, _invLn2Lo));
    }

    // atan(z). |z| is reduced to <= 1 by inversion, then three halvings give |z| <= tan(pi / 32), about 0.0985.
    // The series is cut after 18 odd terms; the omitted terms are below 2^-120.
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDAtan((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) z)
    {
        bool negative = z.Hi < Zero;
        if (negative)
        {
            z = DDNegate(z);
        }

        bool inverted = z.Hi > One;
        if (inverted)
        {
            z = DDDiv((One, Zero), z);
        }

        // three halvings, then series
        for (int i = 0; i < 3; i++)
        {
            (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) root = DDSqrt(DDAdd((One, Zero), DDMul(z, z)));
            z = DDDiv(z, DDAdd((One, Zero), root));
        }

        // series
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) z2 = DDMul(z, z);
        Float128<TAccelerator>[] odd = OddReciprocals;
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) p = (Zero, Zero);
        for (int n = 17; n >= 0; n--)
        {
            p = DDAdd((odd[n], Zero), DDNegate(DDMul(z2, p)));
        }
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) result = DDScale(DDMul(z, p), 3);

        if (inverted)
        {
            result = DDAdd((ScaleB(_piHi, -1), ScaleB(_piLo, -1)), DDNegate(result));
        }

        return negative ? DDNegate(result) : result;
    }

    #endregion
}
