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

using System.Runtime.InteropServices;

namespace QuadrupleLib.Utilities;

[StructLayout(LayoutKind.Sequential)]
internal struct BigMul256
{
#if BIGENDIAN
    public ulong _3;
    public ulong _2;
    public ulong _1;
    public ulong _0;
#else
    public ulong _0;
    public ulong _1;
    public ulong _2;
    public ulong _3;
#endif

    private static BigMul256 Add(BigMul256 left, BigMul256 right)
    {
        ulong carry;
        BigMul256 result = new BigMul256();

        result._0 = left._0 + right._0;

        carry = (ulong)Math.Max(0, left._0.CompareTo(result._0));
        result._1 = left._1 + right._1 + carry;

        carry = (ulong)Math.Max(0, left._1.CompareTo(result._1));
        result._2 = left._2 + right._2 + carry;

        carry = (ulong)Math.Max(0, left._2.CompareTo(result._2));
        result._3 = left._3 + right._3 + carry;

        return result;
    }

    private static BigMul256 Multiply<TAccelerator>(UInt128 left, ulong right)
        where TAccelerator : IAccelerator
    {
        var result = new BigMul256();

        ulong hi1 = TAccelerator.BigMul((ulong)left, right, out ulong lo1);
        result._0 = lo1;

        ulong hi2 = TAccelerator.BigMul((ulong)(left >> 64), right, out ulong lo2);
        result._1 = lo2 + hi1;
        result._2 = hi2 + (ulong)Math.Max(0, lo2.CompareTo(lo2 + hi1));

        return result;
    }

    public static BigMul256 Multiply<TAccelerator>(UInt128 left, UInt128 right)
        where TAccelerator : IAccelerator
    {
        var leftProd = Multiply<TAccelerator>(left, (ulong)right);
        var rightProd = Multiply<TAccelerator>(left, (ulong)(right >> 64));

        var rightShift = new BigMul256 // 64-bit left-shift
        {
            _1 = rightProd._0,
            _2 = rightProd._1,
            _3 = rightProd._2,
        };

        return Add(leftProd, rightShift);
    }
}