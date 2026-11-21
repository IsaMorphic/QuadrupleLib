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

namespace QuadrupleLib;

// Internal extended-precision arithmetic. A value is held as an unevaluated sum hi + lo of two binary128 numbers,
// which carries about 226 significant bits. The transcendental functions compute in this form and round once at the
// end, so their results are within one unit in the last place of the exact value.
public partial struct Float128<TAccelerator>
{
    #region Constants (each is the binary128 value of the constant, and the binary128 value of the remainder)

    private static readonly Float128<TAccelerator> _log2_10Hi = Parse("3.3219280948873623478703194294893902863042970131526463323338936822586020536363093924592249095439910888671875");
    private static readonly Float128<TAccelerator> _log2_10Lo = Parse("-1.1043946562012806572027913728644266896030167451329050686145587790377896955232355e-34");
    private static readonly Float128<TAccelerator> _invLn2Hi = Parse("1.442695040888963407359924681001892043334674933537275198973620597624558693772289785783868865109980106353759765625");
    private static readonly Float128<TAccelerator> _invLn2Lo = Parse("9.4091971020615710735161828809306554689666984432786418344140961767459146897959919e-35");
    private static readonly Float128<TAccelerator> _invLn10Hi = Parse("0.4342944819032518276511289189166050957622625721549327728439280984926379805544094381275499472394585609436035156250");
    private static readonly Float128<TAccelerator> _invLn10Lo = Parse("-1.3467865566351266206729474315326772719703877842977256695004758995360200826325784e-35");
    private static readonly Float128<TAccelerator> _piHi = Parse("3.141592653589793238462643383279502797479068098137295573004504331874296718662975536062731407582759857177734375");
    private static readonly Float128<TAccelerator> _piLo = Parse("8.6718101301237810247970440260433522541054144901523884386078071930357488340431829e-35");

    #endregion

    #region Extended-precision primitives

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDTwoSum(Float128<TAccelerator> a, Float128<TAccelerator> b)
    {
        Float128<TAccelerator> s = a + b;
        Float128<TAccelerator> bb = s - a;
        return (s, (a - (s - bb)) + (b - bb));
    }

