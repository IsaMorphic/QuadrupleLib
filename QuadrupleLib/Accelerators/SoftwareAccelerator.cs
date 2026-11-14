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
            BigMul256 _df;
            ulong factor = (1UL << 32) / ((uint)(y >> (32 * (m - 1))) + 1UL);

            _df = BigMul256.Multiply<SoftwareAccelerator>(x, factor);
            UInt256 dividend = _df._0 | ((UInt256)_df._1 << 64) | ((UInt256)_df._2 << 128);
            n = 8 - (int)UInt256.LeadingZeroCount(dividend) / 32;

            _df = BigMul256.Multiply<SoftwareAccelerator>(y, factor);
            UInt128 divisor = _df._0 | ((UInt128)_df._1 << 64);
            uint d1 = (uint)(divisor >> (32 * (m - 1)));

            UInt128 quotient = 0;
            for (int k = Math.Max(n - m, 1); k > 0; k--)
            {
                ulong r2 = (ulong)(dividend >> (32 * (k + m - 2)));
                ulong qt = r2 / d1;

                BigMul256 _dq = BigMul256.Multiply<SoftwareAccelerator>(divisor, qt);
                UInt256 dq = _dq._0 | ((UInt256)_dq._1 << 64) | ((UInt256)_dq._2 << 128);
                while (dividend < (dq << 32 * (k - 1)))
                {
                    _dq = BigMul256.Multiply<SoftwareAccelerator>(divisor, --qt);
                    dq = _dq._0 | ((UInt256)_dq._1 << 64) | ((UInt256)_dq._2 << 128);
                }

                quotient = (quotient << 32) | qt;
                dividend -= dq << 32 * (k - 1);
            }

            rem = Divide((UInt128)dividend, (uint)factor, out uint _);
            return quotient;
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
        return (Divide(a, b, out UInt128 r), r);
    }
}
