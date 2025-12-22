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

global using Quad = QuadrupleLib.Float128<QuadrupleLib.Accelerators.DefaultAccelerator>;

using QuadrupleLib.Tests.Assertions.Exceptions;
using QuadrupleLib.Tests.Assertions.Types;
using System.Numerics;

namespace QuadrupleLib.Tests;

internal class Assert : Xunit.Assert
{
    private Assert() { }

    public static void NearlyEqual<T>(T expected, T actual, Precision precision)
        where T : IBinaryFloatingPointIeee754<T>
    {
        T roundedDiff = T.Round(expected - actual, (int)precision);
        if (roundedDiff != T.Zero)
        {
            throw new NearlyEqualException($"Assert.NearlyEqual() failure: Values differ\nExpected (within {precision}): {expected}\nActual: {actual}.");
        }
    }
}
