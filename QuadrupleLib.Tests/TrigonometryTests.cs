using Xunit.Abstractions;

namespace QuadrupleLib.Tests
{
    public class TrigonometryTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public TrigonometryTests(ITestOutputHelper outputHelper) 
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void ComputeTableCoRDiC() 
        {
            _outputHelper.WriteLine($"{new string(' ', 2)}x{new string(' ', 36)}sin(x){new string(' ', 36)}cos(x)");
            for (int i = 0; i <= 360; i += 15)
            {
                (Float128 sin, Float128 cos) = Float128.SinCos(i * Float128.Pi / 180);
                _outputHelper.WriteLine($"{i,3}{sin,42}{cos,42}");
            }
        }

        [Theory]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(-285)]
        [InlineData(-300)]
        [InlineData(-315)]
        [InlineData(-330)]
        [InlineData(-345)]
        public void IsFirstQuadrantCoRDiC(double thetaDeg) 
        {
            (Float128 y, Float128 x) = Float128.SinCos(thetaDeg * Float128.Pi / 180);
            Assert.True(y > Float128.Zero && x > Float128.Zero);
        }

        [Theory]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        [InlineData(-195)]
        [InlineData(-210)]
        [InlineData(-225)]
        [InlineData(-240)]
        [InlineData(-255)]
        public void IsSecondQuadrantCoRDiC(double thetaDeg)
        {
            (Float128 y, Float128 x) = Float128.SinCos(thetaDeg * Float128.Pi / 180);
            Assert.True(y > Float128.Zero && x < Float128.Zero);
        }

        [Theory]
        [InlineData(195)]
        [InlineData(210)]
        [InlineData(225)]
        [InlineData(240)]
        [InlineData(255)]
        [InlineData(-105)]
        [InlineData(-120)]
        [InlineData(-135)]
        [InlineData(-150)]
        [InlineData(-165)]
        public void IsThirdQuadrantCoRDiC(double thetaDeg)
        {
            (Float128 y, Float128 x) = Float128.SinCos(thetaDeg * Float128.Pi / 180);
            Assert.True(y < Float128.Zero && x < Float128.Zero);
        }

        [Theory]
        [InlineData(285)]
        [InlineData(300)]
        [InlineData(315)]
        [InlineData(330)]
        [InlineData(345)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        public void IsFourthQuadrantCoRDiC(double thetaDeg)
        {
            (Float128 y, Float128 x) = Float128.SinCos(thetaDeg * Float128.Pi / 180);
            Assert.True(y < Float128.Zero && x > Float128.Zero);
        }
    }
}
