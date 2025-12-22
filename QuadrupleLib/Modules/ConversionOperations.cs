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
using System.Runtime.CompilerServices;

namespace QuadrupleLib;

public partial struct Float128<TAccelerator>
{
    #region Public API (conversion methods)

    static bool INumberBase<Float128<TAccelerator>>.TryConvertFromChecked<TOther>(TOther value, out Float128<TAccelerator> result)
    {
        if (typeof(TOther).GetGenericTypeDefinition() == typeof(Float128<>))
        {
            result = Unsafe.BitCast<TOther, Float128<TAccelerator>>(value);
            return true;
        }

        switch (value)
        {
            // Floating-point conversions
            case double x:
                result = x;
                return true;
            case float x:
                result = x;
                return true;
            case Half x:
                result = (Float128<TAccelerator>)x;
                return true;

            // Signed integer conversions
            case Int128 n:
                result = (Float128<TAccelerator>)n;
                return true;
            case long n:
                result = n;
                return true;
            case int n:
                result = n;
                return true;
            case short n:
                result = n;
                return true;
            case sbyte n:
                result = n;
                return true;

            // Unsigned integer conversions
            case UInt128 n:
                result = (Float128<TAccelerator>)n;
                return true;
            case ulong n:
                result = n;
                return true;
            case uint n:
                result = n;
                return true;
            case ushort n:
                result = n;
                return true;
            case byte n:
                result = n;
                return true;
        }

        try
        {
            result = (Float128<TAccelerator>)(object)value;
            return true;
        }
        catch (InvalidCastException)
        {
            result = _sNaN;
            return false;
        }
    }

    static bool INumberBase<Float128<TAccelerator>>.TryConvertFromSaturating<TOther>(TOther value, out Float128<TAccelerator> result)
    {
        if (typeof(TOther).GetGenericTypeDefinition() == typeof(Float128<>))
        {
            result = Unsafe.BitCast<TOther, Float128<TAccelerator>>(value);
            return true;
        }

        switch (value)
        {
            // Floating-point conversions
            case double x:
                result = x;
                return true;
            case float x:
                result = x;
                return true;
            case Half x:
                result = (Float128<TAccelerator>)x;
                return true;

            // Signed integer conversions
            case Int128 n:
                result = (Float128<TAccelerator>)n;
                return true;
            case long n:
                result = n;
                return true;
            case int n:
                result = n;
                return true;
            case short n:
                result = n;
                return true;
            case sbyte n:
                result = n;
                return true;

            // Unsigned integer conversions
            case UInt128 n:
                result = (Float128<TAccelerator>)n;
                return true;
            case ulong n:
                result = n;
                return true;
            case uint n:
                result = n;
                return true;
            case ushort n:
                result = n;
                return true;
            case byte n:
                result = n;
                return true;
        }

        try
        {
            result = (Float128<TAccelerator>)(object)value;
            return true;
        }
        catch (InvalidCastException)
        {
            result = _sNaN;
            return false;
        }
    }

    static bool INumberBase<Float128<TAccelerator>>.TryConvertFromTruncating<TOther>(TOther value, out Float128<TAccelerator> result)
    {
        if (typeof(TOther).GetGenericTypeDefinition() == typeof(Float128<>))
        {
            result = Unsafe.BitCast<TOther, Float128<TAccelerator>>(value);
            return true;
        }

        switch (value)
        {
            // Floating-point conversions
            case double x:
                result = x;
                return true;
            case float x:
                result = x;
                return true;
            case Half x:
                result = (Float128<TAccelerator>)x;
                return true;

            // Signed integer conversions
            case Int128 n:
                result = (Float128<TAccelerator>)n;
                return true;
            case long n:
                result = n;
                return true;
            case int n:
                result = n;
                return true;
            case short n:
                result = n;
                return true;
            case sbyte n:
                result = n;
                return true;

            // Unsigned integer conversions
            case UInt128 n:
                result = (Float128<TAccelerator>)n;
                return true;
            case ulong n:
                result = n;
                return true;
            case uint n:
                result = n;
                return true;
            case ushort n:
                result = n;
                return true;
            case byte n:
                result = n;
                return true;
        }

        try
        {
            result = (Float128<TAccelerator>)(object)value;
            return true;
        }
        catch (InvalidCastException)
        {
            result = _sNaN;
            return false;
        }
    }

    static bool INumberBase<Float128<TAccelerator>>.TryConvertToChecked<TOther>(Float128<TAccelerator> value, out TOther result)
    {
        if (typeof(TOther).GetGenericTypeDefinition() == typeof(Float128<>))
        {
            result = Unsafe.BitCast<Float128<TAccelerator>, TOther>(value);
            return true;
        }

        switch (Type.GetTypeCode(typeof(TOther)))
        {
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                return TOther.TryConvertFromChecked((Int128)value, out result!);

            case TypeCode.Byte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return TOther.TryConvertFromChecked((UInt128)value, out result!);

            default:
                try
                {
                    result = (TOther)(object)value;
                    return true;
                }
                catch (InvalidCastException)
                {
                    result = TOther.Zero;
                    return false;
                }
        }
    }

    static bool INumberBase<Float128<TAccelerator>>.TryConvertToSaturating<TOther>(Float128<TAccelerator> value, out TOther result)
    {
        if (typeof(TOther).GetGenericTypeDefinition() == typeof(Float128<>))
        {
            result = Unsafe.BitCast<Float128<TAccelerator>, TOther>(value);
            return true;
        }

        switch (Type.GetTypeCode(typeof(TOther)))
        {
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                return TOther.TryConvertFromSaturating((Int128)value, out result!);

            case TypeCode.Byte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return TOther.TryConvertFromSaturating((UInt128)value, out result!);

            default:
                try
                {
                    result = (TOther)(object)value;
                    return true;
                }
                catch (InvalidCastException)
                {
                    result = TOther.Zero;
                    return false;
                }
        }
    }

    static bool INumberBase<Float128<TAccelerator>>.TryConvertToTruncating<TOther>(Float128<TAccelerator> value, out TOther result)
    {
        if (typeof(TOther).GetGenericTypeDefinition() == typeof(Float128<>))
        {
            result = Unsafe.BitCast<Float128<TAccelerator>, TOther>(value);
            return true;
        }

        switch (Type.GetTypeCode(typeof(TOther)))
        {
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                return TOther.TryConvertFromTruncating((Int128)value, out result!);

            case TypeCode.Byte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return TOther.TryConvertFromTruncating((UInt128)value, out result!);

            default:
                try
                {
                    result = (TOther)(object)value;
                    return true;
                }
                catch (InvalidCastException)
                {
                    result = TOther.Zero;
                    return false;
                }
        }
    }

    #endregion
}
