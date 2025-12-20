namespace QuadrupleLib.Accelerators;

public sealed class DefaultAccelerator : IAccelerator
{
    private DefaultAccelerator() { }

    static ulong IAccelerator.BigMul(ulong a, ulong b, out ulong low)
    {
        return Math.BigMul(a, b, out low);
    }

    static (UInt128 Quotient, UInt128 Remainder) IAccelerator.DivRem(UInt128 a, UInt128 b)
    {
        return UInt128.DivRem(a, b);
    }
}
