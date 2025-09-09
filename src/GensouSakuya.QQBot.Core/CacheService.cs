using Microsoft.Extensions.Caching.Memory;
using System;

namespace GensouSakuya.QQBot.Core
{
    public class CacheService
    {
        private static readonly MemoryCacheEntryOptions _neverExpireOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = System.TimeSpan.FromDays(100),
        };
        private readonly IMemoryCache _memoryCache;
        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set(string key, object value)
        {
            _memoryCache.Set(key, value, _neverExpireOptions);
        }

        public void Set(string key, object value, TimeSpan expire, bool isSlide)
        {
            _memoryCache.Set(key, value, isSlide ? new MemoryCacheEntryOptions
            {
                SlidingExpiration = expire,
            } : new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expire,
            });
        }

        public bool TryGet<T>(string key, out T value)
        {
            return _memoryCache.TryGetValue<T>(key, out value);
        }
    }
}
