namespace QuadrupleLib.Tests
{
    public class ConversionTests
    {
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.300)]
        [InlineData(-263.0)]
        [InlineData(123.4567)]
        public void ConvertToDoubleIsEqual(double x) 
        {
            Assert.Equal((double)(Float128)x, x);
        }

        [Theory]
        [InlineData(0.5f)]
        [InlineData(1.300f)]
        [InlineData(-263.0f)]
        [InlineData(123.4567f)]
        public void ConvertToSingleIsEqual(float x)
        {
            Assert.Equal((float)(Float128)x, x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-263)]
        [InlineData(12345)]
        public void ConvertToInt32IsEqual(int x)
        {
            Assert.Equal((int)(Float128)x, x);
        }

        [Theory]
        [InlineData(5L)]
        [InlineData(-1L)]
        [InlineData(-263L)]
        [InlineData(12345L)]
        public void ConvertToInt64IsEqual(long x)
        {
            Assert.Equal((long)(Float128)x, x);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.300)]
        [InlineData(-263.0)]
        [InlineData(123.4567)]
        public void ConvertToStringIsEqual(double x) 
        {
            string s_0 = $"{(Float128)x}";
            string s_1 = $"{Float128.Parse(s_0)}";
            Assert.Equal(s_0, s_1);
        }
    }
}