    // requires |a| >= |b| or a == 0
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
        var s = DDTwoSum(a.Hi, b.Hi);
        var t = DDTwoSum(a.Lo, b.Lo);
        var u = DDQuickTwoSum(s.Hi, s.Lo + t.Hi);
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
        var p = DDTwoProd(a.Hi, b.Hi);
        // an overflowing product has no finite error term (FMA would give NaN), so keep the infinity
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
        var r = DDAdd(a, DDNegate(DDMul((q1, Zero), b)));
        Float128<TAccelerator> q2 = r.Hi / b.Hi;
        r = DDAdd(r, DDNegate(DDMul((q2, Zero), b)));
        Float128<TAccelerator> q3 = r.Hi / b.Hi;
        return DDAdd(DDQuickTwoSum(q1, q2), (q3, Zero));
    }

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDSqrt((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a)
    {
        Float128<TAccelerator> x = Sqrt(a.Hi);
        var residual = DDAdd(a, DDNegate(DDTwoProd(x, x)));
        return DDQuickTwoSum(x, residual.Hi / (2 * x));
    }

    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDScale((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a, int n)
    {
        return (ScaleB(a.Hi, n), ScaleB(a.Lo, n));
    }

    #endregion

    #region Extended-precision elementary functions

    // ln(1 + z) for |z| < 1/2, through ln(1 + z) = 2 atanh(s) with s = z / (2 + z) and |s| < 0.1716
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDLog1pSmall((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) z)
    {
        if (z.Hi == Zero)
        {
            return (Zero, Zero);
        }

        var s = DDDiv(z, DDAdd((2, Zero), z));
        Float128<TAccelerator> s2 = s.Hi * s.Hi;

        // tail = s2 / 3 + s2^2 / 5 + ..., so that ln(1 + z) = 2 s (1 + tail)
        Float128<TAccelerator> p = One / (2 * 26 + 1);
        for (int n = 25; n >= 1; n--)
        {
            p = p * s2 + One / (2 * n + 1);
        }
        Float128<TAccelerator> tail = s2 * p;

        return DDQuickTwoSum(2 * s.Hi, 2 * s.Lo + 2 * s.Hi * tail);
    }

    // ln(y) for finite y > 0, as an extended-precision value
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDLog(Float128<TAccelerator> y)
    {
        // y = m * 2^k with m recentred to [sqrt(1/2), sqrt(2)), so that |m - 1| < 0.42 and m - 1 is exact
        int k = ILogB(y);
        Float128<TAccelerator> m = ScaleB(y, -k);
        if (m > 1.4142135623730951)
        {
            m = ScaleB(m, -1);
            k++;
        }

        var lnM = DDLog1pSmall((m - One, Zero));

        // k * ln(2): k has at most 15 bits, so k * _ln2Hi (93 significant bits, so exact for |k| < 2^20) is exact
        Float128<TAccelerator> kf = k;
        var kLn2 = (kf * _ln2Hi, kf * _ln2Lo);
        return DDAdd(kLn2, lnM);
    }

    // ln(u) for an extended-precision u > 0
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDLogOf((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) u)
    {
        return DDAdd(DDLog(u.Hi), DDLog1pSmall((u.Lo / u.Hi, Zero)));
    }

    // exp(r) for |r| <= 0.35, by the Taylor series in Horner form
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDExpSmall((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) r)
    {
        var s = (One, Zero);
        for (int n = 27; n >= 1; n--)
        {
            s = DDAdd((One, Zero), DDMul(DDDiv(r, (n, Zero)), s));
        }
        return s;
    }

    // 2^w for an extended-precision exponent w; there is no overflow check here,
    // so Exp2FromDD checks the range first and this must only be called with a finite result
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDExp2((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) w)
    {
        // w = k + f with |f| <= 1/2, so that 2^w = 2^k * exp(f * ln 2)
        Float128<TAccelerator> kf = Round(w.Hi);
        var f = DDQuickTwoSum(w.Hi - kf, w.Lo);
        var r = DDMul(f, (_ln2Hi, _ln2Lo));
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

        var e = DDExp2(w);
        return e.Hi + e.Lo;
    }

    // exp(a) as an extended-precision value, for 0 <= a <= 90
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDExpOf(Float128<TAccelerator> a)
    {
        return DDExp2(DDMul((a, Zero), (_invLn2Hi, _invLn2Lo)));
    }

    // exp(a) / 2 rounded to binary128, for a where sinh and cosh are finite; halving is exact
    private static Float128<TAccelerator> HalfExp(Float128<TAccelerator> a)
    {
        return ScaleB(Exp2FromDD(DDMul((a, Zero), (_invLn2Hi, _invLn2Lo))), -1);
    }

    // thresholds (as properties, so no static field initialisation order is involved)
    private static Float128<TAccelerator> SmallArgument => ScaleB(One, -57);          // below this, sinh, tanh, asinh, atan, atanh return x
    private static Float128<TAccelerator> HyperbolicLargeArgument => 45;             // above this, sinh/cosh are e^|x|/2 and tanh is 1 to rounding
    private static Float128<TAccelerator> LogLargeArgument => ScaleB(One, 60);       // above this, asinh and acosh are ln(2|x|) to rounding

    // b^n for n >= 1, by repeated squaring; the caller keeps the magnitude of the result in range
    private static (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) DDPowInt((Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) b, ulong n)
    {
        var result = (One, Zero);
        while (true)
        {
            if ((n & 1) != 0)
            {
                result = DDMul(result, b);
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

    // arctangent of an extended-precision value, by halving the argument three times and summing the series
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

        // atan(z) = 2 atan(z / (1 + sqrt(1 + z^2))); three halvings leave |z| < tan(pi / 32) < 0.1
        for (int i = 0; i < 3; i++)
        {
            var root = DDSqrt(DDAdd((One, Zero), DDMul(z, z)));
            z = DDDiv(z, DDAdd((One, Zero), root));
        }

        // atan(z) = z - z^3 / 3 + z^5 / 5 - ... = z * (1 - z^2 / 3 + ...)
        var z2 = DDMul(z, z);
        var p = (Zero, Zero);
        for (int n = 17; n >= 0; n--)
        {
            p = DDAdd((One / (2 * n + 1), Zero), DDNegate(DDMul(z2, p)));
        }
        var result = DDScale(DDMul(z, p), 3);

        if (inverted)
        {
            result = DDAdd((ScaleB(_piHi, -1), ScaleB(_piLo, -1)), DDNegate(result));
        }

        return negative ? DDNegate(result) : result;
    }

    #endregion
}
