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

using QuadrupleLib.Utilities;

namespace QuadrupleLib;

public partial struct Float128<TAccelerator>
{
    #region Public API (trig functions)

    private const int SINCOS_ITER_COUNT = 48;

    private static readonly Float128<TAccelerator>[] _thetaTable;

    private static readonly Float128<TAccelerator> _invK_n;

    private static Float128<TAccelerator> AtanPow2(int k)
    {
        Float128<TAccelerator> x_n = One;
        if (k == 0) return QuarterPi;
        for (int n = 0; n < 25; n++)
        {
            x_n = ScaleB(
                FusedMultiplyAdd(x_n * x_n,
                    FusedMultiplyAdd(x_n,
                        FusedMultiplyAdd(x_n,
                            FusedMultiplyAdd(x_n,
                                FusedMultiplyAdd(x_n,
                                    FusedMultiplyAdd(x_n,
                                        FusedMultiplyAdd(x_n,
                                            FusedMultiplyAdd(x_n,
                                                FusedMultiplyAdd(x_n,
                                                    FusedMultiplyAdd(x_n,
                                                        FusedMultiplyAdd(x_n,
                                                            FusedMultiplyAdd(x_n,
                                                                FusedMultiplyAdd(x_n,
                                                                    FusedMultiplyAdd(x_n,
                                                                        FusedMultiplyAdd(x_n,
                                                                            9 - ScaleB(x_n, k),
                                                                        ScaleB(One, k + 7)),
                                                                    -1008),
                                                                ScaleB(-273, k + 5)),
                                                            58464),
                                                        ScaleB(1449, k + 8)),
                                                    -2056320),
                                                ScaleB(-315, k + 15)),
                                            46448640),
                                        ScaleB(2835, k + 16)),
                                    -650280960),
                                -ScaleB(59535, k + 15)),
                            4877107200),
                        ScaleB(297675, k + 15)),
                    -14631321600),
                14631321600),
            -k - 14) / 893025;
        }
        return x_n;
    }

    private static Float128<TAccelerator> ComputeInverseK(int n)
    {
        Float128<TAccelerator> K_i = One;
        for (int i = 0; i < n; i++)
        {
            K_i = FusedMultiplyAdd(K_i, ScaleB(One, i * -2), K_i);
        }
        return Sqrt(K_i);
    }

    public static Float128<TAccelerator> Hypot(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        Float128<TAccelerator> ax = Abs(x);
        Float128<TAccelerator> ay = Abs(y);
        if (IsPositiveInfinity(ax) || IsPositiveInfinity(ay))
        {
            return _pInf;
        }
        else if (ax == Zero && ay == Zero)
        {
            return Zero;
        }
        else
        {
            // scale by 2^k
            int scale = Math.Max(ax.Exponent, ay.Exponent);
            Float128<TAccelerator> sx = ScaleB(ax, -scale);
            Float128<TAccelerator> sy = ScaleB(ay, -scale);
            (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) root = DDSqrt(DDAdd(DDTwoProd(sx, sx), DDTwoProd(sy, sy)));
            return ScaleB(root.Hi + root.Lo, scale);
        }
    }

    public static Float128<TAccelerator> Cos(Float128<TAccelerator> x)
    {
        return SinCos(x).Cos;
    }

    public static Float128<TAccelerator> CosPi(Float128<TAccelerator> x)
    {
        return SinCosPi(x).CosPi;
    }

    public static Float128<TAccelerator> Sin(Float128<TAccelerator> x)
    {
        return SinCos(x).Sin;
    }

    public static Float128<TAccelerator> SinPi(Float128<TAccelerator> x)
    {
        return SinCosPi(x).SinPi;
    }

