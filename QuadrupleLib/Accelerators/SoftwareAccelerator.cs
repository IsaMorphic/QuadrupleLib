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

namespace QuadrupleLib.Accelerators;

public sealed class SoftwareAccelerator : IAccelerator
{
    #region Private API: unsigned 128-bit division routine

    private static UInt128 Divide(UInt128 n, ulong d, out ulong r)
    {
        uint n_0 = (uint)n;
        uint n_1 = (uint)(n >> 32);
        ulong n_2 = (ulong)(n >> 64);

        (UInt128 q_2, ulong r_2) = Math.DivRem(n_2, d);
        (UInt128 q_1, ulong r_1) = Math.DivRem((r_2 << 32) | n_1, d);
        (UInt128 q_0, r) = Math.DivRem((r_1 << 32) | n_0, d);

        return q_0 | (q_1 << 32) | (q_2 << 64);
    }

    private static UInt128 Divide(UInt128 n, UInt128 d, out UInt128 r)
    {
        var dLoBits = (ulong)d;
        var dHiBits = (ulong)(d >> 64);
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
            UInt128 q = UInt128.Zero; r = n;
            while (r >= d)
            {
                var p = Divide(r, dHiBits, out ulong _);
                var p_hi = (ulong)(p >> 64);

                var prod = BigMul256.Multiply<SoftwareAccelerator>(d, p_hi);
                UInt128 s = prod._0 | ((UInt128)prod._1 << 64);
                while (p_hi > 0 && r < s)
                {
                    prod = BigMul256.Multiply<SoftwareAccelerator>(d, p_hi >>= 1);
                    s = prod._0 | ((UInt128)prod._1 << 64);
                }

                if (p_hi == 0)
                {
                    ++q; r -= d;
                    break;
                }
                else
                {
                    q += p_hi; r -= s;
                }
            }

            return q;
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
        low = prod._0 | ((ulong)prod._1 << 32);
        return prod._2 | ((ulong)prod._3 << 32);
    }

    static (UInt128 Quotient, UInt128 Remainder) IAccelerator.DivRem(UInt128 a, UInt128 b)
    {
        return (Divide(a, b, out UInt128 r), r);
    }
}
