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

        private static readonly Float128<TAccelerator> _relativeTolerance = Float128<TAccelerator>.Parse("1E-32");

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

        [Fact]
        public void AsinOfNaNIsNaN()
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Asin(Float128<TAccelerator>.NaN);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
        }

        [Fact]
        public void AcosOfNaNIsNaN()
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Acos(Float128<TAccelerator>.NaN);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void AsinOfOutOfDomainIsNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Asin(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void AcosOfOutOfDomainIsNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.Acos(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(y));
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void AsinPiOfOutOfDomainIsNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.AsinPi(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(-1.3)]
        [InlineData(1.5)]
        [InlineData(123.456)]
        public void AcosPiOfOutOfDomainIsNaN(double x)
        {
            Float128<TAccelerator> y = Float128<TAccelerator>.AcosPi(x);
            Assert.True(Float128<TAccelerator>.IsNaN(y));
        }

        [Theory]
        [InlineData(3.0, 4.0)]
        [InlineData(1.5, 2.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(5.6, 7.125)]
        public void HypotOfFiniteLegsIsDoubleHypot(double a, double b)
        {
            double c = double.Hypot(a, b);
            AssertX.RelativelyEqual(c, Float128<TAccelerator>.Hypot(a, b), Float128<TAccelerator>.Parse("1E-15"));
        }

        [Fact]
        public void HypotWithInfiniteLegIsPositiveInfinity()
        {
            // infinite leg gives +Inf
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Hypot(Float128<TAccelerator>.PositiveInfinity, (Float128<TAccelerator>)3.0)));
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Hypot((Float128<TAccelerator>)3.0, Float128<TAccelerator>.NegativeInfinity)));
        }

        [Fact]
        public void HypotWithInfiniteLegAndNaNLegIsPositiveInfinity()
        {
            // IEEE 754-2019: hypot(+Inf, NaN) is +Inf
            Assert.True(Float128<TAccelerator>.IsPositiveInfinity(Float128<TAccelerator>.Hypot(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.NaN)));
        }

        [Fact]
        public void HypotOfZeroLegsIsPositiveZero()
        {
            // IEEE 754: hypot(±0, ±0) is +0
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

        [Theory]
        [InlineData(3.0, 4.0, 5.0)]
        [InlineData(-5.0, 12.0, 13.0)]
        [InlineData(0.0, -7.0, 7.0)]
        [InlineData(20.0, 21.0, 29.0)]
        public void HypotOfPythagoreanLegsIsExact(double a, double b, double expected)
        {
            // exact integer hypotenuse
            Assert.Equal((Float128<TAccelerator>)expected, Float128<TAccelerator>.Hypot(a, b));
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
        public void SinOfArgumentAtLimitIsNaN()
        {
            // BREAKING_CHANGES.md §2: finite |x| >= 2^112 returns Float128.NaN, with the sign bit clear
            Float128<TAccelerator> result = Float128<TAccelerator>.Sin(Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 112));

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void CosOfArgumentAtLimitIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Cos(Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 112));

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void TanOfArgumentAtLimitIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.Tan(Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 112));

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void SinCosOfArgumentAtLimitIsNaN()
        {
            Float128<TAccelerator> limit = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 112);
            (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = Float128<TAccelerator>.SinCos(limit);

            Assert.True(Float128<TAccelerator>.IsNaN(sin));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(sin));
            Assert.True(Float128<TAccelerator>.IsNaN(cos));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(cos));

            (sin, cos) = Float128<TAccelerator>.SinCos(-limit);

            Assert.True(Float128<TAccelerator>.IsNaN(sin));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(sin));
            Assert.True(Float128<TAccelerator>.IsNaN(cos));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(cos));
        }

        [Fact]
        public void SinPiOfArgumentAtPiLimitIsNaN()
        {
            // x * Pi reaches 2^112 near 1.65E+33; 2^111 is above it
            Float128<TAccelerator> result = Float128<TAccelerator>.SinPi(Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 111));

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void CosPiOfArgumentAtPiLimitIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.CosPi(Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 111));

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void TanPiOfArgumentAtPiLimitIsNaN()
        {
            Float128<TAccelerator> result = Float128<TAccelerator>.TanPi(Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 111));

            Assert.True(Float128<TAccelerator>.IsNaN(result));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
        }

        [Fact]
        public void SinCosPiOfArgumentAtPiLimitIsNaN()
        {
            (Float128<TAccelerator> sinPi, Float128<TAccelerator> cosPi) = Float128<TAccelerator>.SinCosPi(Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 111));

            Assert.True(Float128<TAccelerator>.IsNaN(sinPi));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(sinPi));
            Assert.True(Float128<TAccelerator>.IsNaN(cosPi));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(cosPi));
        }

        [Fact]
        public void SinAndCosOfArgumentJustBelowLimitIsFinite()
        {
            // the largest finite value below 2^112
            Float128<TAccelerator> below = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 112) - Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -1);
            (Float128<TAccelerator> sin, Float128<TAccelerator> cos) = Float128<TAccelerator>.SinCos(below);

            Assert.True(Float128<TAccelerator>.IsFinite(sin));
            Assert.True(Float128<TAccelerator>.IsFinite(cos));
            Assert.True(Float128<TAccelerator>.IsFinite(Float128<TAccelerator>.Tan(below)));
        }

        [Fact]
        public void PiVariantsOfArgumentBelowLimitIsFinite()
        {
            // x * Pi is below 2^112 at x = 2^110, about 20% below the limit of about 1.65E+33
            Float128<TAccelerator> x = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 110);
            (Float128<TAccelerator> sinPi, Float128<TAccelerator> cosPi) = Float128<TAccelerator>.SinCosPi(x);

            Assert.True(Float128<TAccelerator>.IsFinite(sinPi));
            Assert.True(Float128<TAccelerator>.IsFinite(cosPi));
            Assert.True(Float128<TAccelerator>.IsFinite(Float128<TAccelerator>.TanPi(x)));
        }

        [Theory]
        [InlineData("1.6E+33")]
        [InlineData("-1.6E+33")]
        public void PiVariantsJustBelowLimitAreFinite(string text)
        {
            // 2^112 / Pi is about 1.6528E+33, so x * Pi is below 2^112 at 1.6E+33
            Float128<TAccelerator> x = Float128<TAccelerator>.Parse(text);
            (Float128<TAccelerator> sinPi, Float128<TAccelerator> cosPi) = Float128<TAccelerator>.SinCosPi(x);

            Assert.True(Float128<TAccelerator>.IsFinite(sinPi));
            Assert.True(Float128<TAccelerator>.IsFinite(cosPi));
            Assert.True(Float128<TAccelerator>.IsFinite(Float128<TAccelerator>.TanPi(x)));
        }

        [Theory]
        [InlineData("1.7E+33")]
        [InlineData("-1.7E+33")]
        public void PiVariantsJustAboveLimitAreNaNWithSignClear(string text)
        {
            // 2^112 / Pi is about 1.6528E+33, so x * Pi reaches 2^112 at 1.7E+33 and SinCos returns the library NaN
            Float128<TAccelerator> x = Float128<TAccelerator>.Parse(text);
            (Float128<TAccelerator> sinPi, Float128<TAccelerator> cosPi) = Float128<TAccelerator>.SinCosPi(x);
            Float128<TAccelerator> tanPi = Float128<TAccelerator>.TanPi(x);

            Assert.True(Float128<TAccelerator>.IsNaN(sinPi));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(sinPi));
            Assert.True(Float128<TAccelerator>.IsNaN(cosPi));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(cosPi));
            Assert.True(Float128<TAccelerator>.IsNaN(tanPi));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(tanPi));
        }

        [Theory]
        [InlineData("1E+34")]
        [InlineData("-1E+34")]
        public void PiVariantsAtOneE34AreNaNWithSignClear(string text)
        {
            // 0.5.2 returned exact values here (+0 for the sines and tangent, 1 for the cosines), by an inexact reduction; see BREAKING_CHANGES §6
            Float128<TAccelerator> x = Float128<TAccelerator>.Parse(text);
            Float128<TAccelerator>[] results =
            [
                Float128<TAccelerator>.SinPi(x),
                Float128<TAccelerator>.CosPi(x),
                Float128<TAccelerator>.TanPi(x),
                Float128<TAccelerator>.SinCosPi(x).SinPi,
                Float128<TAccelerator>.SinCosPi(x).CosPi,
            ];

            foreach (Float128<TAccelerator> result in results)
            {
                Assert.True(Float128<TAccelerator>.IsNaN(result));
                Assert.False(RawStorage<TAccelerator>.IsSignSet(result));
            }
        }

        [Fact]
        public void PiVariantsOfThreeQuartersAreWithinKernelTolerance()
        {
            // sqrt(2)/2 to 55 digits. The sine and cosine kernel's relative error at 0.75 is about 5E-11, from a probe run
            // (BREAKING_CHANGES §6 records no bound). The 1E-10 here is a test tolerance only.
            Float128<TAccelerator> root = Float128<TAccelerator>.Parse("0.70710678118654752440084436210484903928483593768847403658834");
            Float128<TAccelerator> tolerance = Float128<TAccelerator>.Parse("1E-10");
            Float128<TAccelerator> threeQuarters = Float128<TAccelerator>.Parse("0.75");

            // sin(3Pi/4) = +sqrt(2)/2, cos(3Pi/4) = -sqrt(2)/2
            AssertX.RelativelyEqual(root, Float128<TAccelerator>.SinPi(threeQuarters), tolerance);
            AssertX.RelativelyEqual(root, Float128<TAccelerator>.SinCosPi(threeQuarters).SinPi, tolerance);
            AssertX.RelativelyEqual(-root, Float128<TAccelerator>.CosPi(threeQuarters), tolerance);

            // sin is odd and cos is even: sin(-3Pi/4) = -sqrt(2)/2, cos(-3Pi/4) = -sqrt(2)/2
            AssertX.RelativelyEqual(-root, Float128<TAccelerator>.SinPi(-threeQuarters), tolerance);
            AssertX.RelativelyEqual(-root, Float128<TAccelerator>.CosPi(-threeQuarters), tolerance);
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
            // reference: pi/2 to 100 digits
            Float128<TAccelerator> halfPi = Float128<TAccelerator>.Parse("1.5707963267948966192313216916397514420985846996875529104874722961539082031431044993140174126710585339");
            AssertX.RelativelyEqual(halfPi, Float128<TAccelerator>.Atan(Float128<TAccelerator>.PositiveInfinity), _relativeTolerance);
        }

        [Fact]
        public void AtanOfNegativeInfinityIsNegativeHalfPi()
        {
            // reference: pi/2 to 100 digits
            Float128<TAccelerator> halfPi = Float128<TAccelerator>.Parse("1.5707963267948966192313216916397514420985846996875529104874722961539082031431044993140174126710585339");
            AssertX.RelativelyEqual(-halfPi, Float128<TAccelerator>.Atan(Float128<TAccelerator>.NegativeInfinity), _relativeTolerance);
        }

        [Fact]
        public void AtanOfSmallArgumentIsArgument()
        {
            // atan(x) rounds to x
            Float128<TAccelerator> tiny = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -57);
            Float128<TAccelerator> subnormal = RawStorage<TAccelerator>.FromBits((UInt128)1);
            Assert.Equal(RawStorage<TAccelerator>.Bits(tiny), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan(tiny)));
            Assert.Equal(RawStorage<TAccelerator>.Bits(subnormal), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan(subnormal)));
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
            // reference: pi
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(Float128<TAccelerator>.Zero, -Float128<TAccelerator>.Zero);

            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("3.14159265358979323846264338327950288"), result, _relativeTolerance);
            Assert.False(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void Atan2OfNegativeZeroAndNegativeZeroIsNegativePi()
        {
            // reference: -pi (atan2(-0, -0) is negative pi)
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(-Float128<TAccelerator>.Zero, -Float128<TAccelerator>.Zero);

            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("-3.14159265358979323846264338327950288"), result, _relativeTolerance);
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
        public void Atan2OfPositiveOneAndZeroIsHalfPi()
        {
            // reference: pi / 2
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(1.0, 0.0);

            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("1.57079632679489661923132169163975144"), result, _relativeTolerance);
        }

        [Fact]
        public void Atan2OfNegativeOneAndZeroIsNegativeHalfPi()
        {
            // reference: pi / 2
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(-1.0, 0.0);

            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("-1.57079632679489661923132169163975144"), result, _relativeTolerance);
        }

        [Fact]
        public void Atan2OfPositiveInfinityAndNegativeInfinityIsThreePiOverFour()
        {
            // reference: 3 pi / 4
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.NegativeInfinity);

            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("2.35619449019234492884698253745962716"), result, _relativeTolerance);
        }

        [Fact]
        public void Atan2OfNegativeInfinityAndNegativeInfinityIsNegativeThreePiOverFour()
        {
            // reference: 3 pi / 4
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.NegativeInfinity);

            AssertX.RelativelyEqual(-Float128<TAccelerator>.Parse("2.35619449019234492884698253745962716"), result, _relativeTolerance);
        }

        [Fact]
        public void Atan2OfPositiveInfinityAndPositiveInfinityIsPiOverFour()
        {
            // reference: pi / 4
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(Float128<TAccelerator>.PositiveInfinity, Float128<TAccelerator>.PositiveInfinity);

            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("0.785398163397448309615660845819875721"), result, _relativeTolerance);
        }

        [Fact]
        public void Atan2OfNegativeInfinityAndPositiveInfinityIsNegativePiOverFour()
        {
            // reference: pi / 4
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(Float128<TAccelerator>.NegativeInfinity, Float128<TAccelerator>.PositiveInfinity);

            AssertX.RelativelyEqual(-Float128<TAccelerator>.Parse("0.785398163397448309615660845819875721"), result, _relativeTolerance);
        }

        [Fact]
        public void Atan2OfInfiniteYAndFiniteXIsHalfPiWithSignOfY()
        {
            // reference: pi / 2
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(Float128<TAccelerator>.PositiveInfinity, 1.0)));
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("BFFF921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(Float128<TAccelerator>.NegativeInfinity, 1.0)));
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(Float128<TAccelerator>.PositiveInfinity, -1.0)));
        }

        [Fact]
        public void Atan2OfFiniteYAndPositiveInfinityXIsZeroWithSignOfY()
        {
            // IEEE 754-2008: atan2(y, +inf) is +/- 0 for finite y
            Float128<TAccelerator> positive = Float128<TAccelerator>.Atan2(1.0, Float128<TAccelerator>.PositiveInfinity);
            Float128<TAccelerator> negative = Float128<TAccelerator>.Atan2(-1.0, Float128<TAccelerator>.PositiveInfinity);

            Assert.True(Float128<TAccelerator>.IsZero(positive));
            Assert.False(Float128<TAccelerator>.IsNegative(positive));
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.True(Float128<TAccelerator>.IsNegative(negative));
        }

        [Fact]
        public void Atan2OfFiniteYAndNegativeInfinityXIsPiWithSignOfY()
        {
            // reference: pi
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("4000921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(1.0, Float128<TAccelerator>.NegativeInfinity)));
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("C000921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(-1.0, Float128<TAccelerator>.NegativeInfinity)));
        }

        [Fact]
        public void Atan2OfNegativeZeroAndPositiveZeroIsNegativeZero()
        {
            // IEEE 754-2008: atan2(-0, +0) is -0
            Float128<TAccelerator> result = Float128<TAccelerator>.Atan2(-Float128<TAccelerator>.Zero, Float128<TAccelerator>.Zero);

            Assert.True(Float128<TAccelerator>.IsZero(result));
            Assert.True(Float128<TAccelerator>.IsNegative(result));
        }

        [Fact]
        public void Atan2OfNegativeZeroAndNegativeInfinityIsNegativePi()
        {
            // reference: pi
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("C000921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(-Float128<TAccelerator>.Zero, Float128<TAccelerator>.NegativeInfinity)));
        }

        [Fact]
        public void Atan2OfNegativeZeroAndNegativeOneIsNegativePi()
        {
            // reference: -pi (C99 F.9.1.4: atan2(±0, x) is ±pi for x < 0)
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("C000921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(-Float128<TAccelerator>.Zero, -Float128<TAccelerator>.One)));
        }

        [Fact]
        public void Atan2OfHugeRatioIsHalfPi()
        {
            // reference: pi / 2
            Float128<TAccelerator> y = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 16381);
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(y, Float128<TAccelerator>.One)));
        }

        [Fact]
        public void Atan2OfTinyRatioWithPositiveXIsRatio()
        {
            // atan(t) = t for tiny t
            Float128<TAccelerator> tiny = Float128<TAccelerator>.Parse("1E-4900");
            Assert.Equal(tiny, Float128<TAccelerator>.Atan2(tiny, Float128<TAccelerator>.One));
        }

        [Fact]
        public void Atan2OfTinyRatioWithNegativeXIsPi()
        {
            // reference: pi
            Float128<TAccelerator> tiny = Float128<TAccelerator>.Parse("1E-4900");
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("4000921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(tiny, -Float128<TAccelerator>.One)));
        }

        [Fact]
        public void Atan2OfFiniteYAndNegativeZeroXIsHalfPiWithSignOfY()
        {
            // reference: pi / 2
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(1.0, -Float128<TAccelerator>.Zero)));
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("BFFF921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan2(-1.0, -Float128<TAccelerator>.Zero)));
        }

        [Fact]
        public void AsinOfOneIsHalfPi()
        {
            // reference: pi / 2
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("3FFF921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Asin(Float128<TAccelerator>.One)));
        }

        [Fact]
        public void AsinOfNegativeOneIsNegativeHalfPi()
        {
            // reference: -pi / 2
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("BFFF921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Asin(-Float128<TAccelerator>.One)));
        }

        [Fact]
        public void AsinOfSmallArgumentIsArgument()
        {
            // asin(x) rounds to x
            Float128<TAccelerator> tiny = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, -57);
            Float128<TAccelerator> subnormal = RawStorage<TAccelerator>.FromBits((UInt128)1);
            Assert.Equal(RawStorage<TAccelerator>.Bits(tiny), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Asin(tiny)));
            Assert.Equal(RawStorage<TAccelerator>.Bits(subnormal), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Asin(subnormal)));
        }

        [Fact]
        public void AsinOfSignedZeroIsSignedZero()
        {
            // IEEE 754 and C99: asin(+-0) = +-0
            Float128<TAccelerator> positive = Float128<TAccelerator>.Asin(Float128<TAccelerator>.Zero);
            Assert.True(Float128<TAccelerator>.IsZero(positive));
            Assert.False(RawStorage<TAccelerator>.IsSignSet(positive));

            Float128<TAccelerator> negative = Float128<TAccelerator>.Asin(Float128<TAccelerator>.NegativeZero);
            Assert.True(Float128<TAccelerator>.IsZero(negative));
            Assert.True(RawStorage<TAccelerator>.IsSignSet(negative));
        }

        [Fact]
        public void AcosOfOneIsZero()
        {
            // reference: acos(1) = 0
            Assert.Equal(RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Zero), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Acos(Float128<TAccelerator>.One)));
        }

        [Fact]
        public void AcosOfNegativeOneIsPi()
        {
            // reference: pi
            Assert.Equal(RawStorage<TAccelerator>.ParseBits("4000921FB54442D18469898CC51701B8"), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Acos(-Float128<TAccelerator>.One)));
        }

        [Fact]
        public void HypotOfLargeEqualLegsIsScaledSquareRootOfTwo()
        {
            // reference: sqrt(2)
            Float128<TAccelerator> leg = Float128<TAccelerator>.ScaleB(Float128<TAccelerator>.One, 16000);
            Float128<TAccelerator> ratio = Float128<TAccelerator>.Hypot(leg, leg) / leg;
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("1.41421356237309504880168872420969807856967"), ratio, _relativeTolerance);
        }

        [Fact]
        public void HypotOfSubnormalLegsIsExact()
        {
            // subnormal operands
            Float128<TAccelerator> three = RawStorage<TAccelerator>.FromBits((UInt128)3);
            Float128<TAccelerator> four = RawStorage<TAccelerator>.FromBits((UInt128)4);
            Assert.Equal(RawStorage<TAccelerator>.FromBits((UInt128)5), Float128<TAccelerator>.Hypot(three, four));
        }

        [Fact]
        public void HypotWithNaNLegIsNaN()
        {
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Hypot(Float128<TAccelerator>.NaN, (Float128<TAccelerator>)3.0)));
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Hypot((Float128<TAccelerator>)3.0, Float128<TAccelerator>.NaN)));
            Assert.True(Float128<TAccelerator>.IsNaN(Float128<TAccelerator>.Hypot(Float128<TAccelerator>.NaN, Float128<TAccelerator>.NaN)));
        }

        [Fact]
        public void AcosOfInteriorPointsMatchesReference()
        {
            // reference: acos(1/2) = pi/3, acos(-1/2) = 2pi/3, acos(0) = pi/2
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("1.047197551196597746154214461093167628"), Float128<TAccelerator>.Acos(0.5), _relativeTolerance);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("2.094395102393195492308428922186335256"), Float128<TAccelerator>.Acos(-0.5), _relativeTolerance);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("1.570796326794896619231321691639751442"), Float128<TAccelerator>.Acos(0.0), _relativeTolerance);
        }

        [Fact]
        public void AsinPiOfInteriorPointsMatchesReference()
        {
            // reference: asin(1/2) / pi = 1/6, asin(-1/2) / pi = -1/6
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("0.1666666666666666666666666666666666666667"), Float128<TAccelerator>.AsinPi(0.5), _relativeTolerance);
            AssertX.RelativelyEqual(-Float128<TAccelerator>.Parse("0.1666666666666666666666666666666666666667"), Float128<TAccelerator>.AsinPi(-0.5), _relativeTolerance);
        }

        [Fact]
        public void AcosPiOfEndpointsIsExact()
        {
            // acos(1) = 0 and acos(-1) = pi, so AcosPi(1) = +0 and AcosPi(-1) = 1
            Assert.Equal(RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Zero), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.AcosPi(Float128<TAccelerator>.One)));
            Assert.Equal(RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.One), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.AcosPi(-Float128<TAccelerator>.One)));
        }

        [Fact]
        public void AcosPiOfInteriorPointsMatchesReference()
        {
            // reference: acos(1/2) / pi = 1/3, acos(-1/2) / pi = 2/3
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("0.3333333333333333333333333333333333333333"), Float128<TAccelerator>.AcosPi(0.5), _relativeTolerance);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("0.6666666666666666666666666666666666666667"), Float128<TAccelerator>.AcosPi(-0.5), _relativeTolerance);
        }

        [Fact]
        public void AtanOfInteriorPointsMatchesReference()
        {
            // reference: atan(-1/2) = -atan(1/2), atan(2) = pi/2 - atan(1/2)
            AssertX.RelativelyEqual(-Float128<TAccelerator>.Parse("0.4636476090008061162142562314612144020285"), Float128<TAccelerator>.Atan(-0.5), _relativeTolerance);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("1.107148717794090503017065460178537040070"), Float128<TAccelerator>.Atan(2.0), _relativeTolerance);
        }

        [Fact]
        public void AtanOfSignedZeroIsSignedZero()
        {
            // IEEE 754 and C99: atan(+-0) = +-0
            Assert.Equal(RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Zero), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan(Float128<TAccelerator>.Zero)));
            Assert.Equal(RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.NegativeZero), RawStorage<TAccelerator>.Bits(Float128<TAccelerator>.Atan(Float128<TAccelerator>.NegativeZero)));
        }

        [Fact]
        public void AtanPiOfInteriorPointsMatchesReference()
        {
            // reference: atan(1) / pi = 1/4, atan(-1) / pi = -1/4
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("0.25"), Float128<TAccelerator>.AtanPi(1.0), _relativeTolerance);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("-0.25"), Float128<TAccelerator>.AtanPi(-1.0), _relativeTolerance);
        }

        [Fact]
        public void Atan2OfFiniteNonZeroOperandsMatchesReference()
        {
            // reference: quadrants I, IV, II, III: pi/4, -pi/4, 3pi/4, -3pi/4
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("0.7853981633974483096156608458198757210"), Float128<TAccelerator>.Atan2(1.0, 1.0), _relativeTolerance);
            AssertX.RelativelyEqual(-Float128<TAccelerator>.Parse("0.7853981633974483096156608458198757210"), Float128<TAccelerator>.Atan2(-1.0, 1.0), _relativeTolerance);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("2.356194490192344928846982537459627163"), Float128<TAccelerator>.Atan2(1.0, -1.0), _relativeTolerance);
            AssertX.RelativelyEqual(-Float128<TAccelerator>.Parse("2.356194490192344928846982537459627163"), Float128<TAccelerator>.Atan2(-1.0, -1.0), _relativeTolerance);
        }

        [Fact]
        public void Atan2PiOfFiniteNonZeroOperandsMatchesReference()
        {
            // reference: atan2(y, x) / pi for (1, 1), (1, -1), (-1, -1)
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("0.25"), Float128<TAccelerator>.Atan2Pi(1.0, 1.0), _relativeTolerance);
            AssertX.RelativelyEqual(Float128<TAccelerator>.Parse("0.75"), Float128<TAccelerator>.Atan2Pi(1.0, -1.0), _relativeTolerance);
            AssertX.RelativelyEqual(-Float128<TAccelerator>.Parse("0.75"), Float128<TAccelerator>.Atan2Pi(-1.0, -1.0), _relativeTolerance);
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