    public static (Float128<TAccelerator> Sin, Float128<TAccelerator> Cos) SinCos(Float128<TAccelerator> alpha)
    {
        if (IsNaN(alpha) || IsInfinity(alpha) || 
            alpha.Exponent >= 112)
        {
            return (_qNaN, _qNaN);
        }
        else if (IsZero(alpha))
        {
            return (alpha, One);
        }

        Float128<TAccelerator> x = One, y = CopySign(Zero, alpha);
        Float128<TAccelerator> phi = Ieee754Remainder(alpha, Tau);

        // The remainder lies in [-Pi, Pi] and Tau == 4 * HalfPi exactly, so one quarter turn brings phi into [-HalfPi, HalfPi]
        if (phi > HalfPi)
        {
            phi -= HalfPi;
            (x, y) = (-y, x);
        }
        else if (phi < -HalfPi)
        {
            phi += HalfPi;
            (x, y) = (y, -x);
        }

        if (phi == Zero)
        {
            return (y, x);
        }

        Float128<TAccelerator> sigma, sigma_neg, theta = Zero;
        for (int i = 0; i < SINCOS_ITER_COUNT; i++)
        {
            if (theta > phi)
            {
                (sigma, sigma_neg) = (NegativeOne, One);
            }
            else
            {
                (sigma, sigma_neg) = (One, NegativeOne);
            }

            (x, y) = (FusedMultiplyAdd(y, ScaleB(sigma_neg, -i), x), FusedMultiplyAdd(x, ScaleB(sigma, -i), y));
            theta += sigma * _thetaTable[i];
        }

        return (y / _invK_n, x / _invK_n);
    }

    public static (Float128<TAccelerator> SinPi, Float128<TAccelerator> CosPi) SinCosPi(Float128<TAccelerator> x)
    {
        // x * Pi reaches 2^112 near |x| = 1.65E+33, where SinCos returns NaN. Exact reduction modulo 2 is
        // deferred to the follow-up that adds the CoRDiC kernel (BREAKING_CHANGES §6).
        return SinCos(x * Pi);
    }

    public static Float128<TAccelerator> Asin(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || x > One || x < NegativeOne)
        {
            return _qNaN;
        }
        else if (IsZero(x))
        {
            return x;
        }
        else if (x == NegativeOne || x == One)
        {
            return CopySign(HalfPi, x);
        }
        else if (Abs(x) < _smallArgument)
        {
            // asin(x) rounds to x
            return x;
        }

