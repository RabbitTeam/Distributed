using Enyim.Caching;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Distributed.Utility.Impl
{
    /// <summary>
    /// 基于Memcached的分布式锁实现。
    /// </summary>
    public sealed class MemcachedDistributedLock : IDistributedLock
    {
        #region Field

        private const string DistributedLockGroupKey = "Distributed.Utility.DistributedLocks";
        private readonly IMemcachedClient _client;
        private static readonly object MemcachedSyncLock = new object();

        #endregion Field

        #region Constructor

        /// <summary>
        /// 初始化一个新的基于Memcached的分布式锁。
        /// </summary>
        /// <param name="client">Memcached客户端。</param>
        public MemcachedDistributedLock(IMemcachedClient client)
        {
            _client = client;
        }

        #endregion Constructor

        #region Implementation of IDistributedLock

        /// <summary>
        /// 锁定（超时时间为30秒）。
        /// </summary>
        /// <param name="token">锁记号。</param>
        public void Lock(string token)
        {
            TryLock(token, TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// 尝试锁定。
        /// </summary>
        /// <param name="token">锁记号。</param>
        /// <param name="timeout">超时时间。</param>
        public void TryLock(string token, TimeSpan timeout)
        {
            var startTime = DateTime.Now;

            lock (MemcachedSyncLock)
            {
                var runTime = DateTime.Now.Subtract(startTime);
                while (true)
                {
                    var tokens = GetTokens();
                    //锁已经存在执行。
                    if (tokens.Contains(token))
                    {
                        const int waitTime = 50;
                        Thread.Sleep(waitTime);
                        runTime = runTime.Add(TimeSpan.FromMilliseconds(waitTime));
                        if (runTime >= timeout)
                            throw new TimeoutException("请求分布式锁超时！");
                        continue;
                    }

                    //存入锁。
                    if (Store(tokens.Concat(new[] { token }).ToArray()))
                        break;
                }
            }
        }

        /// <summary>
        /// 解锁。
        /// </summary>
        /// <param name="token">锁记号。</param>
        public void UnLock(string token)
        {
            lock (MemcachedSyncLock)
            {
                var tokens = GetTokens();

                if (!tokens.Contains(token))
                    return;
                if (!Store(tokens.Where(i => i != token)))
                    throw new Exception("解锁失败！");
            }
        }

        #endregion Implementation of IDistributedLock

        #region Private Method

        private bool Store(IEnumerable<string> tokens)
        {
            return _client.Store(StoreMode.Set, DistributedLockGroupKey, tokens.ToArray());
        }

        private string[] GetTokens()
        {
            return _client.Get<string[]>(DistributedLockGroupKey) ?? new string[0];
        }

        #endregion Private Method
    }
}