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
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestTenThousandth);
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
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestTenThousandth);
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
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestTenThousandth);
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
            AssertX.NearlyEqual(thetaA, thetaB * Float128<TAccelerator>.Pi, Precision.NearestTenThousandth);
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
            AssertX.NearlyEqual(thetaA, thetaB * Float128<TAccelerator>.Pi, Precision.NearestTenThousandth);
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
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void IsInverseSinNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Asin(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void IsInverseCosNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Acos(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
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
            AssertX.NearlyEqual(thetaA, thetaB, Precision.NearestTenThousandth);
        }

        [Theory]
        [InlineData(3.0, 4.0)]
        [InlineData(1.5, 2.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(5.6, 7.125)]
        public void IsHypotCorrect(double a, double b)
        {
            double c = double.Hypot(a, b);
            AssertX.NearlyEqual(c, Float128<TAccelerator>.Hypot(a, b), Precision.NearestTenThousandth);
        }

        [Fact]
        public void HypotWithInfiniteLegIsPositiveInfinity()
        {
            // an infinite leg makes the hypotenuse +Inf, whatever the other leg is
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Hypot(Float128<TAccelerator>.PositiveInfinity, (Float128<TAccelerator>)3.0)));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Hypot((Float128<TAccelerator>)3.0, Float128<TAccelerator>.NegativeInfinity)));
        }

        [Fact]
        public void HypotWithInfiniteLegAndNaNLegIsPositiveInfinity()
        {
            // IEEE 754-2019: hypot(+Inf, NaN) is +Inf, because an infinite operand takes precedence over NaN
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Hypot(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.NaN)));
        }

        [Fact]
        public void HypotOfZeroLegsIsPositiveZero()
        {
            // IEEE 754: hypot(±0, ±0) is +0 for every sign combination
            Float128<TAccelerator> result = Float128<TAccelerator>.Hypot(Float128<TAccelerator>.Zero, Float128<TAccelerator>.NegativeZero);
            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void HypotWithPositiveInfinityAndZeroIsPositiveInfinity()
        {
            // IEEE 754: hypot(+Inf, 0) is +Inf
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Hypot(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.Zero)));
        }

        [Fact]
        public void SinCosOfNegativeZeroIsNegativeZeroAndOne()
        {
            // IEEE 754: sin(-0) is -0 and cos(-0) is 1
            (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = Float128<TAccelerator>.SinCos(Float128<TAccelerator>.NegativeZero);
            Assert.True(Float128<TAccelerator>.IsZero(sin));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(sin));
            Assert.True(cos == Float128<TAccelerator>.One);
        }

        [Fact]
        public void SinOfNaNIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Sin(Float128<TAccelerator>.NaN)));
        }

        [Fact]
        public void SinOfPositiveInfinityIsNaN()
        {
            // IEEE 754: sin of an infinity is an invalid operation; this library then returns Float128.NaN, whose sign bit is clear
            Float128<TAccelerator> result = Float128<TAccelerator>.Sin(Float128<TAccelerator>.PositiveInfinity);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void SinOfNegativeInfinityIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Sin(Float128<TAccelerator>.NegativeInfinity);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void CosOfPositiveInfinityIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Cos(Float128<TAccelerator>.PositiveInfinity);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void TanOfPositiveInfinityIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Tan(Float128<TAccelerator>.PositiveInfinity);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void SinCosOfPositiveInfinityIsNaN()
        {
            (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = Float128<TAccelerator>.SinCos(Float128<TAccelerator>.PositiveInfinity);

            Assert.True(Float128<TAccelerator>.IsNaN(sin));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(sin));
            Assert.True(Float128<TAccelerator>.IsNaN(cos));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(cos));
        }

        [Fact]
        public void CosOfNegativeInfinityIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Cos(Float128<TAccelerator>.NegativeInfinity);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void TanOfNegativeInfinityIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Tan(Float128<TAccelerator>.NegativeInfinity);
            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void SinCosOfNegativeInfinityIsNaN()
        {
            (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = Float128<TAccelerator>.SinCos(Float128<TAccelerator>.NegativeInfinity);

            Assert.True(Float128<TAccelerator>.IsNaN(sin));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(sin));
            Assert.True(Float128<TAccelerator>.IsNaN(cos));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(cos));
        }

        [Fact]
        public void SinOfNegativeZeroIsNegativeZero()
        {
            // IEEE 754: sin preserves the sign of a zero argument
            Float128<TAccelerator> result = Float128<TAccelerator>.Sin(-Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void AtanOfNaNIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Atan(Float128<TAccelerator>.NaN)));
        }

        [Fact]
        public void AtanOfPositiveInfinityIsHalfPi()
        {
            // tolerance is the one the library's own transcendental tests use
            AssertX.NearlyEqual(Float128<TAccelerator>.HalfPi, Float128<TAccelerator>.Atan(Float128<TAccelerator>.PositiveInfinity), Precision.NearestTenThousandth);
        }

        [Fact]
        public void AtanOfNegativeInfinityIsNegativeHalfPi()
        {
            AssertX.NearlyEqual(-Float128<TAccelerator>.HalfPi, Float128<TAccelerator>.Atan(Float128<TAccelerator>.NegativeInfinity), Precision.NearestTenThousandth);
        }

        [Fact]
        public void Atan2OfZeroAndZeroIsPositiveZero()
        {
            // IEEE 754-2008: atan2(+0, +0) is +0
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(Float128<TAccelerator>.Zero, Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void Atan2OfPositiveZeroAndNegativeZeroIsPi()
        {
            // IEEE 754-2008: atan2(+0, -0) is +pi; tolerance is the one the library's own transcendental tests use
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(Float128<TAccelerator>.Zero, -Float128<TAccelerator>.Zero);

            AssertX.NearlyEqual(Float128<TAccelerator>.Pi, result, Precision.NearestTenThousandth);
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void Atan2OfNegativeZeroAndNegativeZeroIsNegativePi()
        {
            // IEEE 754-2008: atan2(-0, -0) is -pi
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(-Float128<TAccelerator>.Zero, -Float128<TAccelerator>.Zero);

            AssertX.NearlyEqual(-Float128<TAccelerator>.Pi, result, Precision.NearestTenThousandth);
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void Atan2WithNaNOperandIsPositiveNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(Float128<TAccelerator>.NaN, Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void HypotOfLargeEqualLegsIsScaledSquareRootOfTwo()
        {
            // hypot(a, a) = a * sqrt(2), and a = 2^16000 squares far beyond the binary128 range
            Float128<TAccelerator> leg = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 16000);
            Float128<TAccelerator> ratio = Float128<TAccelerator>.Hypot(leg, leg) / leg;
            Assert.Equal(double.Sqrt(2.0), (double)ratio, 15);
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