﻿using System.Collections.Generic;
using SS.GovApplication.Core.Provider;
using SS.GovApplication.Core.Utils;

namespace SS.GovApplication.Core
{
    public static class DataCountManager
    {
        private static class DataCountManagerCache
        {
            private static readonly object LockObject = new object();

            private static string GetCacheKey(int siteId)
            {
                return $"SS.GovApplication.Core.DataCountManager.{siteId}";
            }

            public static Dictionary<string, int> GetDataCountsCache(int siteId)
            {
                var cacheKey = GetCacheKey(siteId);
                var retval = CacheUtils.Get<Dictionary<string, int>>(cacheKey);
                if (retval != null) return retval;

                lock (LockObject)
                {
                    retval = CacheUtils.Get<Dictionary<string, int>>(cacheKey);
                    if (retval == null)
                    {
                        retval = DataDao.GetDataCounts(siteId);

                        CacheUtils.InsertHours(cacheKey, retval, 1);
                    }
                }

                return retval;
            }

            public static void Clear(int siteId)
            {
                var cacheKey = GetCacheKey(siteId);
                CacheUtils.Remove(cacheKey);
            }
        }

        public static int GetCount(int siteId, string state)
        {
            var entries = DataCountManagerCache.GetDataCountsCache(siteId);
            return entries[state];
        }

        public static int GetTotalCount(int siteId)
        {
            var total = 0;
            var entries = DataCountManagerCache.GetDataCountsCache(siteId);
            foreach (var state in DataState.AllStates)
            {
                total += entries[state.Value];
            }
            return total;
        }

        public static void ClearCache(int siteId)
        {
            DataCountManagerCache.Clear(siteId);
        }
    }
}
