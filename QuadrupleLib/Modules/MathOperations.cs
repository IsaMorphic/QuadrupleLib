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
            // scale both legs by a power of two, which is exact, so the sum of squares neither overflows nor underflows;
            // the sum of squares and its square root are then carried in extended precision
            int scale = Math.Max(ax.Exponent, ay.Exponent);
            Float128<TAccelerator> sx = ScaleB(ax, -scale);
            Float128<TAccelerator> sy = ScaleB(ay, -scale);
            var root = DDSqrt(DDAdd(DDTwoProd(sx, sx), DDTwoProd(sy, sy)));
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
        if (phi > HalfPi)
        {
            do
            {
                phi -= HalfPi;
                (x, y) = (-y, x);
            } while (phi > HalfPi);
        }
        else if (phi < -HalfPi)
        {
            do
            {
                phi += HalfPi;
                (x, y) = (y, -x);
            } while (phi < -HalfPi);
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
        else if (Abs(x) < SmallArgument)
        {
            // asin(x) = x + x^3 / 6 + ..., and x^2 / 6 is below half an ulp of x
            return x;
        }

        // asin(x) = atan(x / sqrt(1 - x^2)), with 1 - x^2 formed exactly in extended precision
        var oneMinusSquare = DDAdd((One, Zero), DDNegate(DDTwoProd(x, x)));
        var a = DDAtan(DDDiv((x, Zero), DDSqrt(oneMinusSquare)));
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
        var ratio = DDSqrt(DDDiv(DDTwoSum(One, -x), DDTwoSum(One, x)));
        var a = DDAtan(ratio);
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
            return Atan2Pi(Sqrt(-FusedMultiplyAdd(x, x, NegativeOne)), x);
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
        else if (IsZero(z) || Abs(z) < SmallArgument)
        {
            // atan(z) = z - z^3 / 3 + ..., and z^2 / 3 is below half an ulp of z
            return z;
        }

        var a = DDAtan((z, Zero));
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
            return CopySign(x > Zero ? PiTimes(ScaleB(One, -2)) : PiTimes(3), y);
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
            // |y / x| is so large that atan2 rounds to +/- pi / 2
            return CopySign(HalfPi, y);
        }
        else if (ILogB(y) - ILogB(x) < -120)
        {
            // |y / x| is so small that atan2 is y / x for x > 0, and +/- pi to rounding for x < 0
            return x > Zero ? y / x : CopySign(Pi, y);
        }

        // atan2(y, x) = atan(y / x) for x > 0, and atan(y / x) +/- pi for x < 0
        var ratio = DDDiv((y, Zero), (x, Zero));
        var a = DDAtan(ratio);
        if (x < Zero)
        {
            a = DDAdd(a, (IsNegative(y) ? DDNegate((_piHi, _piLo)) : (_piHi, _piLo)));
        }
        return a.Hi + a.Lo;
    }

    // pi times a small rational factor, rounded once
    private static Float128<TAccelerator> PiTimes(Float128<TAccelerator> factor)
    {
        var product = DDMul((_piHi, _piLo), (factor, Zero));
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
        else if (ax >= HyperbolicLargeArgument)
        {
            return HalfExp(ax);
        }

        var e = DDExpOf(ax);
        var sum = DDAdd(e, DDDiv((One, Zero), e));
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
        else if (ax < SmallArgument)
        {
            return x;
        }
        else if (ax >= HyperbolicLargeArgument)
        {
            return CopySign(HalfExp(ax), x);
        }

        // sinh(x) = (e^x - e^-x) / 2, with e^x in extended precision so that the difference keeps its digits
        var e = DDExpOf(ax);
        var difference = DDAdd(e, DDNegate(DDDiv((One, Zero), e)));
        return CopySign(ScaleB(difference.Hi + difference.Lo, -1), x);
    }


    public static Float128<TAccelerator> Tanh(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> ax = Abs(x);
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (IsZero(x) || ax < SmallArgument)
        {
            return x;
        }
        else if (IsInfinity(x) || ax >= HyperbolicLargeArgument)
        {
            // 1 - tanh(x) < 2 e^-90, which is far below half an ulp of 1
            return CopySign(One, x);
        }

        // tanh(x) = (e^2x - 1) / (e^2x + 1)
        var e2 = DDExpOf(2 * ax);
        var ratio = DDDiv(DDAdd(e2, (-One, Zero)), DDAdd(e2, (One, Zero)));
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
        else if (x > LogLargeArgument)
        {
            // acosh(x) = ln(2x) - 1 / (4 x^2) + ..., and the correction is below half an ulp; ln(2x) is formed as ln(x) + ln 2 so 2x cannot overflow
            var big = DDAdd(DDLog(x), (_ln2Hi, _ln2Lo));
            return big.Hi + big.Lo;
        }

        // acosh(x) = ln(x + sqrt(x^2 - 1)), with x^2 - 1 formed exactly in extended precision
        var root = DDSqrt(DDAdd(DDTwoProd(x, x), (-One, Zero)));
        var u = DDAdd((x, Zero), root);
        var a = DDLogOf(u);
        return a.Hi + a.Lo;
    }


    public static Float128<TAccelerator> Asinh(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (IsInfinity(x) || IsZero(x) || Abs(x) < SmallArgument)
        {
            return x;
        }

        Float128<TAccelerator> absX = Abs(x);
        if (absX > LogLargeArgument)
        {
            // asinh(x) = ln(2x) + 1 / (4 x^2) - ..., and the correction is below half an ulp; ln(2x) is formed as ln(x) + ln 2 so 2x cannot overflow
            var big = DDAdd(DDLog(absX), (_ln2Hi, _ln2Lo));
            return CopySign(big.Hi + big.Lo, x);
        }

        // asinh(|x|) = ln(|x| + sqrt(x^2 + 1)), with x^2 + 1 formed exactly in extended precision
        var root = DDSqrt(DDAdd(DDTwoProd(absX, absX), (One, Zero)));
        var u = DDAdd((absX, Zero), root);
        var a = DDLogOf(u);
        return CopySign(a.Hi + a.Lo, x);
    }


    public static Float128<TAccelerator> Atanh(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> absX = Abs(x);
        if (IsNaN(x) || absX > One)
        {
            return _qNaN;
        }
        else if (IsZero(x) || absX < SmallArgument)
        {
            return x;
        }
        else if (absX == One)
        {
            return CopySign(_pInf, x);
        }

        // atanh(x) = ln((1 + x) / (1 - x)) / 2, with 1 + x and 1 - x exact in extended precision
        var ratio = DDDiv(DDTwoSum(One, x), DDTwoSum(One, -x));
        var a = DDLogOf(ratio);
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

        // exact for powers of two; otherwise ln(x) / ln(2) in extended precision
        int n = ILogB(x);
        if (ScaleB(x, -n) == One)
        {
            return n;
        }

        var result = DDLog2(x);
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

        var result = DDLog(y);
        return result.Hi + result.Lo;
    }


    public static Float128<TAccelerator> Log(Float128<TAccelerator> y, Float128<TAccelerator> newBase)
    {
        if (IsNaN(y) || IsNaN(newBase))
        {
            return _qNaN;
        }
        else if (y < Zero || newBase <= Zero)
        {
            return _qNaN;
        }
        else if (newBase == One)
        {
            // ln(newBase) is zero, so the quotient divides by zero for every y; matches System.Math.Log(a, 1),
            // which returns NaN
            return _qNaN;
        }
        else if (y == One)
        {
            return Zero;
        }
        else if (IsPositiveInfinity(newBase))
        {
            // matches System.Math.Log(a, +Infinity), which is NaN for a != 1; y = 1 was handled above and gives 0
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

        var quotient = DDDiv(DDLog(y), DDLog(newBase));
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

        var result = DDMul(DDLog(y), (_invLn10Hi, _invLn10Lo));
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

        // y = k * ln(2) + r with |r| <= ln(2) / 2; the first subtraction is exact because k * _ln2Hi is exact
        Float128<TAccelerator> kf = Round(y / _ln2Hi);
        Float128<TAccelerator> r = FusedMultiplyAdd(-kf, _ln2Lo, y - kf * _ln2Hi);

        // exp(r) = 1 + r * (1 + r / 2 * (1 + r / 3 * (...))), evaluated by Horner's rule
        Float128<TAccelerator> series = One;
        for (int n = 27; n >= 1; n--)
        {
            series = One + r * series / n;
        }

        return ScaleB(series, (int)kf);
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

        // 10^y = 2^(y log2(10)), with the product y log2(10) in extended precision
        return Exp2FromDD(DDMul((y, Zero), (_log2_10Hi, _log2_10Lo)));
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
        var logMagnitude = DDMul((y, Zero), DDLog2(baseMagnitude));
        Float128<TAccelerator> magnitude;
        if (IsInteger(y) && Abs(y) < ScaleB(One, 20) && Abs(logMagnitude.Hi) < EXPONENT_BIAS - 3)
        {
            // an integer power in range: repeated squaring in extended precision is exact whenever the result fits in
            // 226 bits, so exact powers such as 2^100 come out exact
            var power = DDPowInt((baseMagnitude, Zero), (ulong)Abs(y));
            if (IsNegative(y))
            {
                power = DDDiv((One, Zero), power);
            }
            magnitude = power.Hi + power.Lo;
        }
        else
        {
            // |x|^y = 2^(y log2|x|), with the product in extended precision
            magnitude = Exp2FromDD(logMagnitude);
        }

        // the sign is negative only for an odd power of a negative base
        return IsNegative(x) && isOddPower ? -magnitude : magnitude;
    }


    #endregion

    #region Private API (exponential functions)
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
            Float128<TAccelerator> y_n = ScaleB(x, -x.Exponent / 2);
            for (int n = 0; n < 25; n++)
            {
                y_n = 0.5 * (y_n + x / y_n);
            }
            return y_n;
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
            // IEEE 754: the cube root of a zero or an infinity is that same value, sign included
            return x;
        }

        // cbrt(x) = 2^(log2(|x|) / 3), with the logarithm in extended precision
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
            // IEEE 754 rootn: a negative degree gives a pole, and an odd degree keeps the sign of zero
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
                default:
                    // x^(1/n) = 2^(log2(x) / n), with the logarithm and quotient in extended precision; n may be negative
                    return Exp2FromDD(DDDiv(DDLog2(x), (n, Zero)));
            }
        }
    }

    #endregion

    #region Private API (root functions)

    #endregion
}
