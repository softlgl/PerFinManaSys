using System;
using System.Collections;
using System.Web;

namespace Cactus.Common
{
    /// <summary>
    /// 缓存处理类
    /// </summary>
    public class CacheHelper
    {
        private static System.Web.Caching.Cache objCache
        {
            get
            {
                return HttpRuntime.Cache;
            }
        }
        /// <summary>
        /// 获取数据缓存
        /// </summary>
        /// <param name="cacheKey">键</param>
        public static object GetCache(string cacheKey)
        {
            return objCache[cacheKey];
        }

        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void SetCache(string cacheKey, object objObject)
        {
            objCache.Insert(cacheKey, objObject);
        }

        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void SetCache(string cacheKey, object objObject, TimeSpan timeout)
        {
            objCache.Insert(cacheKey, objObject, null, DateTime.MaxValue, timeout, System.Web.Caching.CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void SetCache(string cacheKey, object objObject, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            objCache.Insert(cacheKey, objObject, null, absoluteExpiration, slidingExpiration);
        }

        /// <summary>
        /// 移除指定数据缓存
        /// </summary>
        public static void RemoveAllCache(string cacheKey)
        {
            objCache.Remove(cacheKey);
        }

        /// <summary>
        /// 移除全部缓存
        /// </summary>
        public static void RemoveAllCache()
        {
            IDictionaryEnumerator cacheEnum = objCache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                objCache.Remove(cacheEnum.Key.ToString());
            }
        }
    }
}
