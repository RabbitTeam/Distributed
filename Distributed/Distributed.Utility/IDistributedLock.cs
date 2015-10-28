using System;

namespace Distributed.Utility
{
    /// <summary>
    /// 一个抽象的分布式锁控制器。
    /// </summary>
    public interface IDistributedLock
    {
        /// <summary>
        /// 锁定（超时时间为30秒）。
        /// </summary>
        /// <param name="token">锁记号。</param>
        void Lock(string token);

        /// <summary>
        /// 尝试锁定。
        /// </summary>
        /// <param name="token">锁记号。</param>
        /// <param name="timeout">超时时间。</param>
        void TryLock(string token, TimeSpan timeout);

        /// <summary>
        /// 解锁。
        /// </summary>
        /// <param name="token">锁记号。</param>
        void UnLock(string token);
    }
}