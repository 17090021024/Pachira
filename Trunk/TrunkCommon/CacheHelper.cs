using System;
using System.Collections.Generic;

namespace TrunkCommon
{
    /// <summary>
    /// 本地内存被动缓存
    /// </summary>
    public class LocalCache
    {
        /// <summary>
        /// 当加入同样的缓存项，是否覆盖，否则不加入
        /// </summary>
        public bool IsAddOverWriteSameKeyValue { get; set; }
        /// <summary>
        /// 当没有指定错误回调方法时，刷新值失败且本地缓存过期了，是否继续使用本地缓存的值
        /// </summary>
        public bool ContinueUseLocalCacheWhenRefeshFail { get; set; }
        private ShareLock Lock = new ShareLock();
        private Dictionary<string, LocalCacheItem> CacheItems = new Dictionary<string, LocalCacheItem>();

        /// <summary>
        /// 注册一个新缓存项
        /// </summary>
        /// <param name="item">缓存项</param>
        /// <returns>是否注册成功</returns>
        public bool Add(string key, TimeSpan expire, bool isSlidingExpiration, RefreshLocalCacheItem refresh, RefreshLocalCacheItemFail refreshFail)
        {
            LocalCacheItem item = new LocalCacheItem()
            {
                Key = key,
                Value = null,
                Expire = expire,
                IsSlidingExpiration = isSlidingExpiration,
                RefreshTime = DateTime.MinValue,
                Refresh = refresh,
                RefreshFail = refreshFail
            };

            using (Lock.WriteLock)
            {
                if (this.CacheItems.ContainsKey(item.Key))
                {
                    if (IsAddOverWriteSameKeyValue)
                        this.CacheItems[item.Key] = item;
                    else
                        return false;
                }
                else
                {
                    this.CacheItems.Add(item.Key, item);
                }
            }
            return true;
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <param name="key">缓存项键</param>
        /// <returns></returns>
        public LocalCacheItem GetCacheItem(string key)
        {
            DateTime nowTime = DateTime.Now;
            LocalCacheItem item = null;
            using (Lock.ReadLock)
            {
                if (CacheItems.TryGetValue(key, out item) && item != null)//缓存项存在的情况
                {
                    if (item.RefreshTime.Add(item.Expire) < nowTime)//过期了
                    {
                        object newValue = null;
                        Exception exception = null;
                        try
                        {
                            newValue = item.Refresh(item.Key);
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                            newValue = null;
                        }

                        if (newValue != null)
                        {
                            item.Value = newValue;
                            item.RefreshTime = nowTime;
                        }
                        else
                        {
                            if (item.RefreshFail != null)
                            {
                                if (!item.RefreshFail(item, exception))
                                    return null;
                            }
                            else if (!ContinueUseLocalCacheWhenRefeshFail)
                            {
                                return null;
                            }
                        }
                    }
                    else//没过期
                    {
                        if (item.IsSlidingExpiration)//滑动过期
                        {
                            item.RefreshTime = nowTime;
                        }
                    }
                }
                else//缓存项根本就没有
                {
                    return null;
                }
            }
            return item;
        }

        /// <summary>
        /// 获取缓存项值
        /// </summary>
        /// <param name="key">缓存项键</param>
        /// <returns></returns>
        public object Get(string key)
        {
            LocalCacheItem item = GetCacheItem(key);
            if (item != null)
                return item.Value;

            return null;
        }

        /// <summary>
        /// 获取缓存项值
        /// </summary>
        /// <param name="key">缓存项键</param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return (T)Get(key);
        }

        /// <summary>
        /// 删除某个缓存项。时间复杂度为O(1),线程安全
        /// </summary>
        /// <param name="key">缓存项的键</param>
        public bool Remove(string key)
        {
            using (Lock.WriteLock)
            {
                return CacheItems.Remove(key);
            }
        }
        /// <summary>
        /// 清空缓存项
        /// </summary>
        public void Clear()
        {
            using (Lock.WriteLock)
            {
                CacheItems.Clear();
            }
        }

        /// <summary>
        /// 获取缓存词典中缓存项个数
        /// </summary>
        public int Count
        {
            get
            {
                using (Lock.ReadLock)
                {
                    return CacheItems.Count;
                }
            }
        }

        /// <summary>
        /// 是否该缓存项存在在
        /// </summary>
        /// <param name="key">缓存项的键</param>
        /// <returns>是否存在</returns>
        public bool ContainsKey(string key)
        {
            using (Lock.ReadLock)
            {
                return CacheItems.ContainsKey(key);
            }
        }
    }

    [Serializable]
    public class LocalCacheItem
    {
        /// <summary>
        /// 缓存key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 最后刷新值时间
        /// </summary>
        public DateTime RefreshTime { get; set; }
        /// <summary>
        /// 有效时间长度
        /// </summary>
        public TimeSpan Expire { get; set; }
        /// <summary>
        /// 缓存的值(不能为null)
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// 刷新值的委托
        /// </summary>
        public RefreshLocalCacheItem Refresh { get; set; }
        /// <summary>
        /// 刷新失败的委托，返回true则继续使用本地值，否则不使用（返回null）
        /// </summary>
        public RefreshLocalCacheItemFail RefreshFail { get; set; }
        /// <summary>
        /// 是否是滑动过期
        /// </summary>
        public bool IsSlidingExpiration { get; set; }
    }

    [Serializable]
    public delegate object RefreshLocalCacheItem(string key);
    [Serializable]
    public delegate bool RefreshLocalCacheItemFail(LocalCacheItem item, Exception ex);
}
