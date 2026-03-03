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

namespace QuadrupleLib;

public partial struct Float128<TAccelerator>
{
    #region Raw storage properties

    private UInt128 _rawBits;

    private bool RawSignBit
    {
        get => _rawBits > (UInt128.MaxValue >> 1);
        set
        {
            _rawBits = value ?
                _rawBits | SIGNBIT_MASK :
                _rawBits & ~SIGNBIT_MASK;
        }
    }

    private ushort RawExponent
    {
        get => (ushort)((_rawBits & ~SIGNBIT_MASK) >> 112);
        set
        {
            _rawBits = _rawBits & ~EXPONENT_MASK | ((UInt128)value << 112);
        }
    }

    private UInt128 RawSignificand
    {
        get => _rawBits & SIGNIFICAND_MASK;
        set
        {
            _rawBits = (_rawBits & ~SIGNIFICAND_MASK) | value;
        }
    }

    #endregion

    #region Raw storage constructors

    private Float128(UInt128 rawBits)
    {
        _rawBits = rawBits;
    }

    #endregion

    #region Representational properties

    private short Exponent
    {
        get
        {
            if (RawExponent == 0) // check if subnormal
            {
                return (short)(-EXPONENT_BIAS + 1);
            }
            else if (RawExponent != short.MaxValue)
            {
                return (short)(RawExponent - EXPONENT_BIAS);
            }
            else
            {
                return short.MaxValue;
            }
        }
    }

    private UInt128 Significand => RawExponent == 0 ? RawSignificand : RawSignificand | (SIGNIFICAND_MASK + 1);

    #endregion

    #region Representational constructors

    private Float128(UInt128 rawSignificand, int exponent, bool rawSignBit)
    {
        RawSignificand = rawSignificand;

        if (exponent == -EXPONENT_BIAS + 1)
        {
            RawExponent = 0;
        }
        else if (exponent != short.MaxValue)
        {
            RawExponent = (ushort)(exponent + EXPONENT_BIAS);
        }
        else
        {
            RawExponent = (ushort)short.MaxValue;
        }

        RawSignBit = rawSignBit;
    }

    #endregion
}
