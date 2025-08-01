using Xunit.Abstractions;

namespace QuadrupleLib.Tests
{
    public class TrigTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public TrigTests(ITestOutputHelper outputHelper) 
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void ComputeSinCosTable() 
        {
            _outputHelper.WriteLine($"{new string(' ', 2)}x{new string(' ', 42)}sin(x){new string(' ', 42)}cos(x)");
            for (int i = 0; i <= 360; i += 5)
            {
                (Float128 sin, Float128 cos) = Float128.SinCos(i * Float128.Pi / 180);
                _outputHelper.WriteLine($"{i,3}{sin,48}{cos,48}");
            }
        }
    }
}
