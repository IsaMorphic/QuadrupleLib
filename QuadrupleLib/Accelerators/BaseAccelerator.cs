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

namespace QuadrupleLib.Accelerators
{
    public abstract class BaseAccelerator<TAccelerator>
        where TAccelerator : BaseAccelerator<TAccelerator>,
        IAccelerator
    {
        #region Private API: unsigned 256-bit division routine

        private static UInt256 Divide(UInt256 n, uint d, out uint r)
        {
            uint n_0 = (uint)n;
            uint n_1 = (uint)(n >> 32);
            uint n_2 = (uint)(n >> 64);
            uint n_3 = (uint)(n >> 96);
            uint n_4 = (uint)(n >> 128);
            uint n_5 = (uint)(n >> 160);
            ulong n_6 = (ulong)(n >> 192);

            (UInt256 q_6, ulong r_6) = Math.DivRem(n_6, d);
            (UInt256 q_5, ulong r_5) = Math.DivRem((r_6 << 32) | n_5, d);
            (UInt256 q_4, ulong r_4) = Math.DivRem((r_5 << 32) | n_4, d);
            (UInt256 q_3, ulong r_3) = Math.DivRem((r_4 << 32) | n_3, d);
            (UInt256 q_2, ulong r_2) = Math.DivRem((r_3 << 32) | n_2, d);
            (UInt256 q_1, ulong r_1) = Math.DivRem((r_2 << 32) | n_1, d);
            (UInt256 q_0, ulong r_0) = Math.DivRem((r_1 << 32) | n_0, d);

            r = (uint)r_0; return q_0 | (q_1 << 32) | (q_2 << 64) |
                (q_3 << 96) | (q_4 << 128) | (q_5 << 160) | (q_6 << 192);
        }

        // 64 bits of the 288-bit value (carry:low) starting at bit position shift
        private static ulong Window(UInt256 low, ulong carry, int shift)
        {
            if (shift == 0)
            {
                return (ulong)low;
            }
            else if (shift >= 256)
            {
                return carry >> (shift - 256);
            }
            return (ulong)((low >> shift) | ((UInt256)carry << (256 - shift)));
        }

        public static UInt256 Divide(UInt256 x, UInt128 y, out UInt128 rem)
        {
            int m = 4 - (int)UInt128.LeadingZeroCount(y) / 32;
            int n = 8 - (int)UInt256.LeadingZeroCount(x) / 32;
            if (m == 1)
            {
                UInt256 q = Divide(x, (uint)y, out uint r);
                rem = r;
                return q;
            }
            else if (m > n)
            {
                rem = (UInt128)x;
                return UInt128.Zero;
            }
            else
            {
                BigMul256 _df;
                ulong factor = (1UL << 32) / ((uint)(y >> (32 * (m - 1))) + 1UL);

                _df = BigMul256.Multiply2x2<TAccelerator>(y, factor);
                UInt128 divisor = _df._0 | ((UInt128)_df._1 << 64);
                uint d1 = (uint)(divisor >> (32 * (m - 1)));

                // x * factor can need 288 bits. The low 256 bits are in dividend and the
                // 289th-to-288th digit is in carry. Here dividend is the low 256 bits and
                // carry is bits 256..287 of the scaled dividend.
                UInt256 xLow = x & ((UInt256.One << 224) - 1);
                BigMul256 _lowProd = BigMul256.Multiply4x1<TAccelerator>(xLow, factor);
                UInt256 lowProd = _lowProd._0 | ((UInt256)_lowProd._1 << 64) |
                    ((UInt256)_lowProd._2 << 128) | ((UInt256)_lowProd._3 << 192);
                ulong highProd = (ulong)(x >> 224) * factor;
                ulong carry = (ulong)(((UInt128)highProd + (UInt128)(lowProd >> 224)) >> 32);

                _df = BigMul256.Multiply4x1<TAccelerator>(x, factor);
                UInt256 dividend = _df._0 | ((UInt256)_df._1 << 64) |
                    ((UInt256)_df._2 << 128) | ((UInt256)_df._3 << 192);
                n = carry != 0 ? 9 : 8 - (int)UInt256.LeadingZeroCount(dividend) / 32;

                UInt256 quotient = 0;
                for (int k = n - m + 1; k > 0; k--)
                {
                    ulong r2 = Window(dividend, carry, 32 * (k + m - 2));
                    ulong qt = r2 / d1;

                    BigMul256 _dq = BigMul256.Multiply2x2<TAccelerator>(divisor, qt);
                    UInt256 dq = _dq._0 | ((UInt256)_dq._1 << 64) | ((UInt256)_dq._2 << 128);
                    // dq << 32(k-1) as a 288-bit value: dqLow (bits 0..255) and dqHigh (bits 256..287)
                    int shift = 32 * (k - 1);
                    UInt256 dqLow = dq << shift;
                    ulong dqHigh = shift == 0 ? 0 : (ulong)(dq >> (256 - shift));
                    while (carry < dqHigh || (carry == dqHigh && dividend < dqLow))
                    {
                        _dq = BigMul256.Multiply2x2<TAccelerator>(divisor, --qt);
                        dq = _dq._0 | ((UInt256)_dq._1 << 64) | ((UInt256)_dq._2 << 128);
                        dqLow = dq << shift;
                        dqHigh = shift == 0 ? 0 : (ulong)(dq >> (256 - shift));
                    }

                    quotient = (quotient << 32) | qt;
                    ulong borrow = dividend < dqLow ? 1UL : 0UL;
                    dividend -= dqLow;
                    carry = carry - dqHigh - borrow;
                }

                rem = (UInt128)Divide(dividend, (uint)factor, out uint _);
                return quotient;
            }
        }

        #endregion
    }
}
