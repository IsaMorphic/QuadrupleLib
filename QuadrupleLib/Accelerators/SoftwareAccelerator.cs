using System.Runtime.InteropServices;

namespace QuadrupleLib.Accelerators;

public sealed class SoftwareAccelerator : IAccelerator
{
    [StructLayout(LayoutKind.Sequential)]
    private struct BigMul128
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

    private SoftwareAccelerator() { }

    static (ulong lo, ulong hi) IAccelerator.BigMul(ulong a, ulong b)
    {
        BigMul128 prod = BigMul128.Multiply(a, b);

        ulong lo = prod._0 | ((ulong)prod._1 << 32);
        ulong hi = prod._2 | ((ulong)prod._3 << 32);

        return (lo, hi);
    }
}
