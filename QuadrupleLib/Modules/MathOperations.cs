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
            // scale both legs by a power of two, which is exact, so the sum of squares neither overflows nor underflows
            int scale = Math.Max(ax.Exponent, ay.Exponent);
            Float128<TAccelerator> sx = ScaleB(ax, -scale);
            Float128<TAccelerator> sy = ScaleB(ay, -scale);
            return ScaleB(Sqrt(FusedMultiplyAdd(sx, sx, sy * sy)), scale);
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
        if (x > One || x < NegativeOne)
        {
            return _qNaN;
        }
        else
        {
            return Atan2(x, Sqrt(-FusedMultiplyAdd(x, x, NegativeOne)));
        }
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
        if (x > One || x < NegativeOne)
        {
            return _qNaN;
        }
        else
        {
            return Atan2(Sqrt(-FusedMultiplyAdd(x, x, NegativeOne)), x);
        }
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

        Float128<TAccelerator> x = One, y = z, theta = Zero, sigma, sigma_neg;
        for (int i = 0; i < SINCOS_ITER_COUNT; i++)
        {
            if (y > Zero)
            {
                (sigma, sigma_neg) = (NegativeOne, One);
            }
            else
            {
                (sigma, sigma_neg) = (One, NegativeOne);
            }

            (x, y) = (FusedMultiplyAdd(y, ScaleB(sigma_neg, -i), x), FusedMultiplyAdd(x, ScaleB(sigma, -i), y));
            theta += sigma_neg * _thetaTable[i];
        }
        return theta;
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
        else if (y >= Zero && x < Zero)
        {
            return Atan(y / x) + Pi;
        }
        else if (y < Zero && x < Zero)
        {
            return Atan(y / x) - Pi;
        }
        else if (y > Zero && x == Zero)
        {
            return Pi / 2.0;
        }
        else if (y < Zero && x == Zero)
        {
            return Pi / -2.0;
        }
        else if (x > Zero)
        {
            return Atan(y / x);
        }
        else
        {
            // atan2(+/-0, +/-0)
            return IsNegative(x) ? CopySign(Pi, y) : y;
        }
    }

    public static Float128<TAccelerator> Atan2Pi(Float128<TAccelerator> y, Float128<TAccelerator> x)
    {
        return Atan2(y, x) / Pi;
    }

    #endregion

    #region Public API (hyperbolic trig functions)

    public static Float128<TAccelerator> Cosh(Float128<TAccelerator> x)
    {
        if (IsZero(x))
        {
            return One;
        }
        else
        {
            return (Exp(x) + Exp(-x)) * 0.5;
        }
    }

    public static Float128<TAccelerator> Sinh(Float128<TAccelerator> x)
    {
        if (IsZero(x))
        {
            return x;
        }
        else
        {
            return (Exp(x) - Exp(-x)) * 0.5;
        }
    }

    public static Float128<TAccelerator> Tanh(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> t = Exp(-2 * Abs(x));
        return CopySign((One - t) / (One + t), x);
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
        else if (x > ScaleB(One, EXPONENT_BIAS / 2))
        {
            return Log(x + Abs(x) - One / ScaleB(Abs(x), 1));
        }
        else if (x > One)
        {
            return Log(x + Sqrt(FusedMultiplyAdd(x, x, NegativeOne)));
        }
        else
        {
            return _qNaN;
        }
    }

    public static Float128<TAccelerator> Asinh(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (IsPositiveInfinity(x))
        {
            return _pInf;
        }
        else if (IsNegativeInfinity(x))
        {
            return _nInf;
        }
        else if (IsZero(x))
        {
            return x;
        }
        else if (x > ScaleB(One, EXPONENT_BIAS / 2))
        {
            return Log(x + Abs(x) + One / ScaleB(Abs(x), 1));
        }
        else
        {
            Float128<TAccelerator> absX = Abs(x);
            return CopySign(Log(absX + Sqrt(FusedMultiplyAdd(absX, absX, One))), x);
        }
    }

    public static Float128<TAccelerator> Atanh(Float128<TAccelerator> x)
    {
        if (IsZero(x))
        {
            return x;
        }
        
        Float128<TAccelerator> absX = Abs(x);
        if (absX < One)
        {
            return Log((One + x) / (One - x)) * 0.5;
        }
        else if (absX == One)
        {
            return CopySign(_pInf, x);
        }
        else
        {
            return _qNaN;
        }
    }

    #endregion

    #region Public API (logarithm functions)

    public static int ILogB(Float128<TAccelerator> x)
    {
        return x.Exponent - (int)UInt128.LeadingZeroCount(x.Significand) + 15;
    }

    private static Float128<TAccelerator> _Log2(Float128<TAccelerator> y, int N)
    {
        if (N == 0 || y == One)
        {
            return Zero;
        }
        else
        {
            int m = 0;
            while (y < 2.0)
            { y *= y; --m; }
            return ScaleB(One + _Log2(y * 0.5, N - 1), m);
        }
    }

    public static Float128<TAccelerator> Log2(Float128<TAccelerator> x)
    {
        if (IsNaN(x))
        {
            return _qNaN;
        }
        else if (x <= Zero)
        {
            return _qNaN;
        }
        else if (IsPositiveInfinity(x))
        {
            return _pInf;
        }

        int n = ILogB(x);
        Float128<TAccelerator> y = ScaleB(x, -n);
        if (y == One)
        {
            return n;
        }
        else
        {
            return n + _Log2(y, 30);
        }
    }

    public static Float128<TAccelerator> Log(Float128<TAccelerator> y)
    {
        if (IsNaN(y))
        {
            return _qNaN;
        }
        else if (y <= Zero)
        {
            return _qNaN;
        }
        else if (y == One)
        {
            return Zero;
        }
        else if (IsPositiveInfinity(y))
        {
            return _pInf;
        }

        Float128<TAccelerator> k = Log2(E);
        Float128<TAccelerator> x_n = Log2(y) / k;
        for (int i = 0; i < 15; i++)
        {
            x_n += FusedMultiplyAdd(y, Exp2(-k * x_n), NegativeOne);
        }

        return x_n;
    }

    public static Float128<TAccelerator> Log(Float128<TAccelerator> y, Float128<TAccelerator> newBase)
    {
        if (IsNaN(y) || IsNaN(newBase))
        {
            return _qNaN;
        }
        else if (y <= Zero || newBase <= Zero)
        {
            return _qNaN;
        }
        else if (y == One)
        {
            return Zero;
        }
        else if (newBase == One)
        {
            // the base-1 logarithm is undefined (ln(1) is zero); matches System.Math.Log(a, 1), which returns NaN
            return _qNaN;
        }
        else if (IsPositiveInfinity(y))
        {
            return newBase < One ? _nInf : _pInf;
        }

        Float128<TAccelerator> k = Log2(newBase);
        Float128<TAccelerator> x_n = Abs(Log2(y) / k);
        if (k > Zero)
        {
            for (int i = 0; i < 25; i++)
            {
                x_n += FusedMultiplyAdd(y, Exp2(-k * x_n), NegativeOne);
            }

            return x_n;
        }
        else
        {
            for (int i = 0; i < 25; i++)
            {
                x_n += FusedMultiplyAdd(y, Exp2(k * x_n), NegativeOne);
            }

            return -x_n;
        }
    }

    public static Float128<TAccelerator> Log10(Float128<TAccelerator> y)
    {
        if (IsNaN(y))
        {
            return _qNaN;
        }
        else if (y <= Zero)
        {
            return _qNaN;
        }
        else if (y == One)
        {
            return Zero;
        }
        else if (IsPositiveInfinity(y))
        {
            return _pInf;
        }

        Float128<TAccelerator> k = Log2(10);
        Float128<TAccelerator> x_n = Log2(y) / k;
        for (int i = 0; i < 15; i++)
        {
            x_n += FusedMultiplyAdd(y, Exp2(-k * x_n), NegativeOne);
        }

        return x_n;
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

        Float128<TAccelerator> x_n = Exp2(y / Log(2));
        if (IsInfinity(x_n) || IsZero(x_n))
        {
            // the result overflows or underflows, and refining it would produce NaN
            return x_n;
        }

        for (int n = 0; n < 15; n++)
        {
            x_n = Abs(FusedMultiplyAdd(x_n, y - Log(x_n), x_n));
        }

        return x_n;
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

        Float128<TAccelerator> x_n = Exp2(y / Log10(2));
        if (IsInfinity(x_n) || IsZero(x_n))
        {
            // the result overflows or underflows, and refining it would produce NaN
            return x_n;
        }

        for (int n = 0; n < 15; n++)
        {
            x_n = Abs(FusedMultiplyAdd(x_n, y - Log10(x_n), x_n));
        }

        return x_n;
    }

    public static Float128<TAccelerator> Exp2(Float128<TAccelerator> y)
    {
        if (IsNaN(y))
        {
            return _qNaN;
        }
        else if (Abs(y) >= EXPONENT_BIAS + 1)
        {
            return y > Zero ? _pInf : Zero;
        }

        Float128<TAccelerator> x_n = ScaleB(One, (int)y);

        if (y % One != Zero)
        {
            Float128<TAccelerator> log2 = One / Log2(E);
            for (int n = 0; n < 5; n++)
            {
                x_n = Abs(FusedMultiplyAdd(x_n * log2, y - Log2(x_n), x_n));
            }
        }

        return x_n;
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
        else if (IsInteger(y) && Abs(y) <= ulong.MaxValue)
        {
            // an integer power by repeated squaring is exact when the result is representable,
            // and its error grows only with the exponent, so it is far closer than the exponential
            // and logarithm
            ulong n = (ulong)Abs(y);
            if (IsPositive(y))
            {
                return PowN(x, n);
            }

            // take the reciprocal of a normal power; when the power overflows or loses bits
            // as a subnormal, raise the reciprocal instead
            Float128<TAccelerator> power = PowN(x, n);
            return IsNormal(power) ? One / power : PowN(One / x, n);
        }
        else if (IsEvenInteger(y))
        {
            return Exp(y * Log(Abs(x)));
        }
        else if (IsOddInteger(y))
        {
            return CopySign(Exp(y * Log(Abs(x))), x);
        }
        else
        {
            return Exp(y * Log(x));
        }
    }

    #endregion

    #region Private API (exponential functions)

    private static Float128<TAccelerator> PowN(Float128<TAccelerator> x, ulong n)
    {
        Float128<TAccelerator> result = One;
        while (true)
        {
            if ((n & 1) != 0)
            {
                result *= x;
            }

            n >>= 1;
            if (n == 0)
            {
                return result;
            }

            x *= x;
        }
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
        else
        {
            Float128<TAccelerator> y_n = ScaleB(x, -x.Exponent * 2 / 3);
            for (int n = 0; n < 25; n++)
            {
                Float128<TAccelerator> sq = y_n * y_n;
                y_n = (x / sq + 2.0 * y_n) / 3.0;
            }
            return y_n;
        }
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
                case < 0:
                    // widen before negating, since int.MinValue has no positive counterpart
                    return One / RootNPositive(x, (ulong)(-(long)n));
                default:
                    return RootNPositive(x, (ulong)n);
            }
        }
    }

    #endregion

    #region Private API (root functions)

    private static Float128<TAccelerator> RootNPositive(Float128<TAccelerator> x, ulong n)
    {
        switch (n)
        {
            case 1:
                return x;
            case 2:
                return Sqrt(x);
            case 3:
                return Cbrt(x);
        }

        // start from the exponential and logarithm estimate, then refine it with Newton's method
        // on y^n = x; dividing the correction by n cancels the rounding error that the power adds
        Float128<TAccelerator> degree = n;
        Float128<TAccelerator> y = Pow(x, One / degree);
        for (int i = 0; i < 8; i++)
        {
            Float128<TAccelerator> correction = (x / PowN(y, n - 1) - y) / degree;
            if (IsZero(correction) || !IsFinite(correction))
            {
                // converged, or the power underflowed and the estimate cannot be refined
                break;
            }

            y += correction;
        }

        return y;
    }

    #endregion
}
