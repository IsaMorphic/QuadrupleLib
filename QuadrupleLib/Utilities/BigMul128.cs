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

        result._0 = left._0 + right._0;

        carry = (uint)Math.Max(0, left._0.CompareTo(result._0));
        result._1 = left._1 + right._1 + carry;

        carry = (uint)Math.Max(0, left._1.CompareTo(result._1));
        result._2 = left._2 + right._2 + carry;

        carry = (uint)Math.Max(0, left._2.CompareTo(result._2));
        result._3 = left._3 + right._3 + carry;

        return result;
    }

    private static BigMul128 Multiply(ulong left, uint right)
    {
        var result = new BigMul128();

        ulong prod1 = (uint)left * (ulong)right;
        result._0 = (uint)prod1;

        ulong prod2 = (left >> 32) * right;
        result._1 = (uint)prod2 + (uint)(prod1 >> 32);
        result._2 = (uint)(prod2 >> 32) + (uint)Math.Max(0, ((uint)prod2).CompareTo((uint)prod2 + (uint)(prod1 >> 32)));

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