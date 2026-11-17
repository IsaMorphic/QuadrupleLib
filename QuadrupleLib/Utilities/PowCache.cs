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

using System.Collections.Concurrent;
using System.Numerics;

namespace QuadrupleLib.Utilities
{
    internal static class PowCache
    {
        private const long MAX_CACHE_SIZE_BYTES = 4096;

        private static readonly ConcurrentQueue<int> _pow5CacheQueue = new();

        private static readonly ConcurrentDictionary<int, BigInteger> _pow5Cache = new();

        private static long _currCacheSizeBytes;

        public static BigInteger GetPow10(int n)
        {
            return GetPow5(n) << n;
        }

        public static BigInteger GetPow5(int n)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(n, nameof(n));

            bool wasAdded;
            BigInteger pow5;
            if (!_pow5Cache.TryGetValue(n, out pow5))
            {
                pow5 = BigInteger.Pow(5, n);
                wasAdded = _pow5Cache.TryAdd(n, pow5);
            }
            else
            {
                wasAdded = false;
            }

            if (wasAdded)
            {
                Interlocked.Add(ref _currCacheSizeBytes, pow5.GetBitLength() / 8);
                _pow5CacheQueue.Enqueue(n);
            }

            int oldestExp;
            BigInteger removed;
            while (wasAdded && _currCacheSizeBytes > MAX_CACHE_SIZE_BYTES)
            {
                if (_pow5CacheQueue.TryDequeue(out oldestExp) &&
                    _pow5Cache.TryRemove(oldestExp, out removed))
                {
                    Interlocked.Add(ref _currCacheSizeBytes, -removed.GetBitLength() / 8);
                }
                else
                {
                    Thread.Yield();
                }
            }

            return pow5;
        }
    }
}
