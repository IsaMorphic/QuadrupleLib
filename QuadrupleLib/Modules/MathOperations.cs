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

namespace QuadrupleLib;

public partial struct Float128
{
    #region Public API (trig functions)

    private const int SINCOS_ITER_COUNT = 32;

    private static readonly Float128[] _thetaTable;

    private static readonly Float128 _K_n;

    private static Float128 AtanPow2(int k)
    {
        Float128 x_n = One;
        if (k == 0) return Pi * 0.25;
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

    private static Float128 ComputeK(int n)
    {
        Float128 K_i = One;
        for (int i = 0; i < n; i++)
        {
            K_i /= Sqrt(One + ScaleB(One, i * -2));
        }
        return K_i;
    }

    public static Float128 Hypot(Float128 x, Float128 y)
    {
        return Sqrt(x * x + y * y);
    }

    public static Float128 Cos(Float128 x)
    {
        return SinCos(x).Cos;
    }

    public static Float128 CosPi(Float128 x)
    {
        return SinCosPi(x).CosPi;
    }

    public static Float128 Sin(Float128 x)
    {
        return SinCos(x).Sin;
    }

    public static Float128 SinPi(Float128 x)
    {
        return SinCosPi(x).SinPi;
    }

    public static (Float128 Sin, Float128 Cos) SinCos(Float128 alpha)
    {
        Float128 x = One, y = Zero;
        Float128 phi = Ieee754Remainder(alpha, Tau);
        if (phi > Pi / 2)
        {
            do
            {
                phi -= Pi / 2;
                (x, y) = (-y, x);
            } while (phi > Pi / 2);
        }
        else if (phi < -Pi / 2)
        {
            do
            {
                phi += Pi / 2;
                (x, y) = (y, -x);
            } while (phi < -Pi / 2);
        }

        Float128 sigma, theta = Zero;
        for (int i = 0; i < SINCOS_ITER_COUNT; i++)
        {
            sigma = theta < phi ? One : NegativeOne;

            (x, y) = (x - ScaleB(sigma * y, -i), ScaleB(sigma * x, -i) + y);
            theta += sigma * _thetaTable[i];
        }
        return (y * _K_n, x * _K_n);
    }

    public static (Float128 SinPi, Float128 CosPi) SinCosPi(Float128 x)
    {
        return SinCos(x * Pi);
    }

    public static Float128 Asin(Float128 x)
    {
        if (x > One || x < NegativeOne)
        {
            return _sNaN;
        }
        else
        {
            Float128 y_n = Zero;
            for (int n = 0; n < 25; n++)
            {
                (Float128 sin, Float128 cos) = SinCos(y_n);
                y_n += (x - sin) / cos;
            }
            return y_n;
        }
    }

    public static Float128 AsinPi(Float128 x)
    {
        if (x > One || x < NegativeOne)
        {
            return _sNaN;
        }
        else
        {
            Float128 y_n = Zero;
            for (int n = 0; n < 25; n++)
            {
                (Float128 sin, Float128 cos) = SinCosPi(y_n);
                y_n += (x - sin) / (cos * Pi);
            }
            return y_n;
        }
    }

    public static Float128 Acos(Float128 x)
    {
        if (x > One || x < NegativeOne)
        {
            return _sNaN;
        }
        else
        {
            Float128 y_n = One;
            for (int n = 0; n < 25; n++)
            {
                (Float128 sin, Float128 cos) = SinCos(y_n);
                y_n += (cos - x) / sin;
            }
            return y_n;
        }
    }

    public static Float128 AcosPi(Float128 x)
    {
        if (x > One || x < NegativeOne)
        {
            return _sNaN;
        }
        else
        {
            Float128 y_n = One / Pi;
            for (int n = 0; n < 25; n++)
            {
                (Float128 sin, Float128 cos) = SinCosPi(y_n);
                y_n += (cos - x) / (sin * Pi);
            }
            return y_n;
        }
    }

    public static Float128 Tan(Float128 alpha)
    {
        (Float128 y, Float128 x) = SinCos(alpha);
        return y / x;
    }

    public static Float128 TanPi(Float128 alpha)
    {
        (Float128 y, Float128 x) = SinCosPi(alpha);
        return y / x;
    }

    public static Float128 Atan(Float128 x)
    {
        Float128 y_n = Zero;
        for (int n = 0; n < 25; n++)
        {
            (Float128 sin, Float128 cos) = SinCos(y_n);
            y_n = FusedMultiplyAdd(FusedMultiplyAdd(x, cos, -sin), cos, y_n);
        }
        return y_n;
    }

    public static Float128 AtanPi(Float128 x)
    {
        Float128 y_n = Zero;
        for (int n = 0; n < 25; n++)
        {
            (Float128 sin, Float128 cos) = SinCosPi(y_n);
            y_n = FusedMultiplyAdd(FusedMultiplyAdd(x, cos, -sin), cos / Pi, y_n);
        }
        return y_n;
    }

    public static Float128 Atan2(Float128 y, Float128 x)
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

    public static Float128 Atan2Pi(Float128 y, Float128 x)
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

    public static Float128 Cosh(Float128 x)
    {
        return (Exp(x) + Exp(-x)) * 0.5;
    }

    public static Float128 Sinh(Float128 x)
    {
        return (Exp(x) - Exp(-x)) * 0.5;
    }

    public static Float128 Tanh(Float128 x)
    {
        return (Exp(x) - Exp(-x)) / (Exp(x) + Exp(-x));
    }

    public static Float128 Acosh(Float128 x)
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

    public static Float128 Asinh(Float128 x)
    {
        return Log(x + Sqrt(x * x + One));
    }

    public static Float128 Atanh(Float128 x)
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

    public static int ILogB(Float128 x)
    {
        return x.Exponent - (int)UInt128.LeadingZeroCount(x.Significand) + 15;
    }

    private static Float128 _Log2(Float128 y, int N)
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

    public static Float128 Log2(Float128 x)
    {
        if (x <= Zero)
        {
            return _sNaN;
        }

        int n = ILogB(x);
        Float128 y = ScaleB(x, -n);
        if (y == One)
        {
            return n;
        }
        else
        {
            return n + _Log2(y, 25);
        }
    }

    public static Float128 Log(Float128 x)
    {
        return Log2(x) / Log2(E);
    }

    public static Float128 Log(Float128 x, Float128 newBase)
    {
        return Log2(x) / Log2(newBase);
    }

    public static Float128 Log10(Float128 x)
    {
        return Log2(x) / Log2(10);
    }

    #endregion

    #region Public API (exponential functions)

    public static Float128 Exp(Float128 y)
    {
        Float128 x_n = One;
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

    public static Float128 Exp10(Float128 y)
    {
        Float128 x_n = One;
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

        Float128 log10 = Log(10);
        for (int n = 0; n < 25; n++)
        {
            x_n = FusedMultiplyAdd(x_n * log10, y - Log10(x_n), x_n);
        }

        return x_n;
    }

    public static Float128 Exp2(Float128 y)
    {
        Float128 x_n = ScaleB(One, (int)Floor(y));

        Float128 log2 = Log(2);
        for (int n = 0; n < 25; n++)
        {
            x_n = FusedMultiplyAdd(x_n * log2, y - Log2(x_n), x_n);
        }

        return x_n;
    }

    public static Float128 Pow(Float128 x, Float128 y)
    {
        return Exp(y * Log(x));
    }

    #endregion

    #region Public API (root functions)

    public static Float128 Sqrt(Float128 x)
    {
        Float128 y_n = x * 0.5;
        for (int n = 0; n < 25; n++)
        {
            y_n = 0.5 * (y_n + x / y_n);
        }
        return y_n;
    }

    public static Float128 Cbrt(Float128 x)
    {
        Float128 y_n = x * 0.5;
        for (int n = 0; n < 25; n++)
        {
            Float128 sq = y_n * y_n;
            Float128 cb = sq * y_n;
            y_n = x + 2.0 * cb / (3.0 * sq);
        }
        return y_n;
    }

    public static Float128 RootN(Float128 A, int n)
    {
        Float128 x_k = A, f0 = (n - One) / n, f1 = A / n;
        for (int k = 0; k < 25; k++)
        {
            x_k = FusedMultiplyAdd(x_k, f0, f1 * Pow(x_k, 1 - n));
        }
        return x_k;
    }

    #endregion
}
