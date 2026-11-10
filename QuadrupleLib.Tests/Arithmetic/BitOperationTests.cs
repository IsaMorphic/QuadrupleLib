using QuadrupleLib.Accelerators;
using Xunit;

namespace QuadrupleLib.Tests.Arithmetic;

public abstract class BitOperationTests<TAccelerator>
        where TAccelerator : IAccelerator
{
    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(0.33)]
    public void BitIncrementRoundtripIsExactlyEqual(double x)
    {
        Assert.Equal(x, Float128<TAccelerator>.BitIncrement(Float128<TAccelerator>.BitDecrement(x)));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(0.33)]
    public void BitDecrementRoundtripIsExactlyEqual(double x)
    {
        Assert.Equal(x, Float128<TAccelerator>.BitDecrement(Float128<TAccelerator>.BitIncrement(x)));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(0.33)]
    public void BitIncrementIsGreaterThanOriginal(double x)
    {
        Assert.True(Float128<TAccelerator>.BitIncrement(x) > x);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(0.33)]
    public void BitDecrementIsLessThanOriginal(double x)
    {
        Assert.True(Float128<TAccelerator>.BitDecrement(x) < x);
    }

    [Theory]
    [InlineData(0.5, 0)]
    [InlineData(1.0, 0)]
    [InlineData(0.33, 0)]
    [InlineData(-0.288, 0)]
    [InlineData(2.0, 0)]
    [InlineData(-2.0, 0)]
    [InlineData(-4.65, 0)]
    [InlineData(0.5, 1)]
    [InlineData(1.0, 1)]
    [InlineData(0.33, 1)]
    [InlineData(-0.288, 1)]
    [InlineData(2.0, 1)]
    [InlineData(-2.0, 1)]
    [InlineData(-4.65, 1)]
    [InlineData(0.5, 2)]
    [InlineData(1.0, 2)]
    [InlineData(0.33, 2)]
    [InlineData(-0.288, 2)]
    [InlineData(2.0, 2)]
    [InlineData(-2.0, 2)]
    [InlineData(-4.65, 2)]
    [InlineData(0.5, -1)]
    [InlineData(1.0, -1)]
    [InlineData(0.33, -1)]
    [InlineData(-0.288, -1)]
    [InlineData(2.0, -1)]
    [InlineData(-2.0, -1)]
    [InlineData(-4.65, -1)]
    [InlineData(0.5, -2)]
    [InlineData(1.0, -2)]
    [InlineData(0.33, -2)]
    [InlineData(-0.288, -2)]
    [InlineData(2.0, -2)]
    [InlineData(-2.0, -2)]
    [InlineData(-4.65, -2)]
    public void ScaleBIsCorrect(double x, int n)
    {
        double y = double.ScaleB(x, n);
        Assert.Equal(y, Float128<TAccelerator>.ScaleB(x, n));
    }
}

public class BitOperationTests_SoftwareAccelerator :
    BitOperationTests<SoftwareAccelerator>
{ }

public class BitOperationTests_DefaultAccelerator :
    BitOperationTests<DefaultAccelerator>
{ }