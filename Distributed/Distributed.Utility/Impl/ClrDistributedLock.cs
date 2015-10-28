using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Distributed.Utility.Impl
{
    /// <summary>
    /// 基于Monitor的分布式锁。
    /// </summary>
    public sealed class ClrDistributedLock : IDistributedLock
    {
        private readonly ConcurrentDictionary<string, object> _dictionary = new ConcurrentDictionary<string, object>();

        #region Implementation of IDistributedLock

        /// <summary>
        /// 锁定（超时时间为30秒）。
        /// </summary>
        /// <param name="token">锁记号。</param>
        public void Lock(string token)
        {
            TryLock(token, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// 尝试锁定。
        /// </summary>
        /// <param name="token">锁记号。</param>
        /// <param name="timeout">超时时间。</param>
        public void TryLock(string token, TimeSpan timeout)
        {
            var syncLock = _dictionary.GetOrAdd(token, key => new object());
            Monitor.TryEnter(syncLock, timeout);
        }

        /// <summary>
        /// 解锁。
        /// </summary>
        /// <param name="token">锁记号。</param>
        public void UnLock(string token)
        {
            object syncLock;
            if (_dictionary.TryRemove(token, out syncLock))
                Monitor.Exit(syncLock);
        }

        #endregion Implementation of IDistributedLock
    }
}