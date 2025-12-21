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

using QuadrupleLib.Tests.Assertions.Types;
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
            (Quad sin, Quad cos) = Quad.SinCos(i * Quad.Pi / 180);
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
        (Quad y, Quad x) = Quad.SinCos(thetaDeg * Quad.Pi / 180);
        Assert.True(y > Quad.Zero && x > Quad.Zero);
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
        (Quad y, Quad x) = Quad.SinCos(thetaDeg * Quad.Pi / 180);
        Assert.True(y > Quad.Zero && x < Quad.Zero);
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
        (Quad y, Quad x) = Quad.SinCos(thetaDeg * Quad.Pi / 180);
        Assert.True(y < Quad.Zero && x < Quad.Zero);
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
        (Quad y, Quad x) = Quad.SinCos(thetaDeg * Quad.Pi / 180);
        Assert.True(y < Quad.Zero && x > Quad.Zero);
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
        (Quad sinA, _) = Quad.SinCos(thetaDeg * Quad.Pi / 180);
        Quad sinB = Quad.Sin(thetaDeg * Quad.Pi / 180);
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
        (_, Quad cosA) = Quad.SinCos(thetaDeg * Quad.Pi / 180);
        Quad cosB = Quad.Cos(thetaDeg * Quad.Pi / 180);
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
        (Quad sin, Quad cos) = Quad.SinCos(thetaDeg * Quad.Pi / 180);
        Quad tan = Quad.Tan(thetaDeg * Quad.Pi / 180);
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
    public void IsInverseCosEqualCoRDiC(double thetaDeg)
    {
        Quad thetaA = thetaDeg * Quad.Pi / 180;
        Quad cos = Quad.Cos(thetaA);
        Quad thetaB = Quad.Acos(cos);
        Assert.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
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
    public void IsInverseSinEqualCoRDiC(double thetaDeg)
    {
        Quad thetaA = thetaDeg * Quad.Pi / 180;
        Quad sin = Quad.Sin(thetaA);
        Quad thetaB = Quad.Asin(sin);
        Assert.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
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
        Quad thetaA = thetaDeg * Quad.Pi / 180;
        Quad tan = Quad.Tan(thetaA);
        Quad thetaB = Quad.Atan(tan);
        Assert.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
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
        (Quad sinPiA, _) = Quad.SinCosPi(thetaDeg / 180);
        Quad sinPiB = Quad.SinPi(thetaDeg / 180);
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
        (_, Quad cosPiA) = Quad.SinCosPi(thetaDeg / 180);
        Quad cosPiB = Quad.CosPi(thetaDeg / 180);
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
        (Quad sinPi, Quad cosPi) = Quad.SinCosPi(thetaDeg / 180);
        Quad tanPi = Quad.TanPi(thetaDeg / 180);
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
        var pairA = Quad.SinCos(thetaDeg / 180 * Quad.Pi);
        var pairB = Quad.SinCosPi(thetaDeg / 180);
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
        Quad thetaA = thetaDeg / 180;
        Quad cos = Quad.CosPi(thetaA);
        Quad thetaB = Quad.AcosPi(cos);
        Assert.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
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
        Quad thetaA = thetaDeg / 180;
        Quad sin = Quad.SinPi(thetaA);
        Quad thetaB = Quad.AsinPi(sin);
        Assert.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
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
        Quad thetaA = thetaDeg / 180;
        Quad tan = Quad.TanPi(thetaA);
        Quad thetaB = Quad.AtanPi(tan);
        Assert.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
    }

    [Theory]
    [InlineData(-1.3)]
    [InlineData(1.5)]
    [InlineData(123.456)]
    public void IsInverseSinNaN(double x)
    {
        Quad y = Quad.Asin(x);
        Assert.True(Quad.IsNaN(y));
    }

    [Theory]
    [InlineData(-1.3)]
    [InlineData(1.5)]
    [InlineData(123.456)]
    public void IsInverseCosNaN(double x)
    {
        Quad y = Quad.Acos(x);
        Assert.True(Quad.IsNaN(y));
    }

    [Theory]
    [InlineData(-1.3)]
    [InlineData(1.5)]
    [InlineData(123.456)]
    public void IsInverseSinPiNaN(double x)
    {
        Quad y = Quad.AsinPi(x);
        Assert.True(Quad.IsNaN(y));
    }

    [Theory]
    [InlineData(-1.3)]
    [InlineData(1.5)]
    [InlineData(123.456)]
    public void IsInverseCosPiNaN(double x)
    {
        Quad y = Quad.AcosPi(x);
        Assert.True(Quad.IsNaN(y));
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
    public void IsAtan2EqualCoRDiC(double thetaDeg)
    {
        Quad thetaA = thetaDeg * Quad.Pi / 180;
        (Quad sin, Quad cos) = Quad.SinCos(thetaA);
        Quad thetaB = Quad.Atan2(sin, cos);
        Assert.NearlyEqual(thetaA, thetaB, Precision.NearestThousandth);
    }
}