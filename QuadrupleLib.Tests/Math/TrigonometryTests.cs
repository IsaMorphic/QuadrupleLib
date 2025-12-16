/*
 *  Copyright 2025 Chosen Few Software
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

using Xunit.Abstractions;

namespace QuadrupleLib.Tests.Math;

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
    public void IsSineEqualCoRDiC(double thetaDeg)
    {
        (Float128 sinA, _) = Float128.SinCos(thetaDeg * Float128.Pi / 180);
        Float128 sinB = Float128.Sin(thetaDeg * Float128.Pi / 180);
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
    public void IsCosineEqualCoRDiC(double thetaDeg)
    {
        (_, Float128 cosA) = Float128.SinCos(thetaDeg * Float128.Pi / 180);
        Float128 cosB = Float128.Cos(thetaDeg * Float128.Pi / 180);
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
    public void IsTangentEqualCoRDiC(double thetaDeg)
    {
        (Float128 sin, Float128 cos) = Float128.SinCos(thetaDeg * Float128.Pi / 180);
        Float128 tan = Float128.Tan(thetaDeg * Float128.Pi / 180);
        Assert.Equal(sin / cos, tan);
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
    public void IsInverseCosineEqualCoRDiC(double thetaDeg)
    {
        Float128 thetaA = thetaDeg * Float128.Pi / 180;
        Float128 cos = Float128.Cos(thetaA);

        Float128 thetaB = Float128.Acos(cos);
        Float128 diff = thetaA - thetaB;

        Assert.Equal(Float128.Zero, Float128.Round(diff, 6));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(60)]
    [InlineData(75)]
    [InlineData(90)]
    [InlineData(-15)]
    [InlineData(-30)]
    [InlineData(-45)]
    [InlineData(-60)]
    [InlineData(-75)]
    [InlineData(-90)]
    public void IsInverseSineEqualCoRDiC(double thetaDeg)
    {
        Float128 thetaA = thetaDeg * Float128.Pi / 180;
        Float128 sin = Float128.Sin(thetaA);

        Float128 thetaB = Float128.Asin(sin);
        Float128 diff = thetaA - thetaB;

        Assert.Equal(Float128.Zero, Float128.Round(diff, 6));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(-15)]
    [InlineData(-30)]
    [InlineData(-45)]
    public void IsInverseTangentEqualCoRDiC(double thetaDeg)
    {
        Float128 thetaA = thetaDeg * Float128.Pi / 180;
        Float128 tan = Float128.Tan(thetaA);

        Float128 thetaB = Float128.Atan(tan);
        Float128 diff = thetaA - thetaB;

        Assert.Equal(Float128.Zero, Float128.Round(diff, 6));
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
        (Float128 sinPiA, _) = Float128.SinCosPi(thetaDeg / 180);
        Float128 sinPiB = Float128.SinPi(thetaDeg / 180);
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
        (_, Float128 cosPiA) = Float128.SinCosPi(thetaDeg / 180);
        Float128 cosPiB = Float128.CosPi(thetaDeg / 180);
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
        (Float128 sinPi, Float128 cosPi) = Float128.SinCosPi(thetaDeg / 180);
        Float128 tanPi = Float128.TanPi(thetaDeg / 180);
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
        var pairA = Float128.SinCos(thetaDeg / 180 * Float128.Pi);
        var pairB = Float128.SinCosPi(thetaDeg / 180);
        Assert.Equal(pairA, pairB);
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
    public void IsInverseCosPiEqualCoRDiC(double thetaDeg)
    {
        Float128 thetaA = thetaDeg / 180;
        Float128 cos = Float128.CosPi(thetaA);

        Float128 thetaB = Float128.AcosPi(cos);
        Float128 diff = thetaA - thetaB;

        Assert.Equal(Float128.Zero, Float128.Round(diff, 6));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(60)]
    [InlineData(75)]
    [InlineData(90)]
    [InlineData(-15)]
    [InlineData(-30)]
    [InlineData(-45)]
    [InlineData(-60)]
    [InlineData(-75)]
    [InlineData(-90)]
    public void IsInverseSinPiEqualCoRDiC(double thetaDeg)
    {
        Float128 thetaA = thetaDeg / 180;
        Float128 sin = Float128.SinPi(thetaA);

        Float128 thetaB = Float128.AsinPi(sin);
        Float128 diff = thetaA - thetaB;

        Assert.Equal(Float128.Zero, Float128.Round(diff, 6));
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
        Float128 thetaA = thetaDeg / 180;
        Float128 tan = Float128.TanPi(thetaA);

        Float128 thetaB = Float128.AtanPi(tan);
        Float128 diff = thetaA - thetaB;

        Assert.Equal(Float128.Zero, Float128.Round(diff, 6));
    }
}