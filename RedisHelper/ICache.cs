using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisHelper
{
    internal interface ICache
    {
        T GetCache<T>(string cacheKey);
        string GetCacheString(string cacheKey);
        void SetCache<T>(string cacheKey, T objObject, TimeSpan time);
        void SetCache(string cacheKey, object objObject, TimeSpan time);
        bool Exists(string key);
        bool RemoveCache(string cacheKey);
        bool ClearCache();
    }
}
