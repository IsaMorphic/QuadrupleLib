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
using System.Numerics;

namespace QuadrupleLib.Accelerators;

public sealed class SoftwareAccelerator : IAccelerator
{
    #region Private API: unsigned 128-bit division routine

    private static UInt128 Divide(UInt128 n, uint d, out uint r)
    {
        uint n_0 = (uint)n;
        uint n_1 = (uint)(n >> 32);
        ulong n_2 = (ulong)(n >> 64);

        (UInt128 q_2, ulong r_2) = Math.DivRem(n_2, d);
        (UInt128 q_1, ulong r_1) = Math.DivRem((r_2 << 32) | n_1, d);
        (UInt128 q_0, ulong r_0) = Math.DivRem((r_1 << 32) | n_0, d);

        r = (uint)r_0; return q_0 | (q_1 << 32) | (q_2 << 64);
    }

    private static UInt128 Divide(UInt128 n, ulong d, out ulong r)
    {
        uint dLoBits = (uint)d;
        uint dHiBits = (uint)(d >> 32);
        if (d != 0 && d > n)
        {
            r = (ulong)n; return UInt128.Zero;
        }
        else if (d == n)
        {
            r = 0UL; return UInt128.One;
        }
        else if (dHiBits > 0)
        {
            UInt128 quot = UInt128.Zero, rem = UInt128.Zero;
            for (int i = 3; i >= 0; i--)
            {
                rem = (rem << 32) | (uint)(n >> (i * 32));

                UInt128 _quot = Divide(rem, dHiBits, out uint _);
                ulong _quotHi = (ulong)(_quot >> 32);

                BigMul128 _prod = BigMul128.Multiply(_quotHi, d);
                UInt128 prod = _prod._0 | ((UInt128)_prod._1 << 32) | ((UInt128)_prod._2 << 64) | ((UInt128)_prod._3 << 96);

                while (prod > rem)
                {
                    _prod = BigMul128.Multiply(--_quotHi, d);
                    prod = _prod._0 | ((UInt128)_prod._1 << 32) | ((UInt128)_prod._2 << 64) | ((UInt128)_prod._3 << 96);
                }

                quot = quot << 32 | _quotHi;
                rem -= prod;
            }

            r = (ulong)rem;
            return quot;
        }
        else
        {
            var q = Divide(n, dLoBits, out uint _r);
            r = _r; return q;
        }
    }

    public static UInt128 Divide(UInt128 n, UInt128 d, out UInt128 r)
    {
        ulong dLoBits = (ulong)d;
        ulong dHiBits = (ulong)(d >> 64);
        if (d != 0 && d > n)
        {
            r = n; return UInt128.Zero;
        }
        else if (d == n)
        {
            r = UInt128.Zero; return UInt128.One;
        }
        else if (dHiBits > 0)
        {
            UInt128 quot = UInt128.Zero, rem = UInt128.Zero;
            for (int i = 1; i >= 0; i--)
            {
                rem = (rem << 64) | (ulong)(n >> (i * 64));

                UInt128 _quot = Divide(rem, dHiBits, out ulong _);
                UInt128 _quotHi = _quot >> 64;

                BigMul256 _prod = BigMul256.Multiply<SoftwareAccelerator>(_quotHi, d);
                UInt128 prod = _prod._0 | ((UInt128)_prod._1 << 64);

                while(prod > rem && rem > 0) 
                {
                    _prod = BigMul256.Multiply<SoftwareAccelerator>(--_quotHi, d);
                    prod = _prod._0 | ((UInt128)_prod._1 << 64);
                }

                quot = quot << 64 | _quotHi;
                rem -= prod;
            }

            r = rem;
            return quot;
        }
        else
        {
            var q = Divide(n, dLoBits, out ulong _r);
            r = _r; return q;
        }
    }

    #endregion

    private SoftwareAccelerator() { }

    static ulong IAccelerator.BigMul(ulong a, ulong b, out ulong low)
    {
        BigMul128 prod = BigMul128.Multiply(a, b);
        (low, ulong high) = (
            prod._0 | ((ulong)prod._1 << 32),
            prod._2 | ((ulong)prod._3 << 32)
            );
        return high;
    }

    static (UInt128 Quotient, UInt128 Remainder) IAccelerator.DivRem(UInt128 a, UInt128 b)
    {
        var x = (Divide(a, b, out UInt128 r), r);
        var y = UInt128.DivRem(a, b);

        if (x != y)
        {
            Divide(a, b, out UInt128 _);
        }

        return y;
    }
}
