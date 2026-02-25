using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Gateway
{
    /// <summary>
    /// 代理配置并发锁服务（单例）
    /// 用于在多个请求之间协调配置修改操作，防止并发修改导致数据不一致
    /// </summary>
    public class ProxyConfigLockService : IDisposable
    {
        private readonly SemaphoreSlim _lock = new(1, 1);
        private volatile bool _disposed = false;

        /// <summary>
        /// 获取并发锁（跨请求共享）
        /// </summary>
        public SemaphoreSlim Lock => _lock;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _lock?.Dispose();
        }
    }
}
