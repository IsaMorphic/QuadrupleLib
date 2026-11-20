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

using System.Numerics;
using System.Reflection;
using QuadrupleLib.Accelerators;
using Xunit;

namespace QuadrupleLib.Tests.Conversion
{
    public abstract class NumberBaseConversionTests<TAccelerator>
        where TAccelerator : IAccelerator
    {
        #region TryConvertFrom

        [Theory]
        [InlineData("TryConvertFromChecked", double.NaN)]
        [InlineData("TryConvertFromChecked", double.PositiveInfinity)]
        [InlineData("TryConvertFromChecked", double.NegativeInfinity)]
        [InlineData("TryConvertFromChecked", 0.0)]
        [InlineData("TryConvertFromChecked", -0.0)]
        [InlineData("TryConvertFromChecked", double.Epsilon)]
        [InlineData("TryConvertFromSaturating", double.NaN)]
        [InlineData("TryConvertFromSaturating", double.PositiveInfinity)]
        [InlineData("TryConvertFromSaturating", double.NegativeInfinity)]
        [InlineData("TryConvertFromSaturating", -0.0)]
        [InlineData("TryConvertFromSaturating", double.Epsilon)]
        [InlineData("TryConvertFromTruncating", double.NaN)]
        [InlineData("TryConvertFromTruncating", double.PositiveInfinity)]
        [InlineData("TryConvertFromTruncating", double.NegativeInfinity)]
        [InlineData("TryConvertFromTruncating", -0.0)]
        [InlineData("TryConvertFromTruncating", double.Epsilon)]
        public void TryConvertFromSpecialDoubleIsSameValue(string methodName, double value)
        {
            // the infinities, signed zeros and subnormals are exact in binary128, and NaN stays NaN
            bool ok = InvokeTryConvertFrom(methodName, value, typeof(double), out object? result);
            Float128<TAccelerator> converted = (Float128<TAccelerator>)result!;

            Assert.True(ok);
            Assert.Equal(double.IsNaN(value), Float128<TAccelerator>.IsNaN(converted));
            if (!double.IsNaN(value))
            {
                Assert.Equal(double.IsNegative(value), Float128<TAccelerator>.IsNegative(converted));
                Assert.Equal(value, (double)converted);
            }
        }

        [Theory]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(Int128))]
        [InlineData(typeof(UInt128))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(Half))]
        public void TryConvertFromCheckedSucceeds(Type type)
        {
            bool ok = InvokeTryConvertFrom("TryConvertFromChecked", SampleValue(type), type, out _);

            Assert.True(ok);
        }

        [Theory]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(Int128))]
        [InlineData(typeof(UInt128))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(Half))]
        public void TryConvertFromSaturatingSucceeds(Type type)
        {
            bool ok = InvokeTryConvertFrom("TryConvertFromSaturating", SampleValue(type), type, out _);

            Assert.True(ok);
        }

        [Theory]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(Int128))]
        [InlineData(typeof(UInt128))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(Half))]
        public void TryConvertFromTruncatingSucceeds(Type type)
        {
            bool ok = InvokeTryConvertFrom("TryConvertFromTruncating", SampleValue(type), type, out _);

            Assert.True(ok);
        }

        [Fact]
        public void TryConvertFromCheckedOfSelfIsSuccess()
        {
            Float128<TAccelerator> value = 1.5;

            bool ok = InvokeTryConvertFrom("TryConvertFromChecked", value, typeof(Float128<TAccelerator>), out var result);

            Assert.True(ok);
            Assert.Equal(value, (Float128<TAccelerator>)result!);
        }

        [Fact]
        public void TryConvertFromCheckedOfOtherAcceleratorIsSuccess()
        {
            Float128<SoftwareAccelerator> value = 1.5;

            bool ok = InvokeTryConvertFrom("TryConvertFromChecked", value, typeof(Float128<SoftwareAccelerator>), out var result);

            Assert.True(ok);
        }

        [Fact]
        public void TryConvertFromSaturatingOfSelfIsSuccess()
        {
            Float128<TAccelerator> value = 1.5;

            bool ok = InvokeTryConvertFrom("TryConvertFromSaturating", value, typeof(Float128<TAccelerator>), out var result);

            Assert.True(ok);
            Assert.Equal(value, (Float128<TAccelerator>)result!);
        }

        [Fact]
        public void TryConvertFromSaturatingOfOtherAcceleratorIsSuccess()
        {
            Float128<SoftwareAccelerator> value = 1.5;

            bool ok = InvokeTryConvertFrom("TryConvertFromSaturating", value, typeof(Float128<SoftwareAccelerator>), out var result);

            Assert.True(ok);
        }

        [Fact]
        public void TryConvertFromTruncatingOfSelfIsSuccess()
        {
            Float128<TAccelerator> value = 1.5;

            bool ok = InvokeTryConvertFrom("TryConvertFromTruncating", value, typeof(Float128<TAccelerator>), out var result);

            Assert.True(ok);
            Assert.Equal(value, (Float128<TAccelerator>)result!);
        }

        [Fact]
        public void TryConvertFromTruncatingOfOtherAcceleratorIsSuccess()
        {
            Float128<SoftwareAccelerator> value = 1.5;

            bool ok = InvokeTryConvertFrom("TryConvertFromTruncating", value, typeof(Float128<SoftwareAccelerator>), out var result);

            Assert.True(ok);
        }

        [Fact]
        public void TryConvertFromCheckedOfNonConvertibleIsSNaN()
        {
            bool ok = InvokeTryConvertFrom("TryConvertFromChecked", 1.5m, typeof(decimal), out var result);

            Assert.False(ok);
            Assert.True(Float128<TAccelerator>.IsNaN((Float128<TAccelerator>)result!));
        }

        [Fact]
        public void TryConvertFromSaturatingOfNonConvertibleIsSNaN()
        {
            bool ok = InvokeTryConvertFrom("TryConvertFromSaturating", 1.5m, typeof(decimal), out var result);

            Assert.False(ok);
            Assert.True(Float128<TAccelerator>.IsNaN((Float128<TAccelerator>)result!));
        }

        [Fact]
        public void TryConvertFromTruncatingOfNonConvertibleIsSNaN()
        {
            bool ok = InvokeTryConvertFrom("TryConvertFromTruncating", 1.5m, typeof(decimal), out var result);

            Assert.False(ok);
            Assert.True(Float128<TAccelerator>.IsNaN((Float128<TAccelerator>)result!));
        }

        #endregion

        #region TryConvertTo

        [Theory]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(Int128))]
        [InlineData(typeof(UInt128))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(Half))]
        public void TryConvertToCheckedSucceeds(Type type)
        {
            bool ok = InvokeTryConvertTo("TryConvertToChecked", 1.5, type, out _);

            Assert.True(ok);
        }

        [Theory]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(Int128))]
        [InlineData(typeof(UInt128))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(Half))]
        public void TryConvertToSaturatingSucceeds(Type type)
        {
            bool ok = InvokeTryConvertTo("TryConvertToSaturating", 1.5, type, out _);

            Assert.True(ok);
        }

        [Theory]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(Int128))]
        [InlineData(typeof(UInt128))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(Half))]
        public void TryConvertToTruncatingSucceeds(Type type)
        {
            bool ok = InvokeTryConvertTo("TryConvertToTruncating", 1.5, type, out _);

            Assert.True(ok);
        }

        [Fact]
        public void TryConvertToCheckedOfOtherAcceleratorIsSuccess()
        {
            bool ok = InvokeTryConvertTo("TryConvertToChecked", 1.5, typeof(Float128<SoftwareAccelerator>), out var result);

            Assert.True(ok);
        }

        [Fact]
        public void TryConvertToSaturatingOfOtherAcceleratorIsSuccess()
        {
            bool ok = InvokeTryConvertTo("TryConvertToSaturating", 1.5, typeof(Float128<SoftwareAccelerator>), out var result);

            Assert.True(ok);
        }

        [Fact]
        public void TryConvertToTruncatingOfOtherAcceleratorIsSuccess()
        {
            bool ok = InvokeTryConvertTo("TryConvertToTruncating", 1.5, typeof(Float128<SoftwareAccelerator>), out var result);

            Assert.True(ok);
        }

        [Fact]
        public void TryConvertToCheckedOfNonConvertibleIsZero()
        {
            bool ok = InvokeTryConvertTo("TryConvertToChecked", 1.5, typeof(decimal), out var result);

            Assert.False(ok);
            Assert.Equal(0m, (decimal)result!);
        }

        [Fact]
        public void TryConvertToSaturatingOfNonConvertibleIsZero()
        {
            bool ok = InvokeTryConvertTo("TryConvertToSaturating", 1.5, typeof(decimal), out var result);

            Assert.False(ok);
            Assert.Equal(0m, (decimal)result!);
        }

        [Fact]
        public void TryConvertToTruncatingOfNonConvertibleIsZero()
        {
            bool ok = InvokeTryConvertTo("TryConvertToTruncating", 1.5, typeof(decimal), out var result);

            Assert.False(ok);
            Assert.Equal(0m, (decimal)result!);
        }

        #endregion

        #region Private API

        private static object SampleValue(Type type) => type switch
        {
            _ when type == typeof(double) => 1.5,
            _ when type == typeof(float) => 1.5f,
            _ when type == typeof(Half) => (Half)1.5,
            _ when type == typeof(Int128) => new Int128(0UL, 12345UL),
            _ when type == typeof(UInt128) => new UInt128(0UL, 12345UL),
            _ when type == typeof(long) => 12345L,
            _ when type == typeof(ulong) => 12345UL,
            _ when type == typeof(int) => 12345,
            _ when type == typeof(uint) => 12345U,
            _ when type == typeof(short) => (short)12345,
            _ when type == typeof(ushort) => (ushort)12345,
            _ when type == typeof(sbyte) => (sbyte)123,
            _ when type == typeof(byte) => (byte)123,
            _ => throw new ArgumentException($"No sample value for {type}.", nameof(type))
        };

        // The TryConvert* methods are explicit static interface implementations, so they are reached
        // through the interface map. Each name is unique on INumberBase<T>; the lookup throws if one is missing.
        private static bool InvokeTryConvertFrom(string methodName, object value, Type target, out object? result)
        {
            InterfaceMapping map = typeof(Float128<TAccelerator>).GetInterfaceMap(typeof(INumberBase<Float128<TAccelerator>>));
            int idx = Array.FindIndex(map.InterfaceMethods, m => m.Name == methodName);
            if (idx < 0)
            {
                throw new InvalidOperationException($"Method {methodName} not found on INumberBase<Float128<TAccelerator>>.");
            }

            MethodInfo method = map.TargetMethods[idx];
            object?[] parameters = [value, null];
            bool ok = (bool)method.MakeGenericMethod(target).Invoke(null, parameters)!;
            result = parameters[1];
            return ok;
        }

        private static bool InvokeTryConvertTo(string methodName, double value, Type target, out object? result)
        {
            Float128<TAccelerator> f128 = value;
            InterfaceMapping map = typeof(Float128<TAccelerator>).GetInterfaceMap(typeof(INumberBase<Float128<TAccelerator>>));
            int idx = Array.FindIndex(map.InterfaceMethods, m => m.Name == methodName);
            if (idx < 0)
            {
                throw new InvalidOperationException($"Method {methodName} not found on INumberBase<Float128<TAccelerator>>.");
            }

            MethodInfo method = map.TargetMethods[idx];
            object?[] parameters = [f128, null];
            bool ok = (bool)method.MakeGenericMethod(target).Invoke(null, parameters)!;
            result = parameters[1];
            return ok;
        }

        #endregion
    }

    public class NumberBaseConversionTests_DefaultAccelerator :
        NumberBaseConversionTests<DefaultAccelerator>
    { }

    public class NumberBaseConversionTests_SoftwareAccelerator :
        NumberBaseConversionTests<SoftwareAccelerator>
    { }
}
