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

using QuadrupleLib.Accelerators;

using System.Diagnostics.CodeAnalysis;
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

public partial struct Float128 : IComparable, IComparable<Float128>, IEquatable<Float128>, IFormattable, IParsable<Float128>
{
    private const int MAX_STRING_DIGITS = 33;

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
