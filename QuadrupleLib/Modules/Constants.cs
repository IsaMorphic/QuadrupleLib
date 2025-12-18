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

namespace QuadrupleLib;

public partial struct Float128
{
    #region Private API (constants)

    private static readonly UInt128 SIGNIFICAND_MASK = UInt128.MaxValue >> 16;
    private static readonly UInt128 EXPONENT_MASK = ~SIGNIFICAND_MASK ^ SIGNBIT_MASK;
    private static readonly UInt128 SIGNBIT_MASK = (UInt128.MaxValue >> 1) + 1;
    private static readonly ushort EXPONENT_BIAS = short.MaxValue >> 1;

    #endregion

    #region Public API (constants)

    private static readonly Float128 _qNaN = new Float128(UInt128.One, short.MaxValue, false);
    private static readonly Float128 _sNaN = new Float128(UInt128.One, short.MaxValue, true);

    private static readonly Float128 _pInf = new Float128(UInt128.Zero, short.MaxValue, false);
    private static readonly Float128 _nInf = new Float128(UInt128.Zero, short.MaxValue, true);

    public static Float128 PositiveInfinity => _pInf;
    public static Float128 NegativeInfinity => _nInf;
    public static Float128 NaN => _qNaN;

    private static readonly Float128 _epsilon = new Float128(UInt128.One, -EXPONENT_BIAS + 1, false);
    public static Float128 Epsilon => _epsilon;

    public static Float128 One => 1.0;
    public static Float128 Zero => new();
    public static Float128 NegativeOne => -1.0;

    public static int Radix => 2;
    public static Float128 AdditiveIdentity => Zero;
    public static Float128 MultiplicativeIdentity => One;

    public static Float128 NegativeZero => new(UInt128.Zero, 0, true);

    private static readonly Float128 _e = Parse("2.7182818284590452353602874713526625");
    public static Float128 E => _e;
    
    private static readonly Float128 _quarterPi = Parse("0.7853981633974483096156608458198757");
    public static Float128 QuarterPi = _quarterPi;

    private static readonly Float128 _halfPi = Parse("1.5707963267948966192313216916397514");
    public static Float128 HalfPi = _halfPi;

    private static readonly Float128 _pi = Parse("3.1415926535897932384626433832795028");
    public static Float128 Pi => _pi;

    private static readonly Float128 _tau = Parse("6.2831853071795864769252867665590058");
    public static Float128 Tau => _tau;

    #endregion
}
