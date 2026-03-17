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
            UInt128 quot = UInt128.Zero, rem = (uint)(n >> 96);
            for (int i = 2; i >= 0; i--)
            {
                rem = (rem << 32) | (uint)(n >> (i * 32));

                UInt128 _quot = Divide(rem, dHiBits, out uint _);
                ulong _quotHi = (ulong)(_quot >> 32);

                BigMul128 _prod = BigMul128.Multiply(_quotHi, d);
                UInt128 prod = _prod._0 | ((UInt128)_prod._1 << 32) | 
                    ((UInt128)_prod._2 << 64) | ((UInt128)_prod._3 << 96);

                while (prod > rem && rem > 0)
                {
                    _prod = BigMul128.Multiply(--_quotHi, d);
                    prod = _prod._0 | ((UInt128)_prod._1 << 32) | 
                        ((UInt128)_prod._2 << 64) | ((UInt128)_prod._3 << 96);
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

    private static UInt128 Divide(UInt128 x, UInt128 y, out UInt128 rem)
    {
        int m = 4 - (int)UInt128.LeadingZeroCount(y) / 32;
        int n = 4 - (int)UInt128.LeadingZeroCount(x) / 32;
        if (m == 1)
        {
            UInt128 q = Divide(x, (uint)y, out uint r);
            rem = r;
            return q;
        }
        else if (m > n)
        {
            rem = x;
            return UInt128.Zero;
        }
        else
        {
            UInt128 q = UInt128.Zero;
            ulong f = (1UL << 32) / ((uint)(y >> (32 * (m - 1))) + 1UL);

            BigMul256 _r = BigMul256.Multiply<SoftwareAccelerator>(x, f);
            UInt256 r = _r._0 | ((UInt256)_r._1 << 64) | ((UInt256)_r._2 << 128);

            BigMul256 _d = BigMul256.Multiply<SoftwareAccelerator>(y, f);
            UInt128 d = _d._0 | ((UInt128)_d._1 << 64);

            n = 8 - (int)UInt256.LeadingZeroCount(r) / 32;
            for (int k = n - m; k > 0; k--)
            {
                int km = k + m;
                UInt128 r3 = (UInt128)(r >> (32 * (km - 3))) & (UInt128.MaxValue >> 32);
                ulong d2 = (ulong)(d >> (32 * (m - 2)));

                UInt128 qt = Divide(r3, d2, out ulong _);
                BigMul256 _dq = BigMul256.Multiply<SoftwareAccelerator>(d, qt);
                UInt256 dq = _dq._0 | ((UInt256)_dq._1 << 64) | ((UInt256)_dq._2 << 128);
                if (r < dq) 
                {
                    _dq = BigMul256.Multiply<SoftwareAccelerator>(d, --qt);
                    dq = _dq._0 | ((UInt256)_dq._1 << 64) | ((UInt256)_dq._2 << 128);
                }

                q = (q << 32) | qt;
                r = r - dq;
            }

            rem = Divide((UInt128)r, f, out ulong _);
            return q;
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
