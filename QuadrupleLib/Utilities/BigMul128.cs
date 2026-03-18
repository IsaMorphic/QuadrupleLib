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
internal struct BigMul128
{
#if BIGENDIAN
    public uint _3;
    public uint _2;
    public uint _1;
    public uint _0;
#else
    public uint _0;
    public uint _1;
    public uint _2;
    public uint _3;
#endif

    private static BigMul128 Add(BigMul128 left, BigMul128 right)
    {
        uint carry;
        BigMul128 result = new BigMul128();

        ulong r0 = (ulong)left._0 + right._0;
        result._0 = (uint)r0;
        carry = (uint)(r0 >> 32);

        ulong r1 = (ulong)left._1 + right._1 + carry;
        result._1 = (uint)r1;
        carry = (uint)(r1 >> 32);

        ulong r2 = (ulong)left._2 + right._2 + carry;
        result._2 = (uint)r2;
        carry = (uint)(r2 >> 32);

        ulong r3 = (ulong)left._3 + right._3 + carry;
        result._3 = (uint)r3;

        return result;
    }

    private static BigMul128 Multiply(ulong left, uint right)
    {
        var result = new BigMul128();

        ulong prod1 = (uint)left * (ulong)right;
        uint lo1 = (uint)prod1, hi1 = (uint)(prod1 >> 32);
        result._0 = lo1;

        ulong prod2 = (uint)(left >> 32) * (ulong)right + hi1;
        result._1 = (uint)prod2;
        result._2 = (uint)(prod2 >> 32);

        return result;
    }

    public static BigMul128 Multiply(ulong left, ulong right)
    {
        var leftProd = Multiply(left, (uint)right);
        var rightProd = Multiply(left, (uint)(right >> 32));

        var rightShift = new BigMul128 // 32-bit left-shift
        {
            _1 = rightProd._0,
            _2 = rightProd._1,
            _3 = rightProd._2,
        };

        return Add(leftProd, rightShift);
    }
}