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

using System.Numerics;

namespace QuadrupleLib;

public partial struct Float128 : IBinaryFloatingPointIeee754<Float128>
{
    static Float128()
    {
        // Rounding related
        Float128 pow10 = One;
        List<Float128> pow10List = new();
        for (int i = 0; i < 38; i++)
        {
            pow10List.Add(pow10);
            pow10 *= 10;
        }
        _pow10Table = pow10List.ToArray();

        // CoRDiC implementation
        _thetaTable = Enumerable.Range(0, SINCOS_ITER_COUNT)
            .Select(AtanPow2).ToArray();
        _K_n = ComputeK(SINCOS_ITER_COUNT);
    }
}
