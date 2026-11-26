/*
 *  Copyright 2026 Chosen Few Software
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

using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace QuadrupleLib.Tests
{
    // Test-only access to the binary128 bit pattern. Reflection is used here because the raw
    // field and the raw-bits and parts constructors are private.
    internal static class RawStorage<TAccelerator>
        where TAccelerator : IAccelerator
    {
        private const BindingFlags INSTANCE_MEMBERS = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly UInt128 SIGN_BIT = UInt128.One << 127;

        private static readonly FieldInfo _rawBitsField = typeof(Float128<TAccelerator>).GetField("_rawBits", INSTANCE_MEMBERS)!;
        private static readonly ConstructorInfo _rawBitsConstructor = typeof(Float128<TAccelerator>).GetConstructor(INSTANCE_MEMBERS, null, new[] { typeof(UInt128) }, null)!;
        private static readonly ConstructorInfo _partsConstructor = typeof(Float128<TAccelerator>).GetConstructor(INSTANCE_MEMBERS, null, new[] { typeof(UInt128), typeof(int), typeof(bool) }, null)!;

        public static UInt128 Bits(Float128<TAccelerator> value)
        {
            return (UInt128)_rawBitsField.GetValue(value)!;
        }

        public static bool IsSignSet(Float128<TAccelerator> value)
        {
            return (Bits(value) & SIGN_BIT) != UInt128.Zero;
        }

        public static Float128<TAccelerator> FromBits(UInt128 bits)
        {
            return (Float128<TAccelerator>)_rawBitsConstructor.Invoke(new object[] { bits });
        }

        public static Float128<TAccelerator> FromParts(UInt128 significand, int exponent, bool negative)
        {
            return (Float128<TAccelerator>)_partsConstructor.Invoke(new object[] { significand, exponent, negative });
        }

        public static UInt128 ParseBits(string hex)
        {
            return UInt128.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }

        // a normal significand in [2^112, 2^113)
        public static BigInteger RandomSignificand(Random random)
        {
            byte[] bytes = new byte[16];
            random.NextBytes(bytes);
            BigInteger fraction = new BigInteger(bytes, isUnsigned: true);
            return (fraction & ((BigInteger.One << 112) - 1)) | (BigInteger.One << 112);
        }

        // binary128 bits for (-1)^negative * significand * 2^(leadingExponent - 112), normal significand only
        public static UInt128 PackBits(bool negative, int leadingExponent, BigInteger significand)
        {
            UInt128 sign = negative ? UInt128.One << 127 : UInt128.Zero;
            UInt128 biased = (UInt128)(leadingExponent + 16383) << 112;
            return sign | biased | (UInt128)(significand - (BigInteger.One << 112));
        }

        public static Float128<TAccelerator> LargestFinite()
        {
            return FromBits(((UInt128)0x7FFE << 112) | ((UInt128.One << 112) - UInt128.One));
        }
    }
}
