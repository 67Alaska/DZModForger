using System;
using System.Collections.Generic;

namespace DZModForger.Utilities
{
    /// <summary>
    /// Simple in-memory cache with expiration support
    /// </summary>
    public class CacheHelper<TKey, TValue>
    {
        private class CacheEntry
        {
            public TValue Value { get; set; }
            public DateTime ExpirationTime { get; set; }

            public bool IsExpired => DateTime.Now > ExpirationTime;
        }

        private readonly Dictionary<TKey, CacheEntry> _cache = new();
        private readonly TimeSpan _defaultExpiration;

        public CacheHelper(TimeSpan? defaultExpiration = null)
        {
            _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Adds or updates cache entry
        /// </summary>
        public void Set(TKey key, TValue value, TimeSpan? expiration = null)
        {
            TimeSpan exp = expiration ?? _defaultExpiration;
            _cache[key] = new CacheEntry
            {
                Value = value,
                ExpirationTime = DateTime.Now.Add(exp)
            };
        }

        /// <summary>
        /// Gets cache entry if exists and not expired
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;

            if (_cache.TryGetValue(key, out var entry))
            {
                if (!entry.IsExpired)
                {
                    value = entry.Value;
                    return true;
                }
                else
                {
                    _cache.Remove(key);
                }
            }

            return false;
        }

        /// <summary>
        /// Gets cache entry or default
        /// </summary>
        public TValue GetOrDefault(TKey key, TValue defaultValue = default)
        {
            return TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Removes cache entry
        /// </summary>
        public bool Remove(TKey key)
        {
            return _cache.Remove(key);
        }

        /// <summary>
        /// Clears all cache entries
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Gets cache entry count
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// Removes expired entries
        /// </summary>
        public void RemoveExpired()
        {
            var expiredKeys = new List<TKey>();

            foreach (var kvp in _cache)
            {
                if (kvp.Value.IsExpired)
                    expiredKeys.Add(kvp.Key);
            }

            expiredKeys.ForEach(k => _cache.Remove(k));
        }
    }
}
