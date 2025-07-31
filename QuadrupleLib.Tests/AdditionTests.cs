namespace QuadrupleLib.Tests
{
    public class AdditionTests
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.33)]
        public void AddZeroIsIdentity(double x)
        {
            Assert.Equal(x, x + Float128.Zero);
        }

        [Theory]
        [InlineData([0.5, 1.5, 2.0])]
        [InlineData([1.0, 2.0, 3.0])]
        [InlineData([-1.0, 3.5, 2.5])]
        public void AddGeneralIsCorrect(double x, double y, double z) 
        {
            Assert.Equal(z, (Float128)x + y);
        }

        [Theory]
        [InlineData([0.5, 1.5])]
        [InlineData([1.0, 2.0])]
        [InlineData([-1.0, 0.0])]
        public void AddOneIsCorrect(double x, double y)
        {
            Assert.Equal(y, x + Float128.One);
        }

        [Theory]
        [InlineData([0.5, 1.5])]
        [InlineData([1.0, 2.0])]
        [InlineData([-1.0, 0.0])]
        public void IncrementIsCorrect(double x, double y) 
        {
            Float128 x_inc = (Float128)x;
            Assert.Equal(y, ++x_inc);
        }
    }
}