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

        UInt128 r0 = (UInt128)left._0 + right._0;
        result._0 = (ulong)r0;
        carry = (ulong)(r0 >> 64);

        UInt128 r1 = (UInt128)left._1 + right._1 + carry;
        result._1 = (ulong)r1;
        carry = (ulong)(r1 >> 64);

        UInt128 r2 = (UInt128)left._2 + right._2 + carry;
        result._2 = (ulong)r2;
        carry = (ulong)(r2 >> 64);

        UInt128 r3 = (UInt128)left._3 + right._3 + carry;
        result._3 = (ulong)r3;

        return result;
    }

    private static BigMul256 Multiply<TAccelerator>(UInt128 left, ulong right)
        where TAccelerator : IAccelerator
    {
        BigMul256 result = new BigMul256();

        ulong hi0 = TAccelerator.BigMul((ulong)left, right, out ulong lo0);
        result._0 = lo0;

        UInt128 prod = (((UInt128)TAccelerator.BigMul(
            (ulong)(left >> 64), right, out ulong lo1) << 64
            ) | lo1) + hi0;
        result._1 = (ulong)prod;
        result._2 = (ulong)(prod >> 64);

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