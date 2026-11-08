using QuadrupleLib.Accelerators;
using Xunit;

namespace QuadrupleLib.Tests.Integers
{
    public abstract class IntegerTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        [Theory]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(3.0)]
        [InlineData(-3.0)]
        [InlineData(0.125)]
        [InlineData(-0.125)]
        [InlineData(0.00001)]
        [InlineData(-0.00001)]
        [InlineData(3.5)]
        [InlineData(-3.5)]
        [InlineData(6.0)]
        [InlineData(-6.0)]
        public void IsIntegerCorrect(double x)
        {
            bool expected = double.IsInteger(x);
            bool actual = Float128<TAccelerator>.IsInteger(x);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(3.0)]
        [InlineData(-3.0)]
        [InlineData(0.125)]
        [InlineData(-0.125)]
        [InlineData(0.00001)]
        [InlineData(-0.00001)]
        [InlineData(3.5)]
        [InlineData(-3.5)]
        [InlineData(6.0)]
        [InlineData(-6.0)]
        public void IsEvenIntegerCorrect(double x)
        {
            bool expected = double.IsEvenInteger(x);
            bool actual = Float128<TAccelerator>.IsEvenInteger(x);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(2.0)]
        [InlineData(-2.0)]
        [InlineData(3.0)]
        [InlineData(-3.0)]
        [InlineData(0.125)]
        [InlineData(-0.125)]
        [InlineData(0.00001)]
        [InlineData(-0.00001)]
        [InlineData(3.5)]
        [InlineData(-3.5)]
        [InlineData(6.0)]
        [InlineData(-6.0)]
        public void IsOddIntegerCorrect(double x)
        {
            bool expected = double.IsOddInteger(x);
            bool actual = Float128<TAccelerator>.IsOddInteger(x);
            Assert.Equal(expected, actual);
        }
    }

    public class IntegerTests_DefaultAccelerator :
        IntegerTests<DefaultAccelerator>
    { }

    public class IntegerTests_SoftwareAccelerator :
        IntegerTests<SoftwareAccelerator>
    { }
}
