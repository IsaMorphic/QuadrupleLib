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
            Assert.Equal(x, (double)(Float128)x);
        }

        [Theory]
        [InlineData(0.5f)]
        [InlineData(1.300f)]
        [InlineData(-263.0f)]
        [InlineData(123.4567f)]
        public void ConvertToSingleIsEqual(float x)
        {
            Assert.Equal(x, (float)(Float128)x);
        }

        [Theory]
        [InlineData(0.5f)]
        [InlineData(1.300f)]
        [InlineData(-263.0f)]
        [InlineData(123.4567f)]
        public void ConvertToHalfIsEqual(float x)
        {
            Assert.Equal((Half)x, (Half)(Float128)x);
        }

        [Theory]
        [InlineData(5L)]
        [InlineData(-1L)]
        [InlineData(-263L)]
        [InlineData(12345L)]
        public void ConvertToInt128IsEqual(long x)
        {
            Assert.Equal(x, (Int128)(Float128)x);
        }

        [Theory]
        [InlineData(5L)]
        [InlineData(-1L)]
        [InlineData(-263L)]
        [InlineData(12345L)]
        public void ConvertToInt64IsEqual(long x)
        {
            Assert.Equal(x, (long)(Float128)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-263)]
        [InlineData(12345)]
        public void ConvertToInt32IsEqual(int x)
        {
            Assert.Equal(x, (int)(Float128)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-263)]
        [InlineData(12345)]
        public void ConvertToInt16IsEqual(short x)
        {
            Assert.Equal(x, (short)(Float128)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(-128)]
        [InlineData(127)]
        public void ConvertToSByteIsEqual(sbyte x)
        {
            Assert.Equal(x, (sbyte)(Float128)x);
        }

        [Theory]
        [InlineData(5UL)]
        [InlineData(1UL)]
        [InlineData(263UL)]
        [InlineData(12345UL)]
        public void ConvertToUInt128IsEqual(ulong x)
        {
            Assert.Equal(x, (UInt128)(Float128)x);
        }

        [Theory]
        [InlineData(5UL)]
        [InlineData(1UL)]
        [InlineData(263UL)]
        [InlineData(12345UL)]
        public void ConvertToUInt64IsEqual(ulong x)
        {
            Assert.Equal(x, (ulong)(Float128)x);
        }

        [Theory]
        [InlineData(5U)]
        [InlineData(1U)]
        [InlineData(263U)]
        [InlineData(12345U)]
        public void ConvertToUInt32IsEqual(uint x)
        {
            Assert.Equal(x, (uint)(Float128)x);
        }

        [Theory]
        [InlineData(5U)]
        [InlineData(1U)]
        [InlineData(263U)]
        [InlineData(12345U)]
        public void ConvertToUInt16IsEqual(ushort x)
        {
            Assert.Equal(x, (ushort)(Float128)x);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(3)]
        [InlineData(255)]
        [InlineData(0)]
        public void ConvertToByteIsEqual(byte x)
        {
            Assert.Equal(x, (byte)(Float128)x);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.300)]
        [InlineData(-263.0)]
        [InlineData(123.4567)]
        public void ConvertToStringParseRoundtripIsEqual(double x) 
        {
            string s_0 = $"{(Float128)x}";
            string s_1 = $"{Float128.Parse(s_0)}";
            Assert.Equal(s_0, s_1);
        }
    }
}
