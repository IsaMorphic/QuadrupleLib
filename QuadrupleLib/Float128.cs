/*
 *  Copyright 2024-2026 Chosen Few Software
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

using System.Numerics;

namespace QuadrupleLib;

public partial struct Float128<TAccelerator> :
    IBinaryFloatingPointIeee754<Float128<TAccelerator>>
    where TAccelerator : IAccelerator
{
    static Float128()
    {
        // Rounding related
        Float128<TAccelerator> pow10 = One;
        List<Float128<TAccelerator>> pow10List = new();
        for (int i = 0; i < 38; i++)
        {
            pow10List.Add(pow10);
            pow10 *= 10;
        }
        _pow10Table = pow10List.ToArray();

        // CoRDiC implementation
        _thetaTable = Enumerable.Range(0, SINCOS_ITER_COUNT)
            .Select(AtanPow2).ToArray();
        _invK_n = ComputeInverseK(SINCOS_ITER_COUNT);
    }

    public static implicit operator Float128(Float128<TAccelerator> x)
    {
        return new Float128(x._rawBits);
    }

    public static implicit operator Float128<TAccelerator>(Float128 x) 
    {
        return x.WithAccelerator<TAccelerator>();
    }

    public static Float128<UAccelerator> WithAccelerator<UAccelerator>(Float128<TAccelerator> x)
        where UAccelerator : IAccelerator
    {
        return new Float128<UAccelerator>(x._rawBits);
    }

    public Float128<UAccelerator> WithAccelerator<UAccelerator>()
        where UAccelerator : IAccelerator
    {
        return new Float128<UAccelerator>(_rawBits);
    }
}

public partial struct Float128 : IComparable, IComparable<Float128>, IEquatable<Float128>, IFormattable, ISpanFormattable, IParsable<Float128>, ISpanParsable<Float128>
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
    private static readonly Float128 _epsilon = new Float128(UInt128.One, -EXPONENT_BIAS + 1, false);
    private static readonly Float128 _e = Parse("2.7182818284590452353602874713526625");
    private static readonly Float128 _quarterPi = Parse("0.7853981633974483096156608458198757");
    private static readonly Float128 _halfPi = Parse("1.5707963267948966192313216916397514");
    private static readonly Float128 _pi = Parse("3.1415926535897932384626433832795028");
    private static readonly Float128 _tau = Parse("6.2831853071795864769252867665590058");

    #endregion

    #region Public API (string conversion related)

    private const int MAX_STRING_DIGITS = 34;
    private const int MAX_PADDING_DIGITS = 23;

    #endregion

    private UInt128 _rawBits;

    public Float128<TAccelerator> WithAccelerator<TAccelerator>()
        where TAccelerator : IAccelerator
    {
        return new Float128<TAccelerator>(_rawBits);
    }

    public static Float128<TAccelerator> WithAccelerator<TAccelerator>(Float128 x)
        where TAccelerator : IAccelerator
    {
        return new Float128<TAccelerator>(x._rawBits);
    }
}
