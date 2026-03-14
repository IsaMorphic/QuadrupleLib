/*
 *  Copyright 2025-2026 Chosen Few Software
 *  This file is part of QuadrupleLib.
 *
 *  QuadrupleLib is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  QuadrupleLib is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with QuadrupleLib.  If not, see <https://www.gnu.org/licenses/>.
 */

using QuadrupleLib.Accelerators;
using QuadrupleLib.Tests.Assertions.Types;
using Xunit;

namespace QuadrupleLib.Tests.Math
{
    public abstract class TrigonometryTests<TAccelerator>
        where TAccelerator : IAccelerator
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
                (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = Float128<TAccelerator>.SinCos(i * Float128<TAccelerator>.Pi / 180);
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
            (Float128<TAccelerator> y, Float128<TAccelerator> x) = Float128<TAccelerator>.SinCos(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Assert.True(y > Float128<TAccelerator>.Zero && x > Float128<TAccelerator>.Zero);
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
            (Float128<TAccelerator> y, Float128<TAccelerator> x) = Float128<TAccelerator>.SinCos(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Assert.True(y > Float128<TAccelerator>.Zero && x < Float128<TAccelerator>.Zero);
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
            (Float128<TAccelerator> y, Float128<TAccelerator> x) = Float128<TAccelerator>.SinCos(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Assert.True(y < Float128<TAccelerator>.Zero && x < Float128<TAccelerator>.Zero);
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
            (Float128<TAccelerator> y, Float128<TAccelerator> x) = Float128<TAccelerator>.SinCos(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Assert.True(y < Float128<TAccelerator>.Zero && x > Float128<TAccelerator>.Zero);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(-285)]
        [InlineData(-300)]
        [InlineData(-315)]
        [InlineData(-330)]
        [InlineData(-345)]
        [InlineData(-360)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        [InlineData(180)]
        [InlineData(-195)]
        [InlineData(-210)]
        [InlineData(-225)]
        [InlineData(-240)]
        [InlineData(-255)]
        [InlineData(-270)]
        [InlineData(195)]
        [InlineData(210)]
        [InlineData(225)]
        [InlineData(240)]
        [InlineData(255)]
        [InlineData(-180)]
        [InlineData(-105)]
        [InlineData(-120)]
        [InlineData(-135)]
        [InlineData(-150)]
        [InlineData(-165)]
        [InlineData(270)]
        [InlineData(285)]
        [InlineData(300)]
        [InlineData(315)]
        [InlineData(330)]
        [InlineData(345)]
        [InlineData(360)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        [InlineData(-90)]
        public void IsSinEqualCoRDiC(double thetaDeg)
        {
            (Float128<TAccelerator> sinA, _) = Float128<TAccelerator>.SinCos(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Float128<TAccelerator> sinB = Float128<TAccelerator>.Sin(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Assert.Equal(sinA, sinB);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(-285)]
        [InlineData(-300)]
        [InlineData(-315)]
        [InlineData(-330)]
        [InlineData(-345)]
        [InlineData(-360)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        [InlineData(180)]
        [InlineData(-195)]
        [InlineData(-210)]
        [InlineData(-225)]
        [InlineData(-240)]
        [InlineData(-255)]
        [InlineData(-270)]
        [InlineData(195)]
        [InlineData(210)]
        [InlineData(225)]
        [InlineData(240)]
        [InlineData(255)]
        [InlineData(-180)]
        [InlineData(-105)]
        [InlineData(-120)]
        [InlineData(-135)]
        [InlineData(-150)]
        [InlineData(-165)]
        [InlineData(270)]
        [InlineData(285)]
        [InlineData(300)]
        [InlineData(315)]
        [InlineData(330)]
        [InlineData(345)]
        [InlineData(360)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        [InlineData(-90)]
        public void IsCosEqualCoRDiC(double thetaDeg)
        {
            (_, Float128<TAccelerator> cosA) = Float128<TAccelerator>.SinCos(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Float128<TAccelerator> cosB = Float128<TAccelerator>.Cos(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Assert.Equal(cosA, cosB);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(-285)]
        [InlineData(-300)]
        [InlineData(-315)]
        [InlineData(-330)]
        [InlineData(-345)]
        [InlineData(-360)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        [InlineData(180)]
        [InlineData(-195)]
        [InlineData(-210)]
        [InlineData(-225)]
        [InlineData(-240)]
        [InlineData(-255)]
        [InlineData(-270)]
        [InlineData(195)]
        [InlineData(210)]
        [InlineData(225)]
        [InlineData(240)]
        [InlineData(255)]
        [InlineData(-180)]
        [InlineData(-105)]
        [InlineData(-120)]
        [InlineData(-135)]
        [InlineData(-150)]
        [InlineData(-165)]
        [InlineData(270)]
        [InlineData(285)]
        [InlineData(300)]
        [InlineData(315)]
        [InlineData(330)]
        [InlineData(345)]
        [InlineData(360)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        [InlineData(-90)]
        public void IsTanEqualCoRDiC(double thetaDeg)
        {
            (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = Float128<TAccelerator>.SinCos(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Float128<TAccelerator> tan = Float128<TAccelerator>.Tan(thetaDeg * Float128<TAccelerator>.Pi / 180);
            Assert.Equal(sin / cos, tan);
        }

        [Theory]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        public void IsInverseCosEqualCoRDiC(double thetaDeg)
        {
            Float128<TAccelerator> thetaA = thetaDeg * Float128<TAccelerator>.Pi / 180;
            Float128<TAccelerator> cos = Float128<TAccelerator>.Cos(thetaA);
            Float128<TAccelerator> thetaB = Float128<TAccelerator>.Acos(cos);
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        public void IsInverseSinEqualCoRDiC(double thetaDeg)
        {
            Float128<TAccelerator> thetaA = thetaDeg * Float128<TAccelerator>.Pi / 180;
            Float128<TAccelerator> sin = Float128<TAccelerator>.Sin(thetaA);
            Float128<TAccelerator> thetaB = Float128<TAccelerator>.Asin(sin);
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        public void IsInverseTanEqualCoRDiC(double thetaDeg)
        {
            Float128<TAccelerator> thetaA = thetaDeg * Float128<TAccelerator>.Pi / 180;
            Float128<TAccelerator> tan = Float128<TAccelerator>.Tan(thetaA);
            Float128<TAccelerator> thetaB = Float128<TAccelerator>.Atan(tan);
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(-285)]
        [InlineData(-300)]
        [InlineData(-315)]
        [InlineData(-330)]
        [InlineData(-345)]
        [InlineData(-360)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        [InlineData(180)]
        [InlineData(-195)]
        [InlineData(-210)]
        [InlineData(-225)]
        [InlineData(-240)]
        [InlineData(-255)]
        [InlineData(-270)]
        [InlineData(195)]
        [InlineData(210)]
        [InlineData(225)]
        [InlineData(240)]
        [InlineData(255)]
        [InlineData(-180)]
        [InlineData(-105)]
        [InlineData(-120)]
        [InlineData(-135)]
        [InlineData(-150)]
        [InlineData(-165)]
        [InlineData(270)]
        [InlineData(285)]
        [InlineData(300)]
        [InlineData(315)]
        [InlineData(330)]
        [InlineData(345)]
        [InlineData(360)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        [InlineData(-90)]
        public void IsSinPiEqualCoRDiC(double thetaDeg)
        {
            (Float128<TAccelerator> sinPiA, _) = Float128<TAccelerator>.SinCosPi(thetaDeg / 180);
            Float128<TAccelerator> sinPiB = Float128<TAccelerator>.SinPi(thetaDeg / 180);
            Assert.Equal(sinPiA, sinPiB);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(-285)]
        [InlineData(-300)]
        [InlineData(-315)]
        [InlineData(-330)]
        [InlineData(-345)]
        [InlineData(-360)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        [InlineData(180)]
        [InlineData(-195)]
        [InlineData(-210)]
        [InlineData(-225)]
        [InlineData(-240)]
        [InlineData(-255)]
        [InlineData(-270)]
        [InlineData(195)]
        [InlineData(210)]
        [InlineData(225)]
        [InlineData(240)]
        [InlineData(255)]
        [InlineData(-180)]
        [InlineData(-105)]
        [InlineData(-120)]
        [InlineData(-135)]
        [InlineData(-150)]
        [InlineData(-165)]
        [InlineData(270)]
        [InlineData(285)]
        [InlineData(300)]
        [InlineData(315)]
        [InlineData(330)]
        [InlineData(345)]
        [InlineData(360)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        [InlineData(-90)]
        public void IsCosPiEqualCoRDiC(double thetaDeg)
        {
            (_, Float128<TAccelerator> cosPiA) = Float128<TAccelerator>.SinCosPi(thetaDeg / 180);
            Float128<TAccelerator> cosPiB = Float128<TAccelerator>.CosPi(thetaDeg / 180);
            Assert.Equal(cosPiA, cosPiB);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(-285)]
        [InlineData(-300)]
        [InlineData(-315)]
        [InlineData(-330)]
        [InlineData(-345)]
        [InlineData(-360)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        [InlineData(180)]
        [InlineData(-195)]
        [InlineData(-210)]
        [InlineData(-225)]
        [InlineData(-240)]
        [InlineData(-255)]
        [InlineData(-270)]
        [InlineData(195)]
        [InlineData(210)]
        [InlineData(225)]
        [InlineData(240)]
        [InlineData(255)]
        [InlineData(-180)]
        [InlineData(-105)]
        [InlineData(-120)]
        [InlineData(-135)]
        [InlineData(-150)]
        [InlineData(-165)]
        [InlineData(270)]
        [InlineData(285)]
        [InlineData(300)]
        [InlineData(315)]
        [InlineData(330)]
        [InlineData(345)]
        [InlineData(360)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        [InlineData(-90)]
        public void IsTanPiEqualCoRDiC(double thetaDeg)
        {
            (Float128<TAccelerator> sinPi, Float128<TAccelerator> cosPi) = Float128<TAccelerator>.SinCosPi(thetaDeg / 180);
            Float128<TAccelerator> tanPi = Float128<TAccelerator>.TanPi(thetaDeg / 180);
            Assert.Equal(sinPi / cosPi, tanPi);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(-285)]
        [InlineData(-300)]
        [InlineData(-315)]
        [InlineData(-330)]
        [InlineData(-345)]
        [InlineData(-360)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        [InlineData(180)]
        [InlineData(-195)]
        [InlineData(-210)]
        [InlineData(-225)]
        [InlineData(-240)]
        [InlineData(-255)]
        [InlineData(-270)]
        [InlineData(195)]
        [InlineData(210)]
        [InlineData(225)]
        [InlineData(240)]
        [InlineData(255)]
        [InlineData(-180)]
        [InlineData(-105)]
        [InlineData(-120)]
        [InlineData(-135)]
        [InlineData(-150)]
        [InlineData(-165)]
        [InlineData(270)]
        [InlineData(285)]
        [InlineData(300)]
        [InlineData(315)]
        [InlineData(330)]
        [InlineData(345)]
        [InlineData(360)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        [InlineData(-90)]
        public void IsSinCosPiEqualToSinCosCoRDiC(double thetaDeg)
        {
            var pairA = Float128<TAccelerator>.SinCos(thetaDeg / 180 * Float128<TAccelerator>.Pi);
            var pairB = Float128<TAccelerator>.SinCosPi(thetaDeg / 180);
            Assert.Equal(pairA, pairB);
        }

        [Theory]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        public void IsInverseCosPiEqualCoRDiC(double thetaDeg)
        {
            Float128<TAccelerator> thetaA = thetaDeg * Float128<TAccelerator>.Pi / 180;
            Float128<TAccelerator> cos = Float128<TAccelerator>.Cos(thetaA);
            Float128<TAccelerator> thetaB = Float128<TAccelerator>.AcosPi(cos);
            AssertX.NearlyEqual(thetaA, thetaB * Float128<TAccelerator>.Pi, Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        public void IsInverseSinPiEqualCoRDiC(double thetaDeg)
        {
            Float128<TAccelerator> thetaA = thetaDeg * Float128<TAccelerator>.Pi / 180;
            Float128<TAccelerator> sin = Float128<TAccelerator>.Sin(thetaA);
            Float128<TAccelerator> thetaB = Float128<TAccelerator>.AsinPi(sin);
            AssertX.NearlyEqual(thetaA, thetaB * Float128<TAccelerator>.Pi, Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        public void IsInverseTanPiEqualCoRDiC(double thetaDeg)
        {
            Float128<TAccelerator> thetaA = thetaDeg / 180;
            Float128<TAccelerator> tan = Float128<TAccelerator>.TanPi(thetaA);
            Float128<TAccelerator> thetaB = Float128<TAccelerator>.AtanPi(tan);
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void IsInverseSinNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Asin(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void IsInverseCosNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Acos(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void IsInverseSinPiNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.AsinPi(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void IsInverseCosPiNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.AcosPi(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        [InlineData(75)]
        [InlineData(90)]
        [InlineData(105)]
        [InlineData(120)]
        [InlineData(135)]
        [InlineData(150)]
        [InlineData(165)]
        [InlineData(180)]
        [InlineData(-180)]
        [InlineData(-105)]
        [InlineData(-120)]
        [InlineData(-135)]
        [InlineData(-150)]
        [InlineData(-165)]
        [InlineData(-15)]
        [InlineData(-30)]
        [InlineData(-45)]
        [InlineData(-60)]
        [InlineData(-75)]
        [InlineData(-90)]
        public void IsAtan2EqualCoRDiC(double thetaDeg)
        {
            Float128<TAccelerator> thetaA = thetaDeg * Float128<TAccelerator>.Pi / 180;
            (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = Float128<TAccelerator>.SinCos(thetaA);
            Float128<TAccelerator> thetaB = Float128<TAccelerator>.Atan2(sin, cos);
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
        }
    }

    public class TrigonometryTests_DefaultAccelerator :
        TrigonometryTests<DefaultAccelerator>
    {
        public TrigonometryTests_DefaultAccelerator(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }

    public class TrigonometryTests_SoftwareAccelerator :
        TrigonometryTests<SoftwareAccelerator>
    {
        public TrigonometryTests_SoftwareAccelerator(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
    }
}