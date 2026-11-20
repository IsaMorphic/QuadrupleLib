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

        [Fact]
        public void IsIntegerOfPositiveInfinityIsFalse()
        {
            // an infinity is not a finite integer
            Assert.False(Float128<TAccelerator>.IsInteger(Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void IsIntegerOfNegativeInfinityIsFalse()
        {
            Assert.False(Float128<TAccelerator>.IsInteger(Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void IsIntegerOfNaNIsFalse()
        {
            // NaN is not a number, so it is not an integer
            Assert.False(Float128<TAccelerator>.IsInteger(Float128<TAccelerator>.NaN));
        }

        [Fact]
        public void IsIntegerOfZeroIsTrue()
        {
            Assert.True(Float128<TAccelerator>.IsInteger(Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void IsIntegerOfNegativeZeroIsTrue()
        {
            Assert.True(Float128<TAccelerator>.IsInteger(-Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void IsEvenIntegerOfZeroIsTrue()
        {
            Assert.True(Float128<TAccelerator>.IsEvenInteger(Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void IsEvenIntegerOfNegativeZeroIsTrue()
        {
            Assert.True(Float128<TAccelerator>.IsEvenInteger(-Float128<TAccelerator>.Zero));
        }

        [Fact]
        public void IsOddIntegerOfZeroIsFalse()
        {
            // zero is an even integer, so neither signed zero is odd
            Assert.False(Float128<TAccelerator>.IsOddInteger(Float128<TAccelerator>.Zero));
            Assert.False(Float128<TAccelerator>.IsOddInteger(Float128<TAccelerator>.NegativeZero));
        }

        [Fact]
        public void IsEvenIntegerOfPositiveInfinityIsFalse()
        {
            // an infinity is not a finite integer, so it is not an even integer
            Assert.False(Float128<TAccelerator>.IsEvenInteger(Float128<TAccelerator>.PositiveInfinity));
        }

        [Fact]
        public void IsEvenIntegerOfNegativeInfinityIsFalse()
        {
            Assert.False(Float128<TAccelerator>.IsEvenInteger(Float128<TAccelerator>.NegativeInfinity));
        }

        [Fact]
        public void IsEvenIntegerOfLargePowerOfTwoIsTrue()
        {
            // reference: 2^113 is a power of two greater than one, so it is an even integer
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 113);

            Assert.True(Float128<TAccelerator>.IsEvenInteger(value));
        }

        [Fact]
        public void IsOddIntegerOfLargePowerOfTwoIsFalse()
        {
            // reference: 2^113 is a power of two greater than one, so it is not an odd integer
            Float128<TAccelerator> value = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 113);

            Assert.False(Float128<TAccelerator>.IsOddInteger(value));
        }
    }

    public class IntegerTests_DefaultAccelerator :
        IntegerTests<DefaultAccelerator>
    { }

    public class IntegerTests_SoftwareAccelerator :
        IntegerTests<SoftwareAccelerator>
    { }
}
