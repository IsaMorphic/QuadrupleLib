using QuadrupleLib.Accelerators;
using Xunit;

namespace QuadrupleLib.Tests.Utilities
{
    public abstract class AcceleratorTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(1UL, 0UL, 0UL, 0UL)]
        [InlineData(1UL, 1UL, 1UL, 0UL)]
        [InlineData(1UL, 2UL, 2UL, 0UL)]
        [InlineData(4UL, 1UL << 63, 0UL, 2UL)]
        public void IsBigMulCorrect(ulong a, ulong b, ulong expectedLo, ulong expectedHi) 
        {
            ulong actualHi = TAccelerator.BigMul(a, b, out ulong actualLo);
            Assert.Equal((expectedLo, expectedHi), (actualLo, actualHi));
        }

        [Theory]
        [InlineData(2UL, 0UL, 1UL, 0UL, 2UL, 0UL)]
        [InlineData(2UL, 0UL, 2UL, 0UL, 1UL, 0UL)]
        [InlineData(0UL, 4UL, 2UL, 0UL, 0UL, 2UL)]
        [InlineData(0UL, 4UL, 3UL, 0UL, 6148914691236517205UL, 1UL)]
        public void IsDivRemQuotientCorrect(ulong aLo, ulong aHi, ulong bLo, ulong bHi, ulong expectedLo, ulong expectedHi) 
        {
            UInt128 a = aLo | ((UInt128)aHi << 64);
            UInt128 b = bLo | ((UInt128)bHi << 64);
            UInt128 expected = expectedLo | ((UInt128)expectedHi << 64);

            (UInt128 actual, UInt128 _) = TAccelerator.DivRem(a, b);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(2UL, 0UL, 1UL, 0UL, 0UL, 0UL)]
        [InlineData(2UL, 0UL, 2UL, 0UL, 0UL, 0UL)]
        [InlineData(0UL, 4UL, 2UL, 0UL, 0UL, 0UL)]
        [InlineData(0UL, 4UL, 3UL, 0UL, 1UL, 0UL)]
        public void IsDivRemRemainderCorrect(ulong aLo, ulong aHi, ulong bLo, ulong bHi, ulong expectedLo, ulong expectedHi)
        {
            UInt128 a = aLo | ((UInt128)aHi << 64);
            UInt128 b = bLo | ((UInt128)bHi << 64);
            UInt128 expected = expectedLo | ((UInt128)expectedHi << 64);

            (UInt128 _, UInt128 actual) = TAccelerator.DivRem(a, b);
            Assert.Equal(expected, actual);
        }
    }

    public class AcceleratorTests_DefaultAccelerator : 
        AcceleratorTests<DefaultAccelerator> { }

    public class AcceleratorTests_SoftwareAccelerator :
        AcceleratorTests<SoftwareAccelerator>
    { }
}
