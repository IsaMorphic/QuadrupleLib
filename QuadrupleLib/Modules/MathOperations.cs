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

    private const int SINCOS_ITER_COUNT = 64;

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
        return Sqrt(x * x + y * y);
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
        Float128<TAccelerator> x = One, y = Zero;
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
            bool stopFlag;
            switch (theta.CompareTo(phi))
            {
                case < 0:
                    sigma = One;
                    sigma_neg = NegativeOne;
                    stopFlag = false;
                    break;
                case > 0:
                    sigma = NegativeOne;
                    sigma_neg = One;
                    stopFlag = false;
                    break;
                default:
                    sigma = Zero;
                    sigma_neg = Zero;
                    stopFlag = true;
                    break;
            }

            if (stopFlag) break;

            (x, y) = (FusedMultiplyAdd(y, ScaleB(sigma_neg, -i), x), FusedMultiplyAdd(x, ScaleB(sigma, -i), y));
            theta = FusedMultiplyAdd(sigma, _thetaTable[i], theta);
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
            return _sNaN;
        }
        else
        {
            Float128<TAccelerator> y_n = Zero;
            for (int n = 0; n < 25; n++)
            {
                (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = SinCos(y_n);
                y_n += (x - sin) / cos;
            }
            return y_n;
        }
    }

    public static Float128<TAccelerator> AsinPi(Float128<TAccelerator> x)
    {
        if (x > One || x < NegativeOne)
        {
            return _sNaN;
        }
        else
        {
            Float128<TAccelerator> y_n = Zero;
            for (int n = 0; n < 25; n++)
            {
                (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = SinCosPi(y_n);
                y_n += (x - sin) / (cos * Pi);
            }
            return y_n;
        }
    }

    public static Float128<TAccelerator> Acos(Float128<TAccelerator> x)
    {
        if (x > One || x < NegativeOne)
        {
            return _sNaN;
        }
        else
        {
            Float128<TAccelerator> y_n = One;
            for (int n = 0; n < 25; n++)
            {
                (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = SinCos(y_n);
                y_n += (cos - x) / sin;
            }
            return y_n;
        }
    }

    public static Float128<TAccelerator> AcosPi(Float128<TAccelerator> x)
    {
        if (x > One || x < NegativeOne)
        {
            return _sNaN;
        }
        else
        {
            Float128<TAccelerator> y_n = One / Pi;
            for (int n = 0; n < 25; n++)
            {
                (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = SinCosPi(y_n);
                y_n += (cos - x) / (sin * Pi);
            }
            return y_n;
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

    public static Float128<TAccelerator> Atan(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> y_n = Zero;
        for (int n = 0; n < 25; n++)
        {
            (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = SinCos(y_n);
            y_n = FusedMultiplyAdd(FusedMultiplyAdd(x, cos, -sin), cos, y_n);
        }
        return y_n;
    }

    public static Float128<TAccelerator> AtanPi(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> y_n = Zero;
        for (int n = 0; n < 25; n++)
        {
            (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = SinCosPi(y_n);
            y_n = FusedMultiplyAdd(FusedMultiplyAdd(x, cos, -sin), cos / Pi, y_n);
        }
        return y_n;
    }

    public static Float128<TAccelerator> Atan2(Float128<TAccelerator> y, Float128<TAccelerator> x)
    {
        if (x > Zero)
        {
            return Atan(y / x);
        }
        else if (y >= Zero && x < 0)
        {
            return Atan(y / x) + Pi;
        }
        else if (y < Zero && x < 0)
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
        else
        {
            return _sNaN;
        }
    }

    public static Float128<TAccelerator> Atan2Pi(Float128<TAccelerator> y, Float128<TAccelerator> x)
    {
        if (x > Zero)
        {
            return AtanPi(y / x);
        }
        else if (y >= Zero && x < 0)
        {
            return AtanPi(y / x) + Pi;
        }
        else if (y < Zero && x < 0)
        {
            return AtanPi(y / x) - Pi;
        }
        else if (y > Zero && x == Zero)
        {
            return Pi / 2.0;
        }
        else if (y < Zero && x == Zero)
        {
            return Pi / -2.0;
        }
        else
        {
            return _sNaN;
        }
    }

    #endregion

    #region Public API (hyperbolic trig functions)

    public static Float128<TAccelerator> Cosh(Float128<TAccelerator> x)
    {
        return (Exp(x) + Exp(-x)) * 0.5;
    }

    public static Float128<TAccelerator> Sinh(Float128<TAccelerator> x)
    {
        return (Exp(x) - Exp(-x)) * 0.5;
    }

    public static Float128<TAccelerator> Tanh(Float128<TAccelerator> x)
    {
        return (Exp(x) - Exp(-x)) / (Exp(x) + Exp(-x));
    }

    public static Float128<TAccelerator> Acosh(Float128<TAccelerator> x)
    {
        if (x >= One)
        {
            return Log(x + Sqrt(x * x - One));
        }
        else
        {
            return _sNaN;
        }
    }

    public static Float128<TAccelerator> Asinh(Float128<TAccelerator> x)
    {
        return Log(x + Sqrt(x * x + One));
    }

    public static Float128<TAccelerator> Atanh(Float128<TAccelerator> x)
    {
        if (Abs(x) < One)
        {
            return Log((One + x) / (One - x)) * 0.5;
        }
        else
        {
            return _sNaN;
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
        if (N == 0)
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
        if (x <= Zero)
        {
            return _sNaN;
        }

        int n = ILogB(x);
        Float128<TAccelerator> y = ScaleB(x, -n);
        if (y == One)
        {
            return n;
        }
        else
        {
            return n + _Log2(y, 25);
        }
    }

    public static Float128<TAccelerator> Log(Float128<TAccelerator> x)
    {
        return Log2(x) / Log2(E);
    }

    public static Float128<TAccelerator> Log(Float128<TAccelerator> x, Float128<TAccelerator> newBase)
    {
        return Log2(x) / Log2(newBase);
    }

    public static Float128<TAccelerator> Log10(Float128<TAccelerator> x)
    {
        return Log2(x) / Log2(10);
    }

    #endregion

    #region Public API (exponential functions)

    public static Float128<TAccelerator> Exp(Float128<TAccelerator> y)
    {
        Float128<TAccelerator> x_n = One;
        if (y > 0)
        {
            for (int i = 0; i < y; i++)
            {
                x_n *= E;
            }
        }
        else
        {
            for (int i = 0; i > y; i--)
            {
                x_n /= E;
            }
        }

        for (int n = 0; n < 25; n++)
        {
            x_n = FusedMultiplyAdd(x_n, y - Log(x_n), x_n);
        }

        return x_n;
    }

    public static Float128<TAccelerator> Exp10(Float128<TAccelerator> y)
    {
        Float128<TAccelerator> x_n = One;
        if (y > 0)
        {
            for (int i = 0; i < y; i++)
            {
                x_n *= 10;
            }
        }
        else
        {
            for (int i = 0; i > y; i--)
            {
                x_n /= 10;
            }
        }

        Float128<TAccelerator> log10 = Log(10);
        for (int n = 0; n < 25; n++)
        {
            x_n = FusedMultiplyAdd(x_n * log10, y - Log10(x_n), x_n);
        }

        return x_n;
    }

    public static Float128<TAccelerator> Exp2(Float128<TAccelerator> y)
    {
        Float128<TAccelerator> x_n = ScaleB(One, (int)Floor(y));

        Float128<TAccelerator> log2 = Log(2);
        for (int n = 0; n < 25; n++)
        {
            x_n = FusedMultiplyAdd(x_n * log2, y - Log2(x_n), x_n);
        }

        return x_n;
    }

    public static Float128<TAccelerator> Pow(Float128<TAccelerator> x, Float128<TAccelerator> y)
    {
        return Exp(y * Log(x));
    }

    #endregion

    #region Public API (root functions)

    public static Float128<TAccelerator> Sqrt(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> y_n = x * 0.5;
        for (int n = 0; n < 25; n++)
        {
            y_n = 0.5 * (y_n + x / y_n);
        }
        return y_n;
    }

    public static Float128<TAccelerator> Cbrt(Float128<TAccelerator> x)
    {
        Float128<TAccelerator> y_n = x * 0.5;
        for (int n = 0; n < 25; n++)
        {
            Float128<TAccelerator> sq = y_n * y_n;
            Float128<TAccelerator> cb = sq * y_n;
            y_n = x + 2.0 * cb / (3.0 * sq);
        }
        return y_n;
    }

    public static Float128<TAccelerator> RootN(Float128<TAccelerator> A, int n)
    {
        Float128<TAccelerator> x_k = A, f0 = (n - One) / n, f1 = A / n;
        for (int k = 0; k < 25; k++)
        {
            x_k = FusedMultiplyAdd(x_k, f0, f1 * Pow(x_k, 1 - n));
        }
        return x_k;
    }

    #endregion
}
