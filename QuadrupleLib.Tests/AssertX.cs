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

using QuadrupleLib.Tests.Assertions.Exceptions;
using QuadrupleLib.Tests.Assertions.Types;
using System.Numerics;

namespace QuadrupleLib.Tests
{
    // Assertion extensions
    internal static class AssertX
    {
        public static void NearlyEqual<T>(T expected, T actual, Precision precision)
            where T : IBinaryFloatingPointIeee754<T>
        {
            T roundedDiff = T.Round(expected - actual, (int)precision);
            if (roundedDiff != T.Zero)
            {
                throw new NearlyEqualException($"{nameof(AssertX)}.{nameof(NearlyEqual)} failure: Values differ\nExpected (within {precision}): {expected}\nActual: {actual}.");
            }
        }

        public static void RelativelyEqual<T>(T expected, T actual, T relativeTolerance)
            where T : IBinaryFloatingPointIeee754<T>
        {
            T bound = T.Abs(expected) * relativeTolerance;
            // written as a negated <= so that a NaN difference fails the check
            if (!(T.Abs(actual - expected) <= bound))
            {
                throw new NearlyEqualException($"{nameof(AssertX)}.{nameof(RelativelyEqual)} failure: Values differ by more than {relativeTolerance} relative\nExpected: {expected}\nActual: {actual}.");
            }
        }

        public static void WithinUlps<TAccelerator>(Float128<TAccelerator> expected, Float128<TAccelerator> actual, UInt128 maxUlps)
            where TAccelerator : IAccelerator
        {
            // bit-pattern distance
            UInt128 signBit = UInt128.One << 127;
            UInt128 expectedBits = RawStorage<TAccelerator>.Bits(expected);
            UInt128 actualBits = RawStorage<TAccelerator>.Bits(actual);
            UInt128 expectedMagnitude = expectedBits & ~signBit;
            UInt128 actualMagnitude = actualBits & ~signBit;
            UInt128 distance = expectedMagnitude > actualMagnitude ? expectedMagnitude - actualMagnitude : actualMagnitude - expectedMagnitude;
            if (Float128<TAccelerator>.IsNaN(expected) || Float128<TAccelerator>.IsNaN(actual))
            {
                throw new NearlyEqualException($"{nameof(AssertX)}.{nameof(WithinUlps)} failure: A NaN operand is never within ulps\nExpected: {expected}\nActual: {actual}.");
            }
            if ((Float128<TAccelerator>.IsInfinity(expected) || Float128<TAccelerator>.IsInfinity(actual)) && expectedBits != actualBits)
            {
                throw new NearlyEqualException($"{nameof(AssertX)}.{nameof(WithinUlps)} failure: An infinite operand is within ulps only of the identical value\nExpected: {expected}\nActual: {actual}.");
            }
            // sign mismatch
            if ((expectedBits & signBit) != (actualBits & signBit))
            {
                throw new NearlyEqualException($"{nameof(AssertX)}.{nameof(WithinUlps)} failure: Values differ in sign\nExpected: {expected}\nActual: {actual}.");
            }
            if (distance > maxUlps)
            {
                throw new NearlyEqualException($"{nameof(AssertX)}.{nameof(WithinUlps)} failure: Values differ by more than {maxUlps} ulps\nExpected: {expected}\nActual: {actual}.");
            }
        }

        public static void DoesNotThrow<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                throw new DoesNotThrowException($"{nameof(AssertX)}.{nameof(DoesNotThrow)} failure: Expected no exception of type {typeof(TException).Name} to be thrown, but it was thrown with message: {ex.Message}", ex);
            }
        }
    }
}