        // asin(x) = atan(x / sqrt(1 - x^2))
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) oneMinusSquare = DDAdd((One, Zero), DDNegate(DDTwoProd(x, x)));
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a = DDAtan(DDDiv((x, Zero), DDSqrt(oneMinusSquare)));
        return a.Hi + a.Lo;
    }

    public static Float128<TAccelerator> AsinPi(Float128<TAccelerator> x)
    {
        if (x > One || x < NegativeOne)
        {
            return _qNaN;
        }
        else
        {
            return Atan2Pi(x, Sqrt(-FusedMultiplyAdd(x, x, NegativeOne)));
        }
    }

    public static Float128<TAccelerator> Acos(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || x > One || x < NegativeOne)
        {
            return _qNaN;
        }
        else if (x == One)
        {
            return Zero;
        }
        else if (x == NegativeOne)
        {
            return Pi;
        }

        // acos(x) = 2 atan(sqrt((1 - x) / (1 + x)))
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) ratio = DDSqrt(DDDiv(DDTwoSum(One, -x), DDTwoSum(One, x)));
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a = DDAtan(ratio);
        return ScaleB(a.Hi + a.Lo, 1);
    }

    public static Float128<TAccelerator> AcosPi(Float128<TAccelerator> x)
    {
        if (x > One || x < NegativeOne)
        {
            return _qNaN;
        }
        else
        {
            // 1 - x^2 is +0 at x = +-1, so the radicand is +0 and AcosPi(-1) = 1, AcosPi(1) = +0
            return Atan2Pi(Sqrt(FusedMultiplyAdd(-x, x, One)), x);
        }
    }

    public static Float128<TAccelerator> Tan(Float128<TAccelerator> alpha)
    {
        (Float128<TAccelerator> y, Float128<TAccelerator> x) = SinCos(alpha);
        return y / x;
    }

    public static Float128<TAccelerator> TanPi(Float128<TAccelerator> alpha)
    {
        (Float128<TAccelerator> y, Float128<TAccelerator> x) = SinCosPi(alpha);
        return y / x;
    }

    public static Float128<TAccelerator> Atan(Float128<TAccelerator> z)
    {
        if (IsNaN(z))
        {
            return _qNaN;
        }
        else if (IsPositiveInfinity(z))
        {
            return HalfPi;
        }
        else if (IsNegativeInfinity(z))
        {
            return -HalfPi;
        }
        else if (IsZero(z) || Abs(z) < _smallArgument)
        {
            // atan(z) rounds to z
            return z;
        }

        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a = DDAtan((z, Zero));
        return a.Hi + a.Lo;
    }

    public static Float128<TAccelerator> AtanPi(Float128<TAccelerator> x)
    {
        return Atan(x) / Pi;
    }

    public static Float128<TAccelerator> Atan2(Float128<TAccelerator> y, Float128<TAccelerator> x)
    {
        if (IsNaN(y) || IsNaN(x))
        {
            return _qNaN;
        }
        else if (IsInfinity(y) && IsInfinity(x))
        {
            return CopySign(x > Zero ? PiTimes(ScaleB(One, -2)) : PiTimes(ScaleB(3, -2)), y);
        }
        else if (IsInfinity(y))
        {
            return CopySign(HalfPi, y);
        }
        else if (IsInfinity(x))
        {
            return x > Zero ? CopySign(Zero, y) : CopySign(Pi, y);
        }
        else if (IsZero(y))
        {
            return x > Zero || (IsZero(x) && !IsNegative(x)) ? y : CopySign(Pi, y);
        }
        else if (IsZero(x))
        {
            return CopySign(HalfPi, y);
        }
        else if (ILogB(y) - ILogB(x) > 16380)
        {
            // +/- pi / 2
            return CopySign(HalfPi, y);
        }
        else if (ILogB(y) - ILogB(x) < -120)
        {
            // y / x, or +/- pi when x < 0
            return x > Zero ? y / x : CopySign(Pi, y);
        }

        // atan(y / x), plus or minus pi for x < 0
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) ratio = DDDiv((y, Zero), (x, Zero));
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a = DDAtan(ratio);
        if (x < Zero)
        {
            a = DDAdd(a, (IsNegative(y) ? DDNegate((_piHi, _piLo)) : (_piHi, _piLo)));
        }
        return a.Hi + a.Lo;
    }

    // pi * factor, rounded once
    private static Float128<TAccelerator> PiTimes(Float128<TAccelerator> factor)
    {
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) product = DDMul((_piHi, _piLo), (factor, Zero));
        return product.Hi + product.Lo;
    }

    public static Float128<TAccelerator> Atan2Pi(Float128<TAccelerator> y, Float128<TAccelerator> x)
    {
        return Atan2(y, x) / Pi;
    }

    #endregion

    #region Public API (hyperbolic trig functions)

    public static Float128<TAccelerator> Cosh(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> ax = Abs(x);
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (IsZero(x))
        {
            return One;
        }
        else if (IsInfinity(x))
        {
            return _pInf;
        }
        else if (ax >= _hyperbolicLargeArgument)
        {
            return HalfExp(ax);
        }

        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) e = DDExpOf(ax);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) sum = DDAdd(e, DDDiv((One, Zero), e));
        return ScaleB(sum.Hi + sum.Lo, -1);
    }

    public static Float128<TAccelerator> Sinh(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> ax = Abs(x);
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (IsInfinity(x) || IsZero(x))
        {
            return x;
        }
        else if (ax < _smallArgument)
        {
            return x;
        }
        else if (ax >= _hyperbolicLargeArgument)
        {
            return CopySign(HalfExp(ax), x);
        }

        // sinh(x) = (e^x - e^-x) / 2
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) e = DDExpOf(ax);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) difference = DDAdd(e, DDNegate(DDDiv((One, Zero), e)));
        return CopySign(ScaleB(difference.Hi + difference.Lo, -1), x);
    }

    public static Float128<TAccelerator> Tanh(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> ax = Abs(x);
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (IsZero(x) || ax < _smallArgument)
        {
            return x;
        }
        else if (IsInfinity(x) || ax >= _hyperbolicLargeArgument)
        {
            // rounds to +-1
            return CopySign(One, x);
        }

        // tanh(x) = (e^2x - 1) / (e^2x + 1)
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) e2 = DDExpOf(2 * ax);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) ratio = DDDiv(DDAdd(e2, (-One, Zero)), DDAdd(e2, (One, Zero)));
        return CopySign(ratio.Hi + ratio.Lo, x);
    }

    public static Float128<TAccelerator> Acosh(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (IsPositiveInfinity(x))
        {
            return _pInf;
        }
        else if (x == One)
        {
            return Zero;
        }
        else if (x < One)
        {
            return _qNaN;
        }
        else if (x > _logLargeArgument)
        {
            // large x: ln(2x)
            (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) big = DDAdd(DDLog(x), (_ln2Hi, _ln2Lo));
            return big.Hi + big.Lo;
        }

        // acosh(x) = ln(x + sqrt(x^2 - 1))
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) root = DDSqrt(DDAdd(DDTwoProd(x, x), (-One, Zero)));
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) u = DDAdd((x, Zero), root);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a = DDLogOf(u);
        return a.Hi + a.Lo;
    }

    public static Float128<TAccelerator> Asinh(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (IsInfinity(x) || IsZero(x) || Abs(x) < _smallArgument)
        {
            return x;
        }

        Float128<TAccelerator> absX = Abs(x);
        if (absX > _logLargeArgument)
        {
            // large |x|: ln(2|x|)
            (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) big = DDAdd(DDLog(absX), (_ln2Hi, _ln2Lo));
            return CopySign(big.Hi + big.Lo, x);
        }

        // asinh(|x|) = ln(|x| + sqrt(x^2 + 1))
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) root = DDSqrt(DDAdd(DDTwoProd(absX, absX), (One, Zero)));
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) u = DDAdd((absX, Zero), root);
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a = DDLogOf(u);
        return CopySign(a.Hi + a.Lo, x);
    }

    public static Float128<TAccelerator> Atanh(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> absX = Abs(x);
        if (IsNaN(x) || absX > One)
        {
            return _qNaN;
        }
        else if (IsZero(x) || absX < _smallArgument)
        {
            return x;
        }
        else if (absX == One)
        {
            return CopySign(_pInf, x);
        }

        // atanh(x) = ln((1 + x) / (1 - x)) / 2
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) ratio = DDDiv(DDTwoSum(One, x), DDTwoSum(One, -x));
        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) a = DDLogOf(ratio);
        return ScaleB(a.Hi + a.Lo, -1);
    }

    #endregion

    #region Public API (logarithm functions)

    public static int ILogB(Float128<TAccelerator> x)
    {
        return x.Exponent - (int)UInt128.LeadingZeroCount(x.Significand) + 15;
    }

    public static Float128<TAccelerator> Log2(Float128<TAccelerator> x)
    {
        if (IsNaN(x) || x < Zero)
        {
            return _qNaN;
        }
        else if (IsZero(x))
        {
            return _nInf;
        }
        else if (IsPositiveInfinity(x))
        {
            return _pInf;
        }

        // exact for powers of two
        int n = ILogB(x);
        if (ScaleB(x, -n) == One)
        {
            return n;
        }

        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) result = DDLog2(x);
        return result.Hi + result.Lo;
    }

    public static Float128<TAccelerator> Log(Float128<TAccelerator> y)
    {
        if (IsNaN(y) || y < Zero)
        {
            return _qNaN;
        }
        else if (IsZero(y))
        {
            return _nInf;
        }
        else if (y == One)
        {
            return Zero;
        }
        else if (IsPositiveInfinity(y))
        {
            return _pInf;
        }

        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) result = DDLog(y);
        return result.Hi + result.Lo;
    }

    public static Float128<TAccelerator> Log(Float128<TAccelerator> y, Float128<TAccelerator> newBase)
    {
        if (IsNaN(y) || IsNaN(newBase))
        {
            return _qNaN;
        }
        else if (y == One && IsZero(newBase))
        {
            // ln(1) = +0 divided by ln(+-0) = -inf gives -0, as System.Math.Log(1, +-0)
            return NegativeZero;
        }
        else if (y < Zero || newBase <= Zero)
        {
            return _qNaN;
        }
        else if (newBase == One)
        {
            // newBase == 1: NaN, as System.Math.Log(x, 1)
            return _qNaN;
        }
        else if (y == One)
        {
            // ln(1) = +0, divided by ln(newBase)
            return newBase < One ? NegativeZero : Zero;
        }
        else if (IsPositiveInfinity(newBase))
        {
            // NaN for y != 1, as System.Math.Log(a, +inf)
            return _qNaN;
        }
        else if (IsZero(y))
        {
            return newBase < One ? _pInf : _nInf;
        }
        else if (IsPositiveInfinity(y))
        {
            return newBase < One ? _nInf : _pInf;
        }

        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) quotient = DDDiv(DDLog(y), DDLog(newBase));
        return quotient.Hi + quotient.Lo;
    }

    public static Float128<TAccelerator> Log10(Float128<TAccelerator> y)
    {
        if (IsNaN(y) || y < Zero)
        {
            return _qNaN;
        }
        else if (IsZero(y))
        {
            return _nInf;
        }
        else if (y == One)
        {
            return Zero;
        }
        else if (IsPositiveInfinity(y))
        {
            return _pInf;
        }

        (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) result = DDMul(DDLog(y), (_invLn10Hi, _invLn10Lo));
        return result.Hi + result.Lo;
    }

    #endregion

    #region Public API (exponential functions)

    public static Float128<TAccelerator> Exp(Float128<TAccelerator> y)
    {
        if (IsNaN(y))
        {
            return _qNaN;
        }
        else if (IsInfinity(y))
        {
            return y > Zero ? _pInf : Zero;
        }
        else if (y > 11357)
        {
            // beyond ln(MaxValue)
            return _pInf;
        }
        else if (y < -11434)
        {
            // beyond ln(smallest subnormal / 2)
            return Zero;
        }

        // exp(y) = exp2(y / ln(2))
        return Exp2FromDD(DDMul((y, Zero), (_invLn2Hi, _invLn2Lo)));
    }

    public static Float128<TAccelerator> Exp10(Float128<TAccelerator> y)
    {
        if (IsNaN(y))
        {
            return _qNaN;
        }
        else if (IsInfinity(y))
        {
            return y > Zero ? _pInf : Zero;
        }

        // 10^y = 2^(y log2 10)
        return Exp2FromDD(DDMul((y, Zero), (_log210Hi, _log210Lo)));
    }

    public static Float128<TAccelerator> Exp2(Float128<TAccelerator> y)
    {
        if (IsNaN(y))
        {
            return _qNaN;
        }

        return Exp2FromDD((y, Zero));
    }

    public static Float128<TAccelerator> Pow(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        if (IsZero(y) || x == One)
        {
            return One;
        }
        else if (IsNaN(x) || IsNaN(y))
        {
            return _qNaN;
        }
        else if (IsInfinity(y))
        {
            Float128<TAccelerator> absX = Abs(x);
            if (absX == One)
            {
                return One;
            }
            else
            {
                return (absX > One) == IsPositive(y) ? _pInf : Zero;
            }
        }

        bool isOddPower = IsOddInteger(y);
        if (IsZero(x))
        {
            Float128<TAccelerator> result = IsNegative(y) ? _pInf : Zero;
            return isOddPower ? CopySign(result, x) : result;
        }
        else if (IsInfinity(x))
        {
            Float128<TAccelerator> result = y > Zero ? _pInf : Zero;
            return isOddPower ? CopySign(result, x) : result;
        }
        else if (IsNegative(x) && !IsInteger(y))
        {
            return _qNaN;
        }

        Float128<TAccelerator> baseMagnitude = Abs(x);
        Float128<TAccelerator> magnitude;

        // y = 2, then integer y
        if (y == 2)
        {
            magnitude = baseMagnitude * baseMagnitude;
        }
        else if (IsInteger(y) && Abs(y) < ScaleB(One, 20) && Abs(y) * (Math.Abs(ILogB(baseMagnitude)) + 1) < EXPONENT_BIAS - 3)
        {
            (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) power = DDPowInt((baseMagnitude, Zero), (ulong)Abs(y));
            if (IsNegative(y))
            {
                power = DDDiv((One, Zero), power);
            }
            magnitude = power.Hi + power.Lo;
        }
        else
        {
            // |x|^y = 2^(y log2|x|)
            (Float128<TAccelerator> Hi, Float128<TAccelerator> Lo) logMagnitude = DDMul((y, Zero), DDLog2(baseMagnitude));
            magnitude = Exp2FromDD(logMagnitude);
        }

        // sign
        return IsNegative(x) && isOddPower ? -magnitude : magnitude;
    }

    #endregion

    #region Public API (root functions)

    public static Float128<TAccelerator> Sqrt(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (x < Zero)
        {
            return _qNaN;
        }
        else if (x == Zero)
        {
            return x;
        }
        else if (IsPositiveInfinity(x))
        {
            return x;
        }
        else
        {
            // x = radicand * 2^power with power even
            int power = x.Exponent - 112;
            UInt256 radicand = x.Significand;
            if ((power & 1) != 0)
            {
                radicand <<= 1;
                power--;
            }

            // shift so the root has exactly 116 bits
            int bitLength = 256 - (int)UInt256.LeadingZeroCount(radicand);
            int shift = 232 - bitLength - (bitLength & 1);
            radicand <<= shift;
            power -= shift;

            // bit-by-bit sqrt; 2^230 is the top power of four
            UInt256 bit = UInt256.One << 230;
            UInt256 root = UInt256.Zero;
            UInt256 remainder = radicand;
            while (bit != UInt256.Zero)
            {
                if (remainder >= root + bit)
                {
                    remainder -= root + bit;
                    root = (root >> 1) + bit;
                }
                else
                {
                    root >>= 1;
                }
                bit >>= 2;
            }

            // set sticky bit
            UInt128 significand = (UInt128)root | (remainder != UInt256.Zero ? UInt128.One : UInt128.Zero);
            return Compose(RoundToNearestEven(significand), power / 2 + 115, false);
        }
    }

    public static Float128<TAccelerator> Cbrt(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (x == Zero || IsInfinity(x))
        {
            // IEEE 754: keeps the sign of zero
            return x;
        }

        // cbrt(x) = 2^(log2(|x|) / 3)
        return CopySign(Exp2FromDD(DDDiv(DDLog2(Abs(x)), (3, Zero))), x);
    }

    public static Float128<TAccelerator> RootN(Float128<TAccelerator> x, int n)
    {
        if (IsNaN(x) || n == 0)
        {
            return _qNaN;
        }
        else if (x < Zero)
        {
            return (n & 1) == 0 ? _qNaN : -RootN(-x, n);
        }
        else if (x == Zero)
        {
            // zero: n < 0 is a pole, signed for odd n
            bool oddDegree = (n & 1) != 0;
            if (n < 0)
            {
                return oddDegree && x.RawSignBit ? _nInf : _pInf;
            }
            return oddDegree ? x : Zero;
        }
        else if (IsPositiveInfinity(x))
        {
            return n > 0 ? _pInf : Zero;
        }
        else
        {
            switch (n)
            {
                case 1:
                    return x;
                case 2:
                    return Sqrt(x);
                case 3:
                    return Cbrt(x);
                case -1:
                    return One / x;
                default:
                    // x^(1/n) = 2^(log2(x) / n)
                    return Exp2FromDD(DDDiv(DDLog2(x), (n, Zero)));
            }
        }
    }

    #endregion
}
