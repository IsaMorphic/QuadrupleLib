using System.Numerics;

namespace QuadrupleLib.Utilities
{
    internal static class PowCache
    {
        private const int MAX_CACHE_SIZE = 1024;

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
