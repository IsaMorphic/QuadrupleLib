namespace QuadrupleLib.Accelerators;

public sealed class DefaultAccelerator : IAccelerator
{
    private DefaultAccelerator() { }

    static (ulong lo, ulong hi) IAccelerator.BigMul(ulong a, ulong b)
    {
        ulong hi = Math.BigMul(a, b, out ulong lo);
        return (lo, hi);
    }
}
