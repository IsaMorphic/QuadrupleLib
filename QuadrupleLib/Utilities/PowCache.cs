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

namespace QuadrupleLib.Utilities
{
    internal static class PowCache
    {
        public const int MAX_CACHE_SIZE = (1 << 14) - 1;

        private static readonly Queue<int> _pow5CacheQueue = new();

        private static readonly Dictionary<int, BigInteger> _pow5Cache = new();

        public static BigInteger GetPow10(int n)
        {
            return GetPow5(n) << n;
        }

        public static BigInteger GetPow5(int n)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(n, nameof(n));

            int cacheSize;
            bool wasAdded;
            BigInteger pow5;
            lock (_pow5Cache)
            {
                if (!_pow5Cache.TryGetValue(n, out pow5))
                {
                    pow5 = BigInteger.Pow(5, n);
                    _pow5Cache.Add(n, pow5);
                    wasAdded = true;
                }
                else
                {
                    wasAdded = false;
                }

                cacheSize = _pow5Cache.Count;
            }

            int oldestExp = 0;
            bool shouldEvict = false;
            if (wasAdded)
            {
                lock (_pow5CacheQueue)
                {
                    shouldEvict = cacheSize > MAX_CACHE_SIZE && 
                        _pow5CacheQueue.TryDequeue(out oldestExp);
                    _pow5CacheQueue.Enqueue(n);
                }
            }

            if (shouldEvict)
            {
                lock (_pow5Cache)
                {
                    _pow5Cache.Remove(oldestExp);
                }
            }

            return pow5;
        }
    }
}
